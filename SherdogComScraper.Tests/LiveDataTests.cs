using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace SherdogComScraper.Tests
{
    [TestClass]
    public class LiveDataTests
    {
        [TestMethod]
        public void TestScrape()
        {
            var events = new SherdogScraper()
                .Scrape();
            Assert.IsFalse(events.Any(x => x.Location is null));
            Assert.IsFalse(events.Any(x => x.OrganizationName is null));
            Assert.IsFalse(events.Any(x => x.StartDate is null));
        }

        [TestMethod]
        public void TestScrapeFighter()
        {
            var gamrot = new SherdogScraper()
                .ScrapeFigher("https://www.sherdog.com/fighter/Mateusz-Gamrot-90605");

            Assert.IsNotNull(gamrot);
            Assert.IsNotNull(gamrot.Losses);
            Assert.IsNotNull(gamrot.Wins);
            Assert.IsNotNull(gamrot.Height);
            Assert.IsNotNull(gamrot.Association);
            Assert.IsNotNull(gamrot.BirthDate);
            Assert.IsNotNull(gamrot.Birthplace);
            Assert.IsNotNull(gamrot.Birthplace.Flag);
            Assert.IsNotNull(gamrot.Birthplace.Nationality);
            Assert.IsNotNull(gamrot.Birthplace.Locality);
            Assert.IsNotNull(gamrot.ImageSrc);
            Assert.IsNotNull(gamrot.Weight);
            Assert.IsNotNull(gamrot.WeightClass);
        }
    }
}