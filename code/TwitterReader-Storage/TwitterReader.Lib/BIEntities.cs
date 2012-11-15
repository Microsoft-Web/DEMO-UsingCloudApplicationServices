using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TweeterReader.Web.Entities
{
    public class TopicEntity : TableEntity
    {
        public TopicEntity(string date, string topic)
        {
            this.PartitionKey = date;
            this.RowKey = topic;
            this.Mentions = 1;
            this.Topic = topic;
        }
        public TopicEntity() { }
        public string Topic { get; set; }
        public int Mentions { get; set; }
    }
    public class TopicModel:IComparable
    {
        public TopicModel(string topic, int mentions)
        {
            Topic = topic;
            Mentions = mentions;
        }
        public string Topic { get; set; }
        public int Mentions { get; set; }

        public int CompareTo(object obj)
        {
            if (obj != null && obj is TopicModel)
                return -this.Mentions.CompareTo(((TopicModel)obj).Mentions);
            else
                throw new ArgumentException("Expected: TopicModel");
        }
    }
}