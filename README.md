﻿DEMO-UsingCloudApplicationServices
==================================
<a name="outline" />
## Outline ##
- **Demonstrate how to use Windows Azure Caching to improve Cloud Service performance**

    We will start with a simple Twitter Reader application that reads latest tweets from selected Twitter handle. We'll use Windows Azure Caching to cache last retrieved tweets to improve application performance.

- **Demonstrate basic usage of Windows Azure Storage**

    We'll add a feature of tracking current hot topics. We'll read mentioned Twitter handles and save statistic data (# of mentions per day) to Windows Azure table storage. Then we'll extend the UI to display the data.

    ![hottopics](images/hottopics.png?raw=true)
- **Demonstrate how to extend a Cloud Service to use n-Tiered architecture**

    We'll move hot topic analysis to a backend worker role.

- **Demonstrate how to deploy and scale Cloud Services**

    We'll show a deployed version of the service and show how to use NewRelic to monitor application performance.

    > **Note:** This is a fairly large demo. You can use different versions of the source code to jump forward. **TwitterReader-Caching** contains caching implementation. **TwitterReader-Storage** contains both caching and storage implementations. And finally **TwitterReader-End** contains the completed n-Tier service.

<a name="demo-preparation" />
## Demo Preparation ##
- While the caching and storage part can work on a totally disconnected system, the n-Tier part requires Internet connectivity to access Service Bus.
- You need to deploy the service beforehand to show scaling and/or NewRelic.
- If you want to show NewRelic with performance data, you'll need to provision NewRelic beforehand and generate some loads before the demo as it takes a couple of minutes for the static data to show up.
- You should have your Service Bus namespace provisioned with a **ProcessingQueue** queue.

<a name="windows-azure-caching" />
## Windows Azure Caching ##
1. Start with **code\TwitterReader\TwitterReader.sln**.
1. **F5** to launch application. Click on the right arrow icon on home page to load tweets.
1. Observe slow response time.
1. Stop application.
1. Double-click on **TwitterReader.Cloud\Roles\TwitterReader.Web**.
1. In Property window, click on **Caching** tab. Then click on **Enable Caching** checkbox.
1. Type **Ctrl+S** to save changes.
1. Right-click on **TwitterReader.Web** project and select **Manage NuGet Packages...** menu.
1. Search for **Microsoft.WindowsAzure.Caching**.
1. Install the package.
1. Update **TwitterReader.Web\Web.config** to replace **[cache cluster role name]** with **TwitterReader.Web**.
    
    > **Note:** You can optionally enable local cache by uncommenting **localCache** element.

1. Open **TwitterFeedController.cs** under **Controllers** folder.
1. Modify **getTweets()** method

    ````
Tweets getTweets(string name)
{
      DataCache cache = new DataCache();
      Tweets entries = cache.Get(name) as Tweets;
      if (entries == null)
      {
          entries = TwitterFeed.GetTweets(name);
          cache.Add(name, entries);
      }
      return entries;
}
````
1. Resolve namespace for DataCache.
1. **F5** to launch again.
1. The initial load is just as slow. Refreshing browser yeilds much faster as cache kicks in.

<a name="windows-azure-storage" />
## Windows Azure Storage ##
1. Double-click on **TwitterReader.Cloud\Roles\TwitterReader.Web**.
1. Go to **Settings** tab.
1. Click **Add Setting** link.
1. Enter **StorageConnectionString** as Name; change Type to **Connection String**; Use [**...**] button to enter a connection string.

    > **Note:** You can use development storage account.    

1. Right-click on **TwitterReader.Web** project and select **Manage NuGet Packages...** menu.
1. Search and insall **WindowsAzure.Storage** package.
1. Add **TwitterFeedController.cs** from **code\Assets** folder to replace the one under **Controllers** folder.

    > **Note:** The updated controller contains code for both saving data to table storage and retriving data back. You can set up code snippets if you want to, but for the sake of time proabaly replacing the whole file is easier.

1. Walk through the code. Point out that the new **Index()** method calls out to **updateHotTopics()** when new tweets are read.

1. Navigate to **updateHotTopics()** method. The code runs a regular expression against each tweet, retrieve all mentioned Twitter handles, and save number of mentions (by date) to Windows Azure table storage.

    > **Note:** If you have more time, go through more details such as how to get a reference to a table, and how to insert records, etc. Also walk through the definition of TopicEntity, which is defined in **TwitterReader.Lib** roject.  
    > **Note:** The code here doesn't handle transient errors for simplicity. You should point this out to audience.
1. **[OPTIONAL]** Launch the application after above changes. You can use Server Explorer to browse generated records. 

    > **Note:** This step is also a sanity check to make sure storage is working before you move to the next step.
1. Add **HotTopics.cshtml** from **code\Assets** folder to **Views\TwitterFeed** folder.

    > **Note:** This is the partial view to display hot topics.

1. Open **Views\TwitterFeed\Index.cshtml** and add the following code right above the **script** element:

    ````HTML
<div>
        @{Html.RenderAction("HotTopics");}
</div>
````
    ![partial](images/partial.png?raw=true)
1. **F5** to launch the application. Try several different Twitter handles and see screen refreshes.

<a name="n-tier" />
## n-Tier ##
1. Right-click on **TwitterReader.Cloud\Roles** and select **Add->New Worker Role Project...***, pick **Worker Role with Service Bus Queue** template, and name the new project **TwitterReader.Worker**.
1. **Ctrl+S** to save everything.
1. Add both **Windows Azure Storage** and **Windows Azure Caching** NuGet package to the worker role.
1. Remove 
````C#
using Microsoft.WindowsAzure.StorageClient;
```` 
From WorkerRole.cs.

    > **Note:** We are using 2.0 library, which has a different namespace as captured by the Service Bus template.
1. Build to make sure everything is fine.
1. Add a reference to **TwitterReader.Lib** project.
1. Open **App.config** file and replace **[cache cluster role name]** with **TwitterReader.Web**.
1. Double-click on **TwitterReader.Cloud\Roles\TwitterReader.Worker**, go to **Settings** tab, and paste in your Service Bus connection string as the value of **Microsoft.ServiceBus.ConnectionString**.
1. Click **Add Setting** link.
1. Enter **StorageConnectionString** as Name; change Type to **Connection String**; Use [**...**] button to enter a connection string.

    > **Note:** You can use development storage account.    

1. Copy **update hot topics** region from **TwitterReader.Web\Controllers\TwitterFeedController.cs** to **TwitterReader.Worker\WorkerRole.cs**.

    > **Note:** This is the part where processing logics are migrated to backend.
1. Edit **updateHotTopics()** method. Replace 

    ````C#
Tweets tweets = getTweets(name);
````
with
 
    ````C#
DataCache cache = new DataCache();
var tweets = cache.Get(name) as Tweets;
````
1. Now resolve all namespaces and compile.
1. Back in **Run()** method. Add 

    ````C#
updateHotTopics(receivedMessage.GetBody<string>());
````
 before

    ````C#
receivedMessage.Complete();
````

    > **Note:** Now the worker is ready to process the message. Let's go back to web role to send the message.

1. Back in **TwitterReader.Web**, add a reference to **Windows Azure Service Bus** NuGet package.
1. Modify **Web.config** file and paste in your Service Bus connection as value of **Microsoft.ServiceBus.ConnectionString**.
1. Open **Controllers\TwitterFeedController.cs**.
1. Add queue client initialization code

    ````C#
QueueClient mClient;
public TwitterFeedController()
{
    mClient = QueueClient.CreateFromConnectionString(
        CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString"), "ProcessingQueue");
}
````
1. In **Index()** method, instead of calling **updateHotTopics()** method, call

    ````C#
mClient.Send(new BrokeredMessage(name));
````
1. **F5** to launch the app. 
    > **Note:** You can set break points in worker role such as in **updateHotTopics()** method to show the messages coming through.



