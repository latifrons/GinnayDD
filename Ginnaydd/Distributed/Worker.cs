using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using Ginnay.ProxySpider;
using Ginnaydd.Distributed.ProxyPool;

namespace Ginnaydd.Distributed
{
	public class Worker
	{
		private static int BUFFER_SIZE = 32 * 1024;
		static volatile int index = 0;
		private AbstractTaskGuide taskGuide;
		private string name;
		private WorkerManager workerManager;

		public AbstractTaskGuide TaskGuide
		{
			get { return taskGuide; }
			set { taskGuide = value; }
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

		public AbstractTaskPool TaskPool
		{
			get { return taskPool; }
			set { taskPool = value; }
		}

		public AbstractProxyPool ProxyPool
		{
			get { return proxyPool; }
			set { proxyPool = value; }
		}

		public bool CanStop
		{
			get { return canStop; }
			set { canStop = value; }
		}

		public Thread WorkThread
		{
			get { return workThread; }
			set { workThread = value; }
		}

		public Worker(WorkerManager workerManager)
		{
			lock (typeof(Worker))
			{
				index++;
				this.name = "Worker " + index;
			}
			this.workerManager = workerManager;
		}

		private Task task;
		private ProxyInfo proxyInfo;
		private AbstractTaskPool taskPool;
		private AbstractProxyPool proxyPool;

		private TaskResult taskResult;
		private string path;

		private DateTime startTime;
		private Thread workThread;
		private bool canStop = false;

		public event TaskDoneEventHandler OnTaskDone;
		public void BeginWork()
		{
			WorkThread = new Thread(DoWork);
			WorkThread.Name = name;
			WorkThread.Start();

		}
		protected void DoWork()
		{
			while (!CanStop)
			{
				task = TaskPool.ConsumeTask();
				if (taskGuide.ShouldDownload(task))
				{
					proxyInfo = ProxyPool.DequeueProxy(task);

					workerManager.ReportFetching();
					DoFetchWork();
					workerManager.ReportFetchDone();
				}
				else
				{
					DoLocalWork();
				}
			}
		}

		protected void DoLocalWork()
		{
			try
			{
				path = taskGuide.GetLocalStorePath(task);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				taskResult.Success = false;
				InvokeCallBack();
				return;
			}
			taskResult = new TaskResult();
			taskResult.Task = task;
			taskResult.ProxyInfo = proxyInfo;
			taskResult.LoadFromLocal = true;

			MemoryStream ms = null;
			GZipStream gs = null;
			try
			{
				if (taskGuide.ShouldProcess(task))
				{
					taskResult.NeedProcess = true;
					if (File.Exists(path))
					{
						ms = new MemoryStream();
						gs = new GZipStream(File.OpenRead(path), CompressionMode.Decompress);
						byte[] buffer = new byte[BUFFER_SIZE];
						int c;
						while ((c = gs.Read(buffer, 0, BUFFER_SIZE)) >0)
						{
							ms.Write(buffer, 0, c);
						}

						taskGuide.EnqueueProcessTask(task, ms.ToArray(), proxyInfo);
						taskResult.Success = true;
						InvokeCallBack();
						return;
					}
					else
					{
						taskResult.Success = false;
						InvokeCallBack();
						return;
					}

				}
				else
				{
					taskResult.NeedProcess = false;
					taskResult.Success = true;
					InvokeCallBack();
					return;
				}
			}
			catch (Exception)
			{
				taskResult.Success = false;
				InvokeCallBack();
				return;
			}
			finally
			{
				if (gs != null)
				{
					gs.Close();
				}
			}
		}

		protected void DoFetchWork()
		{
			try
			{
				path = taskGuide.GetLocalStorePath(task);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				taskResult.Success = false;
				InvokeCallBack();
				return;
			}
			taskResult = new TaskResult();
			taskResult.Task = task;
			taskResult.ProxyInfo = proxyInfo;
			taskResult.LoadFromLocal = false;
			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(task.Url);
				if (proxyInfo != null)
				{
					request.Proxy = proxyInfo.HttpProxy;
					request.KeepAlive = false;
				}
				taskGuide.SetUpRequest(request);
				startTime = DateTime.Now;
				//				IAsyncResult ar = request.BeginGetResponse(new AsyncCallback(DoGetResponse), request);
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();
				FetchResponse(response);
			}
			catch (Exception)
			{
				taskResult.Success = false;
				InvokeCallBack();
				return;
			}
		}
		protected void FetchResponse(HttpWebResponse response)
		{
			if (response == null)
			{
				taskResult.Success = false;
				InvokeCallBack();
				return;
			}
			proxyInfo.RTT = (int)(DateTime.Now - startTime).TotalMilliseconds;
			HttpStatusCode responseCode = response.StatusCode;
			taskResult.StatusCode = responseCode;
			if (responseCode != HttpStatusCode.OK)
			{
				taskResult.Success = false;
				InvokeCallBack();
				return;
			}
			Stream s = response.GetResponseStream();
			if (s == null)
			{
				taskResult.Success = false;
				InvokeCallBack();
				return;
			}
			s.ReadTimeout = taskGuide.GlobalTimeout;
			s.WriteTimeout = taskGuide.GlobalTimeout;
			//if gzip, decode
			if (response.ContentEncoding == "gzip")
			{
				s = new GZipStream(s, CompressionMode.Decompress);
			}
			//store it
			byte[] output;

			if (response.ContentType.Contains("text"))
			{
				taskResult.Success = StoreText(s, response.CharacterSet, out output);
			}
			else
			{
				taskResult.Success = StoreBytes(s, out output);
			}

			s.Close();
			if (!taskResult.Success)
			{
				InvokeCallBack();
				return;
			}

			if (taskGuide.ShouldProcess(task))
			{
				taskResult.NeedProcess = true;
				//byte[] content = File.ReadAllBytes(path);
				taskGuide.EnqueueProcessTask(task, output, proxyInfo);
			}
			else
			{
				taskResult.NeedProcess = false;
			}
			taskResult.Success = true;
			InvokeCallBack();
			return;
		}

