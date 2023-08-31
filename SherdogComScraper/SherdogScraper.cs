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
            var uri = new Uri(
                Consts.SherdogComUrlBase +//right now I'm testing only on UFC
                "organizations/Ultimate-Fighting-Championship-UFC-2");
            WebPage homePage = _browser.NavigateToPage(
               uri);
            var events = homePage.Html.CssSelect(".event a")
                .Select(x => ScrapeEvent(x.Attributes["href"].Value))
                .ToList();

            return events;
        }

        public SherdogEvent ScrapeEvent(string url)
        {
            var uri = new Uri(Consts.SherdogComUrlBase + url);
            WebPage eventPage = _browser.NavigateToPage(uri);
            var result = new SherdogEvent();
            result.Url = uri.AbsoluteUri;

            var sectionTitle = eventPage.Html;
            var eventName = sectionTitle
                .CssSelect("h1 span")
                .FirstOrDefault();
            var organization = sectionTitle
                .CssSelect(".organization")
                .FirstOrDefault();
            var mainEvent = eventPage.Html
                .CssSelect(".fight_card")
                .FirstOrDefault();
            var mainEventFight = ParseMainEvent(mainEvent);
            var heads = eventPage.Html.CssSelect(".event_match .table_head");
            var parseScore = heads.CssSelect(".col_three").Any()
                && heads.CssSelect(".col_four").Any()
                && heads.CssSelect(".col_five").Any();

            var fights = eventPage
                .Html
                .CssSelect("tr[itemprop='subEvent']")
                .Select(x => ParseFight(x, parseScore))
                .ToList();

            fights.Insert(0, mainEventFight);

            result.Headline = eventName?.ChildNodes.FirstOrDefault()?.InnerHtml;
            result.Fights = fights;
            result.OrganizationHref = organization
                ?.CssSelect("a")
                .FirstOrDefault()
                ?.Attributes["href"]
                .Value;
            result.OrganizationName = organization
                ?.CssSelect("[itemprop='name']")
                .FirstOrDefault()
                ?.InnerText;
            result.StartDate = eventPage.Html
                .CssSelect("[itemprop='startDate']")
                .FirstOrDefault()
                ?.Attributes["content"]
                ?.Value;
            result.Location = eventPage.Html
                .CssSelect("[itemprop='location']")
                .FirstOrDefault()
                ?.InnerText;

            return result;
        }

        private EventFight ParseMainEvent(HtmlNode mainEvent)
        {
            var resume = mainEvent?
                .ParentNode
                .CssSelect(".footer")
                .CssSelect(".resume")
                .FirstOrDefault()
                ?.CssSelect("td")
                .ToDictionary(x => x.FirstChild.InnerText, x => x.LastChild.InnerText);

            var result = new EventFight()
            {
                Performers = mainEvent?.CssSelect(".fighter")
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

            var result = new EventFight();
            result.Name = fight.CssSelect("[itemprop='name']")
                .FirstOrDefault()
                ?.Attributes["content"]
                .Value;
            result.Image = fight.CssSelect("[itemprop='image']")
                    .FirstOrDefault()
                    ?.Attributes["content"]
                    .Value;
            result.Performers = fight.CssSelect("[itemprop='performer']")
                    .Select(x => ParsePerformer(x))
                    .ToList();
            result.Time = cells?[0].InnerText;
            result.Round = cells?[1].InnerText;
            result.Method = cells?[2].FirstChild.InnerText;
            result.Referee = cells?[2].LastChild.InnerText;

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
                    .InnerHtml
                    .Replace("<br>", " "),
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
            var uri = new Uri(url);
            WebPage eventPage = _browser.NavigateToPage(
                uri);
            var fighter = ParseFighter(eventPage.Html);
            fighter.Url = uri.AbsoluteUri;
            return fighter;
        }

        public Fighter ParseFighter(HtmlNode node)
        {
            var result = new Fighter();

            result.ImageSrc = node.CssSelect(".bio_fighter img.profile-image")
                .FirstOrDefault()
                ?.Attributes["src"]
                .Value;
            result.BirthDate = node.CssSelect("[itemprop='birthDate']")
                .FirstOrDefault()
                ?.InnerText;
            result.Birthplace = node.CssSelect(".fighter-nationality")
                .Select(ParseBirthplace)
                .FirstOrDefault();
            result.Height = node.CssSelect("[itemprop='height']")
                .FirstOrDefault()
                ?.InnerText;
            result.Weight = node.CssSelect("[itemprop='weight']")
                .FirstOrDefault()
                ?.InnerText;
            result.Association = node.CssSelect("[itemprop='memberOf'] .association span")
                .FirstOrDefault()
                ?.InnerText;
            result.WeightClass = node.CssSelect(".association-class a")
                .LastOrDefault()
                ?.InnerText;
            result.Losses = int.Parse(node.CssSelect(".winloses.lose span")
                .LastOrDefault()
                ?.InnerText);
            result.Wins = int.Parse(node.CssSelect(".winloses.win span")
                .LastOrDefault()
                ?.InnerText);

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