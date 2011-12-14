using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Ginnaydd.Distributed.ContentProcessor;

namespace Ginnaydd.Distributed.TaskGuide
{
	public class AmazonHelper
	{
		//name, id]
		Dictionary<string, int> classMap = new Dictionary<string, int>();
		Dictionary<string, int> imageSequences = new Dictionary<string, int>();
		
		private const string insertTypeSQL = @"insert into Type(TypeName)
values(@TypeName);select @@IDENTITY;";
		private const string insertFileSQL = @"insert into [File] (URL,LocalFile,FileType,URLHash)
values (@URL,@LocalFile,@FileType,@URLHash)";
		const string selectFileSQL = "select top 1 LocalFile from [File] where URLHash=@URLHash and FileType = @FileType and URL  =@URL";
		const string queryMaxWebSequenceSQL = "select top 1 LocalFile from [File] where FileType = @FileType order by ID desc";
		private const string loadTypesSQL = "select * from Type";

		private string storePath;
		//object locker = new object();
		Semaphore semaphore = new Semaphore(15,15);
		private int webSequence = 0;
		private string connectionString;
		object webSequenceLock = new object();
		object imageSequenceLock = new object();
		private object locker = new object();


		public string StorePath
		{
			get { return storePath; }
			set { storePath = value; }
		}

		public string ConnectionString
		{
			get { return connectionString; }
			set { connectionString = value; }
		}
		public void LoadTypes()
		{
			SqlConnection conn = GetConnection();
			DataTable dt = new DataTable();
			SqlDataAdapter adapter = new SqlDataAdapter(loadTypesSQL,conn);
			adapter.Fill(dt);
			foreach (DataRow dr in dt.Rows)
			{
				classMap[dr["TypeName"] as string] = Convert.ToInt32(dr["ID"]);
			}
			conn.Close();
		}

		public bool CheckLocalExists(Task task)
		{
			if (task.LocalPath == null)
			{
				string ll;
				GetLocalPath(task, out ll);
			}

			bool result = true;

			if (!File.Exists(task.LocalPath))
			{
				result = false;
			}
			return result;
		}

