#!/bin/bash

usage() {
  cat << EOF
Starts all services for the specified environment.

Options:
    -f <filter> --filter <filter>     A filter expression to pick out which test to run
    -h          --help                Prints this help message and exits
EOF
}

# defaults
FILTER=\"\"

# parameters
while [ -n "$1" ]; do
  case $1 in
  -f | --filter)
    shift
    FILTER=$1
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

export TEST_FILTER=${FILTER}

# remove any residual containers and the attached database volume before run
docker compose -p consensus-chess-int \
  -f compose.yaml -f compose.int.yaml -f compose.int-tests.yaml \
  --env-file environments/db-int.env \
  down -v

# start all containers required for the test, exit when it finishes
docker compose -p consensus-chess-int \
  -f compose.yaml -f compose.int.yaml -f compose.int-tests.yaml \
  --env-file environments/db-int.env \
  up --build integration-tests \
  --exit-code-from integration-tests \
  --abort-on-container-exit

TEST_CODE=$?

if test $TEST_CODE -eq 0
then
    GREEN='\033[0;32m'
	NC='\033[0m'
	echo -e "${GREEN}Pass!${NC}"
	echo -e "${GREEN}"
	cat << EOF

 ███████████                           
░░███░░░░░███                          
 ░███    ░███  ██████    █████   █████ 
 ░██████████  ░░░░░███  ███░░   ███░░  
 ░███░░░░░░    ███████ ░░█████ ░░█████ 
 ░███         ███░░███  ░░░░███ ░░░░███
 █████       ░░████████ ██████  ██████ 
░░░░░         ░░░░░░░░ ░░░░░░  ░░░░░░

EOF
	echo -e "${NC}"

else
    RED='\033[0;31m'
	NC='\033[0m'
	echo -e "${RED}Fail!${NC}"
	echo -e "${RED}"
	cat << EOF

  █████▒▄▄▄       ██▓ ██▓    
▓██   ▒▒████▄    ▓██▒▓██▒    
▒████ ░▒██  ▀█▄  ▒██▒▒██░    
░▓█▒  ░░██▄▄▄▄██ ░██░▒██░    
░▒█░    ▓█   ▓██▒░██░░██████▒
 ▒ ░    ▒▒   ▓▒█░░▓  ░ ▒░▓  ░
 ░       ▒   ▒▒ ░ ▒ ░░ ░ ▒  ░
 ░ ░     ░   ▒    ▒ ░  ░ ░   
             ░  ░ ░      ░  ░

EOF
	echo -e "${NC}"
fi

# stop all containers (leave the db intact) after the run
docker compose -p consensus-chess-int \
  -f compose.yaml -f compose.int.yaml \
  --env-file environments/db-int.env \
  stop

exit $TEST_CODE
