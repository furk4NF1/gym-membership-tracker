using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using SporSalonuTakip.Entities;
namespace SporSalonuTakip
{
    internal static class Database
    {
        private const string ConnectionString = @"Data Source=localhost;Initial Catalog=GymTracking;Integrated Security=True;TrustServerCertificate=True";
        public static SqlConnection OpenConnection() { return new SqlConnection(ConnectionString); }

        public static UserRecord FindUser(string username)
        {
            using (SqlConnection connection = OpenConnection())
            using (SqlCommand command = new SqlCommand("SELECT UserId,Username,Email,Role,PasswordHash,PasswordSalt,PasswordIterations,IsLegacyHash,MustChangePassword,IsActive FROM Users WHERE Username=@name", connection))
            {
                command.Parameters.Add("@name", SqlDbType.NVarChar, 50).Value = username;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return null;
                    return new UserRecord
                    {
                        UserId = reader.GetInt32(0), Username = reader.GetString(1), Email = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Role = reader.GetString(3), PasswordHash = reader.GetString(4), PasswordSalt = reader.IsDBNull(5) ? null : (byte[])reader[5],
                        PasswordIterations = reader.GetInt32(6), IsLegacyHash = reader.GetBoolean(7), MustChangePassword = reader.GetBoolean(8), IsActive = reader.GetBoolean(9)
                    };
                }
            }
        }

