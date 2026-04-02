$ErrorActionPreference = "Stop"
$CurrentDir = (Get-Item .).FullName
docker run --rm -v "${CurrentDir}/backend:/src" -w /src mcr.microsoft.com/dotnet/sdk:8.0 bash -c "export PATH=`"`$PATH:/root/.dotnet/tools`" && dotnet tool install --global dotnet-ef && dotnet ef migrations add AddAdvancedQuotingAndContracts --project Crm.Infrastructure --startup-project Crm.Api"
