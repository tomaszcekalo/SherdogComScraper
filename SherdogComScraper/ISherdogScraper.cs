using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SherdogComScraper
{
    public interface ISherdogScraper
    {
        Task<IEnumerable<SherdogEvent>> Scrape();

        Task<SherdogEvent> ScrapeEvent(string url);

        //Performer ParsePerformer(HtmlNode performer);

        //Performer ParseMainEventPerformer(HtmlNode fighter);

        //Fighter ParseFighter(HtmlNode node);
        Task<Fighter> ScrapeFigher(string url);
    }
}