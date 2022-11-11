#!/bin/bash

set -e
set -o pipefail

docker compose -p consensus-chess down -v
docker compose -p consensus-chess-int-tests down -v
docker system prune -f
docker image prune -f
docker volume prune -f
