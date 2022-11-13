#!/bin/bash

# remove any residual containers and the attached database volume before run
docker compose -p consensus-chess-int \
  -f compose.yaml -f compose.int.yaml \
  --env-file environments/int-database.env \
  down -v

# build containers
docker compose -p consensus-chess-int \
  -f compose.yaml -f compose.int.yaml \
  --env-file environments/int-database.env \
  build integration-tests

# start all containers required for the test, exit when it finishes
docker compose -p consensus-chess-int \
  -f compose.yaml -f compose.int.yaml \
  --env-file environments/int-database.env \
  up integration-tests \
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
  --env-file environments/int-database.env \
  stop

exit $TEST_CODE
