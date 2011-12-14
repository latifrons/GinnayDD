﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ginnay.ProxySpider;
using Ginnaydd;
using Ginnaydd.Distributed;
using Ginnaydd.Distributed.ProxyPool;
using Ginnaydd.Distributed.TaskPool;
using TaobaoSpider;

namespace GinnayddGUI
{
	public class Instance
	{
		public WorkerManager workerManager;
		public IntervalProxyPool intervalProxyPool;
		public SQLServerTaskPool sqlServerTaskPool;
		public TaobaoTaskGuide taobaoTaskGuide;
		public string storePath = @"E:\Dev\Grab4\";
		private const string dbConnectionString = @"Data Source=localhost;Initial Catalog=TaobaoGrab;User Id=wim;Password=wimlab;";
		private ProxyManager proxyManager;

		public Instance()
		{
			workerManager = new WorkerManager();
			intervalProxyPool = new IntervalProxyPool();
			sqlServerTaskPool = new SQLServerTaskPool();
			sqlServerTaskPool.ConnectionString = dbConnectionString;
			sqlServerTaskPool.InitTable();
			sqlServerTaskPool.DaemonRestTime = 2000;
			sqlServerTaskPool.MaxDaemonEnqueueThreadCount =2;
			

			taobaoTaskGuide = new TaobaoTaskGuide(storePath,dbConnectionString);
			taobaoTaskGuide.GlobalTimeout = 60000;
			taobaoTaskGuide.MaxWorkers = 1;

			proxyManager = new ProxyManager();
			proxyManager.DaemonRestTime = 1000;
			proxyManager.LoadProxyProviders(ProxyProviderParser.ReadConfig("config/proxyprovider.xml"));
			proxyManager.ProxyValidator = new ProxyValidator();
			proxyManager.ProxyValidator.LoadProxyValidations(ProxyValidateConditionParser.ReadConfig("config/proxyvalidate.xml"));
			proxyManager.MaxValidateThreadCount = 20;

			workerManager.ProxyPool = intervalProxyPool;
			workerManager.TaskPool = sqlServerTaskPool;
			workerManager.TaskGuide = taobaoTaskGuide;

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
			taobaoTaskGuide.StartProcess();
		}
		public void Stop()
		{
			workerManager.StopAll();

			sqlServerTaskPool.StopDaemon();
			intervalProxyPool.StopDaemon();
			proxyManager.StopDownloadProxies();
			proxyManager.StopValidateProxies();
			proxyManager.StopDaemon();
			taobaoTaskGuide.StopProcess();
			//write all tasks in the memory to the sqlite
			sqlServerTaskPool.Flush();
		}

		public int GetWorkerCount()
		{
			return this.workerManager.GetWorkCount();
		}

		internal int GetProcessQueueCount()
		{
			return taobaoTaskGuide.ProcessQueue.Count;
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
