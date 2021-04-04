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
        Fighter ParseFighter(HtmlNode node);
    }

    public class SherdogScraper : ISherdogScraper
    {
        private readonly ScrapingBrowser _browser;
        private char[] _percent = new char[] { '%' };

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

            var sectionTitle = eventPage.Html
                .CssSelect(".section_title");
            var eventName = sectionTitle
                .CssSelect("h1 span")
                .FirstOrDefault();
            var organization = sectionTitle
                .CssSelect("a")
                .FirstOrDefault();
            var mainEvent = eventPage.Html
                .CssSelect(".fight")
                .First();
            var mainEventFight = ParseMainEvent(mainEvent);
            var heads = eventPage.Html.CssSelect(".event_match .table_head");
            var parseScore = heads.CssSelect(".col_three").Any()
                && heads.CssSelect(".col_four").Any()
                && heads.CssSelect(".col_five").Any();

            var info = eventPage.Html
                .CssSelect(".authors_info");

            var fights = eventPage
                .Html
                .CssSelect(".event_match tr")
                .Select(x => ParseFight(x, parseScore))
                .ToList();

            fights.Insert(0, mainEventFight);

            var result = new SherdogEvent()
            {
                Headline = eventName?.ChildNodes[0].InnerHtml,
                Subhead = eventName?.ChildNodes[2].InnerHtml,
                Fights = fights,
                OrganizationHref = organization
                    ?.Attributes["href"]
                    .Value,
                OrganizationName = organization
                    ?.CssSelect("span")
                    .FirstOrDefault()
                    ?.InnerText,
                StartDate = info
                    .CssSelect(".date")
                    .FirstOrDefault()
                    ?.InnerText,
                Location = info
                    .CssSelect(".author")
                    .FirstOrDefault()
                    ?.InnerText
            };
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

        public Fighter ScrapeFigher(string url)
        {
            WebPage eventPage = _browser.NavigateToPage(
                new Uri(url));
            return ParseFighter(eventPage.Html);
        }

        public Fighter ParseFighter(HtmlNode node)
        {
            var result = new Fighter
            {
                ImageSrc = node.CssSelect(".bio_fighter img.profile_image")
                    .FirstOrDefault()
                    ?.Attributes["src"]
                    .Value,
                BirthDate = node.CssSelect(".birthday [itemprop='birthDate']")
                    .FirstOrDefault()
                    ?.InnerText,
                Age = node.CssSelect(".birthday")
                    .FirstOrDefault()
                    ?.LastChild
                    .InnerText
                    .Trim(),
                Birthplace = node.CssSelect(".birthplace")
                    .Select(ParseBirthplace)
                    .FirstOrDefault(),
                Height = node.CssSelect(".height [itemprop='height']")
                    .FirstOrDefault()
                    ?.InnerText,
                HeightMetric = node.CssSelect(".height")
                    .FirstOrDefault()
                    ?.LastChild
                    .InnerText
                    .Trim(),
                Weight = node.CssSelect(".weight [itemprop='weight']")
                    .FirstOrDefault()
                    ?.InnerText,
                WeightMetric = node.CssSelect(".weight")
                    .FirstOrDefault()
                    ?.LastChild
                    .InnerText
                    .Trim(),
                Association = node.CssSelect(".association span")
                    .FirstOrDefault()
                    ?.InnerText,
                WeightClass = node.CssSelect(".wclass .title")
                    .FirstOrDefault()
                    ?.InnerText,
                Losses = node.CssSelect(".loser")
                    .Select(ParseScoreDetails)
                    .FirstOrDefault(),
                Wins = node.CssSelect(".bio_graph")
                    .Select(ParseScoreDetails)
                    .FirstOrDefault(),
            };
            return result;
        }

        public Birthplace ParseBirthplace(HtmlNode node)
        {
            var result = new Birthplace
            {
                Flag = node.CssSelect("img.big_flag")
                    .FirstOrDefault()
                    ?.Attributes["src"]
                    .Value,
                Locality = node.CssSelect(".locality")
                    .FirstOrDefault()
                    ?.InnerText,
                Nationality = node.CssSelect("[itemprop='nationality']")
                    .FirstOrDefault()
                    ?.InnerText
            };
            return result;
        }

        public ScoreDetails ParseScoreDetails(HtmlNode node)
        {
            var result = new ScoreDetails();
            result.Total = int.Parse(
                node.CssSelect(".counter")
                    .FirstOrDefault()
                    ?.InnerText);
            result.Values = node.CssSelect(".graph_tag")
                .Select(x =>
                    (
                        x.InnerText,
                        int.Parse(
                            x.CssSelect("em")
                                .FirstOrDefault()
                                ?.InnerText
                                .TrimEnd(_percent))
                    )).ToList();

            return result;
        }
    }
}