using Microsoft.VisualStudio.TestTools.UnitTesting;
using SherdogComScraper.ScrapySharp;
using System.Linq;
using System.Threading.Tasks;

namespace SherdogComScraper.Tests
{
    [TestClass]
    public class LiveDataTests
    {
        [TestMethod]
        public async Task TestScrape()
        {
            var events = await new SherdogScraper()
                .Scrape();
            Assert.IsFalse(events.Any(x => x.Location is null));
            Assert.IsFalse(events.Any(x => x.OrganizationName is null));
            Assert.IsFalse(events.Any(x => x.StartDate is null));
        }

        [TestMethod]
        public async Task TestScrapeFighter()
        {
            var gamrot = await new SherdogScraper()
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

        [TestMethod]
        public async Task ScrapeEvent()
        {
            var url = "events/UFC-Fight-Night-228-Fiziev-vs-Gamrot-98052";
            var fightNight = await new SherdogScraper()
                .ScrapeEvent(url);
            Assert.IsNotNull(fightNight);
            Assert.IsNotNull(fightNight.Headline);
            Assert.IsNotNull(fightNight.Location);
            Assert.IsNotNull(fightNight.OrganizationName);
            Assert.IsNotNull(fightNight.StartDate);
            Assert.IsNotNull(fightNight.Fights);
            Assert.IsTrue(fightNight.Fights.Any());
            Assert.IsNotNull(fightNight.Fights.First().Performers);
            Assert.IsTrue(fightNight.Fights.First().Performers.Any());
            Assert.IsNotNull(fightNight.Fights.First().Performers.First().Name);
            Assert.IsNotNull(fightNight.Fights.First().Performers.First().FinalResult);
        }

        [TestMethod]
        public async Task TestScrapeScrapySharp()
        {
            var events = await new SherdogScraperScrapySharp()
                .Scrape();
            Assert.IsFalse(events.Any(x => x.Location is null));
            Assert.IsFalse(events.Any(x => x.OrganizationName is null));
            Assert.IsFalse(events.Any(x => x.StartDate is null));
        }

        [TestMethod]
        public async Task TestScrapeFighterScrapySharp()
        {
            var gamrot = await new SherdogScraperScrapySharp()
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

        [TestMethod]
        public async Task ScrapeEventScrapySharp()
        {
            var url = "events/UFC-Fight-Night-228-Fiziev-vs-Gamrot-98052";
            var fightNight = await new SherdogScraperScrapySharp()
                .ScrapeEvent(url);
            Assert.IsNotNull(fightNight);
            Assert.IsNotNull(fightNight.Headline);
            Assert.IsNotNull(fightNight.Location);
            Assert.IsNotNull(fightNight.OrganizationName);
            Assert.IsNotNull(fightNight.StartDate);
            Assert.IsNotNull(fightNight.Fights);
            Assert.IsTrue(fightNight.Fights.Any());
            Assert.IsNotNull(fightNight.Fights.First().Performers);
            Assert.IsTrue(fightNight.Fights.First().Performers.Any());
            Assert.IsNotNull(fightNight.Fights.First().Performers.First().Name);
            Assert.IsNotNull(fightNight.Fights.First().Performers.First().FinalResult);
        }
    }
}