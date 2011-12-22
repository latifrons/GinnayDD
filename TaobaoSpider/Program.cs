using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Dapper;
using Ginnaydd.Distributed;
using GinnayddGUI;
using TaobaoSpider.BLL;
using TaobaoSpider.Model;

namespace TaobaoSpider
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Instance i = new Instance();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1(i));

//			Item i = new Item
//			         	{
//			         		Freight = 5.4,
//			         		Location = "Shanghai",
//			         		Name = "good",
//			         		Price = 3.32,
//			         		RecentDeal = 33,
//			         		SellerTaobaoId = 2,
//			         		UniqId = 23432,
//			         		UrlLink = "http://ddd"
//			         	};
//			ItemOps.Insert(i);

//			Item i = OpsItem.GetFirstModel(2);			
//			i = OpsItem.GetFirstModel(3);
//			string s = File.ReadAllText("f:/dev/user.htm",Encoding.GetEncoding("GB2312"));
//			TaskData td = new TaskData
//							{
//								Bytes = Encoding.Default.GetBytes(s),
//								ProxyInfo = null,
//								Task = new Task
//								       	{
//											Url = @"http://rate.taobao.com/user-rate-ff176d877242acae0c43891b47b78d90.htm",
//											Type = (int)TaobaoTaskType.PROVIDER_RATE,
//											Context = "324123",
//											
//								       	}
//							};
//			TaobaoTaskGuide g = new TaobaoTaskGuide();
//			g.EnqueueProcessTask(td.Task,td.Bytes,td.ProxyInfo);
//			g.StartProcess();
//			Console.ReadKey();
		}
	}
	
}
