#!/bin/bash

set -e
set -o pipefail

docker compose build
docker compose up
