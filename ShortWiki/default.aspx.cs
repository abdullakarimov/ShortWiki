using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
using Humanizer;

namespace ShortWiki
{
    public partial class _default : System.Web.UI.Page
    {
        public string enWpApiUrl = "https://en.wikipedia.org/w/api.php?action=query&prop=categories&cllimit=50&format=xml&clshow=!hidden&redirects&titles=";
        public string wdApiUrl = "https://www.wikidata.org/w/api.php?action=wbgetentities&sites=enwiki&format=xml&props=descriptions&titles=";
        public string enWpOpenSearchUrl = "https://en.wikipedia.org/w/api.php?action=opensearch&limit=1&namespace=0&format=xml&search=";
        public string resultString = "";
        private string resultText = "";

        public string[] nonPlural =
        {
            "california",
            "russia",
            "texas",
            "illinois",
            "malus",
            "travis",
            "los",
            "os"
        };

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public string getOriginalTitle(string title)
        {
            //get original page title
            string nameNodePath = "/api/query/redirects/r";
            string url = enWpApiUrl + title;
            XmlDocument resultXml = new XmlDocument();
            resultXml.Load(url);
            XmlElement root = resultXml.DocumentElement;
            XmlNode nameNode = root.SelectSingleNode(nameNodePath);
            string originalName = title;
            if (nameNode != null) originalName = nameNode.Attributes["to"].Value;
            return originalName;
        }

        public string getCategories(string title)
        {
            //get category titles from EnWiki and convert them to singularized text
            string name = title.Replace(" ", "_");
            string categoryNodePath = "/api/query/pages/page/categories/cl";
            string result = "";
            string url = enWpApiUrl + name;
            XmlDocument resultXml = new XmlDocument();
            resultXml.Load(url);
            XmlElement root = resultXml.DocumentElement;
            string originalTitle = getOriginalTitle(name);
            string originalName = originalTitle.Replace(" ", "_");

            XmlNodeList categoryNodeList = root.SelectNodes(categoryNodePath);
            foreach (XmlNode categoryNode in categoryNodeList)
            {
                string categoryName = categoryNode.Attributes["title"].Value;
                categoryName = categoryName.Replace("Category:", "");
                if (Has(categoryName, "article") || Has(categoryName, "page") || Has(categoryName, "error") ||
                    Has(categoryName, "stub") || categoryName.ToLower() == name.ToLower() ||
                    categoryName.ToLower() == originalName.ToLower() ||
                    categoryName.ToLower().Singularize() == title.ToLower() ||
                    categoryName.ToLower().Singularize() == originalName.ToLower()) categoryName = null;

                if (!String.IsNullOrEmpty(categoryName))
                {
                    if (categoryName.Contains(' '))
                    {
                        foreach (string word in categoryName.Split(' '))
                        {
                            if (!nonPlural.Contains(word.ToLower()))
                            {
                                categoryName = categoryName.Replace(word, word.Singularize(false));
                            }
                        }
                    }
                    else
                    {
                        if (!nonPlural.Contains(categoryName.ToLower())) categoryName = categoryName.Singularize(false);
                    }
                    result += categoryName + ".<br />";
                }
            }
            return result;
        }

        public XmlElement getOpenSearchResultRoot(string query)
        {
            enWpOpenSearchUrl += query;
            XmlDocument tempXml = new XmlDocument();
            tempXml.Load(enWpOpenSearchUrl);
            string osXmlString = tempXml.OuterXml;
            string filter = @"xmlns(:\w+)?=""([^""]+)""|xsi(:\w+)?=""([^""]+)""";
            osXmlString = Regex.Replace(osXmlString, filter, "");
            XmlDocument openSearchXmlDoc = new XmlDocument();
            openSearchXmlDoc.LoadXml(osXmlString);
            XmlElement root = openSearchXmlDoc.DocumentElement;
            return root;
        }

        public XmlDocument getOpenSearchResultDoc(string query)
        {
            enWpOpenSearchUrl += query;
            XmlDocument tempXml = new XmlDocument();
            tempXml.Load(enWpOpenSearchUrl);
            string osXmlString = tempXml.OuterXml;
            string filter = @"xmlns(:\w+)?=""([^""]+)""|xsi(:\w+)?=""([^""]+)""";
            osXmlString = Regex.Replace(osXmlString, filter, "");
            XmlDocument openSearchXmlDoc = new XmlDocument();
            openSearchXmlDoc.LoadXml(osXmlString);
            return openSearchXmlDoc;
        }

        public string getOpenSearchResultPageTitle(string query)
        {
            string pageTitle = "";
            XmlElement root = getOpenSearchResultRoot(query);
            XmlNode pageTitleNode = root.SelectSingleNode("//Text");
            if (pageTitleNode != null) pageTitle = pageTitleNode.InnerText;
            return pageTitle;
        }

        public string getOpenSearchResultPageBody(string query)
        {
            string pageBody = "";
            //XmlElement root = getOpenSearchResultRoot(query);
            XmlDocument doc = getOpenSearchResultDoc(query);
            XmlNode pageBodyNode = doc.SelectSingleNode("//Description");
            if (pageBodyNode != null) pageBody = pageBodyNode.InnerText;
            return Regex.Replace(pageBody, @" ?\(.*?\)", string.Empty);
        }

