/*
  GymTracking kurulum / yükseltme betiği.
  Var olan Members ve admin hesabını silmez. İlk kurulum hesapları:
  admin / admin123 ve user / user123. İlk girişte parola değiştirilir.
  SMTP ile parola sıfırlama için demo e-posta adreslerini gerçek adreslerle güncelleyin.
*/
IF DB_ID(N'GymTracking') IS NULL CREATE DATABASE GymTracking;
GO
USE GymTracking;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
        PasswordHash VARCHAR(128) NOT NULL
    );
END;

IF COL_LENGTH(N'dbo.Users',N'Email') IS NULL ALTER TABLE dbo.Users ADD Email NVARCHAR(254) NULL;
IF COL_LENGTH(N'dbo.Users',N'Role') IS NULL ALTER TABLE dbo.Users ADD Role NVARCHAR(20) NOT NULL CONSTRAINT DF_Users_Role DEFAULT N'User' WITH VALUES;
IF COL_LENGTH(N'dbo.Users',N'PasswordSalt') IS NULL ALTER TABLE dbo.Users ADD PasswordSalt VARBINARY(32) NULL;
IF COL_LENGTH(N'dbo.Users',N'PasswordIterations') IS NULL ALTER TABLE dbo.Users ADD PasswordIterations INT NOT NULL CONSTRAINT DF_Users_PasswordIterations DEFAULT 0 WITH VALUES;
IF COL_LENGTH(N'dbo.Users',N'IsLegacyHash') IS NULL ALTER TABLE dbo.Users ADD IsLegacyHash BIT NOT NULL CONSTRAINT DF_Users_IsLegacyHash DEFAULT 1 WITH VALUES;
IF COL_LENGTH(N'dbo.Users',N'MustChangePassword') IS NULL ALTER TABLE dbo.Users ADD MustChangePassword BIT NOT NULL CONSTRAINT DF_Users_MustChangePassword DEFAULT 1 WITH VALUES;
IF COL_LENGTH(N'dbo.Users',N'IsActive') IS NULL ALTER TABLE dbo.Users ADD IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1 WITH VALUES;
IF COL_LENGTH(N'dbo.Users',N'PasswordUpdatedAt') IS NULL ALTER TABLE dbo.Users ADD PasswordUpdatedAt DATETIME2 NULL;
GO
ALTER TABLE dbo.Users ALTER COLUMN PasswordHash VARCHAR(128) NOT NULL;
UPDATE dbo.Users SET Role=N'Admin' WHERE Username=N'admin';
UPDATE dbo.Users SET Email=N'admin@example.com' WHERE Username=N'admin' AND Email IS NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Users') AND name=N'UX_Users_Email')
    CREATE UNIQUE INDEX UX_Users_Email ON dbo.Users(Email) WHERE Email IS NOT NULL;

IF OBJECT_ID(N'dbo.Members', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Members
    (
        MemberId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Members PRIMARY KEY,
        FullName NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(20) NOT NULL,
        MembershipType NVARCHAR(30) NOT NULL,
        StartDate DATE NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.PasswordHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PasswordHistory
    (
        HistoryId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PasswordHistory PRIMARY KEY,
        UserId INT NOT NULL CONSTRAINT FK_PasswordHistory_Users REFERENCES dbo.Users(UserId),
        PasswordHash VARCHAR(128) NOT NULL,
        PasswordSalt VARBINARY(32) NOT NULL,
        PasswordIterations INT NOT NULL,
        ChangedAt DATETIME2 NOT NULL
    );
    CREATE INDEX IX_PasswordHistory_User_ChangedAt ON dbo.PasswordHistory(UserId,ChangedAt DESC);
END;

IF OBJECT_ID(N'dbo.PasswordResetTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PasswordResetTokens
    (
        TokenId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PasswordResetTokens PRIMARY KEY,
        UserId INT NOT NULL CONSTRAINT FK_PasswordResetTokens_Users REFERENCES dbo.Users(UserId),
        TokenHash VARBINARY(32) NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        ExpiresAt DATETIME2 NOT NULL,
        UsedAt DATETIME2 NULL,
        RequestSource NVARCHAR(100) NOT NULL
    );
    CREATE INDEX IX_PasswordResetTokens_User_Created ON dbo.PasswordResetTokens(UserId,RequestSource,CreatedAt DESC);
    CREATE INDEX IX_PasswordResetTokens_TokenHash ON dbo.PasswordResetTokens(TokenHash);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username=N'admin')
    INSERT INTO dbo.Users(Username,Email,Role,PasswordHash,IsLegacyHash,MustChangePassword,IsActive)
    VALUES(N'admin',N'admin@example.com',N'Admin',LOWER(CONVERT(VARCHAR(64),HASHBYTES('SHA2_256','admin123'),2)),1,1,1);
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username=N'user')
    INSERT INTO dbo.Users(Username,Email,Role,PasswordHash,IsLegacyHash,MustChangePassword,IsActive)
    VALUES(N'user',N'user@example.com',N'User',LOWER(CONVERT(VARCHAR(64),HASHBYTES('SHA2_256','user123'),2)),1,1,1);

IF NOT EXISTS (SELECT 1 FROM dbo.Members)
    INSERT INTO dbo.Members(FullName,Phone,MembershipType,StartDate)
    VALUES(N'Örnek Üye','05550000000',N'Aylık',CONVERT(date,GETDATE()));
GO
