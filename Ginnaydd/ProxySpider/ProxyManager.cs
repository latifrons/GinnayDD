using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Ginnay.ProxySpider.ProxyProviders;
using Ginnaydd.Utilities;
using PriorityQueueDemo;

namespace Ginnay.ProxySpider
{
	public delegate void UsableProxyFoundEventHandler(object sender, UsableProxyFoundEventArgs args);

	public class UsableProxyFoundEventArgs
	{
		private ProxyInfo proxyInfo;

		public ProxyInfo ProxyInfo
		{
			get { return proxyInfo; }
			set { proxyInfo = value; }
		}
	}

	public class ProxyManager
	{
		private volatile bool canStop;
		private BackgroundWorker fetchWorker;
//		private Dictionary<string, ProxyInfo> goodProxies = new Dictionary<string, ProxyInfo>();
		private ProducerConsumerPriorityQueue<double, ProxyInfo> goodProxiesQueue = new ProducerConsumerPriorityQueue<double, ProxyInfo>();
		private int maxValidateThreadCount = 30;
		private Queue<ProxyInfo> pendingProxies = new Queue<ProxyInfo>();
		private HashSet<string> proxies = new HashSet<string>(); 

		private List<AbstractProxyProvider> proxyProviders = new List<AbstractProxyProvider>();
		private volatile int proxyToValidateCount;
		private volatile int proxyValidatedCount;
		private ProxyValidator proxyValidator;
		private List<Thread> validateThreads;
		private BackgroundWorker validateWorker;
		private bool downloading = false;

		public event ProgressChangedEventHandler OnDownloadProgressChanged;
		public event DownloadCompletedEventHandler OnDownloadCompleted;
		public event ProgressChangedEventHandler OnValidateProgressChanged;
		public event RunWorkerCompletedEventHandler OnValidateCompleted;
		public event UsableProxyFoundEventHandler OnUsableProxyFound;
		public event ValidateStartedEventHandler OnValidateStarted;
		public event DownloadStartedEventHandler OnDownloadStarted;
		public event PendingProxyListChangedEventHandler OnPendingProxyListChanged;
		public event ProxyRemovedEventHandler OnProxyRemoved;
		public event ProxyProviderListChangedEventHandler OnProxyProviderListChanged;

		private Thread daemonThread;
		object locker = new object();
		private int daemonRestTime = 2000;

		public ProxyValidator ProxyValidator
		{
			get { return proxyValidator; }
			set { proxyValidator = value; }
		}

		public List<AbstractProxyProvider> ProxyProviders
		{
			get { return proxyProviders; }
			set { proxyProviders = value; }
		}

		public int MaxValidateThreadCount
		{
			get { return maxValidateThreadCount; }
			set { maxValidateThreadCount = value; }
		}

		public ProducerConsumerPriorityQueue<double, ProxyInfo> GoodProxiesQueue
		{
			get { return goodProxiesQueue; }
			set { goodProxiesQueue = value; }
		}

		public Queue<ProxyInfo> PendingProxies
		{
			get { return pendingProxies; }
			set { pendingProxies = value; }
		}

		public int DaemonRestTime
		{
			get { return daemonRestTime; }
			set { daemonRestTime = value; }
		}

		public void StartDownloadProxies(bool startValidationAfterDownload)
		{
			Monitor.Enter(locker);
			if (downloading)
			{
				Monitor.Exit(locker);
				return;
			}
			else
			{
				downloading = true;
			}
			Monitor.Exit(locker);

				pendingProxies.Clear();
				goodProxiesQueue.Clear();
				lock (proxies)
				{
					proxies.Clear();
				}
			
			fetchWorker = new BackgroundWorker();
			fetchWorker.WorkerReportsProgress = true;
			fetchWorker.WorkerSupportsCancellation = true;
			fetchWorker.DoWork += new DoWorkEventHandler(fetchWorker_DoWork);
			fetchWorker.ProgressChanged += new ProgressChangedEventHandler(fetchWorker_ProgressChanged);
			fetchWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(fetchWorker_RunWorkerCompleted);

			if (startValidationAfterDownload)
			{
				fetchWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CallStartValidateProxies);
			}
			if (OnDownloadStarted != null)
			{
				OnDownloadStarted(this,new DownloadStartedEventHandlerArgs());
			}
			fetchWorker.RunWorkerAsync();
		}

		private void CallStartValidateProxies(object sender, RunWorkerCompletedEventArgs e)
		{
			StartValidateProxies();
		}

		public void StopDownloadProxies()
		{
			if (fetchWorker != null)
			{
				fetchWorker.CancelAsync();
			}
		}

