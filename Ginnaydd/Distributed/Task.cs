﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ginnaydd.Distributed
{
	public class Task
	{
		private string url;
		private string context;
		private int taskID;
		private int type;
		private string localPath;
		public string Url
		{
			get { return url; }
			set { url = value; }
		}

		public int FailTimes
		{
			get { return failTimes; }
			set { failTimes = value; }
		}

		public string Context
		{
			get { return context; }
			set { context = value; }
		}

		public int TaskId
		{
			get { return taskID; }
			set { taskID = value; }
		}

		public int Type
		{
			get { return type; }
			set { type = value; }
		}

		public string LocalPath
		{
			get { return localPath; }
			set { localPath = value; }
		}

		private int failTimes;
	}
}
