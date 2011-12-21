using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ginnay.ProxySpider;
using Ginnaydd;
using Ginnaydd.Distributed;
using Ginnaydd.Distributed.ContentProcessor;
using Ginnaydd.Distributed.ProxyPool;
using Ginnaydd.Distributed.TaskPool;

namespace GinnayddGUI
{
	public class Instance
	{
		public WorkerManager workerManager;
		public IntervalProxyPool intervalProxyPool;
		public SQLServerTaskPool sqlServerTaskPool;
		public AmazonPictureTaskGuide amazonPictureTaskGuide;
		public string storePath = @"E:\Dev\Grab4\";
//		private const string dbConnectionString = @"Data Source=localhost;Initial Catalog=AmazonGrab;User Id=wim;Password=wimlab;";
		private ProxyManager proxyManager;

		public Instance()
		{
			workerManager = new WorkerManager();
			intervalProxyPool = new IntervalProxyPool();
			sqlServerTaskPool = new SQLServerTaskPool();
//			sqlServerTaskPool.ConnectionString = dbConnectionString;
			sqlServerTaskPool.InitTable();
			sqlServerTaskPool.DaemonRestTime = 2000;
			sqlServerTaskPool.MaxDaemonEnqueueThreadCount =2;
			

			amazonPictureTaskGuide = new AmazonPictureTaskGuide(storePath);
			amazonPictureTaskGuide.GlobalTimeout = 60000;
			amazonPictureTaskGuide.MaxWorkers = 1;

			proxyManager = new ProxyManager();
			proxyManager.DaemonRestTime = 1000;
			proxyManager.LoadProxyProviders(ProxyProviderParser.ReadConfig("config/proxyprovider.xml"));
			proxyManager.ProxyValidator = new ProxyValidator();
			proxyManager.ProxyValidator.LoadProxyValidations(ProxyValidateConditionParser.ReadConfig("config/proxyvalidate.xml"));
			proxyManager.MaxValidateThreadCount = 20;

			workerManager.ProxyPool = intervalProxyPool;
			workerManager.TaskPool = sqlServerTaskPool;
			workerManager.TaskGuide = amazonPictureTaskGuide;

			intervalProxyPool.ProxyManager = proxyManager;
			intervalProxyPool.DaemonRestTime = 1000;
			intervalProxyPool.ProxyRestTime = new TimeSpan(0, 0, 20);
		}
		public void Start()
		{
			workerManager.StartAll();
			sqlServerTaskPool.StartDaemon();
			intervalProxyPool.StartDaemon();
			proxyManager.StartDaemon();
			amazonPictureTaskGuide.StartProcess();
		}
		public void Stop()
		{
			workerManager.StopAll();

			sqlServerTaskPool.StopDaemon();
			intervalProxyPool.StopDaemon();
			proxyManager.StopDownloadProxies();
			proxyManager.StopValidateProxies();
			proxyManager.StopDaemon();
			amazonPictureTaskGuide.StopProcess();
			//write all tasks in the memory to the sqlite
			sqlServerTaskPool.Flush();
		}

		public int GetWorkerCount()
		{
			return this.workerManager.GetWorkCount();
		}

//		public int GetPicCount()
//		{
//			return sqlServerTaskPool.CountStateType((int)TaskState.FINISHED, (int)AmazonTaskType.IMAGE);
//		}

//		internal int GetTaskDBCount()
//		{
//			return sqlServerTaskPool.CountState((int) TaskState.NOT_FINISHED);
//		}
//
//		internal int GetTaskFinishedCount()
//		{
//			return sqlServerTaskPool.CountState((int)TaskState.FINISHED);
//		}
		internal int GetProcessQueueCount()
		{
			return amazonPictureTaskGuide.ProcessQueue.Count;
		}
		internal int GetTaskMemoryCount()
		{
			return sqlServerTaskPool.TaskQueue.Count;
		}
		internal int GetTaskEnqueuingCount()
		{
			return sqlServerTaskPool.EnqueueQueue.Count;
		}
		internal int GetFetchingWorkerCount()
		{
			return workerManager.FetchingWorkerCount;
		}
		public int[] GetProxyPoolCount()
		{
			return new int[]{intervalProxyPool.ReadyCount,intervalProxyPool.RestCount,proxyManager.GoodProxiesQueue.Count,proxyManager.PendingProxies.Count};
		}
	}
}
