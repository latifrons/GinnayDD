using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Ginnay.ProxySpider;
using Ginnaydd.Distributed.ProxyPool;

namespace Ginnaydd.Distributed
{
	public class WorkerManager
	{
		private int maxWorkerCount;
		private AbstractTaskPool taskPool;
		private AbstractProxyPool proxyPool;
		private AbstractTaskGuide taskGuide;
		private int fetchingWorkerCount = 0;
		object fetchingWorkerLock = new object();

		protected List<Worker> workingWorkers = new List<Worker>();

		//public event AllWorkDoneEventHandler OnAllWorkDone;
		public event TaskDoneEventHandler OnTaskDone;
		//public event TaskProcessDoneEventHandler OnTaskProcessDone;
		private TaskProcessDoneEventHandler taskProcessDoneEventHandler;

		object workerLocker = new object();
		

		private TaskDoneEventHandler taskDoneEventHandler;

		public int GetWorkCount()
		{
			lock (workingWorkers)
			{
				return workingWorkers.Count;
			}
		}

		public WorkerManager()
		{
			taskDoneEventHandler = new TaskDoneEventHandler(DoWorkerDone);
			taskProcessDoneEventHandler = new TaskProcessDoneEventHandler(DoTaskProcessDone);
//			OnAllWorkDone += new AllWorkDoneEventHandler(DoAllWorkDone);
//			OnTaskProcessDone += taskProcessDoneEventHandler;
		}

		private void DoWorkerDone(object sender, TaskDoneEventHandlerArgs args)
		{
			if (OnTaskDone != null)
			{
				OnTaskDone(sender, args);
			}
			if (args.TaskResult.Success)
			{
				if (!args.TaskResult.LoadFromLocal)
				{
					proxyPool.EnqueueProxy(args.TaskResult.ProxyInfo);
				}
				if (!args.TaskResult.NeedProcess)
				{
					//directly finish task
					taskPool.FinishTask(args.TaskResult.Task);
				}
			}
			else
			{
				if (!args.TaskResult.LoadFromLocal)
				{
					proxyPool.FailProxy(args.TaskResult.ProxyInfo);
				}
				//re-enqueue
				taskPool.FailTask(args.TaskResult.Task);
			}
		}

		private void DoTaskProcessDone(object sender, TaskProcessDoneEventHandlerArgs args)
		{
			if (args.ContentProcessResult.NewTasks != null)
			{
				taskPool.ProduceTasks(args.ContentProcessResult.NewTasks);
			}
			ProxyInfo pi = args.ProxyInfo;
			if (pi != null)
			{
				if (!args.ContentProcessResult.Success)
				{
					ProxyPool.FailProxy(pi);
				}
			}
			if (args.ContentProcessResult.Success)
			{
				//finish task
				taskPool.FinishTask(args.Task);
			}
			else
			{
				//file not ok
				string path = args.Task.LocalPath;
				if (taskGuide.ShouldStore(args.Task))
				{
					try
					{
						File.Delete(path);
						Console.WriteLine("MalFile deleted {0}", Path.GetFileName(path));
					}
					catch (IOException)
					{

					}
				}
				
				//re enqueue
				taskPool.FailTask(args.Task);
			}
		}

		private void DoAllWorkDone(object sender, AllWorkDoneEventHandlerArgs args)
		{
			//stop all 
		}

		public int MaxWorkerCount
		{
			get { return maxWorkerCount; }
			set { maxWorkerCount = value; }
		}

		public AbstractTaskGuide TaskGuide
		{
			get { return taskGuide; }
			set
			{
				if (taskGuide != null)
				{
					taskGuide.OnTaskProcessDone -= taskProcessDoneEventHandler;
				}
				taskGuide = value;
				taskGuide.OnTaskProcessDone += taskProcessDoneEventHandler;
			}
		}

		public AbstractProxyPool ProxyPool
		{
			get { return proxyPool; }
			set { proxyPool = value; }
		}

		public AbstractTaskPool TaskPool
		{
			get { return taskPool; }
			set { taskPool = value; }
		}

		public int FetchingWorkerCount
		{
			get { return fetchingWorkerCount; }
			set { fetchingWorkerCount = value; }
		}

		public void StartAll()
		{
			FitToMaxWoker();
		}
		public void FitToMaxWoker()
		{
			Monitor.Enter(workerLocker);
			int count = workingWorkers.Count;

			if (count < maxWorkerCount)
			{
				List<Worker> startWorkers = new List<Worker>();
				for (int i = count; i < maxWorkerCount; i++)
				{
					Worker w = new Worker(this);
					w.OnTaskDone += taskDoneEventHandler;
					w.TaskGuide = taskGuide;
					w.ProxyPool = ProxyPool;
					w.TaskPool = taskPool;
					workingWorkers.Add(w);
					startWorkers.Add(w);
				}
				foreach (Worker w in startWorkers)
				{
					w.CanStop = false;
					w.BeginWork();
				}
			}
			else
			{
				int eliminateCount = count-maxWorkerCount;
				for (int i = 0; i < eliminateCount; i++)
				{
					if (workingWorkers.Count != 0)
					{
						Worker w = workingWorkers[0];
						workingWorkers.RemoveAt(0);
						w.CanStop = true;
					}
				}
			}
			
			Monitor.Exit(workerLocker);
		}

		public void StopAll()
		{
			Monitor.Enter(workerLocker);
			foreach (Worker w in workingWorkers)
			{
				w.CanStop = true;
			}
			foreach (Worker w in workingWorkers)
			{
				w.WorkThread.Join(5000);
				w.WorkThread.Abort();
			}
			Monitor.Exit(workerLocker);
		}
		public void ReportFetching()
		{
			Monitor.Enter(fetchingWorkerLock);
			FetchingWorkerCount++;
			Monitor.Exit(fetchingWorkerLock);
		}
		public void ReportFetchDone()
		{
			Monitor.Enter(fetchingWorkerLock);
			FetchingWorkerCount--;
			Monitor.Exit(fetchingWorkerLock);
		}
	}

	public delegate void AllWorkDoneEventHandler(object sender, AllWorkDoneEventHandlerArgs args);

	public class AllWorkDoneEventHandlerArgs
	{
	}
}
