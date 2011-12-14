using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Ginnaydd.Distributed;

namespace GinnayddGUI
{
	public partial class Form1 : Form
	{
		public Instance ins;
		private DateTime lastLogUpdate;
		private int download;
		private int local;
		private int fail;
		private int process;
		private int contentFail;
		object locker = new object();
		private Thread checkThread;
		TimeSpan logUpdateInterval = new TimeSpan(0, 0, 5);
		TimeSpan dataCheckInterval = new TimeSpan(0, 0, 5);
		public Form1(Instance ins)
		{
			this.ins = ins;
			InitializeComponent();
			ins.workerManager.OnTaskDone += new TaskDoneEventHandler(DoTaskDone);
			ins.taobaoTaskGuide.OnTaskProcessDone += new TaskProcessDoneEventHandler(DoTaskProcessDone);
			//			ins.th.OnAllWorkDone += new AllWorkDoneEventHandler(DoAllWorkDone);
			foreach (string s in Enum.GetNames(typeof(AmazonTaskType)))
			{
				comboBox1.Items.Add(s);
			}
			comboBox1.SelectedIndex = 0;
		}

		private void DoTaskProcessDone(object sender, TaskProcessDoneEventHandlerArgs args)
		{
			lock (locker)
			{
				if (args.ContentProcessResult.Success)
				{
					process++;
				}
				else
				{
					contentFail++;
				}
			}
		}

		private void DoAllWorkDone(object sender, AllWorkDoneEventHandlerArgs args)
		{
			ins.Stop();
			checkThread.Abort();
			UpdateStatistics();
			MessageBox.Show("Done");
		}

		private void DoTaskDone(object sender, TaskDoneEventHandlerArgs args)
		{
			lock (locker)
			{
				if (args.TaskResult.Success)
				{
					if (args.TaskResult.LoadFromLocal)
					{
						local++;
					}
					else
					{
						download++;
					}
				}
				else
				{
					fail++;
				}
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			lastLogUpdate = DateTime.Now;
			checkThread = new Thread(new ThreadStart(new Action(() =>
																	{
																		double r = dataCheckInterval.TotalMilliseconds;
																		int rr = (int)r;
																		while (true)
																		{
																			UpdateStatistics();
																			Thread.Sleep(rr);
																		}
																	})));
			checkThread.IsBackground = true;
			checkThread.Start();
			SetWorkerCount(false);
			ins.Start();
		}

		private void UpdateStatistics()
		{
//			int picCount = ins.GetPicCount();
//			int taskDB = ins.GetTaskDBCount();
//			int taskFinished = ins.GetTaskFinishedCount();
			int taskMemory = ins.GetTaskMemoryCount();
			int taskEnqueuing = ins.GetTaskEnqueuingCount();
			int[] proxyCount = ins.GetProxyPoolCount();
			int workerCount = ins.GetWorkerCount();
			int processQueueCount = ins.GetProcessQueueCount();
			int fetchingCount = ins.GetFetchingWorkerCount();
			long mem = System.GC.GetTotalMemory(false);
			int threadCount = Process.GetCurrentProcess().Threads.Count;
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("working ").Append(workerCount);
				sb.Append(", fetching ").Append(fetchingCount);
				sb.Append(", thread ").Append(threadCount);
				sb.Append(", memUsage ").Append(mem);

				StringBuilder sb2 = new StringBuilder();
				sb2.Append("ready ").Append(proxyCount[0]).Append(", ");
				sb2.Append("rest ").Append(proxyCount[1]).Append(", ");
				sb2.Append("good ").Append(proxyCount[2]).Append(", ");
				sb2.Append("pending ").Append(proxyCount[3]);

				StringBuilder sb3 = new StringBuilder();
				sb3.Append("memory ").Append(taskMemory).Append(", ");
				sb3.Append("enqueuing ").Append(taskEnqueuing).Append(", ");
				sb3.Append("processQueue ").Append(processQueueCount).Append(", ");

				this.lblPicCount.BeginInvoke(new Action(() =>
				{
//					this.lblPicCount.Text = picCount.ToString();
//					this.lblTaskDB.Text = taskDB.ToString();
//					this.lblTaskFinished.Text = taskFinished.ToString();
					this.lblTaskMemory.Text = sb3.ToString();
					this.lblWorkerCount.Text = sb.ToString();
					this.lblProxyReady.Text = sb2.ToString();
				}));
			}
			//			if ((DateTime.Now - lastLogUpdate) > logUpdateInterval)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(DateTime.Now.ToString("HH:mm:ss")).Append(' ');
				lock (locker)
				{
					sb.Append("local: ").Append(local).Append("; ");
					sb.Append("download: ").Append(download).Append("; ");
					sb.Append("process: ").Append(process).Append("; ");
					sb.Append("fail: ").Append(fail).Append("; ");
					sb.Append("cfail: ").Append(contentFail).Append("; ");

				}
				sb.Append(Environment.NewLine);
				//update
				this.txtLog.BeginInvoke(new Action(() =>
				{
					this.txtLog.AppendText(sb.ToString());
					this.txtLog.ScrollToCaret();
				}));
			}
			//			else
			//			{
			//				lastLogUpdate = DateTime.Now;
			//				success = 0;
			//				fail = 0;
			//				contentFail = 0;
			//			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			ins.Stop();
			checkThread.Abort();
			UpdateStatistics();
			MessageBox.Show("Stopped");

		}

		private void button3_Click(object sender, EventArgs e)
		{
			SetWorkerCount(true);
		}
		private void SetWorkerCount(bool notice)
		{
			int count;
			if (Int32.TryParse(txtWorkerCount.Text, out count))
			{
				ins.workerManager.MaxWorkerCount = count;

				if (notice)
				{
					ins.workerManager.FitToMaxWoker();
					MessageBox.Show("OK");
				}

			}
		}

		private void button4_Click(object sender, EventArgs e)
		{
			string s = comboBox1.SelectedItem as string;
			AmazonTaskType att = (AmazonTaskType)Enum.Parse(typeof(AmazonTaskType), s);
			if (!string.IsNullOrEmpty(txtNewTask.Text) && txtNewTask.Text.StartsWith("http://"))
			{
				ins.sqlServerTaskPool.ProduceTask(new Task
										{
											Url = txtNewTask.Text,
											Type = (int)att,
										});
			}
			UpdateStatistics();
		}

		private void button5_Click(object sender, EventArgs e)
		{
			System.GC.Collect();
		}
	}
}
