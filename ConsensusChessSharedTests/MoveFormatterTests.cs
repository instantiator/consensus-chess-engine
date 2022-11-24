using System;
using Chess;
using ConsensusChessShared.Content;
using ConsensusChessShared.DTO;

namespace ConsensusChessSharedTests
{
	[TestClass]
	public class MoveFormatterTests
	{

		[TestMethod]
		public void CCFRegexTests()
		{
            Assert.IsTrue(MoveFormatter.CCF.IsMatch("e2-e4"));
            Assert.IsTrue(MoveFormatter.CCF.IsMatch("e2,e4"));
            Assert.IsTrue(MoveFormatter.CCF.IsMatch("e2 -e4"));
            Assert.IsTrue(MoveFormatter.CCF.IsMatch("e2 ,e4"));
            Assert.IsTrue(MoveFormatter.CCF.IsMatch("e2- e4"));
            Assert.IsTrue(MoveFormatter.CCF.IsMatch("e2, e4"));
            Assert.IsTrue(MoveFormatter.CCF.IsMatch("e2 - e4"));
            Assert.IsTrue(MoveFormatter.CCF.IsMatch("e2 , e4"));

            Assert.IsFalse(MoveFormatter.CCF.IsMatch("i2-e4"));
            Assert.IsFalse(MoveFormatter.CCF.IsMatch("e2+e4"));
            Assert.IsFalse(MoveFormatter.CCF.IsMatch("e0-e4"));
            Assert.IsFalse(MoveFormatter.CCF.IsMatch("e2-e9"));
            Assert.IsFalse(MoveFormatter.CCF.IsMatch("12-e4"));
            Assert.IsFalse(MoveFormatter.CCF.IsMatch("horsey to king 4"));
        }

        [TestMethod]
        public void GetChessMoveFromCCFTests()
        {
            Assert.IsNotNull(MoveFormatter.GetChessMoveFromCCF("e2 - e4"));
            Assert.IsNull(MoveFormatter.GetChessMoveFromCCF("horsey to king 4"));
        }

        [TestMethod]
        public void GetChessMoveFromSANTests()
        {
            var board = new Board();
            Assert.IsNotNull(MoveFormatter.GetChessMoveFromSAN(board, "e4"));

            Assert.ThrowsException<ChessArgumentException>(() =>
            {
                MoveFormatter.GetChessMoveFromSAN(board, "horsey to king 4");
            });

            Assert.ThrowsException<ChessSanNotFoundException>(() =>
            {
                MoveFormatter.GetChessMoveFromSAN(board, "e2"); // not a valid move
            });
        }

    }
}

