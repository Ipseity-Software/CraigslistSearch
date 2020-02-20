using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace CraigslistSearch
{
    class Program
    {
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
            public bool Valid => !string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Description);
            public override string ToString() => Title;
        }
        static void Main()
        {
            List<CLEntry> results = new List<CLEntry>();
            int complete = 0;
            Console.WriteLine($"Searching {Craigslist.Endpoints.Length} locations...");
            foreach (string endpoint in Craigslist.Endpoints)
            { 
                results.AddRange(Craigslist.Search(endpoint).Content.Split("<item rdf").Select(rssItem => new CLEntry
                {
                    Description = rssItem.GrabBetween("<description>", "</description>"),
                    Title = rssItem.GrabBetween("<title>", "</title>"),
                    URL = rssItem.GrabBetween("<link>", "</link>")
                }));
                Console.Write("\r                                                                                                \r");
                Console.Write($"{++complete}/{Craigslist.Endpoints.Length} ({complete * 100 / Craigslist.Endpoints.Length}%)");
            }
            Console.WriteLine("\nProcessing...");
            string[] keywords, banned;
            keywords = File.ReadAllLines(ConfigurationManager.AppSettings["Keywords"]);
            try
            {
                banned = File.ReadAllLines(ConfigurationManager.AppSettings["BannedWords"]);
            }
            catch (FileNotFoundException)
            {
                banned = null;
            }
            results = results.Where(x => x.Valid && keywords.ContainsIns(x.Search) && !(banned?.ContainsIns(x.Title) ?? false)).Distinct(new CLEntry.Compr()).ToList();
            foreach (CLEntry result in results)
                Console.WriteLine($"{result.Title}\n\t{result.URL}");
            if (ConfigurationManager.AppSettings["EnableEmail"] == "true")
                SendEmail(results);
        }
        static void SendEmail(List<CLEntry> clresults)
        {
            string body = string.Join("\n\n", clresults.Select(x => $"{x.Title}\n{x.URL}"));

            NetworkCredential credential = new NetworkCredential(ConfigurationManager.AppSettings["EmailAddr"], ConfigurationManager.AppSettings["EmailPasswd"]);
            using (SmtpClient client = new SmtpClient(ConfigurationManager.AppSettings["SMTPServer"])
            {
                UseDefaultCredentials = false,
                EnableSsl = true,
                Port = int.Parse(ConfigurationManager.AppSettings["SMTPPort"]),
                Credentials = credential
            })
            {
                using (MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(ConfigurationManager.AppSettings["EmailAddr"])
                })
                {
                    mailMessage.To.Add(ConfigurationManager.AppSettings["EmailAddr"]);
                    mailMessage.Body = body;
                    mailMessage.Subject = "CL Report";
                    client.Send(mailMessage);
                }
            }
        }
    }
}
