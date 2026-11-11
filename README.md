# Gym Membership Tracker

**Spor Salonu Üye Takip Sistemi** is a Windows desktop application for managing gym members and their memberships. The interface is in Turkish.

## Features

- Add, edit, and remove member records.
- Track monthly, quarterly, and yearly memberships, including expiry status.
- Use separate Admin and User roles.
- Store passwords with PBKDF2 and per-user salts; require a password change on first login.
- Reset passwords by email when SMTP settings are configured.
- Close inactive sessions automatically.

## Technology

- C# and Windows Forms
- .NET Framework 4.8
- Microsoft SQL Server

## Setup

1. Install Visual Studio with the **.NET desktop development** workload and .NET Framework 4.8 targeting pack.
2. Install SQL Server and SQL Server Management Studio (SSMS).
3. In SSMS, open and execute [SQL/Kurulum.sql](SQL/Kurulum.sql). It creates or upgrades the `GymTracking` database and keeps existing member records.
4. If your SQL Server instance is not `localhost`, update the connection string in `Database.cs`.
5. Open `SporSalonuTakip.sln` in Visual Studio and run the project.

The setup script creates demo accounts if they do not already exist:

| Username | Initial password | Role |
| --- | --- | --- |
| `admin` | `admin123` | Admin |
| `user` | `user123` | User |

Both demo accounts require a password change on first login. Change the initial passwords before using the application with real member data.

## Email password reset

The SMTP settings in `App.config` are empty by default. Add your provider's SMTP details to your local configuration if you want to use password reset by email. **Never commit real SMTP passwords or other secrets to a public repository.**

## Project layout

- `LoginForm.cs`, `MainForm.cs`, and the other `*Form.cs` files contain the Windows Forms screens.
- `BLL/` contains authentication, membership, user, and session logic.
- `Entities/` contains application data models.
- `Helpers/` contains password, email, and temporary-password helpers.
- `SQL/Kurulum.sql` creates or upgrades the database and adds demo records when needed.

## Türkçe

Proje açıklaması ve ayrıntılı kurulum yönergeleri için [KURULUM.md](KURULUM.md) dosyasına bakabilirsin.
