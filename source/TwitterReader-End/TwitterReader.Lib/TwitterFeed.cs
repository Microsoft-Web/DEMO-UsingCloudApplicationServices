using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using TwitterReader.Lib.Properties;

namespace TweetReader.Web.Repositories
{
    public class TwitterFeed
    {
        static Random rand = new Random();
        public static Tweets GetTweets(string screenName)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Tweets ret = new Tweets();
            TwitterTweets tweets = new TwitterTweets();
            ret.screen_name = screenName;
            try
            {
                bool requestSucceeded = false;
                try
                {                    
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://api.twitter.com/1/statuses/user_timeline.json?include_entities=true&screen_name=" + screenName + "&count=25&include_rts=1");
                    request.Timeout = 4000;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Twitter.entry>));
                    var outStream = response.GetResponseStream();
                    tweets.Items = serializer.ReadObject(outStream) as List<Twitter.entry>;
                    outStream.Close();
                    convertEntries(ret, tweets.Items);
                    requestSucceeded = true;
                }
                catch (Exception exp)
                {
                    Trace.WriteLine(exp.ToString());
                    requestSucceeded = false;
                }
                if (!requestSucceeded)
                {
                    try
                    {
                            MemoryStream stream = new MemoryStream(Resources.WindowsAzure);
                            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Twitter.entry>));
                            tweets.Items = serializer.ReadObject(stream) as List<Twitter.entry>;
                            stream.Close();
                            convertEntries(ret, tweets.Items);
                    }
                    catch (Exception exp)
                    {
                        Trace.WriteLine(exp.ToString());
                    }
                }
            }
            catch (Exception exp)
            {
                Trace.WriteLine(exp.ToString());
            }
            watch.Stop();
            if (watch.ElapsedMilliseconds < 1500)
                Thread.Sleep((int)(1500 + rand.Next(0,200) - watch.ElapsedMilliseconds));
            return ret;
        }
        private static void convertEntries(Tweets model, List<Twitter.entry> entries)
        {
            bool first = true;
            foreach (var tweet in entries)
            {
                if (first)
                {
                    model.name = tweet.user.name;
                    model.profile_image_url = tweet.user.profile_image_url;
                    model.screen_name = tweet.user.screen_name;
                    first = false;
                }
                model.Items.Add(new Twitter.entryModel
                {
                    created_at_display = convertDateString(tweet.created_at),
                    text = tweet.text
                });
            }
        }
        private static string convertDateString(string date)
        {
            if (string.IsNullOrEmpty(date))
                return "";
            try
            {
                DateTime postDate = DateTime.Now;
                if (DateTime.TryParseExact(date, "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out postDate))
                {
                    TimeSpan span = DateTime.Now - postDate;
                    if (span.TotalDays <= 1)
                    {
                        if (span.TotalHours <= 1)
                        {
                            if (span.TotalMinutes <= 1)
                                return ((int)span.TotalSeconds) + "s";
                            else
                                return ((int)span.TotalMinutes) + "m";
                        }
                        else
                            return ((int)span.TotalHours) + "h";
                    }
                    else
                        return postDate.ToString("dd MMM");
                }
                else
                    return "";
            }
            catch (Exception exp)
            {
                Trace.WriteLine(exp.ToString());
                return "";
            }
        }
    }
    public class TwitterTweets
    {
        public List<Twitter.entry> Items { get; set; }
        public TwitterTweets()
        {
            Items = new List<Twitter.entry>();
        }
        public string screen_name { get; set; }
    }
    public class Tweets
    {
        [DataMember]
        public List<Twitter.entryModel> Items { get; set; }
        public Tweets()
        {
            Items = new List<Twitter.entryModel>();
        }
        [DataMember]
        public string screen_name { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string profile_image_url { get; set; }

        public byte[] GetBytes()
        {
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Tweets));
            serializer.WriteObject(stream, this);
            var ret = stream.ToArray();
            stream.Close();
            return ret;
        }
        public static Tweets FromObject(object data)
        {
            if (data == null)
                return null;
            MemoryStream stream = new MemoryStream((byte[])data);
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Tweets));
            var ret = serializer.ReadObject(stream) as Tweets;
            stream.Close();
            return ret;
        }
    }
    namespace Twitter
    {
        [DataContract]
        public class entryModel
        {
            [DataMember]
            public string text { get; set; }
            [DataMember]
            public string created_at_display {get;set;}
        }
        [DataContract]
        public class entry
        {
            [DataMember]
            public string created_at { get; set; }
            [DataMember]
            public string text { get; set; }
            [DataMember]
            public user user { get; set; }
        }
        [DataContract]
        public class user
        {
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public string profile_image_url { get; set; }
            [DataMember]
            public string screen_name { get; set; }
        }
    }
}