using System;
using ConsensusChessShared.Database;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Migrations;

namespace ConsensusChessEngine.Service
{
    public class GameManager
    {
        private ILogger log;

        public GameManager(ILogger log)
        {
            this.log = log;
        }
        public Game CreateSimpleMoveLockGame(IEnumerable<string> shortcodes)
        {
            var game = new Game(shortcodes, shortcodes, SideRules.MoveLock);
            return game;
        }
    }
}

