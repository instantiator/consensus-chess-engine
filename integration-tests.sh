#!/bin/bash

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

# start all containers required for the test, exit when it finishes
docker compose -p consensus-chess-int-tests \
  -f compose.yaml -f compose.int.yaml \
  --env-file environments/database.env \
  up --exit-code-from integration-tests \
  --abort-on-container-exit \
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
docker compose -p consensus-chess-int-tests -f compose.yaml -f compose.int.yaml --env-file environments/database.env stop

exit $TEST_CODE
