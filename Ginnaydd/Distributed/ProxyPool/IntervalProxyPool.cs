using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Ginnay.ProxySpider;
using Ginnaydd.Utilities;
using PriorityQueueDemo;

namespace Ginnaydd.Distributed.ProxyPool
{
	public class IntervalProxyPool : AbstractProxyPool
	{
		public const int MAX_FAIL_TIMES = 3;
		protected ProducerConsumerPriorityQueue<int, ProxyInfo> readyQueue = new ProducerConsumerPriorityQueue<int, ProxyInfo>();
		protected PriorityQueue<DateTime, ProxyInfo> restQueue = new PriorityQueue<DateTime, ProxyInfo>();
		private ProxyManager proxyManager;

		private TimeSpan proxyRestTime;
		private Thread daemonThread;
		private object daemonLocker = new object();
		private int daemonRestTime = 5000;
		private bool canStop = false;

		public int ReadyCount
		{
			get { return readyQueue.Count; }
		}
		public int RestCount
		{
			get { return restQueue.Count; }
		}

		public Thread DaemonThread
		{
			get { return daemonThread; }
			set { daemonThread = value; }
		}

		public ProxyManager ProxyManager
		{
			get { return proxyManager; }
			set { proxyManager = value; }
		}

		public int DaemonRestTime
		{
			get { return daemonRestTime; }
			set { daemonRestTime = value; }
		}

		public bool CanStop
		{
			get { return canStop; }
			set { canStop = value; }
		}

		public TimeSpan ProxyRestTime
		{
			get { return proxyRestTime; }
			set { proxyRestTime = value; }
		}

		public void StartDaemon()
		{
			Monitor.Enter(daemonLocker);
			{
				if (daemonThread != null)
				{
					Monitor.Exit(daemonLocker);
					return;
				}
				canStop = false;
				daemonThread = new Thread(Daemon);
				daemonThread.Name = "IntervalProxyPool_Daemon";
				daemonThread.IsBackground = true;
			}
			Monitor.Exit(daemonLocker);
			daemonThread.Start();
		}
		public void StopDaemon()
		{
			Monitor.Enter(daemonLocker);
			canStop = true;
			if (daemonThread != null)
			{
				daemonThread.Join(5000);
				daemonThread.Abort();
				daemonThread = null;
			}
			Monitor.Exit(daemonLocker);
		}

		private void Daemon()
		{
			ProxyInfo makeAlive;
			while (!canStop)
			{
				DateTime barrier = DateTime.Now - proxyRestTime;
				while (restQueue.Count > 0)
				{
					KeyValuePair<DateTime, ProxyInfo> peek = restQueue.Peek();
					if (peek.Key <= barrier)
					{
						makeAlive = restQueue.DequeueValue();
						readyQueue.Produce(makeAlive.RTT, makeAlive);
					}
					else
					{
						break;
					}
				}

				
				ProxyInfo tmp;
				while ((tmp = proxyManager.TryDequeueFastestProxy()) != null)
				{
					readyQueue.Produce(tmp.RTT, tmp);
				}
				Thread.Sleep(daemonRestTime);
			}
		}

		public override ProxyInfo DequeueProxy()
		{
			return readyQueue.ConsumeValue();
		}


		public override void EnqueueProxy(ProxyInfo proxyinfo)
		{
			restQueue.Enqueue(DateTime.Now, proxyinfo);
		}

		public override void FailProxy(ProxyInfo proxyInfo)
		{
			proxyInfo.FailTimes++;
			if (proxyInfo.FailTimes < MAX_FAIL_TIMES)
			{
				restQueue.Enqueue(DateTime.Now, proxyInfo);
			}
			else
			{
				proxyManager.DeleteProxy(proxyInfo);
			}
		}
	}
}
