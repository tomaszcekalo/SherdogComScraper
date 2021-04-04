using System.Collections.Generic;

namespace SherdogComScraper
{
    public class ScoreDetails
    {
        public int Total { get; set; }
        public List<(string, int)> Values { get; set; }
    }
}