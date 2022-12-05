#!/bin/bash

set -e
set -o pipefail

usage() {
  cat << EOF
Stops all services for the specified environment.

Options:
    -e <env>   --environment <env>    The environment to stop. Choices are: int, prod
    -h         --help                 Prints this help message and exits
EOF
}

# defaults

# parameters
while [ -n "$1" ]; do
  case $1 in
  -e | --environment)
    shift
    ENVIRO=$1
    ;;
  -h | --help)
    usage
    exit 0
    ;;
  *)
    echo -e "Unknown option $1...\n"
    usage
    exit 1
    ;;
  esac
  shift
done

case $ENVIRO in
    int|prod)
      ENVIRO_VALID=true
      ;;
    *)
      ENVIRO_VALID=false
      echo "Please set environment with -e/--environment to one of: int, prod"
      usage
      exit 1
      ;;
esac

# stop the containers
docker compose -p consensus-chess-$ENVIRO \
  -f compose.yaml -f compose.$ENVIRO.yaml \
  --env-file environments/db-$ENVIRO.env \
  down
