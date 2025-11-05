@echo off
docker compose -f sql-server-dck.yml down
echo Stopped SQL Server
docker compose -f rabbit-mq-dck.yml down
echo Stopped RabbitMQ
docker compose -f warehouse-api.yml down
echo Stopped WareHouse.Api
pause
