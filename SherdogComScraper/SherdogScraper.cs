using AngleSharp;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SherdogComScraper
{
    public class SherdogScraper : ISherdogScraper
    {
        private IConfiguration config;
        private IBrowsingContext context;

        public SherdogScraper()
        {
            config = Configuration.Default.WithDefaultLoader();
            context = BrowsingContext.New(config);
        }

        public async Task<IEnumerable<SherdogEvent>> Scrape()
        {
            var url = Consts.SherdogComUrlBase +//right now I'm testing only on UFC
                "organizations/Ultimate-Fighting-Championship-UFC-2";
            var document = await context.OpenAsync(url);
            var tasks = document.QuerySelectorAll(".event a")
                .Select(x => ScrapeEvent(x.Attributes["href"].Value));
            var result = await Task.WhenAll(tasks);
            return result;
        }

        public async Task<SherdogEvent> ScrapeEvent(string url)
        {
            var uri = new Uri(Consts.SherdogComUrlBase + url);
            var document = await context.OpenAsync(uri.AbsoluteUri);
            var result = new SherdogEvent();
            result.Url = uri.AbsoluteUri;

            var eventName = document
                .QuerySelector("h1 span");
            var organization = document
                .QuerySelector("[itemtype=\"http://schema.org/Organization\"]");
            var mainEvent = document
                .QuerySelector(".fight_card");
            var mainEventFight = ParseMainEvent(mainEvent);
            var heads = document.QuerySelectorAll(".event_match .table_head");
            var parseScore = heads.SelectMany(x => x.QuerySelectorAll(".col_three")).Any()
                && heads.SelectMany(x => x.QuerySelectorAll(".col_four")).Any()
                && heads.SelectMany(x => x.QuerySelectorAll(".col_five")).Any();

            var fights = document
                .QuerySelectorAll("tr[itemprop='subEvent']")
                .Select(x => ParseFight(x, parseScore))
                .ToList();

            fights.Insert(0, mainEventFight);

            result.Headline = eventName?.ChildNodes.FirstOrDefault()?.TextContent;
            result.Fights = fights;
            //result.OrganizationHref = organization
            //    ?.QuerySelector("a")
            //    ?.Attributes["href"]
            //    .Value;
            result.OrganizationName = organization
                ?.QuerySelector("[itemprop='name']")
                ?.TextContent;
            result.StartDate = document
                .QuerySelector("[itemprop='startDate']")
                ?.Attributes["content"]
                ?.Value;
            result.Location = document
                .QuerySelector("[itemprop='location']")
                ?.TextContent;
            if (result.Location is null)
            {
                ;
            }
            if (result.OrganizationName is null)
            {
                ;
            }
            if (result.StartDate is null)
            {
                ;
            }

            return result;
        }

        private EventFight ParseFight(IElement fight, bool parseScore)
        {
            List<IElement> cells = null;
            if (parseScore)
            {
                cells = fight.QuerySelectorAll("td")
                    .ToList();
                cells.Reverse();
            }

            var result = new EventFight();
            result.Name = fight.QuerySelector("[itemprop='name']")
                ?.Attributes["content"]
                .Value;
            result.Image = fight.QuerySelector("[itemprop='image']")
                    ?.Attributes["content"]
                    .Value;
            result.Performers = fight.QuerySelectorAll("[itemprop='performer']")
                    .Select(x => ParsePerformer(x))
                    .ToList();
            result.Time = cells?[0].TextContent;
            result.Round = cells?[1].TextContent;
            result.Method = cells?[2].FirstChild.TextContent;
            result.Referee = cells?[2].LastChild.TextContent;

            return result;
        }

        private Performer ParsePerformer(IElement performer)
        {
            var image = performer.QuerySelector("img");
            var url = performer
                .QuerySelector("a");

            var result = new Performer()
            {
                Name = url
                    ?.FirstChild
                    .TextContent
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
                    .QuerySelector(".record em")
                    ?.TextContent,
                FinalResult = performer
                    .QuerySelector(".final_result")
                    ?.TextContent
            };
            return result;
        }

        private EventFight ParseMainEvent(IElement mainEvent)
        {
            var resume = mainEvent
                ?.ParentElement.QuerySelector(".fight_card_resume")
                ?.QuerySelectorAll("td")
                .ToDictionary(x => x.FirstElementChild?.TextContent, x => x.LastElementChild?.TextContent);

            var result = new EventFight()
            {
                Performers = mainEvent?.QuerySelectorAll(".fighter")
                    .Select(x => ParseMainEventPerformer(x))
                    .ToList(),
                Method = resume?["Method"],
                Round = resume?["Round"],
                Time = resume?["Time"],
                Referee = resume?["Referee"]
            };
            return result;
        }

        private Performer ParseMainEventPerformer(IElement performer)
        {
            var link = performer.QuerySelector("h3 a");
            var image = performer.QuerySelector("img");

            var result = new Performer()
            {
                Record = performer.QuerySelector(".record")
                    ?.FirstChild
                    .TextContent,
                ImageSrc = image
                    ?.Attributes["src"]
                    .Value,
                Title = image
                    ?.Attributes["title"]
                    .Value,
                Href = link?.Attributes["href"].Value,
                Name = link?.FirstChild.TextContent,
                FinalResult = performer
                    .QuerySelector(".final_result")
                    ?.TextContent
            };

            return result;
        }

        public async Task<Fighter> ScrapeFigher(string url)
        {
            var uri = new Uri(url);
            var document = await context.OpenAsync(url);

            var fighter = ParseFighter(document);
            fighter.Url = uri.AbsoluteUri;
            return fighter;
        }

        private Fighter ParseFighter(IDocument node)
        {
            var result = new Fighter();

            result.ImageSrc = node.QuerySelector(".bio_fighter img.profile-image")
                ?.Attributes["src"]
                .Value;
            result.BirthDate = node.QuerySelector("[itemprop='birthDate']")
                ?.TextContent;
            result.Birthplace = node.QuerySelectorAll(".fighter-nationality")
                .Select(ParseBirthplace)
                .FirstOrDefault();
            result.Height = node.QuerySelector("[itemprop='height']")
                ?.TextContent;
            result.Weight = node.QuerySelector("[itemprop='weight']")
                ?.TextContent;
            result.Association = node.QuerySelector("[itemprop='memberOf'] .association span")
                ?.TextContent;
            result.WeightClass = node.QuerySelector(".association-class a")
                ?.TextContent;
            result.Losses = int.Parse(
                node.QuerySelectorAll(".winloses.lose span")
                .LastOrDefault()
                .TextContent);
            result.Wins = int.Parse(node.QuerySelectorAll(".winloses.win span")
                .LastOrDefault()
                ?.TextContent);

            return result;
        }

        public Birthplace ParseBirthplace(IElement node)
        {
            var result = new Birthplace
            {
                Flag = node.QuerySelector("img.big_flag")
                    ?.Attributes["src"]
                    .Value,
                Locality = node.QuerySelector(".locality")
                    ?.TextContent,
                Nationality = node.QuerySelector("[itemprop='nationality']")
                    ?.TextContent
            };
            return result;
        }
    }
}