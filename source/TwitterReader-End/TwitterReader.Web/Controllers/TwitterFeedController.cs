using Microsoft.ApplicationServer.Caching;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TweeterReader.Web.Entities;
using TweetReader.Web.Repositories;

namespace TweetReader.Web.Controllers
{
    public class TwitterFeedController : Controller
    {
        QueueClient mClient;

        public TwitterFeedController()
        {
            mClient = QueueClient.CreateFromConnectionString(
                CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString"), "ProcessingQueue");
        }

        public ActionResult Index(string name)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            DataCache cache = new DataCache();
            Tweets entries = cache.Get(name) as Tweets;
            if (entries == null)
            {
                entries = TwitterFeed.GetTweets(name);
                cache.Add(name, entries);
                mClient.Send(new BrokeredMessage(name));
            }

            timer.Stop();
            ViewBag.LoadTime = timer.Elapsed.TotalMilliseconds;

            return View(entries);
        }

        Tweets getTweets(string name)
        {
            return TwitterFeed.GetTweets(name);
        }

        #region retrieve hot topics
        List<TopicModel> getHotTopics()
        {
            CloudTable table = getCloudTable();

            TableQuery<TopicEntity> query = new TableQuery<TopicEntity>().Where
                (TableQuery.GenerateFilterCondition("PartitionKey",
                                                     QueryComparisons.Equal,
                                                     DateTime.Now.ToString("yyyy-MM-dd")));

            List<TopicModel> topics = new List<TopicModel>();
            foreach (TopicEntity entity in table.ExecuteQuery(query))
            {
                topics.Add(new TopicModel(entity.Topic, entity.Mentions));
            }
            topics.Sort();
            return topics.Take(50).ToList();
        }
        
        public ActionResult HotTopics()
        {
            return PartialView(getHotTopics());
        }
        #endregion

        #region update hot topics
        CloudTable getCloudTable()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("hottopics");
            table.CreateIfNotExists();
            return table;
        }

        void updateHotTopics(string name)
        {
            Tweets tweets = getTweets(name);
            
            if (tweets != null)
            {
                string pattern = "(#|@)[a-zA-Z0-9_]{1,15}";
                Regex regx = new Regex(pattern);

                CloudTable table = getCloudTable();
                foreach (var tweet in tweets.Items)
                {
                    string date = DateTime.Now.ToString("yyyy-MM-dd");
                    foreach (var match in regx.Matches(tweet.text))
                    {
                        string matchString = match.ToString().Substring(1);
                        TableOperation retrieveOperation = TableOperation.Retrieve<TopicEntity>
                                        (date, matchString);
                        TableResult retrievedResult = table.Execute(retrieveOperation);
                        TopicEntity updateEntity = (TopicEntity)retrievedResult.Result;
                        if (updateEntity != null)
                        {
                            updateEntity.Mentions += 1;
                            TableOperation updateOperation = TableOperation.Replace(updateEntity);
                            table.Execute(updateOperation);
                        }
                        else
                        {
                            TopicEntity entity = new TopicEntity(date, matchString);
                            TableOperation insertOperation = TableOperation.Insert(entity);
                            table.Execute(insertOperation);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