        public static UserRecord FindUserByEmail(string email)
        {
            using (SqlConnection connection = OpenConnection()) using (SqlCommand command = new SqlCommand("SELECT UserId,Username,Email,Role,PasswordHash,PasswordSalt,PasswordIterations,IsLegacyHash,MustChangePassword,IsActive FROM Users WHERE Email=@email", connection))
            {
                command.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = email;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return null;
                    return new UserRecord
                    {
                        UserId = reader.GetInt32(0), Username = reader.GetString(1), Email = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Role = reader.GetString(3), PasswordHash = reader.GetString(4), PasswordSalt = reader.IsDBNull(5) ? null : (byte[])reader[5],
                        PasswordIterations = reader.GetInt32(6), IsLegacyHash = reader.GetBoolean(7), MustChangePassword = reader.GetBoolean(8), IsActive = reader.GetBoolean(9)
                    };
                }
            }
        }

        public static void UpgradeLegacyPassword(int userId, PasswordCredential credential)
        {
            using (SqlConnection connection = OpenConnection()) using (SqlCommand command = new SqlCommand("UPDATE Users SET PasswordHash=@hash,PasswordSalt=@salt,PasswordIterations=@iterations,IsLegacyHash=0 WHERE UserId=@id", connection))
            {
                AddCredential(command, credential); command.Parameters.Add("@id", SqlDbType.Int).Value = userId;
                connection.Open(); command.ExecuteNonQuery();
            }
        }

        public static List<PasswordCredential> GetPasswordHistory(int userId)
        {
            List<PasswordCredential> result = new List<PasswordCredential>();
            using (SqlConnection connection = OpenConnection()) using (SqlCommand command = new SqlCommand("SELECT TOP 3 PasswordHash,PasswordSalt,PasswordIterations FROM PasswordHistory WHERE UserId=@id ORDER BY ChangedAt DESC", connection))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = userId; connection.Open();
                using (SqlDataReader reader = command.ExecuteReader()) while (reader.Read()) result.Add(new PasswordCredential { Hash = reader.GetString(0), Salt = (byte[])reader[1], Iterations = reader.GetInt32(2) });
            }
            return result;
        }

        public static void ChangePassword(int userId, PasswordCredential credential)
        {
            using (SqlConnection connection = OpenConnection())
            {
                connection.Open(); using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    SaveCurrentPasswordToHistory(connection, transaction, userId);
                    using (SqlCommand command = new SqlCommand("UPDATE Users SET PasswordHash=@hash,PasswordSalt=@salt,PasswordIterations=@iterations,IsLegacyHash=0,MustChangePassword=0,PasswordUpdatedAt=SYSUTCDATETIME() WHERE UserId=@id", connection, transaction))
                    { AddCredential(command, credential); command.Parameters.Add("@id", SqlDbType.Int).Value = userId; command.ExecuteNonQuery(); }
                    PruneHistory(connection, transaction, userId); transaction.Commit();
                }
            }
        }

        public static void CreateUser(string username, string email, string role, PasswordCredential credential)
        {
            using (SqlConnection connection = OpenConnection()) using (SqlCommand command = new SqlCommand("INSERT INTO Users(Username,Email,Role,PasswordHash,PasswordSalt,PasswordIterations,IsLegacyHash,MustChangePassword,IsActive) VALUES(@name,@email,@role,@hash,@salt,@iterations,0,1,1)", connection))
            {
                command.Parameters.Add("@name", SqlDbType.NVarChar, 50).Value = username;
                command.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = email;
                command.Parameters.Add("@role", SqlDbType.NVarChar, 20).Value = role;
                AddCredential(command, credential); connection.Open(); command.ExecuteNonQuery();
            }
        }

        public static DataTable GetUsers()
        {
            using (SqlConnection connection = OpenConnection()) using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT UserId,Username,Email,Role,IsActive,MustChangePassword FROM Users ORDER BY UserId", connection))
            { DataTable table = new DataTable(); adapter.Fill(table); return table; }
        }

        public static int CountAdmins()
        {
            using (SqlConnection connection = OpenConnection()) using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Role=N'Admin' AND IsActive=1", connection))
            { connection.Open(); return (int)command.ExecuteScalar(); }
        }

        public static string GetActiveRole(int userId)
        {
            using (SqlConnection connection = OpenConnection()) using (SqlCommand command = new SqlCommand("SELECT Role FROM Users WHERE UserId=@id AND IsActive=1", connection))
            { command.Parameters.Add("@id", SqlDbType.Int).Value = userId; connection.Open(); object role = command.ExecuteScalar(); return role == null || role == DBNull.Value ? null : Convert.ToString(role); }
        }

        public static void ChangeRole(int targetUserId, string role)
        {
            using (SqlConnection connection = OpenConnection()) using (SqlCommand command = new SqlCommand("UPDATE Users SET Role=@role WHERE UserId=@id", connection))
            { command.Parameters.Add("@role", SqlDbType.NVarChar, 20).Value = role; command.Parameters.Add("@id", SqlDbType.Int).Value = targetUserId; connection.Open(); command.ExecuteNonQuery(); }
        }

        public static int CreateResetToken(string email, byte[] tokenHash, string source)
        {
            using (SqlConnection connection = OpenConnection())
            {
                connection.Open(); using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    int userId;
                    using (SqlCommand find = new SqlCommand("SELECT UserId FROM Users WHERE Email=@email AND IsActive=1", connection, transaction))
                    { find.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = email; object value = find.ExecuteScalar(); if (value == null) { transaction.Commit(); return 0; } userId = (int)value; }
                    using (SqlCommand limit = new SqlCommand("SELECT SUM(CASE WHEN CreatedAt>=DATEADD(SECOND,-60,SYSUTCDATETIME()) THEN 1 ELSE 0 END),SUM(CASE WHEN CreatedAt>=DATEADD(MINUTE,-15,SYSUTCDATETIME()) THEN 1 ELSE 0 END),SUM(CASE WHEN CreatedAt>=DATEADD(DAY,-1,SYSUTCDATETIME()) THEN 1 ELSE 0 END) FROM PasswordResetTokens WHERE UserId=@id AND RequestSource=@source", connection, transaction))
                    {
                        limit.Parameters.Add("@id", SqlDbType.Int).Value = userId; limit.Parameters.Add("@source", SqlDbType.NVarChar, 100).Value = source;
                        using (SqlDataReader reader = limit.ExecuteReader()) { reader.Read(); if ((reader.IsDBNull(0) ? 0 : reader.GetInt32(0)) >= 1 || (reader.IsDBNull(1) ? 0 : reader.GetInt32(1)) >= 3 || (reader.IsDBNull(2) ? 0 : reader.GetInt32(2)) >= 10) { reader.Close(); transaction.Commit(); return 0; } }
                    }
                    using (SqlCommand insert = new SqlCommand("INSERT INTO PasswordResetTokens(UserId,TokenHash,CreatedAt,ExpiresAt,RequestSource) VALUES(@id,@hash,SYSUTCDATETIME(),DATEADD(MINUTE,15,SYSUTCDATETIME()),@source); SELECT CAST(SCOPE_IDENTITY() AS INT)", connection, transaction))
                    { insert.Parameters.Add("@id", SqlDbType.Int).Value = userId; insert.Parameters.Add("@hash", SqlDbType.VarBinary, 32).Value = tokenHash; insert.Parameters.Add("@source", SqlDbType.NVarChar, 100).Value = source; int tokenId = (int)insert.ExecuteScalar(); transaction.Commit(); return tokenId; }
                }
            }
        }

        public static UserRecord FindValidResetToken(string email, byte[] tokenHash, out int tokenId)
        {
            tokenId = 0;
            using (SqlConnection connection = OpenConnection()) using (SqlCommand command = new SqlCommand("SELECT t.TokenId,u.UserId,u.Username,u.Email,u.Role,u.PasswordHash,u.PasswordSalt,u.PasswordIterations,u.IsLegacyHash,u.MustChangePassword,u.IsActive FROM PasswordResetTokens t JOIN Users u ON u.UserId=t.UserId WHERE u.Email=@email AND t.TokenHash=@hash AND t.UsedAt IS NULL AND t.ExpiresAt>SYSUTCDATETIME() AND u.IsActive=1", connection))
            {
                command.Parameters.Add("@email", SqlDbType.NVarChar, 254).Value = email; command.Parameters.Add("@hash", SqlDbType.VarBinary, 32).Value = tokenHash; connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return null;
                    tokenId = reader.GetInt32(0);
                    return new UserRecord { UserId=reader.GetInt32(1), Username=reader.GetString(2), Email=reader.GetString(3), Role=reader.GetString(4), PasswordHash=reader.GetString(5), PasswordSalt=reader.IsDBNull(6)?null:(byte[])reader[6], PasswordIterations=reader.GetInt32(7), IsLegacyHash=reader.GetBoolean(8), MustChangePassword=reader.GetBoolean(9), IsActive=reader.GetBoolean(10) };
                }
            }
        }

        public static bool CompletePasswordReset(int userId, int tokenId, PasswordCredential credential)
        {
            using (SqlConnection connection = OpenConnection())
            {
                connection.Open(); using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    using (SqlCommand valid = new SqlCommand("SELECT COUNT(*) FROM PasswordResetTokens WITH (UPDLOCK,HOLDLOCK) WHERE TokenId=@token AND UserId=@user AND UsedAt IS NULL AND ExpiresAt>SYSUTCDATETIME()", connection, transaction))
                    { valid.Parameters.Add("@token", SqlDbType.Int).Value = tokenId; valid.Parameters.Add("@user", SqlDbType.Int).Value = userId; if ((int)valid.ExecuteScalar() != 1) { transaction.Rollback(); return false; } }
                    SaveCurrentPasswordToHistory(connection, transaction, userId);
                    using (SqlCommand update = new SqlCommand("UPDATE Users SET PasswordHash=@hash,PasswordSalt=@salt,PasswordIterations=@iterations,IsLegacyHash=0,MustChangePassword=0,PasswordUpdatedAt=SYSUTCDATETIME() WHERE UserId=@id", connection, transaction))
                    { AddCredential(update, credential); update.Parameters.Add("@id", SqlDbType.Int).Value = userId; update.ExecuteNonQuery(); }
                    using (SqlCommand used = new SqlCommand("UPDATE PasswordResetTokens SET UsedAt=SYSUTCDATETIME() WHERE UserId=@id AND UsedAt IS NULL", connection, transaction)) { used.Parameters.Add("@id", SqlDbType.Int).Value = userId; used.ExecuteNonQuery(); }
                    PruneHistory(connection, transaction, userId); transaction.Commit(); return true;
                }
            }
        }

        public static DataTable GetMembers()
        {
            using (SqlConnection connection = OpenConnection())
            using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT MemberId, FullName, Phone, MembershipType, StartDate FROM Members ORDER BY MemberId DESC", connection))
            { DataTable table = new DataTable(); adapter.Fill(table); return table; }
        }
        public static void SaveMember(int? id, string name, string phone, string type, DateTime startDate)
        {
            string sql = id.HasValue ? "UPDATE Members SET FullName=@name, Phone=@phone, MembershipType=@type, StartDate=@date WHERE MemberId=@id" : "INSERT INTO Members(FullName, Phone, MembershipType, StartDate) VALUES(@name,@phone,@type,@date)";
            using (SqlConnection connection = OpenConnection()) using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = name;
                command.Parameters.Add("@phone", SqlDbType.NVarChar, 20).Value = phone;
                command.Parameters.Add("@type", SqlDbType.NVarChar, 30).Value = type;
                command.Parameters.Add("@date", SqlDbType.Date).Value = startDate;
                if (id.HasValue) command.Parameters.Add("@id", SqlDbType.Int).Value = id.Value;
                connection.Open(); command.ExecuteNonQuery();
            }
        }
        public static void DeleteMember(int id)
        {
            using (SqlConnection connection = OpenConnection()) using (SqlCommand command = new SqlCommand("DELETE FROM Members WHERE MemberId=@id", connection))
            { command.Parameters.Add("@id", SqlDbType.Int).Value = id; connection.Open(); command.ExecuteNonQuery(); }
        }

        private static void AddCredential(SqlCommand command, PasswordCredential credential)
        {
            command.Parameters.Add("@hash", SqlDbType.VarChar, 128).Value = credential.Hash;
            command.Parameters.Add("@salt", SqlDbType.VarBinary, 32).Value = credential.Salt;
            command.Parameters.Add("@iterations", SqlDbType.Int).Value = credential.Iterations;
        }

        private static void SaveCurrentPasswordToHistory(SqlConnection connection, SqlTransaction transaction, int userId)
        {
            using (SqlCommand command = new SqlCommand("INSERT INTO PasswordHistory(UserId,PasswordHash,PasswordSalt,PasswordIterations,ChangedAt) SELECT UserId,PasswordHash,PasswordSalt,PasswordIterations,SYSUTCDATETIME() FROM Users WHERE UserId=@id AND IsLegacyHash=0", connection, transaction))
            { command.Parameters.Add("@id", SqlDbType.Int).Value = userId; command.ExecuteNonQuery(); }
        }

        private static void PruneHistory(SqlConnection connection, SqlTransaction transaction, int userId)
        {
            using (SqlCommand command = new SqlCommand("DELETE FROM PasswordHistory WHERE UserId=@id AND HistoryId NOT IN (SELECT TOP 3 HistoryId FROM PasswordHistory WHERE UserId=@id ORDER BY ChangedAt DESC)", connection, transaction))
            { command.Parameters.Add("@id", SqlDbType.Int).Value = userId; command.ExecuteNonQuery(); }
        }
    }
}
