SqlAzureBackup
==============

Simple console app that creates .bacpac backup of Sql Azure database it can be used as a scheduled task to create periodic backups, or triggered from deployment scripts before the deployment.

Sample Usage:

		SqlBackup.exe -u DB_USER_NAME -p DB_PASS -d EastUS -n DB_NAME -s SQL_SERVER_NAME -a STORAGE_ACCOUNT -c bacpac -f testbackup -t true -k STORAGE_KEY
		
