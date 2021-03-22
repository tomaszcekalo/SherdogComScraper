using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Linq;

namespace SherdogComScraper
{
    public interface ISherdogScraper
    {
        void Scrape();
        void ScrapeEvent(string url);
        void ParsePerformer(HtmlNode performer);
        void ParseFighter(HtmlNode fighter);
    }

    public class SherdogScraper : ISherdogScraper
    {
        private readonly ScrapingBrowser _browser;

        public SherdogScraper()
        {
            _browser = new ScrapingBrowser();
        }

        public SherdogScraper(ScrapingBrowser browser)
        {
            _browser = browser;
        }

        public void Scrape()
        {
            WebPage homePage = _browser.NavigateToPage(
                new Uri(
                Consts.SherdogComUrlBase +//right now I'm testing only on UFC
                "organizations/Ultimate-Fighting-Championship-UFC-2"));
            var events = homePage.Html.CssSelect(".event a");
            foreach (var item in events)
            {
                //go by href
                var linkHref = item.GetAttributeValue("href", string.Empty);
                ScrapeEvent(linkHref);
            }
        }

        public void ScrapeEvent(string url)
        {
            WebPage eventPage = _browser.NavigateToPage(
                new Uri(
                Consts.SherdogComUrlBase + url));
            var eventName = eventPage.Html
                .CssSelect(".section_title h1 span")
                .First();
            var a = eventName.ChildNodes[0].InnerHtml;
            var b = eventName.ChildNodes[2].InnerHtml;
            var mainEvent = eventPage.Html
                .CssSelect(".fight")
                .First();
            var fighterLeftSide = mainEvent
                .CssSelect(".left_side")
                .First();
            var fighterRightSide = mainEvent
                .CssSelect(".right_side")
                .First();
            ParseFighter(fighterLeftSide);
            ParseFighter(fighterRightSide);
            var fights = eventPage
                .Html
                .CssSelect(".event_match tr");
            foreach (var fight in fights)
            {
                var performers = fight.CssSelect("[itemprop='performer']");
                foreach (var performer in performers)
                {
                    ParsePerformer(performer);
                }
            }
            ;
        }
        //https://www.sherdog.com/organizations/Ultimate-Fighting-Championship-UFC-2

        public void ParsePerformer(HtmlNode performer)
        {
            var image = performer.CssSelect("img")
                .First();
            var src = image
            .Attributes["src"]
            .Value;
            var title = image
                .Attributes["title"]
                .Value;
            var url = performer
                .CssSelect("a")
                .First();
            var href = url
                .Attributes["href"]
                .Value;
            var name = url
                .FirstChild
                .InnerHtml;
            var record = performer
                .CssSelect(".record em")
                .First()
                .InnerHtml;
        }

        public void ParseFighter(HtmlNode fighter)
        {
            var record = fighter.CssSelect(".record")
                .First()
                .FirstChild
                .InnerHtml;
            var image = fighter.CssSelect("img")
                .First()
                .Attributes["src"]
                .Value;
            var link = fighter.CssSelect("h3 a")
                .First();
            var href = link.Attributes["href"].Value;
            var name = link.FirstChild.InnerHtml;
        }
    }
}