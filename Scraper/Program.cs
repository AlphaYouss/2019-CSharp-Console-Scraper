using System;
using System.Linq;

namespace Scraper
{
    class Program
    {
        static void Main(string[] args)
        {
            Scraper s = new Scraper("https://www.radiuscollege.nl/", "D://", true);

            s.Start();

            if (s.fileCreator() == "Successful")
            {
                Console.WriteLine("Succesvol de bestand/bestanden aangemaakt.");
            }
            else
            {
                Console.WriteLine("Bestanden niet aangemaakt.");
            }

            var temp = s.dContent;
            var temp1 = s.dUrls;
        }
    }
}