		private void fetchWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			fetchWorker = null;
//			int elim = this.pendingProxies.Count - 20;
//			for (int i = 0; i < elim; i++)
//			{
//				this.pendingProxies.Dequeue();
//			}
			if (OnPendingProxyListChanged != null)
			{
				OnPendingProxyListChanged(sender,new PendingProxyListChangedEventHandlerArgs());
			}
			if (OnDownloadCompleted != null)
			{
				OnDownloadCompleted(sender, new DownloadCompletedEventHandlerArgs
				                            	{
				                            		Count = this.pendingProxies.Count
				                            	});
			}
		}

		private void fetchWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (OnDownloadProgressChanged != null)
			{
				OnDownloadProgressChanged(sender, e);
			}
		}

		private void fetchWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker worker = (BackgroundWorker)sender;
			worker.ReportProgress(0);

			int count = proxyProviders.Count;
			int current = 0;
			foreach (AbstractProxyProvider pp in proxyProviders)
			{
				if (fetchWorker.CancellationPending)
				{
					continue;
				}
				List<ProxyInfo> pis;
				try
				{
					pis = pp.ProvideProxy();
					Console.WriteLine("{0} {1}",pp.ClassName,pis.Count);
				}
				catch (Exception)
				{
					current++;
					worker.ReportProgress(current * 100 / count, "fail");
					return;
				}
				if (pis != null)
				{
					foreach (ProxyInfo pi in pis)
					{
						EnqueuePendingProxy(pi);
					}
				}
				current++;
				worker.ReportProgress(current * 100 / count);
			}