		public void GetLocalPath(Task task, out string filePath)
		{		
			AmazonTaskType type = (AmazonTaskType)task.Type;
			string f = null;
			int t1 = System.Environment.TickCount;
			
			semaphore.WaitOne();
			int t2 = System.Environment.TickCount;
			try
			{
				if (type == AmazonTaskType.IMAGE
					//|| type == AmazonTaskType.IMAGE_ORI
					)
				{
					string filename;
					f = "I";
					ImageToFilePath(task, out filePath, out filename);
				}
				else
				{
					f = "W";
					int timea, timeb;
					WebPageToFilePath(task, out filePath, out timea, out timeb);
					if (timea > 5000)
					{
						Console.WriteLine("TimeA {0}", timea);
					}
					if (timeb > 5000)
					{
						Console.WriteLine("TimeB {0}", timeb);
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				filePath = null;
			}
			finally
			{
				semaphore.Release();
			}
			//Monitor.Exit(locker);
			
			int t3 = System.Environment.TickCount;
			
			if (t3 - t2 > 10000)
			{
				Console.WriteLine("{0} {1} {2}", f, t2 - t1, t3 - t2);
				Console.WriteLine("{0} {1} {2} {3}",task.TaskId,task.Type,task.Url.GetHashCode(),task.Url);
			}
			
		}


		private void WebPageToFilePath(Task task, out string filePath,out int timea, out int timeb)
		{
			timea = timeb = 0;
			// http://www.amazon.cn/gp/search/ref=sr_nr_n_0?rh=n:2029189051,n:!2029190051,n:2112003051,n:2112004051,n:2112005051&bbn=2112004051&ie=UTF8&qid=1306560047&rnid=2112004051
			string path = task.Url;
			//string cleanURL = path.Substring(HTTP_PREFIX.Length);
			//cleanURL = RemoveQid.Replace(cleanURL, "");
			//cleanURL = HttpUtility.UrlDecode(cleanURL, Encoding.UTF8);
			//cleanURL = CleanURL(cleanURL);


			string s;
//			Console.WriteLine("Enter");
//			long a = Environment.TickCount;
			
			SqlConnection conn = GetConnection();
//			SqlTransaction transaction = conn.BeginTransaction();
			{
				SqlCommand cmd = new SqlCommand(selectFileSQL, conn);
//				cmd.Transaction = transaction;
				cmd.Parameters.Add(new SqlParameter("URL", SqlDbType.VarChar));
				cmd.Parameters.Add(new SqlParameter("URLHash", SqlDbType.Int));
				cmd.Parameters.Add(new SqlParameter("FileType",SqlDbType.Int));
				cmd.Parameters["URL"].Value = path;
				cmd.Parameters["URLHash"].Value = path.GetHashCode();
				cmd.Parameters["FileType"].Value = FileType.WebPage;
				int t1 = System.Environment.TickCount;
				s = cmd.ExecuteScalar() as string;
				timea = System.Environment.TickCount - t1;
			}

			if (s== null)
			{
				//url not exists, generate one
				int sequence = GetNextWebSequence(conn,null);
//				int sequence = GetNextWebSequence(conn, transaction);

				SqlCommand cmd = new SqlCommand(insertFileSQL, conn);
//				cmd.Transaction = transaction;
				cmd.Parameters.Add(new SqlParameter("URL", SqlDbType.VarChar));
				cmd.Parameters.Add(new SqlParameter("URLHash", SqlDbType.Int));
				cmd.Parameters.Add(new SqlParameter("LocalFile", SqlDbType.NVarChar));
				cmd.Parameters.Add(new SqlParameter("FileType", SqlDbType.Int));
				cmd.Parameters["URL"].Value = path;
				cmd.Parameters["URLHash"].Value = path.GetHashCode();
				cmd.Parameters["LocalFile"].Value = sequence;
				cmd.Parameters["FileType"].Value = FileType.WebPage;
				int t1 = System.Environment.TickCount;
				int c = cmd.ExecuteNonQuery();
				timeb = System.Environment.TickCount - t1;

				if (c > 0)
				{
					filePath = Path.Combine(StorePath, @"web\");
					filePath = Path.Combine(filePath, sequence.ToString());
				}
				else
				{
					filePath = null;
				}
			}
			else
			{
				string fileName = s;
				filePath = Path.Combine(StorePath, @"web\");
				filePath = Path.Combine(filePath, fileName);
			}
//			transaction.Commit();
			conn.Close();
//			Console.WriteLine("Exit {0}",Environment.TickCount -a);
		}
		private int GetNextImageSequence(string folderPath, string filenamePrefix)
		{
			Monitor.Enter(imageSequenceLock);
			try
			{
				int seq;
				if (!imageSequences.TryGetValue(folderPath, out seq))
				{
					seq = -1;
					//check real Path
					string root = Path.Combine(storePath, "image\\");
					string fullFolderPath = Path.Combine(root, folderPath);
					if (Directory.Exists(fullFolderPath))
					{
						string[] files = Directory.GetFiles(fullFolderPath, filenamePrefix + "*.*", SearchOption.TopDirectoryOnly);
						string index;
						foreach (string file in files)
						{
							index = file.Substring(file.LastIndexOf('_') + 1);
							index = index.Substring(0, index.IndexOf('.'));
							seq = Int32.Parse(index);
						}
					}
				}
				seq++;
				imageSequences[folderPath] = seq;
				int s = seq;
				return s;
			}
			finally
			{
				Monitor.Exit(imageSequenceLock);
			}
		}

		private SqlConnection GetConnection()
		{
//			SqlConnection conn = new SqlConnection("Data Source=" + dbPath + ";Version=3;Pooling=True;Max Pool Size=100;");
			SqlConnection conn = new SqlConnection(ConnectionString);
			conn.Open();			
			return conn;
		}
		private int GetNextWebSequence(SqlConnection conn,SqlTransaction transaction)
		{
			Monitor.Enter(webSequenceLock);
			try
			{
				if (webSequence == 0)
				{
					//query from the database
					SqlCommand cmd = new SqlCommand(queryMaxWebSequenceSQL, conn);
					//				cmd.Transaction = transaction;
					cmd.Parameters.Add(new SqlParameter("FileType", SqlDbType.Int));
					cmd.Parameters["FileType"].Value = FileType.WebPage;
					string s = cmd.ExecuteScalar() as string;
					if (s != null)
					{
						webSequence = Convert.ToInt32(s);
					}

				}

				webSequence++;
				int w = webSequence;
				return w;
			}
			finally
			{
				Monitor.Exit(webSequenceLock);
			}
		}

		private void ImageToFilePath(Task task, out string filePath, out string filename)
		{
			
			SqlConnection conn = GetConnection();
//			SqlTransaction transaction = conn.BeginTransaction();

			string s = null;
			{
				SqlCommand cmd = new SqlCommand(selectFileSQL, conn);
//				cmd.Transaction = transaction;
				cmd.Parameters.Add(new SqlParameter("URL", SqlDbType.VarChar));
				cmd.Parameters.Add(new SqlParameter("FileType", SqlDbType.Int));
				cmd.Parameters.Add(new SqlParameter("URLHash", SqlDbType.Int));
				cmd.Parameters["URL"].Value = task.Url;
				cmd.Parameters["FileType"].Value = FileType.Image;
				cmd.Parameters["URLHash"].Value = task.Url.GetHashCode();
				s = cmd.ExecuteScalar() as string;
			}
			if (s == null)
			{
				//new record
				string[] types = task.Context.Split(new string[] { "\\" }, StringSplitOptions.None);
				StringBuilder sb = new StringBuilder();
				foreach (string type in types)
				{
					int typeID;
					Monitor.Enter(locker);
					try
					{
						string ctype = CleanFilePath(type);
						if (classMap.TryGetValue(ctype, out typeID))
						{
							sb.Append(typeID).Append("_");
						}
						else
						{
							//no this entry
							SqlCommand cmd = new SqlCommand(insertTypeSQL, conn);
							//						cmd.Transaction = transaction;
							cmd.Parameters.Add(new SqlParameter("TypeName", SqlDbType.NVarChar));
							cmd.Parameters["TypeName"].Value = ctype;
							typeID = Convert.ToInt32(cmd.ExecuteScalar());
							classMap[ctype] = typeID;
							sb.Append(typeID).Append("_");
						}
					}
					finally
					{
						Monitor.Exit(locker);
					}
				}
				string folderPath = task.Context;
				//get sequence
				sb.Append(GetNextImageSequence(folderPath, sb.ToString()));

				//get extension
				int extPos = task.Url.LastIndexOf('.');
				if (extPos < task.Url.Length)
				{
					string ext = task.Url.Substring(extPos);
					sb.Append(ext);
				}
				filename = sb.ToString();

				string root = Path.Combine(storePath, "image\\");
				string fullFolderPath = Path.Combine(root, folderPath);
				filePath = Path.Combine(fullFolderPath, filename);

				{
					SqlCommand cmd = new SqlCommand(insertFileSQL, conn);
//					cmd.Transaction = transaction;
					cmd.Parameters.Add(new SqlParameter("URL", SqlDbType.VarChar));
					cmd.Parameters.Add(new SqlParameter("LocalFile", SqlDbType.NVarChar));
					cmd.Parameters.Add(new SqlParameter("FileType",SqlDbType.Int));
					cmd.Parameters.Add(new SqlParameter("URLHash", SqlDbType.Int));
					cmd.Parameters["URL"].Value = task.Url;
					cmd.Parameters["LocalFile"].Value = Path.Combine(folderPath, filename);
					cmd.Parameters["FileType"].Value = FileType.Image;
					cmd.Parameters["URLHash"].Value = task.Url.GetHashCode();
					cmd.ExecuteNonQuery();
				}
			}
			else
			{
				//has record,
				string localFile = s;
				string root = Path.Combine(storePath, "image\\");
				filePath = Path.Combine(root, localFile);
				filename = Path.GetFileName(filePath);
			}
//			transaction.Commit();
			conn.Close();
		}

		private static string CleanFilePath(string url)
		{
			// \/:*?"<>|
			url = url.Replace('/', '_');
			url = url.Replace('\\', '_');
			url = url.Replace(':', '_');
			url = url.Replace('*', '_');
			url = url.Replace('?', '_');
			url = url.Replace('\"', '_');
			url = url.Replace('<', '_');
			url = url.Replace('>', '_');
			url = url.Replace('|', '_');

			return url;
		}
		public void s()
		{
			string sql = "select * from [File]";
			SqlConnection conn = GetConnection();
			SqlDataAdapter a = new SqlDataAdapter(sql, conn);
			DataTable dt = new DataTable();
			a.Fill(dt);
			string sql2 = "update [File] set urlhash = @urlhash where id = @id";
			foreach (DataRow dr in dt.Rows)
			{
				SqlCommand cmd = new SqlCommand(sql2, conn);
				string url = (string) dr["url"];
				
				cmd.Parameters.AddWithValue("urlhash", url.GetHashCode());
				cmd.Parameters.AddWithValue("id", (int)dr["id"]);
				cmd.ExecuteNonQuery();
			}
			conn.Close();
		}
	}
	public enum FileType
	{
		WebPage = 0,
		Image = 1
	}
}
