using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Scraper
{
    class FileWriter
    {
        private string sFileLocation { get; set; }

        private bool bContent { get; set; }
        private bool bUrl { get; set; }

        private SortedDictionary<string, List<string>> dContent { get; set; }
        private SortedDictionary<string, List<string>> dUrls { get; set; }

        public FileWriter(string sFileLocation, bool bUrl, bool bContent, SortedDictionary<string, List<string>> dContent, SortedDictionary<string, List<string>> dUrls)
        {
            this.sFileLocation = sFileLocation;

            this.bUrl = bUrl;
            this.bContent = bContent;

            this.dContent = dContent;
            this.dUrls = dUrls;
        }

        public string sWrite()
        {
            if (bUrl == true)
            {
                Console.WriteLine("Starting to write all found urls.");
                sCreateUrlTxt();
            }

            if (bContent == true)
            {
                Console.WriteLine("Starting to write the page content.");
                sCreateContentTxt();
            }

            if (bUrl == false && bContent == false)
            {
                return "Unsuccessful";
            }
            else
            {
                return "Successful";
            }
        }

        private void sCreateUrlTxt()
        {
            if (File.Exists(sFileLocation + "_urls.txt"))
            {
                Console.WriteLine("Het bestand " + sFileLocation + "_Urls.txt" + " bestaat, verwijderen...");
                File.Delete(sFileLocation + "_urls.txt");
            }
            else
            {
                Console.WriteLine("Het bestand " + sFileLocation + "_Urls.txt" + " bestaat niet, aanmaken...");
            }

            using (StreamWriter swWriter = new StreamWriter(sFileLocation + "_urls.txt", true))
            {
                swWriter.WriteLine("Gevonden links:");
                swWriter.WriteLine("-----");
                swWriter.WriteLine();

                foreach (var sUrl in dUrls)
                {
                    swWriter.WriteLine(sUrl.Key);
                    swWriter.WriteLine("");

                    foreach (var lUrl in sUrl.Value)
                    {
                        swWriter.WriteLine(lUrl);
                    }

                    swWriter.WriteLine("");
                    swWriter.WriteLine("*****");
                    swWriter.WriteLine("");
                }
            }
            Console.WriteLine("Text bestand met alle webpagina/webpagina's succesvol gemaakt.");
        }

        private void sCreateContentTxt()
        {
            if (File.Exists(sFileLocation + ".txt"))
            {
                Console.WriteLine("Het bestand " + sFileLocation + ".txt" + " bestaat, verwijderen...");
                File.Delete(sFileLocation + ".txt");
            }
            else
            {
                Console.WriteLine("Het bestand " + sFileLocation + "_Urls.txt" + " bestaat niet, aanmaken...");
            }

            using (StreamWriter swWriter = new StreamWriter(sFileLocation + ".txt", true))
            {
                swWriter.WriteLine("Gevonden inhoud:");
                swWriter.WriteLine("-----");
                swWriter.WriteLine();

                foreach (var url in dContent)
                {
                    swWriter.WriteLine(url.Key);
                    swWriter.WriteLine("");

                    foreach (var item in url.Value)
                    {
                        swWriter.WriteLine(item);
                    }

                    swWriter.WriteLine("");
                    swWriter.WriteLine("*****");
                    swWriter.WriteLine("");
                }
            }
            Console.WriteLine("Text bestand met inhoudelijke text van de webpagina / webpagina's succesvol gemaakt.");
        }
    }
}
