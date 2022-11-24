using System;
using ConsensusChessShared.DTO;
using ConsensusChessSharedTests.Data;

namespace ConsensusChessSharedTests
{
	[TestClass]
	public class SocialUsernameTests
	{
		[TestMethod]
		public void SocialUsernameEqualityAndCaseTests()
		{
            Assert.AreEqual(
                $"unittest@{SampleDataGenerator.FakeNetwork.NetworkServer}",
                SocialUsername.From("unittest", "Unit test", SampleDataGenerator.FakeNetwork).Full);

            Assert.IsTrue(
				SocialUsername.From("unittest", "Unit test", SampleDataGenerator.FakeNetwork)
					.Equals($"unittest@{SampleDataGenerator.FakeNetwork.NetworkServer}"));

            Assert.AreEqual(
                $"unittest@{SampleDataGenerator.FakeNetwork.NetworkServer}",
                SocialUsername.From("UnitTest", "Unit test", SampleDataGenerator.FakeNetwork).Full);

            Assert.IsTrue(
                SocialUsername.From("UnitTest", "Unit test", SampleDataGenerator.FakeNetwork)
                    .Equals($"unittest@{SampleDataGenerator.FakeNetwork.NetworkServer}"));
        }

        [TestMethod]
        public void SocialUsernameFormatTests()
        {
            var username1a = SocialUsername.From("unittest1@fake.mastodon.social", "Unit test", SampleDataGenerator.FakeNetwork);
            var username1b = SocialUsername.From("unittest1@fake.mastodon.social", "Unit test", SampleDataGenerator.FakeNetwork);
            var username2 = SocialUsername.From("unittest2@fake.mastodon.social", "Unit test", SampleDataGenerator.FakeNetwork);


            Assert.AreEqual("unittest1@fake.mastodon.social", username1a.Full);
            Assert.AreEqual("unittest1@fake.mastodon.social", username1b.Full);
            Assert.AreEqual("unittest2@fake.mastodon.social", username2.Full);

            Assert.IsFalse(username1a.Equals(username2));
            Assert.IsTrue(username1a.Equals(username1b));
        }

    }
}

