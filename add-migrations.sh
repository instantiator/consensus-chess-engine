#!/bin/bash

set -e
set -o pipefail

usage() {
  cat << EOF
Adds migrations for the core database in the ConsensusChessShared project, and
the feature tests database in the ConsensusChessFeatureTests project.

Options:
    -m <name>  --migration-name <name>  Name for the migration
    -h         --help                   Prints this help message and exits
EOF
}

# parameters
while [ -n "$1" ]; do
  case $1 in
  -m | --migration-name)
    shift
    MIGRATION=$1
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

if [ -z "$MIGRATION" ]; then
  echo "Please set the migration name with -m/--migration-name"
  usage
  exit 1
fi

dotnet ef migrations add $MIGRATION --project ConsensusChessShared/ConsensusChessShared.csproj --context ConsensusChessPostgresContext
dotnet ef migrations add $MIGRATION --project ConsensusChessFeatureTests/ConsensusChessFeatureTests.csproj
