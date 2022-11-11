#!/bin/bash

set -e
set -o pipefail

docker compose down && docker system prune && docker image prune && docker volume prune
