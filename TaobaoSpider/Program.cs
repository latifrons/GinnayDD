using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using Dapper;
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
//			Instance i = new Instance();
//			Application.EnableVisualStyles();
//			Application.SetCompatibleTextRenderingDefault(false);
//			Application.Run(new Form1(i));

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

			Item i = OpsItem.GetFirstModel(2);			
			i = OpsItem.GetFirstModel(3);
			Console.ReadKey();
		}
	}
	
}
