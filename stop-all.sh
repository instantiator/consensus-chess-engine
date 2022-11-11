#!/bin/bash

set -e
set -o pipefail

# stop the containers
docker compose -p consensus-chess \
  --env-file environments/database.env \
  down
