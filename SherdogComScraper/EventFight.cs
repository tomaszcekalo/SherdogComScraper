using System.Collections.Generic;

namespace SherdogComScraper
{
    internal class EventFight
    {
        public List<Performer> Performers { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Method { get; set; }
        public string Round { get; set; }
        public string Time { get; set; }
        public string Referee { get; internal set; }
    }
}