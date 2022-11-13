#!/bin/bash

set -e
set -o pipefail

usage() {
  cat << EOF
Starts the database only.

Options:
    -e <env>   --environment <env>    The database environment. Choices are: int, prod
    -d         --detach               Detach and run in the background, sets -d for docker compose
    -h         --help                 Prints this help message and exits
EOF
}

# defaults
DETACH=false

# parameters
while [ -n "$1" ]; do
  case $1 in
  -e | --environment)
    shift
    ENVIRO=$1
    ;;
  -d | --detach)
    DETACH=true
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

ADDITIONAL_OPTIONS=
if $DETACH; then
  ADDITIONAL_OPTIONS="${ADDITIONAL_OPTIONS} --detach"
else
  ADDITIONAL_OPTIONS="${ADDITIONAL_OPTIONS} --exit-code-from database"
fi

# start the containers
docker compose -p consensus-chess-$ENVIRO \
  -f compose.yaml -f compose.$ENVIRO.yaml \
  --env-file environments/$ENVIRO-database.env \
  up --build $ADDITIONAL_OPTIONS \
  database
