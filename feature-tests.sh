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
docker compose -p consensus-chess-feature \
  -f compose.feature-tests.yaml \
  down -v

# start all containers required for the test, exit when it finishes
docker compose -p consensus-chess-feature \
  -f compose.feature-tests.yaml \
  up --build feature-tests \
  --exit-code-from feature-tests \
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
docker compose -p consensus-chess-feature \
  -f compose.feature-tests.yaml \
  stop

exit $TEST_CODE
