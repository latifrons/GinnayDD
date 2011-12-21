using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Ginnaydd.Utilities;
using TaobaoSpider.BLL;

namespace Ginnaydd.Distributed.TaskPool
{
	public class SQLServerTaskPool : AbstractTaskPool
	{
		
		const string updateSQL = @"update Task set url=@url,state=@state,retrytimes = @retrytimes,context =@context,type=@type,URLHash = @URLHash
where id=@id";
		const string insertSQL =
			@"insert into [Task](url,state,retrytimes,context,type,urlhash) 
values(@url,@state,@retrytimes,@context,@type,@urlhash)";

		private const string insertNoDupSQL =
			@"if not exists (select top 1 id from [Task] where urlhash = @urlhash and url = @url)
insert into [Task](url,state,retrytimes,context,type,urlhash) 
values(@url,@state,@retrytimes,@context,@type,@urlhash)";

		private const string selectSQL = "select top (@limit) * from Task where state = @state";
		private const string countStateTypeSQL = "select count(*) from Task where state = @state and type=@type";
		private const string countStateSQL = "select count(*) from Task where state = @state";

		private int BUFFER_SIZE = 5000;
		public const int MAX_FAIL_TIMES = 3;
		private int maxDaemonEnqueueThreadCount;
		private List<Thread> daemonEnqueueThread = new List<Thread>();
		private Thread daemonPoolThread;
		private int daemonRestTime = 3000;
		private bool canStop = false;
		private bool allowDuplicatedTask = false;
		object daemonLocker = new object();

		private ProducerConsumerQueue<EnqueueTaskInfo> enqueueQueue = new ProducerConsumerQueue<EnqueueTaskInfo>();
//		private string connectionString;

		public class EnqueueTaskInfo
		{
			private Task task;
			private TaskState taskState;

			public TaskState TaskState
			{
				get { return taskState; }
				set { taskState = value; }
			}

			public Task Task
			{
				get { return task; }
				set { task = value; }
			}
		}

