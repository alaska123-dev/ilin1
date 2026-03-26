# Runbook стенда is-stack

## 1. Проверка статуса сервисов
```bash
cd /home/deployer/deploy/is-stack
docker compose ps
bash
# Все логи
docker compose logs

# Логи приложения
docker compose logs app

# Логи базы данных
docker compose logs mssql

# Последние 50 строк
docker compose logs --tail 50 app
# Проверка работоспособности
curl -k https://192.168.0.111/health

# Версия приложения
curl -k https://192.168.0.111/version

# Подключение к базе данных
curl -k https://192.168.0.111/db/ping
cd /home/deployer/deploy/is-stack

# 1. Изменить тег образа
nano docker-compose.yml
# image: ghcr.io/alaskal23-dev/ilin1:новый_тег

# 2. Загрузить новый образ
docker compose pull app

# 3. Перезапустить приложение
docker compose up -d app

# 4. Проверить версию
curl -k https://192.168.0.111/version
cd /home/deployer/deploy/is-stack

# 1. Вернуть старый тег
nano docker-compose.yml
# image: ghcr.io/alaskal23-dev/ilin1:старый_тег

# 2. Загрузить образ
docker compose pull app

# 3. Перезапустить
docker compose up -d app

# 4. Проверить работоспособность
curl -k https://192.168.0.111/health
# Создать каталог для бэкапов
docker exec -it mssql mkdir -p /var/opt/mssql/backup

# Выполнить бэкап
docker exec -it mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Passw0rd" -C -Q "BACKUP DATABASE IsLabDb TO DISK = N'/var/opt/mssql/backup/IsLabDb_full.bak' WITH INIT, COMPRESSION;"

# Проверить бэкап
docker exec -it mssql ls -la /var/opt/mssql/backup/
# Восстановить в тестовую базу
docker exec -it mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Passw0rd" -C -Q "RESTORE DATABASE IsLabDb_RestoreTest FROM DISK = N'/var/opt/mssql/backup/IsLabDb_full.bak' WITH MOVE 'IsLabDb' TO '/var/opt/mssql/data/IsLabDb_RestoreTest.mdf', MOVE 'IsLabDb_log' TO '/var/opt/mssql/data/IsLabDb_RestoreTest_log.ldf', REPLACE;"

# Проверить данные
docker exec -it mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Passw0rd" -C -d IsLabDb_RestoreTest -Q "SELECT COUNT(*) FROM Notes"

# Удалить тестовую базу
docker exec -it mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Passw0rd" -C -Q "DROP DATABASE IsLabDb_RestoreTest;"
# Оставить последние 5 бэкапов
docker exec -it mssql bash -c "cd /var/opt/mssql/backup && ls -t *.bak | tail -n +6 | xargs -r rm"

# Удалить бэкапы старше 7 дней
docker exec -it mssql find /var/opt/mssql/backup -name "*.bak" -mtime +7 -delete
