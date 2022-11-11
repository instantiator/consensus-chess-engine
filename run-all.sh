#!/bin/bash

set -e
set -o pipefail

# build containers
docker compose -p consensus-chess \
  --env-file environments/database.env \
  build

# start the containers
docker compose -p consensus-chess \
  --env-file environments/database.env \
  up
