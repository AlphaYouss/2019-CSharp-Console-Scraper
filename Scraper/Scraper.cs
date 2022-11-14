using System;
using System.Collections.Generic;
using System.Linq;

namespace Scraper
{
    class Scraper
    {
        // Initial related

        private Uri uBase { get; set; }

        private string sInitUrl { get; set; }

        private string sPath { get; set; }

        private bool bDomain { get; set; }

        private string[] aNodeNames { get; set; } = new string[3];

        private string sResult { get; set; }

        // Content related

        public SortedDictionary<string, List<string>> dContent { get; private set; } = new SortedDictionary<string, List<string>>();
        public SortedDictionary<string, List<string>> dUrls { get; private set; } = new SortedDictionary<string, List<string>>();

        private List<string> lLinksToDo { get; set; } = new List<string>();
        private List<string> lScrapedUrls { get; set; } = new List<string>();

        //

        public Scraper(string sInitUrl, string sPath, bool bDomain)
        {
            this.sInitUrl = sInitUrl;
            this.sPath = sPath;
            this.bDomain = bDomain;

            aNodeNames[0] = "script";
            aNodeNames[1] = "comment";
            aNodeNames[2] = "style";
        }

        public void Start()
        {
            Page pStart = new Page(sInitUrl);
            if (pStart.getContent() == "Successful")
            {
                uBase = pStart.uResponse;

                pStart.setContent();
                pStart.getValidLinks(uBase);

                lScrapedUrls.Add(uBase.AbsoluteUri);
                dContent.Add(pStart.uResponse.AbsoluteUri, pStart.lPageContent.ToList());
                dUrls.Add(pStart.uResponse.AbsoluteUri, pStart.lUrls.ToList());
            }
            else
            {
                Console.WriteLine("Niet gelukt om de volgende webpagina in te laden: " + pStart.sUrl);
                sResult = "Mislukt";
            }

            switch (bDomain)
            {
                case true:
                    if (sResult != "Mislukt")
                    {
                        lLinksToDo.AddRange(pStart.lUrls);

                        for (int i = 0; i < lLinksToDo.Count; i++)
                        {
                            Page p = new Page(lLinksToDo[i]);

                            if (p.getContent() == "Successful")
                            {
                                if (checkToDo(p.uInitial.AbsoluteUri, p.uResponse.AbsoluteUri) == "Nieuw")
                                {
                                    p.setContent();
                                    p.getValidLinks(uBase);

                                    lScrapedUrls.Add(p.uResponse.AbsoluteUri);
                                    dContent.Add(p.uResponse.AbsoluteUri, p.lPageContent.ToList());
                                    dUrls.Add(p.uResponse.AbsoluteUri, p.lUrls.ToList());
                                }
                            }
                            else
                            {
                                Console.WriteLine("Niet gelukt om de volgende webpagina in te laden: " + pStart.sUrl);

                                dContent.Add(p.sUrl, null);
                                dUrls.Add(p.sUrl, null);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Kan domein niet verder scrapen omdat het niet gelukt is om de eerste webpagina op te halen.");
                    }
                    break;
                case false:
                    Console.WriteLine("Webpagina binnengehaald.");
                    break;
            }
        }

        internal string checkToDo(string sInitialUrl, string sResponseUrl)
        {
            if (lScrapedUrls.Contains(sInitialUrl) || lScrapedUrls.Contains(sResponseUrl) || lScrapedUrls.Contains(sInitialUrl + "/") || lScrapedUrls.Contains(sResponseUrl + "//"))
            {
                lLinksToDo.Remove(sInitialUrl);
                lLinksToDo.Remove(sResponseUrl);

                return "Bestaat";
            }
            else
            {
                return "Nieuw";
            }
        }

        internal string fileCreator()
        {
            if (sResult != "Mislukt")
            {
                FileWriter fW = new FileWriter(sPath + uBase.Host, true, false, dContent, dUrls);
                return fW.sWrite();
            }
            else
            {
                Console.WriteLine("Niet instaat om het bestand/bestanden aan te maken.");
                return "Mislukt";
            }
        }
    }
}
