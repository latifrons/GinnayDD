using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GinnayddGUI;

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
		}
	}
}