		public bool AllowDuplicatedTask
		{
			get { return allowDuplicatedTask; }
			set { allowDuplicatedTask = value; }
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

		public Thread DaemonPoolThread
		{
			get { return daemonPoolThread; }
			set { daemonPoolThread = value; }
		}

		public ProducerConsumerQueue<EnqueueTaskInfo> EnqueueQueue
		{
			get { return enqueueQueue; }
			set { enqueueQueue = value; }
		}

//		public string ConnectionString
//		{
//			get { return connectionString; }
//			set { connectionString = value; }
//		}

		public List<Thread> DaemonEnqueueThread
		{
			get { return daemonEnqueueThread; }
			set { daemonEnqueueThread = value; }
		}

		public int MaxDaemonEnqueueThreadCount
		{
			get { return maxDaemonEnqueueThreadCount; }
			set { maxDaemonEnqueueThreadCount = value; }
		}

		public void StartDaemon()
		{
			Monitor.Enter(daemonLocker);
			if (daemonPoolThread != null || daemonEnqueueThread.Count !=0)
			{
				Monitor.Exit(daemonLocker);
				return;
			}
			daemonPoolThread = new Thread(DaemonPoolMaintain);
			daemonPoolThread.IsBackground = true;
			daemonPoolThread.Name = "SQLServerTaskPool_DaemonPoolThread";

			for (int i = 0; i < maxDaemonEnqueueThreadCount; i++)
			{
				Thread t = new Thread(DaemonEnqueue);
				t.IsBackground = true;
				t.Name = "SQLServerTaskPool_DaemonEnqueueThread " + i;
				daemonEnqueueThread.Add(t);
			}
				
			
			canStop = false;
			Monitor.Exit(daemonLocker);

			daemonPoolThread.Start();
			foreach (Thread t in daemonEnqueueThread)
			{
				t.Start();
			}
		}
		public void StopDaemon()
		{
			Monitor.Enter(daemonLocker);
			canStop = true;
			foreach (Thread t in daemonEnqueueThread)
			{
				t.Join(1000);
				t.Abort();
			}
			daemonEnqueueThread.Clear();
			if (daemonPoolThread != null)
			{
				daemonPoolThread.Join(5000);
				daemonPoolThread.Abort();
				daemonPoolThread = null;
			}
			
			Monitor.Exit(daemonLocker);
		}

		protected void DaemonPoolMaintain()
		{
			while (!canStop)
			{
				//check if the task pool has no sufficient elements
				int count;
				count = taskQueue.Count;

				//if so, fetch from sql
				if (count < BUFFER_SIZE / 2)
				{
					try
					{
						LoadFromDB(BUFFER_SIZE - count);
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
					}
				}
				//sleep
				Thread.Sleep(daemonRestTime);
			}
		}
		protected void DaemonEnqueue()
		{
			List<Task> inserts = new List<Task>();
			while (!canStop)
			{
				EnqueueTaskInfo eti;
				if (inserts.Count == 0)
				{
					eti = enqueueQueue.Consume();
				}
				else
				{
					if (!enqueueQueue.TryConsume(out eti))
					{
						//submit inserts
						try
						{
							InsertTask(inserts, TaskState.NOT_FINISHED);
							inserts.Clear();
						}
						catch (Exception e)
						{
							Console.WriteLine(e);
						}
						continue;
					}
					else
					{
						if (eti.TaskState != TaskState.NOT_FINISHED || inserts.Count > 10)
						{
							try
							{
								InsertTask(inserts, TaskState.NOT_FINISHED);
								inserts.Clear();
							}
							catch (Exception e)
							{
								Console.WriteLine(e);
							}
							inserts.Clear();
						}
					} 
				}
				
				 
				Task task = eti.Task;
				
				switch (eti.TaskState)
				{
					case TaskState.NOT_FINISHED:
						{
							if (taskQueue.Count >= BUFFER_SIZE)
							{
								if (task.TaskId != 0)
								{
									try
									{
										UpdateTask(task, TaskState.NOT_FINISHED);
									}
									catch (Exception e)
									{
										enqueueQueue.Produce(eti);
										Console.WriteLine(e);
									}
								}
								else
								{
									inserts.Add(eti.Task);
								}
							}
							else
							{
								taskQueue.Produce(task);
							}
							
							break;
						}
					case TaskState.FINISHED:
						{
							try
							{
								UpsertTask(task, TaskState.FINISHED);
							}
							catch (Exception e)
							{
								enqueueQueue.Produce(eti);
								Console.WriteLine(e);
							}
							break;
						}
					case TaskState.FAILED:
						{
							task.FailTimes++;
							if (task.FailTimes < MAX_FAIL_TIMES)
							{
								//re-enqueue
								if (taskQueue.Count >= BUFFER_SIZE)
								{
									try
									{
										UpsertTask(task, TaskState.NOT_FINISHED);
									}
									catch (Exception e)
									{
										enqueueQueue.Produce(eti);
										Console.WriteLine(e);
									}
								}
								else
								{
									TaskQueue.Produce(task);
								}
							}
							else
							{
								try
								{
									UpsertTask(task, TaskState.FAILED);
								}
								catch (Exception e)
								{
									enqueueQueue.Produce(eti);
									Console.WriteLine(e);
								}
							}
							break;
						}
					case TaskState.IN_MEMORY:
						throw new ArgumentOutOfRangeException();
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
		protected void UpsertTask(Task task, TaskState taskState)
		{
			if (task.TaskId != 0)
			{
				UpdateTask(task, taskState);
			}
			else
			{
				List<Task> tasks = new List<Task>(1);
				tasks.Add(task);
				InsertTask(tasks, taskState);
			}
		}

		public override Task ConsumeTask()
		{
			return TaskQueue.Consume();
		}

		public override void ProduceTask(Task task)
		{
			enqueueQueue.Produce(new EnqueueTaskInfo
			                     	{
			                     		Task = task,
										TaskState = TaskState.NOT_FINISHED
			                     	});
		}

		public override void ProduceTasks(List<Task> tasks)
		{
			List<EnqueueTaskInfo> taskInfos = new List<EnqueueTaskInfo>();
			foreach (Task task in tasks)
			{
				taskInfos.Add(new EnqueueTaskInfo
				{
					Task = task,
					TaskState = TaskState.NOT_FINISHED
				});
			}
			enqueueQueue.Produce(taskInfos);
		}

		public override void FailTask(Task task)
		{
			enqueueQueue.Produce(new EnqueueTaskInfo
			                     	{
			                     		Task = task,
										TaskState = TaskState.FAILED
			                     	});
		}

		protected void UpdateTask(Task task, TaskState state)
		{
			if (task.TaskId != 0)
			{
				SqlConnection conn = Database.GetConnection();
				SqlCommand cmd = new SqlCommand(updateSQL, conn);
//				cmd.Transaction = transaction;
				cmd.Parameters.AddWithValue("id", task.TaskId);
				cmd.Parameters.AddWithValue("url", task.Url);
				cmd.Parameters.AddWithValue("urlhash", task.Url.GetHashCode());
				cmd.Parameters.AddWithValue("state", state);
				cmd.Parameters.AddWithValue("retrytimes", task.FailTimes);
				cmd.Parameters.AddWithValue("context", task.Context == null?(object)DBNull.Value:task.Context);
				cmd.Parameters.AddWithValue("type", task.Type);

				cmd.ExecuteNonQuery();
				conn.Close();
			}
		}

//		protected SqlConnection GetConnection()
//		{
//			SqlConnection conn = new SqlConnection(connectionString);
//			conn.Open();
//			return conn;
//		}

		public override void FinishTask(Task task)
		{
			enqueueQueue.Produce(new EnqueueTaskInfo
			{
				Task = task,
				TaskState = TaskState.FINISHED
			});
		}
		protected void InsertTask(IEnumerable<Task> tasks, TaskState state)
		{
			SqlCommand cmd = null;
			SqlConnection conn = Database.GetConnection();
			if (allowDuplicatedTask)
			{
				cmd = new SqlCommand(insertSQL, conn);
			}
			else
			{
				cmd = new SqlCommand(insertNoDupSQL, conn);
			}

			//cmd.Transaction = cmd.Connection.BeginTransaction();
			cmd.Parameters.Add("url", SqlDbType.VarChar);
			cmd.Parameters.Add("urlhash", SqlDbType.Int);
			cmd.Parameters.Add("state", SqlDbType.Int);
			cmd.Parameters.Add("retrytimes", SqlDbType.Int);
			cmd.Parameters.Add("context", SqlDbType.NVarChar);
			cmd.Parameters.Add("type", SqlDbType.Int);
			//cmd.Prepare();
			foreach (Task task in tasks)
			{

				cmd.Parameters["url"].Value = task.Url;
				cmd.Parameters["urlhash"].Value = task.Url.GetHashCode();
				cmd.Parameters["state"].Value = state;
				cmd.Parameters["retrytimes"].Value = 0;
				cmd.Parameters["context"].Value = task.Context == null ? (object)DBNull.Value : task.Context;
				cmd.Parameters["type"].Value = task.Type;
				cmd.ExecuteNonQuery();
			}
			//cmd.Transaction.Commit();
			conn.Close();
			
		}
		public void Flush()
		{
			Task task;
			List<Task> tasks = new List<Task>();
			while (TaskQueue.TryConsume(out task))
			{
				tasks.Add(task);
			}
			InsertTask(tasks,TaskState.NOT_FINISHED);
		}

		protected void LoadFromDB(int limit)
		{
			SqlConnection conn = Database.GetConnection();
			SqlCommand cmd = new SqlCommand(selectSQL, conn);
			//cmd.Transaction = conn.BeginTransaction();
			cmd.Parameters.Add("state", SqlDbType.Int);
			cmd.Parameters.Add("limit", SqlDbType.Int);

			cmd.Parameters["state"].Value = TaskState.NOT_FINISHED;
			cmd.Parameters["limit"].Value = limit;
			SqlDataAdapter adapter = new SqlDataAdapter(cmd);
			DataTable dt = new DataTable();
			adapter.Fill(dt);
			List<Task> tasks = new List<Task>();
			foreach (DataRow dr in dt.Rows)
			{
				Task t = new Task
				{
					Url = dr["url"] as string,
					Context = dr["context"] as string,
					FailTimes = 0,
					TaskId = Convert.ToInt32(dr["id"]),
					Type = Convert.ToInt32(dr["type"]),

				};
				tasks.Add(t);
			}
			//cmd.Transaction.Commit();
			conn.Close();
			foreach (Task t in tasks)
			{
				UpdateTask(t, TaskState.IN_MEMORY);
				
			}
			TaskQueue.Produce(tasks);
		}

		public int CountStateType(int state, int type)
		{
			try
			{
				SqlConnection conn = Database.GetConnection();
				SqlCommand cmd = new SqlCommand(countStateTypeSQL, conn);
				cmd.Parameters.Add("state", SqlDbType.Int);
				cmd.Parameters.Add("type", SqlDbType.Int);

				cmd.Parameters["state"].Value = state;
				cmd.Parameters["type"].Value = type;
				int aa = Convert.ToInt32(cmd.ExecuteScalar());
				conn.Close();
				return aa;
			}
			catch (Exception e)
			{
				Console.WriteLine("count" + e);
				return 0;
			}
		}
		public int CountState(int state)
		{
			try
			{
				SqlConnection conn = Database.GetConnection();
				SqlCommand cmd = new SqlCommand(countStateSQL, conn);
				cmd.Parameters.Add("state", SqlDbType.Int);

				cmd.Parameters["state"].Value = state;
				int aa = Convert.ToInt32(cmd.ExecuteScalar());
				conn.Close();
				return aa;
			}
			catch (Exception e)
			{
				Console.WriteLine("counts" + e);
				return 0;
			}
		}

		public void InitTable()
		{
			UpdateAllInMemory();
		}

		private void UpdateAllInMemory()
		{
			string sql = "Update Task set state = @state where state = @state2";
			SqlConnection conn = Database.GetConnection();
			SqlCommand cmd = new SqlCommand(sql, conn);
			cmd.Parameters.Add("state", SqlDbType.Int);
			cmd.Parameters.Add("state2", SqlDbType.Int);

			cmd.Parameters["state"].Value = TaskState.NOT_FINISHED;
			cmd.Parameters["state2"].Value = TaskState.IN_MEMORY;
			cmd.ExecuteNonQuery();
			conn.Close();
		}	
	}
}
