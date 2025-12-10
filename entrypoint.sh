#!/bin/bash
set -e

# Wait for postgres to be ready
DB_HOST=${POSTGRES_HOST:-postgres}
DB_USER=${POSTGRES_USER:-postgres}
DB_PORT=${POSTGRES_PORT:-5432}

until pg_isready -h "$DB_HOST" -U "$DB_USER" -p "$DB_PORT"; do
  echo "Waiting for postgres..."
  sleep 2
done

echo "Postgres is ready, waiting a bit more..."
sleep 5

echo "Starting app..."

# Start the app
exec dotnet JackLimited.Api.dll