using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using Ginnaydd.Distributed.TaskGuide;
using Ginnaydd.Distributed.TaskPool;
using Ginnay.ProxySpider.ProxyProviders;

namespace GinnayddGUI
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Instance ins = new Instance();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1(ins));

//			AmazonHelper a = new AmazonHelper();
//			a.ConnectionString = @"Data Source=localhost;Initial Catalog=AmazonGrab;User Id=wim;Password=wimlab;";
//			a.s();
//			string[] files = Directory.GetFiles(@"e:\dev\grab3\web\");
//			foreach (string s in files)
//			{
//				int i;
//				if ((i = s.LastIndexOf("_gz"))!=-1)
//				{
//					string n = s.Substring(0, i);
//					Console.WriteLine(n);
//					File.Move(s,n);
//					
//				}
//			}

		}
		static void V1()
		{
			int BUFFERSIZE = 524288;
			byte[] buffer = new byte[BUFFERSIZE];
			int c;
			string[] files = Directory.GetFiles(@"e:\dev\grab3\web\");
			int count = files.Length;
			int cur = 0;
			bool start = false;
			foreach (string s in files)
			{
				FileStream fs = null;
				GZipStream gs = null;
				string newFile = null;
				try
				{
					if (s.EndsWith("_gz"))
					{
						continue;
					}
					newFile = s + "_gz";
					fs = new FileStream(s, FileMode.Open);
					byte b1 = (byte)fs.ReadByte();
					byte b2 = (byte)fs.ReadByte();
					if (b1 == 0x1f && b2 == 0x8b)
					{
						if (fs != null)
						{
							fs.Close();
							fs = null;
						}
						File.Move(s, newFile);
						Console.WriteLine("M {0} {1}", Path.GetFileName(s), Path.GetFileName(newFile));
						continue;
					}
					else
					{
						fs.Seek(0, SeekOrigin.Begin);
					}
					gs = new GZipStream(File.OpenWrite(newFile), CompressionMode.Compress);
					while ((c = fs.Read(buffer, 0, BUFFERSIZE)) > 0)
					{
						gs.Write(buffer, 0, c);
					}

				}
				finally
				{
					if (fs != null)
						fs.Close();
					if (gs != null)
						gs.Close();
				}
				try
				{
					//delete and rename
					if (File.Exists(newFile))
					{
						File.Delete(s);
						Console.WriteLine("D {0} {1}", Path.GetFileName(s), Path.GetFileName(newFile));
						//File.Move(newFile, s);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}

				cur++;
				if (cur % 5000 == 0)
				{
					Console.WriteLine(cur);
				}
			}
			
		}
	}
}
