using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Scraper
{
    class Scraper_oud
    {
        // Initial related

        private bool bFirstRun { get; set; }

        private string sInitUrl { get; set; }

        private string sPath { get; set; }

        private string sFileLocation { get; set; }

        private Uri uBase { get; set; }
        private Uri uInitial { get; set; }
        private Uri uResponse { get; set; }

        private string[] aNodeNames { get; set; } = new string[3];

        private string sValidateResult { get; set; }

        // Content related

        private HtmlAgilityPack.HtmlNode[] htmlDocItems { get; set; }

        private List<string> lPageContent { get; set; } = new List<string>();

        public SortedDictionary<string, List<string>> dContent { get; private set; } = new SortedDictionary<string, List<string>>();
        public SortedDictionary<string, List<string>> dUrls { get; private set; } = new SortedDictionary<string, List<string>>();

        private List<string> lUrlsToDo { get; set; } = new List<string>();

        private List<string> lFoundUrls { get; set; } = new List<string>();
        private List<string> lScrapedUrls { get; set; } = new List<string>();

        private ushort ushortPages { get; set; }

        //

        public Scraper_oud(string sInitUrl, string sPath)
        {
            this.sInitUrl = sInitUrl;
            this.sPath = sPath;

            aNodeNames[0] = "script";
            aNodeNames[1] = "comment";
            aNodeNames[2] = "style";
        }

        public void GetPage()
        {
            loadPage(sInitUrl);

            if (sValidateResult != "loadError")
            {
                sFileLocation = sPath + uBase.Host;

                getContent(htmlDocItems);
                setContent();

                sValidateResult = "";
            }
            else
            {
                Console.WriteLine("Unable to load in the following webpage: " + sInitUrl);
                Console.ReadLine();

                Environment.Exit(0);
            }
        }

        public void GetDomain()
        {
            lUrlsToDo.Add(sInitUrl);

            for (int i = 0; i < lUrlsToDo.Count; i++)
            {
                loadPage(lUrlsToDo[i]);

                if (sValidateResult != "loadError")
                {
                    if (lScrapedUrls.Contains(uResponse.AbsoluteUri))
                    {
                        lUrlsToDo.Remove(uResponse.AbsoluteUri);
                    }
                    else
                    {
                        if (bFirstRun == false)
                        {
                            uBase = uInitial;
                            sFileLocation = sPath + uBase.Host;

                            bFirstRun = true;
                        }

                        getContent(htmlDocItems);
                        setContent();

                        sValidateResult = "";

                        lUrlsToDo.Remove(uResponse.AbsoluteUri);
                        lUrlsToDo.Remove(uInitial.AbsoluteUri);
                    }
                    i--;
                }
                else
                {
                    setContent();
                }
            }
        }

        private HtmlAgilityPack.HtmlNode[] loadPage(string inputUrl)
        {
            try
            {
                HtmlAgilityPack.HtmlWeb htmlWeb = new HtmlAgilityPack.HtmlWeb();
                HtmlAgilityPack.HtmlDocument htmlDoc = htmlWeb.Load(inputUrl);

                initializeUris(inputUrl, htmlWeb.ResponseUri);

                htmlDocItems = htmlDoc.DocumentNode.DescendantsAndSelf().ToArray();
                return htmlDocItems;
            }
            catch (Exception)
            {
                sValidateResult = "loadError";

                return null;
            }
        }

        private void initializeUris(string sInitUrl, Uri uResponse)
        {
            uInitial = new Uri(sInitUrl);
            this.uResponse = uResponse;
        }

        private void getContent(HtmlAgilityPack.HtmlNode[] htmlDocItems)
        {
            foreach (HtmlAgilityPack.HtmlNode htmlNode in htmlDocItems)
            {

                if (htmlNode.OriginalName == "a" || htmlNode.OriginalName == "A")
                {
                    string sHrefAttribute = htmlNode.GetAttributeValue("href", "null");

                    if (sHrefAttribute != "null")
                    {
                        lUrlsToDo.Add(htmlNode.Attributes["href"].Value.ToString());
                        lFoundUrls.Add(htmlNode.Attributes["href"].Value.ToString());
                    }
                }

                int iScriptCheck = htmlNode.Name.IndexOf(aNodeNames[0], StringComparison.InvariantCultureIgnoreCase);

                if (iScriptCheck == -1)
                {
                    if (!htmlNode.HasChildNodes)
                    {
                        int iCommentCheck = htmlNode.Name.IndexOf(aNodeNames[1], StringComparison.InvariantCultureIgnoreCase);

                        if (iCommentCheck == -1)
                        {
                            string sOriginalNodeText = htmlNode.InnerText;
                            string sTrimmedNodeText = Regex.Replace(sOriginalNodeText, @"\r\n?|\n|", "").Trim();

                            bool bContainsScript = Regex.IsMatch(sTrimmedNodeText, @"\bfunction\b");

                            if (bContainsScript != true)
                            {
                                iScriptCheck = htmlNode.ParentNode.Name.IndexOf(aNodeNames[0], StringComparison.InvariantCultureIgnoreCase);

                                if (iScriptCheck == -1)
                                {
                                    int iStyleCheck = htmlNode.ParentNode.OriginalName.IndexOf(aNodeNames[2], StringComparison.InvariantCultureIgnoreCase);

                                    if (iStyleCheck == -1)
                                    {
                                        if (sTrimmedNodeText != "" & sTrimmedNodeText != "</form>")
                                        {
                                            lPageContent.Add(sTrimmedNodeText);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void setContent()
        {
            if (sValidateResult == "loadError")
            {
                lPageContent.Add("Unable to load page.");

                dContent.Add(sInitUrl, lPageContent.ToList());
                dUrls.Add(sInitUrl, null);
                lScrapedUrls.Add(sInitUrl);

                lPageContent.Clear();
            }
            else
            {
                dContent.Add(uResponse.AbsoluteUri, lPageContent.ToList());
                dUrls.Add(uResponse.AbsoluteUri, lFoundUrls.ToList());
                lScrapedUrls.Add(uResponse.AbsoluteUri);

                lPageContent.Clear();
                lFoundUrls.Clear();

                repairLinksList();
            }
            ushortPages += 1;
        }

        public void repairLinksList()
        {
            List<string> lNewUrls = new List<string>();

            lUrlsToDo = lUrlsToDo.Distinct().ToList();
            lUrlsToDo.Sort();

            for (int i = 0; i < lUrlsToDo.Count; i++)
            {
                string sFixUrl = lUrlsToDo[i];

                if (sFixUrl == "/")
                {
                    lUrlsToDo.Remove(sFixUrl);
                    i--;
                }
                else if (sFixUrl.StartsWith("#") || sFixUrl.StartsWith(".") || sFixUrl.StartsWith("ts3server:") || sFixUrl.StartsWith("tel:") || sFixUrl.StartsWith("mailto:"))
                {
                    lUrlsToDo.Remove(sFixUrl);
                    i--;
                }
                else if (sFixUrl.StartsWith("https") || sFixUrl.StartsWith("http"))
                {
                    if (sFixUrl.StartsWith(uBase.AbsoluteUri))
                    {
                        if (lScrapedUrls.Contains(sFixUrl))
                        {
                            lUrlsToDo.RemoveAt(i);
                            i--;
                        }
                    }
                    else
                    {
                        lUrlsToDo.RemoveAt(i);
                        i--;
                    }
                }
                else if (sFixUrl.StartsWith("/"))
                {
                    int index = sFixUrl.IndexOf('/');
                    string sSecondSubstring = sFixUrl.Substring(index + 1);

                    sSecondSubstring = sSecondSubstring.TrimEnd('/');

                    if (lScrapedUrls.Contains(uResponse.Scheme + "://" + uResponse.Host + "/" + sSecondSubstring) || lScrapedUrls.Contains(uResponse.Scheme + "://" + uResponse.Host + "/" + sSecondSubstring + "/"))
                    {
                        lUrlsToDo.RemoveAt(i);
                    }
                    else
                    {
                        if (lUrlsToDo.Contains(uResponse.Scheme + "://" + uResponse.Host + "/" + sSecondSubstring) || (lUrlsToDo.Contains(uResponse.Scheme + "://" + uResponse.Host + "/" + sSecondSubstring + "/")))
                        {
                            lUrlsToDo.RemoveAt(i);
                        }
                        else
                        {
                            lNewUrls.Add(uResponse.Scheme + "://" + uResponse.Host + "/" + sSecondSubstring);
                            lUrlsToDo.RemoveAt(i);
                        }
                    }
                    i--;
                }
                else
                {
                    if (lScrapedUrls.Contains(uResponse.Scheme + "://" + uResponse.Host + "/" + sFixUrl) || lScrapedUrls.Contains(uResponse.Scheme + "://" + uResponse.Host + "/" + sFixUrl + "/"))
                    {
                        lUrlsToDo.RemoveAt(i);
                    }
                    else
                    {
                        if (lUrlsToDo.Contains(uResponse.Scheme + "://" + uResponse.Host + "/" + sFixUrl) || lUrlsToDo.Contains(uResponse.Scheme + "://" + uResponse.Host + "/" + sFixUrl + "/"))
                        {
                            lUrlsToDo.RemoveAt(i);
                        }
                        else
                        {
                            lNewUrls.Add(uResponse.Scheme + "://" + uResponse.Host + "/" + sFixUrl);
                            lUrlsToDo.RemoveAt(i);
                        }
                    }
                    i--;
                }
            }
            lUrlsToDo.AddRange(lNewUrls.Distinct());
            lUrlsToDo = lUrlsToDo.Distinct().ToList();
            lUrlsToDo.Sort();

            lNewUrls.Clear();
        }

        public void fileMaker()
        {
            sFileLocation = sPath + uBase.Host + ".txt";
            if (File.Exists(sFileLocation))
            {
                File.Delete(sFileLocation);
            }

            using (StreamWriter swWriter = new StreamWriter(sFileLocation, true))
            {
                swWriter.WriteLine("Content:");
                swWriter.WriteLine("-----");
                swWriter.WriteLine();

                foreach (var url in dContent)
                {
                    swWriter.WriteLine(url.Key);
                    swWriter.WriteLine("");

                    foreach (var items in url.Value)
                    {
                        swWriter.WriteLine(items);
                    }

                    swWriter.WriteLine("");
                    swWriter.WriteLine("*****");
                    swWriter.WriteLine("");
                }
            }
        }

        public void urlFileMaker()
        {
            sFileLocation = sPath + uBase.Host + "_Urls.txt";

            if (File.Exists(sFileLocation))
            {
                File.Delete(sFileLocation);
            }

            using (StreamWriter swWriter = new StreamWriter(sFileLocation, true))
            {
                swWriter.WriteLine("Scraped links:");
                swWriter.WriteLine("-----");
                swWriter.WriteLine();

                foreach (var url in dUrls)
                {
                    swWriter.WriteLine(url.Key);
                }

                swWriter.WriteLine("");
                swWriter.WriteLine("*****");
                swWriter.WriteLine("");
            }
        }
    }
}