		protected bool StoreText(Stream stream, string characterSet,out byte[] bytes)
		{
			string html;
			bool success = HtmlHelper.GetHtml(stream, characterSet, out html);
			bytes = null;
			if (success)
			{
				GZipStream gs = null;
				FileStream fs = null;
				try
				{
					bytes = Encoding.Default.GetBytes(html);
					MemoryStream ms = new MemoryStream(bytes);
					ms.Seek(0, SeekOrigin.Begin);
					if (taskGuide.ShouldStore(task))
					{
						try
						{
							if (!Directory.Exists(Path.GetDirectoryName(path)))
							{
								Directory.CreateDirectory(Path.GetDirectoryName(path));
							}
							File.Delete(path);
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
							return false;
						}

						fs = File.OpenWrite(path);
						gs = new GZipStream(fs, CompressionMode.Compress);
						byte[] buffer = new byte[BUFFER_SIZE];
						int c;
						while ((c = ms.Read(buffer, 0, BUFFER_SIZE)) > 0)
						{
							gs.Write(buffer, 0, c);
						}
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					return false;
				}
				finally
				{
					if (gs != null)
					{
						gs.Close();
					}
				}
				//File.WriteAllBytes(path, Encoding.Default.GetBytes(html));
			}
			return success;
		}

		protected bool StoreBytes(Stream stream,out byte[] bytes)
		{
			FileStream fs = null;
			MemoryStream ms = new MemoryStream();
			bytes = null;
			try
			{
				byte[] buffer = new byte[BUFFER_SIZE];
				int c = 0;
				int total = 0;
				while ((c = stream.Read(buffer, 0, BUFFER_SIZE)) > 0)
				{
					ms.Write(buffer, 0, c);
					total += c;
				}
				if (total == 0)
				{
					bytes = null;
					return false;
				}
				
				bytes = ms.ToArray();
				ms.Seek(0, SeekOrigin.Begin);

				if (taskGuide.ShouldStore(task))
				{
					if (!Directory.Exists(Path.GetDirectoryName(path)))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(path));
					}
					File.Delete(path);
					fs = File.Create(path);

					ms.Seek(0, SeekOrigin.Begin);
					while ((c = ms.Read(buffer, 0, BUFFER_SIZE)) > 0)
					{
						fs.Write(buffer, 0, c);
					}
					
				}
				return true;
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				if (fs != null)
				{
					fs.Close();
				}
			}
		}

		protected void InvokeCallBack()
		{
			if (OnTaskDone != null)
			{
				OnTaskDone(this, new TaskDoneEventHandlerArgs
									{
										TaskResult = taskResult
									});
			}
		}
	}

	public delegate void TaskDoneEventHandler(object sender, TaskDoneEventHandlerArgs args);

	public class TaskDoneEventHandlerArgs
	{
		private TaskResult taskResult;

		public TaskResult TaskResult
		{
			get { return taskResult; }
			set { taskResult = value; }
		}
	}
}
