using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SherdogComScraper.Tests
{
    [TestClass]
    public class LiveDataTests
    {
        [TestMethod]
        public void TestScrape()
        {
            new SherdogScraper()
                .Scrape();
        }
        [TestMethod]
        public void TestScrapeFighter()
        {
            new SherdogScraper()
                .ScrapeFigher("https://www.sherdog.com/fighter/Mateusz-Gamrot-90605");
        }
    }
}