@echo off
docker compose -f sql-server-dck.yml up -d
echo Started SQL Server
docker compose -f rabbit-mq-dck.yml up -d
echo Started RabbitMQ
docker compose -f warehouse-api.yml up -d
echo Started WareHouse.Api
pause
