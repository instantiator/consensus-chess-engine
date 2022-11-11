#!/bin/bash

set -e
set -o pipefail

# remove any residual containers and the attached database volume before run
docker compose -p consensus-chess-int-tests \
  -f compose.yaml -f compose.int.yaml \
  --env-file environments/database.env \
  down -v

# build containers
docker compose -p consensus-chess-int-tests \
  -f compose.yaml -f compose.int.yaml \
  --env-file environments/database.env \
  build integration-tests

# start the database container only
docker compose -p consensus-chess-int-tests \
  -f compose.yaml -f compose.int.yaml \
  --env-file environments/database.env \
  up --exit-code-from integration-tests \
  integration-tests

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

# remove all containers and the attached database volume after run
docker compose -p consensus-chess-int-tests -f compose.yaml -f compose.int.yaml --env-file environments/database.env down -v

exit $TEST_CODE
