using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
namespace CraigslistSearch
{
    class Program
    {
        private static string[] keywords, banned;
        public struct CLEntry
        {
            public class Compr : IEqualityComparer<CLEntry>
            {
                public bool Equals(CLEntry i1, CLEntry i2) => i1.URL == i2.URL;
                public int GetHashCode(CLEntry i) => i.URL.GetHashCode();
            }
            public string Title { get; set; }
            public string Description { get; set; }
            public string URL { get; set; }
            public string Search => $"{Title} {Description}";
            public override string ToString() => Title;
        }
        public Program()
        {
            keywords = File.ReadAllLines(ConfigurationManager.AppSettings["Keywords"]);
            banned = File.ReadAllLines(ConfigurationManager.AppSettings["BannedWords"]);
        }
        static void Main()
        {
            List<CLEntry> results = new List<CLEntry>();
            System.Console.WriteLine($"Searching {Craigslist.Endpoints.Length} locations...");
            int complete = 0;
            foreach (string endpoint in Craigslist.Endpoints)
            { 
                results.AddRange(Craigslist.Search(endpoint).Content.Split("<item rdf").Select(rssItem => new CLEntry
                {
                    Description = rssItem.GrabBetween("<description>", "</description>"),
                    Title = rssItem.GrabBetween("<title>", "</title>"),
                    URL = rssItem.GrabBetween("<link>", "</link>")
                }));
                System.Console.Write("\r                                                                                                \r");
                System.Console.Write($"{++complete}/{Craigslist.Endpoints.Length} ({complete * 100 / Craigslist.Endpoints.Length}%)");
            }
            System.Console.WriteLine(string.Empty);
            System.Console.WriteLine("Processing...");
            results = results.Where(x => !string.IsNullOrEmpty(x.Title) && !string.IsNullOrEmpty(x.Description) && keywords.ContainsIns(x.Search) && !banned.ContainsIns(x.Title)).Distinct(new CLEntry.Compr()).ToList();
            System.Console.WriteLine($"Found {results.Count} results");
#if !DEBUG
            SendEmail(results);
#endif
        }
#if !DEBUG
        static void SendEmail(List<CLEntry> clresults)
        {
            string body = string.Join("\n\n", clresults.Select(x => $"{x.Title}\n{x.URL}"));
            try
            {
                EASendMail.SmtpMail oMail = new EASendMail.SmtpMail("TryIt")
                {
                    From = ConfigurationManager.AppSettings["EmailAddr"],
                    To = ConfigurationManager.AppSettings["EmailAddr"],
                    Subject = "CL Report",
                    TextBody = body
                };
                EASendMail.SmtpServer oServer = new EASendMail.SmtpServer(ConfigurationManager.AppSettings["SMTPServer"])
                {
                    User = ConfigurationManager.AppSettings["EmailAddr"],
                    Password = ConfigurationManager.AppSettings["EmailPasswd"],
                    Port = 587,
                    ConnectType = EASendMail.SmtpConnectType.ConnectSSLAuto
                };
                EASendMail.SmtpClient oSmtp = new EASendMail.SmtpClient();
                oSmtp.SendMail(oServer, oMail);
            }
            catch (System.Exception ep)
            {
                System.Console.WriteLine("failed to send email with the following error:");
                System.Console.WriteLine(ep.Message);
            }
        }
#endif
    }
}
