#!/bin/bash
connection_string=$ConnectionStrings__DefaultSQLConnection

# Parse the connection string to extract the relevant parameters
DB_HOST=$(echo $connection_string | grep -o 'Host=[^;]*' | cut -d= -f2)
DB_NAME=$(echo $connection_string | grep -o 'Database=[^;]*' | cut -d= -f2)
DB_USER=$(echo $connection_string | grep -o 'Username=[^;]*' | cut -d= -f2)
DB_PORT=$(echo $connection_string | grep -o 'Port=[^;]*' | cut -d= -f2)

# Number of attempts
ATTEMPTS=10

# Delay in seconds between attempts
DELAY=5

for ((i=1; i<=$ATTEMPTS; i++)); do
  # Try to connect to the database
  pg_isready -h $DB_HOST -p $DB_PORT -d $DB_NAME -U $DB_USER 
  
  # Check the exit code of psql
  if [ $? -eq 0 ]; then
    echo "Database is available."
    exit 0
  else
    echo "Attempt $i: Database is not available. Retrying in $DELAY seconds..."
    sleep $DELAY
  fi
done

echo "Database is still not available after $ATTEMPTS attempts."
exit 1