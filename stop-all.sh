#!/bin/bash

set -e
set -o pipefail

# stop the int containers
docker compose -p consensus-chess-int \
  -f compose.yaml -f compose.prod.yaml \
  --env-file environments/int-database.env \
  down

# stop the prod containers
docker compose -p consensus-chess-prod \
  -f compose.yaml -f compose.prod.yaml \
  --env-file environments/prod-database.env \
  down
