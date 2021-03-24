using Newtonsoft.Json;
using System;

namespace SherdogComScraper.ConsoleExample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var result = new SherdogScraper().Scrape();

            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            Console.ReadKey();
        }
    }
}