using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Ginnay.ProxySpider;

namespace Ginnaydd.Distributed.ProxyPool
{
	public class DirectProxyPool : AbstractProxyPool
	{
		private int restTime;

		public int RestTime
		{
			get { return restTime; }
			set { restTime = value; }
		}

		public override ProxyInfo DequeueProxy()
		{
			return new ProxyInfo
			       	{
			       		HttpProxy = null
			       	};
		}

		public override void EnqueueProxy(ProxyInfo proxyinfo)
		{
			Thread.Sleep(restTime);
			return;
		}

		public override void FailProxy(ProxyInfo proxyInfo)
		{
			return;
		}
	}
}
