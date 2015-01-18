using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;


namespace ICEsTATUS
{
    class HttpHelper
    {
        public const string LOGIN_PAGE = @"https://www.ice.no/mypages/Default.aspx";
        public const string PING_PAGE = "google.no";
        private static CookieContainer _cookieContainer;
        private static string[] _htmlIds = {"__EVENTTARGET", "__EVENTARGUMENT", "__VIEWSTATE", "__EVENTVALIDATION"};
        public const string UsedQuota = "usedQuota";
        public const string MaxQuota = "maxQuota";


        public static Dictionary<string,string> GetCapacityUsage(string username, string password)
        {
            var htmlValues = GetHtmlState(LOGIN_PAGE);
            if (htmlValues.Count <= 0)
                return htmlValues;

            var postString = BuildPostString(username, password, htmlValues);

            // Do the post and get response
            var htmlDoc = DoPostOperation(postString, LOGIN_PAGE, LOGIN_PAGE);

            if (htmlDoc == null)
                return null;

            // Parse Response
            var quotaValues = ParseQuotaUsage(htmlDoc);

            return quotaValues;
        }

        private static Dictionary<string, string> ParseQuotaUsage(HtmlDocument htmlDoc)
        {
            var usedQuota = htmlDoc.DocumentNode.SelectNodes("//span[contains(@class, 'abo_antall')]");
            var maxQuota = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'abo_tekst')]");

            if (usedQuota == null || maxQuota == null)
                return null;
            if (usedQuota.Count != 1 || maxQuota.Count != 1)
                return null;

            var retDic = new Dictionary<string, string>();
            retDic.Add(HttpHelper.UsedQuota, usedQuota[0].InnerText.Replace(" ", "").Replace("\r\n", ""));
            retDic.Add(HttpHelper.MaxQuota, maxQuota[0].InnerText.Replace(" ", "").Replace("\r\n", "").Replace("av", "").Replace("Â", "").Replace("MB", ""));

