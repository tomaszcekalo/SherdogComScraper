using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SherdogComScraper
{
    public interface ISherdogScraper
    {
        List<SherdogEvent> Scrape();
        SherdogEvent ScrapeEvent(string url);
        Performer ParsePerformer(HtmlNode performer);
        Performer ParseMainEventPerformer(HtmlNode fighter);
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

        public List<SherdogEvent> Scrape()
        {
            WebPage homePage = _browser.NavigateToPage(
                new Uri(
                Consts.SherdogComUrlBase +//right now I'm testing only on UFC
                "organizations/Ultimate-Fighting-Championship-UFC-2"));
            var events = homePage.Html.CssSelect(".event a")
                .Select(x => ScrapeEvent(x.Attributes["href"].Value))
                .ToList();

            return events;
        }

        public SherdogEvent ScrapeEvent(string url)
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
            var mainEventFight = ParseMainEvent(mainEvent);
            var heads = eventPage.Html.CssSelect(".event_match .table_head");
            var parseScore = heads.CssSelect(".col_three").Any()
                && heads.CssSelect(".col_four").Any()
                && heads.CssSelect(".col_five").Any();

            var fights = eventPage
                .Html
                .CssSelect(".event_match tr")
                .Select(x => ParseFight(x, parseScore))
                .ToList();

            var result = new SherdogEvent();
            return result;
        }

        private EventFight ParseMainEvent(HtmlNode mainEvent)
        {
            var resume = mainEvent
                .ParentNode
                .CssSelect(".footer")
                .CssSelect(".resume")
                .FirstOrDefault()
                ?.CssSelect("td")
                .ToDictionary(x => x.FirstChild.InnerText, x => x.LastChild.InnerText);

            var result = new EventFight()
            {
                Performers = mainEvent.CssSelect(".fighter")
                    .Select(x => ParseMainEventPerformer(x))
                    .ToList(),
                Method = resume?["Method"],
                Round = resume?["Round"],
                Time = resume?["Time"],
                Referee = resume?["Referee"]
            };
            return result;
        }

        private EventFight ParseFight(HtmlNode fight, bool parseScrore)
        {
            List<HtmlNode> cells = null;
            if (parseScrore)
            {
                cells = fight.CssSelect("td")
                    .ToList();
                cells.Reverse();
            }

            var result = new EventFight()
            {
                Name = fight.CssSelect("[itemprop='name']")
                    .FirstOrDefault()
                    ?.Attributes["content"]
                    .Value,
                Image = fight.CssSelect("[itemprop='image']")
                    .FirstOrDefault()
                    ?.Attributes["content"]
                    .Value,
                Performers = fight.CssSelect("[itemprop='performer']")
                    .Select(x => ParsePerformer(x))
                    .ToList(),
                Time = cells?[0].InnerText,
                Round = cells?[1].InnerText,
                Method = cells?[2].FirstChild.InnerText,
                Referee = cells?[2].LastChild.InnerText
            };
            return result;
        }

        //https://www.sherdog.com/organizations/Ultimate-Fighting-Championship-UFC-2

        public Performer ParsePerformer(HtmlNode performer)
        {
            var image = performer.CssSelect("img")
                .FirstOrDefault();
            var url = performer
                .CssSelect("a")
                .FirstOrDefault();

            var result = new Performer()
            {
                Name = url
                    ?.FirstChild
                    .InnerHtml,
                Href = url
                    ?.Attributes["href"]
                    .Value,
                ImageSrc = image?
                    .Attributes["src"]
                    .Value,
                Title = image?
                    .Attributes["title"]
                    .Value,
                Record = performer
                    .CssSelect(".record em")
                    .FirstOrDefault()
                    ?.InnerHtml,
                FinalResult = performer
                    .CssSelect(".final_result")
                    .FirstOrDefault()
                    ?.InnerText
            };
            return result;
        }

        public Performer ParseMainEventPerformer(HtmlNode performer)
        {
            var link = performer.CssSelect("h3 a")
                .FirstOrDefault();
            var image = performer.CssSelect("img")
                    .FirstOrDefault();

            var result = new Performer()
            {
                Record = performer.CssSelect(".record")
                    .FirstOrDefault()
                    ?.FirstChild
                    .InnerHtml,
                ImageSrc = image
                    ?.Attributes["src"]
                    .Value,
                Title = image
                    ?.Attributes["title"]
                    .Value,
                Href = link?.Attributes["href"].Value,
                Name = link?.FirstChild.InnerHtml,
                FinalResult = performer
                    .CssSelect(".final_result")
                    .FirstOrDefault()
                    ?.InnerText
            };

            return result;
        }
    }
}