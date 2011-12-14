using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Ginnay.ProxySpider;

namespace Ginnaydd.Distributed
{
	public class TaskResult
	{

		private bool loadFromLocal = false;
		private ProxyInfo proxyInfo;
		private HttpStatusCode? statusCode;
		private bool success = false;
		private bool needProcess = false;
		private Task task;

		public bool LoadFromLocal
		{
			get { return loadFromLocal; }
			set { loadFromLocal = value; }
		}

		public ProxyInfo ProxyInfo
		{
			get { return proxyInfo; }
			set { proxyInfo = value; }
		}

		public HttpStatusCode? StatusCode
		{
			get { return statusCode; }
			set { statusCode = value; }
		}

		public bool Success
		{
			get { return success; }
			set { success = value; }
		}

		public Task Task
		{
			get { return task; }
			set { task = value; }
		}

		public bool NeedProcess
		{
			get { return needProcess; }
			set { needProcess = value; }
		}
	}
}
