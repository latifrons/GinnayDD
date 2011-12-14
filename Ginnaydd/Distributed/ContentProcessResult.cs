using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ginnay.ProxySpider;

namespace Ginnaydd.Distributed
{
	public class ContentProcessResult
	{
		private bool success = true;
		
		private List<Task> newTasks = new List<Task>();

		public bool Success
		{
			get { return success; }
			set { success = value; }
		}

		public List<Task> NewTasks
		{
			get { return newTasks; }
			set { newTasks = value; }
		}

		
	}
}
