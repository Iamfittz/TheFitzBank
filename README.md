# TheFitzBank

A custom ASP.NET CORE applications with 1k pre-loaded accounts for testing purpose and future development.

# Features:
- Account management
- Transaction operations
- Swagger UI
- No setup required (fully containerized).
- Covered by autotest

# Quick Start
Install Docker Desktop

# Run App (Recomended)
## Start SQL Server
```
docker run -d --name fitzbank-sql \
  -e SA_PASSWORD="YourStrong!Pass123" \
  -e ACCEPT_EULA=Y \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```
Wait around 10 sec for Db initialization.
# Start Banking API
```
docker run -d --name fitzbank-api \
  -p 8080:8080 \
  your-dockerhub-username/thefitzbank-api
```
**Swagger UI**: http://localhost:8080

## Run App Locally
```
git clone https://github.com/Iamfittz/TheFitzBank.git
```
- cd TheFitzBank/TheFitzBank
- dotnet restore
- dotnet ef database update
- dotnet run

## Start SQL Server
## API Endpoints
Accounts

### Account Management
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/accounts` | **List all accounts** *(1000 pre-loaded)* |
| `GET` | `/api/accounts/{accountNumber}` | **Get specific account** |
| `POST` | `/api/accounts` | **Create new account** |
### Transaction Operations
| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/transactions/deposit` | **Deposit money** |
| `POST` | `/api/transactions/withdraw` | **Withdraw money** |
| `POST` | `/api/transactions/transfer` | **Transfer between accounts** |




# Technology Stack

- ASP.NET Core 8
- Entity Framework Core 9
- SQL Server 2022
- Docker