        public string getOpenSearchResultImage(string query)
        {
            string image = "";
            XmlElement openSearchXmlRoot = getOpenSearchResultRoot(query);
            XmlNode imageNode = openSearchXmlRoot.SelectSingleNode("/SearchSuggestion/Section/Item/Image");
            if (imageNode != null) image = imageNode.Attributes["source"].Value;
            return image;
        }

        public string getOpenSearchResultPageUrl(string query)
        {
            string pageUrl = "";
            XmlElement root = getOpenSearchResultRoot(query);
            XmlNode pageUrlNode = root.SelectSingleNode("//Url");
            if (pageUrlNode != null) pageUrl = pageUrlNode.InnerText;
            return pageUrl;
        }

        public static XmlDocument RemoveXmlns(XmlDocument doc)
        {
            XDocument d;
            using (var nodeReader = new XmlNodeReader(doc))
                d = XDocument.Load(nodeReader);

            d.Root.Descendants().Attributes().Where(x => x.IsNamespaceDeclaration).Remove();

            foreach (var elem in d.Descendants())
                elem.Name = elem.Name.LocalName;

            var xmlDocument = new XmlDocument();
            using (var xmlReader = d.CreateReader())
                xmlDocument.Load(xmlReader);

            return xmlDocument;
        }
        
        protected void searchButton_Click(object sender, EventArgs e)
        {
            string name = searchQuery.Text;
            ShowResults(name);
        }

        public void ShowResults(string name)
        {
            if (!String.IsNullOrEmpty(name))
            {
                if (name.Length > 200)
                {
                    resultText = "Query string is too long.";
                }
                else
                {
                    string resultString = getCategories(name);
                    //get description from Wikidata
                    string originalTitle = getOriginalTitle(name);
                    string originalName = originalTitle.Replace(" ", "_");
                    wdApiUrl += originalName;
                    XmlDocument wikidataXML = new XmlDocument();
                    wikidataXML.Load(wdApiUrl);
                    XmlElement wdRoot = wikidataXML.DocumentElement;
                    XmlNode wdDescNode = wikidataXML.SelectSingleNode("/api/entities/entity/descriptions/description[@language='en']");
                    string wdDesc = "";
                    if (wdDescNode != null) wdDesc = wdDescNode.Attributes["value"].Value;
                    string wikiUrl = "https://en.wikipedia.org/wiki/" + originalName;
                    string openSearchUrl = getOpenSearchResultPageUrl(name);

                    string wikiLink = "<b><a href='https://en.wikipedia.org/wiki/" + originalName.Replace("'", "%27") + "'>" + originalTitle +
                                      "</a></b>";
                    string openSearchDesc = getOpenSearchResultPageBody(name);
                    string openSearchTitle = getOpenSearchResultPageTitle(name);

                    if (!String.IsNullOrEmpty(wdDesc))
                    {
                        resultText += wikiLink + " is " + wdDesc + ".<br />";
                    }
                    if (!String.IsNullOrEmpty(openSearchDesc) && String.IsNullOrEmpty(wdDesc) && openSearchUrl==wikiUrl)
                    {
                        openSearchDesc.Replace(originalTitle, wikiLink);
                        resultText += openSearchDesc + "<br />";
                    }
                    if (!String.IsNullOrEmpty(resultString))
                    {
                        resultText += "These facts are known about " + wikiLink + ":<br />" + resultString;
                    }
                    if (String.IsNullOrEmpty(wdDesc) && String.IsNullOrEmpty(resultString))
                    {
                        resultText = "Sorry, not enough information about " + wikiLink + " to display.";
                    }
                    searchResult.Text = resultText;
                }
            }
        }

        public bool Has(string c, string str)
        {
            return c.ToLower().Contains(str);
        }

        protected void aboutLink_Click(object sender, EventArgs e)
        {
            //displays the about text
            const string aboutText = @"<i>Shorter Wikipedia</i> is a website aimed to get short, concise material from 
                                <a href='https://en.wikipedia.org/wiki/Wikipedia'>Wikipedia, the free encyclopedia</a>.<br/>
                                This service uses API of Wikipedia and <a href='https://en.wikipedia.org/wiki/Wikidata'>Wikidata</a>.<br />
                                Singularization of category titles is done by <a href='https://humanizr.net/'>Humanizer</a>.";
            searchResult.Text = aboutText;
        }

        [System.Web.Services.WebMethodAttribute(), System.Web.Script.Services.ScriptMethodAttribute()]
        public static string[] GetCompletionList(string prefixText, int count, string contextKey)
        {
            string enWpApiStartsWithUrl = "https://en.wikipedia.org/w/api.php?action=query&generator=allpages&gaplimit=" + count + "&format=xml&gapfrom=";
            List<string> resultList = new List<string>();
            string prefix = prefixText;
            if (!String.IsNullOrEmpty(prefix))
            {
                enWpApiStartsWithUrl += prefix.Replace(" ","_");
                XmlDocument resultXML = new XmlDocument();
                resultXML.Load(enWpApiStartsWithUrl);
                XmlElement root = resultXML.DocumentElement;
                XmlNodeList pages = resultXML.SelectNodes("/api/query/pages/page");
                foreach (XmlNode page in pages)
                {
                    string title = page.Attributes["title"].Value;
                    if(!String.IsNullOrEmpty(title)) resultList.Add(title);
                }
            }
            string[] result = resultList.ToArray();
            return result;
        }
    }
}