            return retDic;
        }


        private static string BuildPostString(string username, string password, Dictionary<string, string> htmlValues)
        {
            var sb = new StringBuilder();
            foreach (var key in htmlValues.Keys)
            {
                sb.Append(key + "=" + HttpUtility.UrlEncode(htmlValues[key]) + "&");
            }
            sb.Append(HttpUtility.UrlEncode("ctl00$ContentPlaceHolder1$tbUserID") + "=" + HttpUtility.UrlEncode(username) + "&");
            sb.Append(HttpUtility.UrlEncode("ctl00$ContentPlaceHolder1$tbPIN") + "=" + HttpUtility.UrlEncode(password) + "&");
            sb.Append(HttpUtility.UrlEncode("ctl00$ContentPlaceHolder1$lbtnLogin") + "=Logg+inn&");
            sb.Append("tf_name=&");
            sb.Append("tf_email=&");
            sb.Append("tf_friend_name=&");
            sb.Append("tf_friend_email=&");
            sb.Append("tf_message=");

            var retVal = sb.ToString();
            return sb.ToString();

            //return 
            //    "__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE=%2FwEPDwUKLTQyNjc5NDYzNw9kFgJmD2QWAgIDD2QWBgIFD2QWBAIJDw8WAh4EVGV4dAUHTG9nZyB1dGRkAg0PDxYCHgtOYXZpZ2F0ZVVybAUOfi9EZWZhdWx0LmFzcHhkZAIHD2QWBgIBDw8WAh8ABRVMb2dnIGlubiBww6UgTWluIFNpZGVkZAIFD2QWCAIDDw8WAh8ABRJCcnVrZXJuYXZuIChlcG9zdClkZAIHDw8WAh8ABQdQYXNzb3JkZGQCCQ9kFgJmDw8WAh8ABQ5HbGVtdCBwYXNzb3JkP2RkAg8PDxYCHwAFCExvZ2cgaW5uZGQCBw9kFgYCAw9kFgICAQ9kFgJmDxAPFgIfAAUNRW5kcmUgcGFzc29yZGRkZGQCBA9kFgJmD2QWAmYPDxYCHwAFDE55dHQgcGFzc29yZGRkAgUPZBYCZg9kFgJmDw8WAh8ABQ5HamVudGEgcGFzc29yZGRkAgkPZBYCZg8PFgIfAQViaHR0cDovL2t1bmRlc2VydmljZS5pY2Uubm8vUHJpdmF0L0t1bmRlc2VydmljZS9TcCVDMyVCOHJzbSVDMyVBNWxvZ3N2YXIvVGVrbmlza3N1cHBvcnRXbGFuUjkwLmFzcHhkZGT%2FrzDXMkLtwSzTP7ESrSYF8ZwOdQ%3D%3D&__EVENTVALIDATION=%2FwEWCAKayL3HAQLP4LeoDwKrpqfhAgKl7r6gCwLNjPOlCQKgiM%2BoDAKP6PmbAQKimuCLCDP1MVQtlr0PkfogjreOreXvkExN&ctl00%24ContentPlaceHolder1%24tbUserID=sigvefast88%40yahoo.no&ctl00%24ContentPlaceHolder1%24tbPIN=88jodiProd&ctl00%24ContentPlaceHolder1%24lbtnLogin=Logg+inn&tf_name=&tf_email=&tf_friend_name=&tf_friend_email=&tf_message=";
              
        }

        static string UrlEncodeUpperCase(string value)
        {
            value = HttpUtility.UrlEncode(value);
            return Regex.Replace(value, "(%[0-9a-f][0-9a-f])", c => c.Value.ToUpper());
        }

        // Performs a POST-command
        private static HtmlDocument DoPostOperation(string postString, string url, string referer)
        {
            try
            {
                // Create a request using a URL that can receive a post. 
                var request = (HttpWebRequest)WebRequest.Create(url);

                // Set cookie container so it is populated when making the request
                request.CookieContainer = _cookieContainer ?? new CookieContainer();

                // Set the Method property of the request to POST.
                request.Method = WebRequestMethods.Http.Post;

                // Create POST data and convert it to a byte array.
                byte[] byteArray = Encoding.ASCII.GetBytes(postString);

                // Set the headewr values
                request.Host = "www.ice.no";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:8.0) Gecko/20100101 Firefox/8.0";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                request.Headers.Add("Accept-Language", "en-us,en;q=0.5");
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
                request.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
                request.ContentType = "application/x-www-form-urlencoded";
                request.Referer = referer;

                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;

                // Get the request stream.
                Stream dataStream = request.GetRequestStream();

                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);

                // Close the Stream object.
                dataStream.Close();

                // Get the response.
                var response = (HttpWebResponse)request.GetResponse();

                // Display the status.
                Console.WriteLine(response.StatusDescription);

                // Get the stream containing content returned by the server.
                var htmlDoc = new HtmlDocument();
                htmlDoc.Load(response.GetResponseStream());

                response.Close();

                return htmlDoc;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static Dictionary<string,string> GetHtmlState(string url)
        {
            var htmlDocument = DoGetOperation(url, null);
            var viewState = ParseHtmlState(htmlDocument);

            return viewState;
        }

        private static Dictionary<string,string> ParseHtmlState(HtmlDocument htmlDocument)
        {
            var dictionary = new Dictionary<string, string>();

            if (htmlDocument == null)
                return dictionary;


            for (int i = 0; i < _htmlIds.Length; i++)
            {
                var nodes = htmlDocument.DocumentNode.SelectNodes("//input[contains(@id, '"+ _htmlIds[i] +"')]");
                if (nodes == null)
                {
                    dictionary.Add(_htmlIds[i], string.Empty);
                    continue;
                }
                if (nodes.Count != 1)
                    throw new Exception("HttpHelper: ParseHtmlState() Could not find html state node \""+_htmlIds[i]+"\"");

                foreach (var node in nodes)
                {
                    foreach (var attr in node.Attributes)
                    {
                        if (attr.Name.Equals("value"))
                            dictionary.Add(_htmlIds[i], attr.Value);
                    }
                }
            }
            return dictionary;
        }

        // Perfomrs a GET-command
        private static HtmlDocument DoGetOperation(string url, CookieContainer cookies)
        {
            // Create a request using a URL that can do a GET.
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = cookies ?? new CookieContainer();
            request.Method = WebRequestMethods.Http.Get;

            try
            {
                var response = (HttpWebResponse) request.GetResponse();
                var htmlDocument = new HtmlDocument();
                htmlDocument.Load(response.GetResponseStream());

                _cookieContainer = new CookieContainer();
                _cookieContainer.Add(response.Cookies);

                return htmlDocument;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        // Do a ping to check if connected to internet
        public static bool IsOnline(string host)
        {
            if (host.Length > 0)
            {
                Ping pingSender = new Ping();
                PingOptions options = new PingOptions();
                options.DontFragment = true;
                // Create a buffer of 32 bytes of data to be transmitted.  
                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                const int timeout = 120;
                try
                {
                    PingReply reply = pingSender.Send(host, timeout, buffer, options);
                    if (reply != null)
                        if (reply.Status == IPStatus.Success)
                            return true;        //Ping was successful

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

            return false;               // Ping failed
        }
    }
}
