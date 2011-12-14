using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Ginnay.ProxySpider;
using Ginnaydd.Utilities;

namespace Ginnaydd.Distributed
{
	public abstract class AbstractTaskGuide
	{

		protected bool canStop = false;
		protected int globalTimeout;
 		protected object locker = new object();
		protected int maxWorkers;
		private ProducerConsumerQueue<TaskData> processQueue = new ProducerConsumerQueue<TaskData>();
		protected List<Thread> workThreads = new List<Thread>();



		public int GlobalTimeout
		{
			get { return globalTimeout; }
			set { globalTimeout = value; }
		}

		public int MaxWorkers
		{
			get { return maxWorkers; }
			set { maxWorkers = value; }
		}

		public List<Thread> WorkThreads
		{
			get { return workThreads; }
			set { workThreads = value; }
		}

		public ProducerConsumerQueue<TaskData> ProcessQueue
		{
			get { return processQueue; }
			set { processQueue = value; }
		}


		public event TaskProcessDoneEventHandler OnTaskProcessDone;





		public void EnqueueProcessTask(Task task, byte[] bytes,ProxyInfo proxyInfo)
		{
			Debug.Assert(task != null);
			processQueue.Produce(new TaskData
			                     	{
										Task = task,
										Bytes = bytes,
										ProxyInfo = proxyInfo
			                     	});
		}

		public abstract string GetLocalStorePath(Task task);

		public void HandleTask()
		{
			while (!canStop)
			{
				TaskData td = processQueue.Consume();
				//td cannot be null
				ContentProcessResult cpr = Process(td);
				InvokeTaskProcessDoneEvent(this, new TaskProcessDoneEventHandlerArgs
				                                 	{
				                                 		ContentProcessResult = cpr,
														ProxyInfo = td.ProxyInfo,
														Task = td.Task
				                                 	});
			}
		}

		public abstract Task NewTask();

		public abstract void SetUpRequest(HttpWebRequest request);

		public abstract bool ShouldDownload(Task task);

		public abstract bool ShouldProcess(Task task);
		public abstract bool ShouldStore(Task task);

		public void StartProcess()
		{
			lock(locker)
			{
				if (workThreads.Count != 0)
				{
					return;
				}
				for (int i = 0; i < maxWorkers; i++)
				{
					Thread t = new Thread(HandleTask);
					t.Name = "TaskGuide " + i;
					workThreads.Add(t);
					t.Start();
				}
			}
		}

		public void StopProcess()
		{
			lock (locker)
			{
				foreach (Thread t in workThreads)
				{
					t.Join(5000);
					t.Abort();
				}
				workThreads.Clear();
			}
		}


		protected void InvokeTaskProcessDoneEvent(object sender, TaskProcessDoneEventHandlerArgs args)
		{
			if (OnTaskProcessDone != null)
			{
				OnTaskProcessDone(sender, args);
			}
		}

		protected abstract ContentProcessResult Process(TaskData td);
	}

	public class TaskData
	{

		private byte[] bytes;
		private ProxyInfo proxyInfo;
		private Task task;
		
		public byte[] Bytes
		{
			get { return bytes; }
			set { bytes = value; }
		}

		public ProxyInfo ProxyInfo
		{
			get { return proxyInfo; }
			set { proxyInfo = value; }
		}

		public Task Task
		{
			get { return task; }
			set { task = value; }
		}
	}

	public delegate void TaskProcessDoneEventHandler(object sender, TaskProcessDoneEventHandlerArgs args);

	public class TaskProcessDoneEventHandlerArgs
	{

		private ContentProcessResult contentProcessResult;
		private Task task;
		private ProxyInfo proxyInfo;


		public ContentProcessResult ContentProcessResult
		{
			get { return contentProcessResult; }
			set { contentProcessResult = value; }
		}
		public Task Task
		{
			get { return task; }
			set { task = value; }
		}

		public ProxyInfo ProxyInfo
		{
			get { return proxyInfo; }
			set { proxyInfo = value; }
		}
	}
}
