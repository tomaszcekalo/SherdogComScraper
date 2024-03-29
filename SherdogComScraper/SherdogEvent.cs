﻿using System.Collections.Generic;

namespace SherdogComScraper
{
    public class SherdogEvent
    {
        public string Headline { get; set; }

        //public string OrganizationHref { get; set; }
        public string OrganizationName { get; set; }

        public string StartDate { get; set; }
        public string Location { get; set; }
        public string Url { get; set; }
        public List<EventFight> Fights { get; set; }
    }
}