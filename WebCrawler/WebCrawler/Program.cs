using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            WebCrawler wc = new WebCrawler();

            WebClient wbc = new WebClient();
            string src = wbc.DownloadString("http://www.youtube.com/robots.txt");
            string[] lines = src.Split('\n');

            List<String> disallowedSites = wc.GetDisallowedSites(lines, "Xenu");
            List<String> frontier = wc.FetchUrlsFromSource("http://www.youtube.com");
            int x = 5;
        }
    }

    class WebCrawler
    {
        public List<String> GetDisallowedSites(string[] data, string myAgent)
        {
            List<String> disallowedSites = new List<String>();

            Dictionary<String, List<String>> siteList = new Dictionary<String, List<String>>();

            string agent = "";

            for(int i = 0; i < data.Length; i++)
            {
                Match match = Regex.Match(data[i], @"User-agent:\s(\S+)", RegexOptions.IgnoreCase);
               
                if(match.Success)
                {
                    agent = match.Groups[1].ToString();
                    siteList[agent] = new List<string>();
                }

                Match match2 = Regex.Match(data[i], @"Disallow:\s(\/\S*)", RegexOptions.IgnoreCase);

                if (match2.Success)
                {
                    siteList[agent].Add(match2.Groups[1].ToString());
                }
            }

            
            foreach (var item in siteList[myAgent])
            {
                disallowedSites.Add(item);
            }
            

            return disallowedSites;
        }

        public List<String> FetchUrlsFromSource(string site)
        {
            WebClient wc = new WebClient();
            string source = wc.DownloadString(site);

            // regex for finding links
            string rgx = @"href=""(\S+)""";
            MatchCollection linkMatches = Regex.Matches(source, rgx);

            List<String> frontier = new List<string>();

            foreach (var link in linkMatches)
            {
                Match m = (Match)link;
                frontier.Add(NormalizeUrl(m.Groups[1].ToString(), "http://www.youtube.com"));
                //Console.WriteLine(ExpandUrl(m.Groups[1].ToString(), "http://www.moodle.aau.dk"));
            }

            return frontier;
        }

        public string NormalizeUrl(string url, string rootDomain)
        {
            string expandedUrl = ExpandUrl(url, rootDomain);
            Console.WriteLine(expandedUrl);

            return "";
        }

        private string ExpandUrl(string url, string rootDomain)
        {
            string urlNew = url;

            if(url.Contains("//"))
            {
                // absolute address             
                // if not a full path e.g //www.example.com append "http:"
                if (url.Length >= 7)
                {
                    // no protocol found, append one
                    if(!url.Contains("://"))
                    {
                        urlNew = "http:" + url;
                    }
                }
            }
            else if (url.Length > 1)
            {
                // check whether the url is relative (e.g. /example/test) and does not contain a protocol
                if ((!url.Contains("://")) || url[1] != '/')
                {
                    urlNew = rootDomain + url;
                }
                else
                {
                    // a root domain is added to the url so it is made absolute 
                    return (rootDomain + url).ToLower();
                }
            }

            return urlNew;
        }

        private string setCase(string url)
        {
            // this function fixes protocol and domain cases


            int i;
            List<Char> tempCharList = new List<Char>();
            
            // set protocol and host to lower case
            for (i = 0; i < url.Length; i++)
            {
                if (Char.IsLetter(url[i]))
                {
                    tempCharList.Add(Char.ToLower(url[i]));
                }
                else
                {
                    // do nothing
                    tempCharList.Add(url[i]);
                }

                // determine the end of the domain. E.g .....com/ <---------
                if (i > 0 && url[i] == '/' && (url[i - 1] != ':' || url[i - 1] == '/'))
                {
                    break;
                }
            }

            
            // the -2 is to avoid an index out of bound exception. The code continues from after the domain
            for ( ; i < url.Length - 2; i++)
            {
                if (url[i] == '%')
                {
                    if (Char.IsLetter(url[i + 1]))
                        Char.ToUpper(url[i + 1]);
                    if (Char.IsLetter(url[i + 2]))
                        Char.ToUpper(url[i + 2]);
                }
            }
            
            string tempUrl = tempCharList.ToString();

            // replace octets
            tempUrl.Replace("%7E", "~");
            tempUrl.Replace("%2D", "-");
            tempUrl.Replace("%2E", ".");
            tempUrl.Replace("%5F", "_");

            // return url
            return tempUrl;
        }

    }
}
