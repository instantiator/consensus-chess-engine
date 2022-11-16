# Architecture

The application is composed of a number of microservices:

* Database
* Engine
* Nodes

![Consensus diagram showing arrangement of services](images/consensus-diagram.png "Consensus diagram showing arrangement of services")

## Database

This is a simple postgres instance, obtained by docker image.

All microservices (the engine, and nodes) share this database - and use it to store games, boards, moves, votes, etc.

## Engine

There is a single engine.

### Responsibilities

* Managing games, boards, moves
* Tallying votes, advancing games
* Ending games

## Nodes

There is a node per social network (ie. mastodon server) that can participate in games.

### Responsibilities

* Posting board updates
* Collecting votes
* Validating votes
* Player communication

