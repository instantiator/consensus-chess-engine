#!/bin/bash

set -e
set -o pipefail

usage() {
  cat << EOF
Erases all containers, images, and volumes for the specified environment.

Options:
    -e <env>   --environment <env>    The environment to erase. Choices are: int, prod
    -y / -f    --force                Set to perform the erasure without confirmation
    -h         --help                 Prints this help message and exits
EOF
}

# defaults
SURE=false

# parameters
while [ -n "$1" ]; do
  case $1 in
  -e | --environment)
    shift
    ENVIRO=$1
    ;;
  -y | -f | --force)
    SURE=true
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

if [ "$SURE" = false ]; then
    read -p "Are you sure you want to erase the $ENVIRO environment? " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
      SURE=true
    else
      echo "Aborted."
    fi
fi

if [ "$SURE" = true ]; then
  docker compose -p consensus-chess-$ENVIRO down -v
  docker system prune -f
  docker image prune -f
  docker volume prune -f
fi
