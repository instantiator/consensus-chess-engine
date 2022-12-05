using System;
using ConsensusChessShared.Helpers;

namespace ConsensusChessSharedTests
{
	[TestClass]
	public class StringTests
	{
		[TestMethod]
		public void SplitOutsideQuotesTests()
		{
			var data = "one two \"three four five\" six seven";

			var parts = data.SplitOutsideQuotes(' ', true, true, true);

			Assert.AreEqual(5, parts.Count());
			Assert.AreEqual("one", parts[0]);
            Assert.AreEqual("two", parts[1]);
            Assert.AreEqual("\"three four five\"", parts[2]);
            Assert.AreEqual("six", parts[3]);
            Assert.AreEqual("seven", parts[4]);
        }

	}
}

