using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Scraper
{
    class Page
    {
        internal string sUrl { get; set; }

        internal Uri uInitial { get; set; }
        internal Uri uResponse { get; set; }

        private string[] aNodeNames { get; set; } = new string[3];

        private HtmlAgilityPack.HtmlNode[] htmlDocItems { get; set; }

        internal List<string> lPageContent { get; set; } = new List<string>();
        private List<string> lFoundUrls { get; set; } = new List<string>();
        internal List<string> lUrls { get; set; } = new List<string>();

        public Page(string sUrl)
        {
            this.sUrl = sUrl;

            aNodeNames[0] = "script";
            aNodeNames[1] = "comment";
            aNodeNames[2] = "style";
        }

        internal string getContent()
        {
            try
            {
                HtmlAgilityPack.HtmlWeb htmlWeb = new HtmlAgilityPack.HtmlWeb();
                HtmlAgilityPack.HtmlDocument htmlDoc = htmlWeb.Load(sUrl);

                initializeUris(sUrl, htmlWeb.ResponseUri);

                htmlDocItems = htmlDoc.DocumentNode.DescendantsAndSelf().ToArray();

                Console.WriteLine("Pagina ingeladen.");
                return "Successful";
            }
            catch (Exception)
            {
                return "Unsuccessful";
            }
        }

        private void initializeUris(string sInitUrl, Uri uResponse)
        {
            uInitial = new Uri(sInitUrl);
            this.uResponse = uResponse;
        }

        internal void setContent()
        {
            foreach (HtmlAgilityPack.HtmlNode htmlNode in htmlDocItems)
            {
                if (htmlNode.OriginalName == "a" || htmlNode.OriginalName == "A")
                {
                    string sHrefAttribute = htmlNode.GetAttributeValue("href", "null");

                    if (sHrefAttribute != "null")
                    {
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
                                if (htmlNode.ParentNode != null)
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
        }

        internal void getValidLinks(Uri uBase)
        {
            lUrls.AddRange(lFoundUrls);

            lUrls = lUrls.Distinct().ToList();
            lUrls.Sort();

            for (int i = 0; i < lUrls.Count; i++)
            {
                string sFixUrl = lUrls[i];

                if (sFixUrl == "/")
                {
                    lUrls.RemoveAt(i);
                    i--;
                }
                else if (sFixUrl.StartsWith("#") || sFixUrl.StartsWith(".") || sFixUrl.StartsWith("ts3server:") || sFixUrl.StartsWith("tel:") || sFixUrl.StartsWith("mailto:"))
                {
                    lUrls.RemoveAt(i);
                    i--;
                }
                else if (sFixUrl.StartsWith("https") || sFixUrl.StartsWith("http"))
                {
                    if (sFixUrl.StartsWith(uBase.AbsoluteUri))
                    { }
                    else
                    {
                        lUrls.RemoveAt(i);
                        i--;
                    }
                }
                else if (sFixUrl.StartsWith("/"))
                {
                    int index = sFixUrl.IndexOf('/');
                    string sSecondSubstring = sFixUrl.Substring(index + 1);

                    sSecondSubstring = sSecondSubstring.TrimEnd('/');

                    if (lUrls.Contains(uResponse.Scheme + "://" + uResponse.Host + "/" + sSecondSubstring) || lUrls.Contains(uResponse.Scheme + "://" + uResponse.Host + "/" + sSecondSubstring + "/"))
                    {
                        lUrls.RemoveAt(i);
                    }
                    else
                    {
                        lUrls.Add(uResponse.Scheme + "://" + uResponse.Host + "/" + sSecondSubstring);
                        lUrls.RemoveAt(i);
                    }
                    i--;
                }
                else
                {
                    if (lUrls.Contains(uResponse.Scheme + "://" + uResponse.Host + "/" + sFixUrl) || lUrls.Contains(uResponse.Scheme + "://" + uResponse.Host + "/" + sFixUrl + "/"))
                    {
                        lUrls.RemoveAt(i);
                    }
                    else
                    {
                        lUrls.Add(uResponse.Scheme + "://" + uResponse.Host + "/" + sFixUrl);
                        lUrls.RemoveAt(i);
                    }
                    i--;
                }
            }

            //var query = lUrls.GroupBy(x => x)
            //  .Where(g => g.Count() > 1)
            //  .Select(y => new { Element = y.Key, Counter = y.Count() })
            //  .ToList();
 
            lUrls.Sort();
        }
    }
}
