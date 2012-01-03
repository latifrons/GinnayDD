using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Ginnay.ProxySpider;
using Ginnaydd.Distributed;
using Ginnaydd.Distributed.ProxyPool;
using Ginnaydd.Utilities;
using PriorityQueueDemo;

namespace TaobaoSpider
{
	public class TaobaoSelectiveIntervalProxyPool : AbstractProxyPool
	{
		public const int MAX_FAIL_TIMES = 3;
		protected ProducerConsumerPriorityQueue<int, ProxyInfo> proxyReadyQueue = new ProducerConsumerPriorityQueue<int, ProxyInfo>();
		protected ProducerConsumerPriorityQueue<int, ProxyInfo> localReadyQueue = new ProducerConsumerPriorityQueue<int, ProxyInfo>();

		protected PriorityQueue<DateTime, ProxyInfo> proxyRestQueue = new PriorityQueue<DateTime, ProxyInfo>();
		protected PriorityQueue<DateTime, ProxyInfo> localRestQueue = new PriorityQueue<DateTime, ProxyInfo>();

		private TimeSpan proxyRestTime;
		private TimeSpan localRestTime;

		private ProxyManager proxyManager;
		private List<Thread> daemonThreads = new List<Thread>();

		private object daemonLocker = new object();
		private TimeSpan daemonRestTime;
		private int maxLocalConnections = 1;
		private bool canStop = false;

		public int ReadyCount
		{
			get { return proxyReadyQueue.Count; }
		}
		public int RestCount
		{
			get { return proxyRestQueue.Count; }
		}

		public ProxyManager ProxyManager
		{
			get { return proxyManager; }
			set { proxyManager = value; }
		}

		public TimeSpan DaemonRestTime
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

		public TimeSpan LocalRestTime
		{
			get { return localRestTime; }
			set { localRestTime = value; }
		}

		public int MaxLocalConnections
		{
			get { return maxLocalConnections; }
			set { maxLocalConnections = value; }
		}

		public void StartDaemon()
		{
			Monitor.Enter(daemonLocker);
			if (daemonThreads.Count == 0)
			{
				Thread daemonLocalThread;
				StartProxyDaemon(DaemonLocal, "DaemonLocal", out daemonLocalThread);
				daemonThreads.Add(daemonLocalThread);

				Thread daemonProxyThread;
				StartProxyDaemon(DaemonProxy, "DaemonProxy", out daemonProxyThread);
				daemonThreads.Add(daemonProxyThread);
			}
			Monitor.Exit(daemonLocker);
		}
		private void StartProxyDaemon(ThreadStart func, string name, out Thread thread)
		{
			canStop = false;
			this.localReadyQueue.Clear();
			this.localRestQueue.Clear();
			for (int i = 0; i < maxLocalConnections; i++)
			{
				this.localReadyQueue.Produce(0, new ProxyInfo
				{
					HttpProxy = null
				});
			}

			thread = new Thread(func);
			thread.Name = name;
			thread.IsBackground = true;
			thread.Start();
		}

		public void StopDaemon()
		{
			Monitor.Enter(daemonLocker);
			canStop = true;

			foreach (Thread t in daemonThreads)
			{
				t.Join(5000);
				t.Abort();
			}
			daemonThreads.Clear();
			Monitor.Exit(daemonLocker);
		}
		private void DaemonProxy()
		{
			ProxyInfo makeAlive;
			while (!canStop)
			{
				//proxy queue
				{
					DateTime barrier = DateTime.Now - proxyRestTime;
					while (proxyRestQueue.Count > 0)
					{
						KeyValuePair<DateTime, ProxyInfo> peek = proxyRestQueue.Peek();
						if (peek.Key <= barrier)
						{
							makeAlive = proxyRestQueue.DequeueValue();
							proxyReadyQueue.Produce(makeAlive.RTT, makeAlive);
						}
						else
						{
							break;
						}
					}


					ProxyInfo tmp;
					while ((tmp = proxyManager.TryDequeueFastestProxy()) != null)
					{
						proxyReadyQueue.Produce(tmp.RTT, tmp);
					}
				}
				Thread.Sleep(daemonRestTime);
			}
		}

		private void DaemonLocal()
		{
			ProxyInfo makeAlive;
			while (!canStop)
			{
				//local queue
				{
					DateTime barrier = DateTime.Now - localRestTime;
					while (localRestQueue.Count > 0)
					{
						KeyValuePair<DateTime, ProxyInfo> peek = localRestQueue.Peek();
						if (peek.Key <= barrier)
						{
							makeAlive = localRestQueue.DequeueValue();
							localReadyQueue.Produce(makeAlive.RTT, makeAlive);
						}
						else
						{
							break;
						}
					}
				}
				Thread.Sleep(daemonRestTime);
			}
		}

		public override ProxyInfo DequeueProxy(Task task)
		{
			if (task.Type == (int)TaobaoTaskType.PROVIDER_RATE)
			{
				return proxyReadyQueue.ConsumeValue();
			}
			else
			{
				return localReadyQueue.ConsumeValue();
			}
		}


		public override void EnqueueProxy(ProxyInfo proxyinfo)
		{
			if (proxyinfo.HttpProxy != null)
			{
				proxyRestQueue.Enqueue(DateTime.Now, proxyinfo);
			}
			else
			{
				localRestQueue.Enqueue(DateTime.Now, proxyinfo);
			}
		}

		public override void FailProxy(ProxyInfo proxyInfo)
		{
			if (proxyInfo.HttpProxy == null)
			{
				localRestQueue.Enqueue(DateTime.Now, proxyInfo);
			}
			proxyInfo.FailTimes++;
			if (proxyInfo.FailTimes < MAX_FAIL_TIMES)
			{
				proxyRestQueue.Enqueue(DateTime.Now, proxyInfo);
			}
			else
			{
				proxyManager.DeleteProxy(proxyInfo);
			}
		}
	}
}
