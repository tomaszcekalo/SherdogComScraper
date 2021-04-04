namespace SherdogComScraper
{
    public class Fighter
    {
        public string ImageSrc { get; set; }
        public string BirthDate { get; set; }
        public string Age { get; set; }
        public Birthplace Birthplace { get; set; }
        public string Height { get; set; }
        public string HeightMetric { get; set; }
        public string Weight { get; set; }
        public string WeightMetric { get; set; }
        public string Association { get; set; }
        public string WeightClass { get; set; }
        public ScoreDetails Wins { get; set; }
        public ScoreDetails Losses { get; set; }
        public int NoContest { get; set; }
        //public string Wins { get; set; }
        //public string Losses { get; set; }
        //public string Draws { get; set; }
    }
}