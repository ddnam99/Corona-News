using RestSharp;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json;
using System.Net;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Corona_News
{
    class nCoV
    {
        private static string[] GetNews()
        {
            //Helper.ConsoleLogs("Corona News: Lấy thông tin nCoV...");

            string html = GetHTML("https://suckhoedoisong.vn/Virus-nCoV-cap-nhat-moi-nhat-lien-tuc-n168210.html");

            var pattern = "Cập nhật lúc(.*?)trường hợp\\.</li>";
            var htmlNews = Regex.Match(html, pattern, RegexOptions.Singleline).Value;

            var text = ConvertToText(htmlNews).Replace("\n\n", "\n").Replace("\n", "\n>").Replace(">*Việt Nam*", "\n>*Việt Nam*").Replace(">*Thế giới*", "\n>*Thế giới*");

            return text.Split("\n\n");
        }

        //public static string VietNam()
        //{
        //    Helper.ConsoleLogs("Cập nhật nCoV Việt Nam...");
        //    string html = GetHTML("https://ncov.moh.gov.vn/");

        //    var pattern = "<strong>VIỆT NAM(.*?)>Ngoài ra(.*?)</p>";
        //    var htmlNews = Regex.Match(html, pattern, RegexOptions.Singleline).Value;

        //    pattern = "Cập nhật đến(.*?)</strong>|Tử vong(.*?)</span></div>|Số trường hợp mắc(.*?)</p>\n</div>|Điều trị khỏi(.*?)</span></span>|Số trường hợp nghi ngờ(.*?)</div>\n</div>|Ngoài ra(.*?)</p>";
        //    var regex = Regex.Matches(htmlNews, pattern, RegexOptions.Singleline);

        //    var news = regex.Select(i => ConvertToText(i.Value).Replace("\n\n\n", " ").Replace(" \n\n", ". ").Replace("         ", " ").Replace("*", "-").Replace("\n\n", "\n").Replace(";. ", ";\n"));

        //    var text = string.Join("\n", news.ToArray()).Replace("\n", "\n>").Replace("Tử vong:", "Tình hình nCoV tại Việt Nam:\n>*Tử vong:*").Replace("Số trường hợp mắc:", "*Số trường hợp mắc:*").Replace("Điều trị khỏi:", "*Điều trị khỏi:*").Replace(">\n", "\n");

        //    return text;
        //}

        private static string Timeline()
        {
            //Helper.ConsoleLogs("Corona News: Lấy thông timeline nCoV Việt Nam...");

            string html = GetHTML("https://ncov.moh.gov.vn/");

            var pattern = "<div class=\"timeline-head\">(.*?)</p>";
            var htmlNews = Regex.Match(html, pattern, RegexOptions.Singleline).Value;

            var regex = Regex.Matches(htmlNews, "<h3>(?<time>.*?)</h3>|<p>(?<news>.*?)</p>", RegexOptions.Singleline);
            var time = regex[0].Groups["time"].Value;
            var news = regex[1].Groups["news"].Value;

            var text = $"*Cập nhật lúc {time}:*\n>>>{news}";

            return text;
        }

        private static void CheckNews()
        {
            var news = GetNews();
            var newsOld = new string[] { "", "", "" };

            if (File.Exists(Environment.CovidPath))
                newsOld = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(Environment.CovidPath, Encoding.UTF8));

            var message = news[0]; bool update = false;
            for (int i = 1; i < news.Count(); i++)
                if (news[i] != newsOld[i])
                {
                    message += $"\n\n{news[i]}";
                    update = true;
                }

            if (update)
            {
                File.WriteAllText(Environment.CovidPath, JsonConvert.SerializeObject(news, Formatting.Indented), Encoding.UTF8);
                Helper.ConsoleLogs($"Corona News: Đã có thông tin cập nhật. Sendding... {Helper.Message(message)}");
            }
            //else Helper.ConsoleLogs("Corona News: Không có thông tin nCoV cập nhật.");
        }

        public static void CheckTimeline()
        {
            var timeline = Timeline();
            var timelineOld = "";

            if (File.Exists(Environment.TimelinePath))
                timelineOld = JsonConvert.DeserializeObject<string>(File.ReadAllText(Environment.TimelinePath, Encoding.UTF8));

            if (!(timeline == timelineOld && File.Exists(Environment.TimelinePath)))
            {
                Helper.ConsoleLogs($"Corona News: Đã có timeline cập nhật. Sendding... {Helper.Message(timeline)}");
                File.WriteAllText(Environment.TimelinePath, JsonConvert.SerializeObject(timeline, Formatting.Indented), Encoding.UTF8);
            }
            //else Helper.ConsoleLogs("Corona News: Không có timeline Việt Nam cập nhật.");
        }

        public static void NotifyNews()
        {
            var taskCheckNews = new Task(() =>
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        CheckNews();
                        break;
                    }
                    catch (Exception e) { Helper.LogError($"Error: Check News...\n{e.ToString()}"); }
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                }
            });

            var taskChecTimeline = new Task(() =>
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        CheckTimeline();
                        break;
                    }
                    catch (Exception e) { Helper.LogError($"Error: Check Timeline...\n{e.ToString()}"); }
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                }
            });

            taskCheckNews.Start();
            taskChecTimeline.Start();

            Task.WaitAll(taskCheckNews, taskChecTimeline);
        }

        private static string ConvertToText(string html)
        {
            html = html.Replace("\n", "").Replace("\r", "").Replace("</p>", "\n").Replace("<li>", "\n").Replace("<br />", "\n");

            html = RemoveTags(html);

            var text = html.Replace("Việt Nam:", "*Việt Nam*:").Replace("Thế giới", "*Thế giới*").Replace("  ", " ").Replace(":-", ":\n-");

            return text;
        }

        private static string RemoveTags(string html)
        {
            var regex = Regex.Matches(html, "<(.*?)>", RegexOptions.Singleline);

            foreach (Match i in regex)
                html = html.Replace(i.Value, "");

            return html;
        }

        private static string GetHTML(string url)
        {
            var client = new RestClient(url);

            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            return WebUtility.HtmlDecode(response.Content);
        }
    }
}