using Newtonsoft.Json;
using System;

namespace SherdogComScraper.ConsoleExample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var result = new SherdogScraper()
                //.Scrape();
                .ScrapeFigher("https://www.sherdog.com/fighter/Mateusz-Gamrot-90605");

            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            Console.ReadKey();
        }
    }
}