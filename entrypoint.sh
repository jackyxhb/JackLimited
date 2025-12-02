#!/bin/bash
set -e

# Wait for postgres to be ready
until pg_isready -h postgres -U postgres; do
  echo "Waiting for postgres..."
  sleep 2
done

echo "Postgres is ready, waiting a bit more..."
sleep 5

echo "Starting app..."

# Start the app
exec dotnet JackLimited.Api.dll