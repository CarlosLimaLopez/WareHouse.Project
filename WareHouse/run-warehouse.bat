@echo off
docker compose -f docker-compose.yml up -d

timeout /t 60

dotnet ef database update --project WareHouse.Data --startup-project WareHouse.API
dotnet ef database update --project WareHouse.Data --startup-project WareHouse.API.Query
