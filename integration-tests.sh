#!/bin/bash

set -e
set -o pipefail

# remove any residual containers and the attached database volume before run
docker compose -p consensus-chess-int-tests \
  -f compose.yaml -f compose.int.yaml \
  --env-file environments/database.env \
  down -v

# build containers
docker compose -p consensus-chess-int-tests \
  -f compose.yaml -f compose.int.yaml \
  --env-file environments/database.env \
  build integration-tests

# start the database container only
docker compose -p consensus-chess-int-tests \
  -f compose.yaml -f compose.int.yaml \
  --env-file environments/database.env \
  up --exit-code-from integration-tests \
  integration-tests

# remove all containers and the attached database volume after run
# docker compose -p consensus-chess-int-tests -f compose.yaml -f compose.int.yaml --env-file environments/database.env down -v
