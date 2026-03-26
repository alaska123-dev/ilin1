#!/bin/bash
# Скрипт очистки старых бэкапов SQL Server
# Правило: хранить последние 5 бэкапов

echo "=== Backup cleanup started at $(date) ==="

# Оставить последние 5 бэкапов
docker exec -it mssql bash -c "cd /var/opt/mssql/backup && ls -t *.bak | tail -n +6 | xargs -r rm"

# Удалить бэкапы старше 7 дней
docker exec -it mssql find /var/opt/mssql/backup -name "*.bak" -mtime +7 -delete

echo "Backup cleanup completed at $(date)"
