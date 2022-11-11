#!/bin/bash

set -e
set -o pipefail

# start the containers
docker compose -p consensus-chess \
  --env-file environments/database.env \
  up --exit-code-from database \
  database