//			LoadFile();
		}

		private void LoadFile()
		{
			string file = "f:/dev/p.txt";
			string s = File.ReadAllText(file);
			Regex r = new Regex(@"(?<ip>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):(?<port>\d{1,5})");
			MatchCollection mc = r.Matches(s);
			foreach (Match m in mc)
			{
				string ip = m.Groups["ip"].Value;
				string port = m.Groups["port"].Value;
				int porti;

				if (Int32.TryParse(port, out porti))
				{
					EnqueuePendingProxy(new ProxyInfo
					{
						HttpProxy = new WebProxy(ip, Int32.Parse(port)),
						//Location = IPLocationSearch.GetIPLocation(ip).country
					});
				}
			}
		}

		public void StartValidateProxies()
		{
			if (validateWorker != null)
			{
				if (validateWorker.IsBusy)
				{
					return;
				}
			}
			validateWorker = new BackgroundWorker();
			validateWorker.WorkerReportsProgress = true;
			validateWorker.WorkerSupportsCancellation = true;
			validateWorker.DoWork += new DoWorkEventHandler(validateWorker_DoWork);
			validateWorker.ProgressChanged += new ProgressChangedEventHandler(validateWorker_ProgressChanged);
			validateWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(validateWorker_RunWorkerCompleted);
			if (OnValidateStarted != null)
			{
				OnValidateStarted(this,new ValidateStartedEventHandlerArgs());
			}
			validateWorker.RunWorkerAsync();
		}
		public void StopValidateProxies()
		{
			if (validateWorker != null)
			{
				validateWorker.CancelAsync();
			}
		}

		private void validateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			validateWorker = null;
			Monitor.Enter(locker);
			downloading = false;
			Monitor.Exit(locker);
			if (OnValidateCompleted != null)
			{
				OnValidateCompleted(sender, e);
			}
		}

		private void validateWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (OnValidateProgressChanged != null)
			{
				OnValidateProgressChanged(sender, e);
			}

		}

		private void validateWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker worker = (BackgroundWorker)sender;
			//move all good proxies to pending proxy and clear rtt

			proxyToValidateCount = pendingProxies.Count;
			proxyValidatedCount = 0;

			validateThreads = new List<Thread>();
			for (int i = 0; i < maxValidateThreadCount; i++)
			{
				Thread t = new Thread(new ThreadStart(ValidateWork));
				t.Name = "validate " + i;
				
				validateThreads.Add(t);
			}
			foreach (Thread t in validateThreads)
			{
				t.Start();
			}
			foreach (Thread t in validateThreads)
			{
				t.Join();
			}
			worker.ReportProgress(100);
		}

		private void ValidateWork()
		{
			ProxyInfo pi;
			while (!validateWorker.CancellationPending && (pi = DequeuePendingProxy()) != null)
			{
				if (proxyValidator.ValidateProxy(pi))
				{
//					Console.WriteLine("OK {0} {1}ms", pi.HttpProxy.Address, pi.RTT);
					EnqueueGoodProxy(pi);
				}
				else
				{
//					Console.WriteLine("F {0} {1}ms", pi.HttpProxy.Address, pi.RTT);
//					EnqueuePendingProxy(pi);
				}
				proxyValidatedCount++;
				if (proxyValidatedCount > proxyToValidateCount)
				{
					throw new Exception();
				}
				validateWorker.ReportProgress(proxyValidatedCount * 100 / proxyToValidateCount, pi);
			}
		}

		private ProxyInfo DequeuePendingProxy()
		{
			ProxyInfo pi = null;
			lock (pendingProxies)
			{
				if (pendingProxies.Count > 0)
				{
					pi = pendingProxies.Dequeue();
				}
			}
			return pi;
		}

		public void CancelValidation()
		{
			foreach (Thread t in validateThreads)
			{
				t.Abort();
			}
		}
		public void StartDaemon()
		{
			Monitor.Enter(locker);
			if (daemonThread != null)
			{
				Monitor.Exit(locker);
				return;
			}
			daemonThread = new Thread(Daemon);
			daemonThread.IsBackground = true;
			daemonThread.Name = "ProxyManager_Daemon";
			canStop = false;
			Monitor.Exit(locker);
			
			daemonThread.Start();
		}
		public void StopDaemon()
		{
			Monitor.Enter(locker);
			canStop = true;
			if (daemonThread != null)
			{
				daemonThread.Join();
			}
			daemonThread = null;
			Monitor.Exit(locker);
		}
		protected void Daemon()
		{
			while (!canStop)
			{
				if (goodProxiesQueue.Count <= 5)
				{
					StartDownloadProxies(true);
				}
				Thread.Sleep(daemonRestTime);
			}
		}

		public ProxyInfo DequeueFastestProxy()
		{
			return goodProxiesQueue.ConsumeValue();
		}
		public ProxyInfo TryDequeueFastestProxy()
		{
			ProxyInfo pi;
			if (goodProxiesQueue.TryConsumeValue(out pi))
			{
				return pi;
			}
			else
			{
				return null;
			}
		}
		public void EnqueueGoodProxy(ProxyInfo proxyInfo)
		{
			bool notice = false;
			lock (proxies)
			{
				if (!proxies.Contains(proxyInfo.ProxyAddress))
				{
					goodProxiesQueue.Produce(proxyInfo.RTT, proxyInfo);
					proxies.Add(proxyInfo.ProxyAddress);
					notice = true;
				}
			}
			if (notice && OnUsableProxyFound != null)
			{
				OnUsableProxyFound(this, new UsableProxyFoundEventArgs
				{
					ProxyInfo = proxyInfo
				});
			}
		}
		public void EnqueuePendingProxy(ProxyInfo proxyInfo)
		{
			lock (pendingProxies)
			{
				pendingProxies.Enqueue(proxyInfo);
			}
			
		}
		public void DeleteProxy(ProxyInfo proxyInfo)
		{
			lock (proxies)
			{
				proxies.Remove(proxyInfo.ProxyAddress);
			}
			if (OnProxyRemoved != null)
			{
				OnProxyRemoved(this, new ProxyRemovedEventHandlerArgs
				                     	{
				                     		ProxyInfo = proxyInfo
				                     	});
			}
		}
		public void LoadProxies(IEnumerable<ProxyInfo> proxyInfos)
		{
			bool notice = false;
			lock (proxies)
			{
				foreach (ProxyInfo pi in proxyInfos)
				{
					if (!proxies.Contains(pi.ProxyAddress))
					{
						proxies.Add(pi.ProxyAddress);
						pendingProxies.Enqueue(pi);
						notice = true;
					}
				}
			}
			if (notice &&OnPendingProxyListChanged != null)
			{
				OnPendingProxyListChanged(this, new PendingProxyListChangedEventHandlerArgs());
			}
		}
		public void LoadProxyProviders(IEnumerable<AbstractProxyProvider> providers)
		{
			lock (proxyProviders)
			{
				foreach (AbstractProxyProvider ipp in providers)
				{
					proxyProviders.Add(ipp);
				}
			}
			if (OnProxyProviderListChanged != null)
			{
				OnProxyProviderListChanged(this,new ProxyProviderListChangedEventHandlerArgs());
			}
		}
	}

	public delegate void ProxyProviderListChangedEventHandler(object sender, ProxyProviderListChangedEventHandlerArgs args);

	public class ProxyProviderListChangedEventHandlerArgs
	{
	}

	public delegate void ProxyRemovedEventHandler(object sender, ProxyRemovedEventHandlerArgs args);

	public class ProxyRemovedEventHandlerArgs
	{
		private ProxyInfo proxyInfo;

		public ProxyInfo ProxyInfo
		{
			get { return proxyInfo; }
			set { proxyInfo = value; }
		}
	}

	public delegate void DownloadStartedEventHandler(object sender, DownloadStartedEventHandlerArgs args);

	public class DownloadStartedEventHandlerArgs
	{
	}

	public delegate void DownloadCompletedEventHandler(object sender, DownloadCompletedEventHandlerArgs args);

	public class DownloadCompletedEventHandlerArgs
	{
		private int count;

		public int Count
		{
			get { return count; }
			set { count = value; }
		}
	}

	public delegate void PendingProxyListChangedEventHandler(object sender, PendingProxyListChangedEventHandlerArgs args);

	public class PendingProxyListChangedEventHandlerArgs
	{
	}

	public delegate void ValidateStartedEventHandler(object sender, ValidateStartedEventHandlerArgs args);

	public class ValidateStartedEventHandlerArgs
	{
	}
}