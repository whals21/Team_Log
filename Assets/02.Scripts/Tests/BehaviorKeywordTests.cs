using System.Collections.Generic;
using NUnit.Framework;
using TeamLog.Skill;

namespace TeamLog.Tests
{
    /// <summary>
    /// Phase BK: BehaviorTag / BehaviorTagResolver 단위 테스트.
    /// null/빈 목록 경계, 다중 rank 합산, 키워드 필터링 검증.
    /// </summary>
    [TestFixture]
    public class BehaviorKeywordTests
    {
        // ── Has ──

        [Test]
        public void Has_ReturnsTrue_WhenKeywordPresent()
        {
            var tags = new List<BehaviorTag>
            {
                new BehaviorTag(BehaviorKeyword.Pierce, 1),
                new BehaviorTag(BehaviorKeyword.Spread, 1),
            };
            Assert.IsTrue(BehaviorTagResolver.Has(tags, BehaviorKeyword.Pierce));
            Assert.IsTrue(BehaviorTagResolver.Has(tags, BehaviorKeyword.Spread));
        }

        [Test]
        public void Has_ReturnsFalse_WhenKeywordAbsent()
        {
            var tags = new List<BehaviorTag>
            {
                new BehaviorTag(BehaviorKeyword.Pierce, 1),
            };
            Assert.IsFalse(BehaviorTagResolver.Has(tags, BehaviorKeyword.Lifesteal));
        }

        [Test]
        public void Has_ReturnsFalse_OnNullList()
        {
            Assert.IsFalse(BehaviorTagResolver.Has(null, BehaviorKeyword.Pierce));
        }

        [Test]
        public void Has_ReturnsFalse_OnEmptyList()
        {
            Assert.IsFalse(BehaviorTagResolver.Has(new List<BehaviorTag>(), BehaviorKeyword.Pierce));
        }

        // ── First ──

        [Test]
        public void First_ReturnsFirstMatch_WhenPresent()
        {
            var tags = new List<BehaviorTag>
            {
                new BehaviorTag(BehaviorKeyword.Chain, 1),
                new BehaviorTag(BehaviorKeyword.Chain, 2), // 두 번째
            };
            var first = BehaviorTagResolver.First(tags, BehaviorKeyword.Chain);
            Assert.IsTrue(first.HasValue);
            Assert.AreEqual(1, first.Value.Rank); // 첫 번째 rank
        }

        [Test]
        public void First_ReturnsNull_WhenAbsent()
        {
            var tags = new List<BehaviorTag>
            {
                new BehaviorTag(BehaviorKeyword.Pierce, 1),
            };
            Assert.IsFalse(BehaviorTagResolver.First(tags, BehaviorKeyword.Chain).HasValue);
        }

        // ── All ──

        [Test]
        public void All_ReturnsAllMatches()
        {
            var tags = new List<BehaviorTag>
            {
                new BehaviorTag(BehaviorKeyword.Bounce, 1),
                new BehaviorTag(BehaviorKeyword.Chain, 2),
                new BehaviorTag(BehaviorKeyword.Bounce, 3),
            };
            var matches = BehaviorTagResolver.All(tags, BehaviorKeyword.Bounce);
            Assert.AreEqual(2, matches.Count);
            Assert.AreEqual(1, matches[0].Rank);
            Assert.AreEqual(3, matches[1].Rank);
        }

        // ── RankSum ──

        [Test]
        public void RankSum_SumsMultipleRanks()
        {
            var tags = new List<BehaviorTag>
            {
                new BehaviorTag(BehaviorKeyword.Bounce, 2),
                new BehaviorTag(BehaviorKeyword.Bounce, 3),
            };
            Assert.AreEqual(5, BehaviorTagResolver.RankSum(tags, BehaviorKeyword.Bounce));
        }

        [Test]
        public void RankSum_ReturnsZero_OnNullList()
        {
            Assert.AreEqual(0, BehaviorTagResolver.RankSum(null, BehaviorKeyword.Bounce));
        }

        [Test]
        public void RankSum_ReturnsZero_OnEmptyList()
        {
            Assert.AreEqual(0, BehaviorTagResolver.RankSum(new List<BehaviorTag>(), BehaviorKeyword.Bounce));
        }

        [Test]
        public void RankSum_ReturnsZero_WhenOnlyUnrelatedTags()
        {
            var tags = new List<BehaviorTag>
            {
                new BehaviorTag(BehaviorKeyword.Pierce, 1),
                new BehaviorTag(BehaviorKeyword.Chain, 2),
            };
            Assert.AreEqual(0, BehaviorTagResolver.RankSum(tags, BehaviorKeyword.Bounce));
        }

        // ── BehaviorTag ToString ──

        [Test]
        public void ToString_IncludesRank_WhenNonZero()
        {
            Assert.AreEqual("Bounce(2)", new BehaviorTag(BehaviorKeyword.Bounce, 2).ToString());
            Assert.AreEqual("Pierce", new BehaviorTag(BehaviorKeyword.Pierce, 0).ToString());
        }
    }
}
