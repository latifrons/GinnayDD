using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ginnay.ProxySpider;

namespace Ginnaydd.Distributed.ProxyPool
{
	public class DirectPool : AbstractProxyPool
	{
		public override ProxyInfo DequeueProxy()
		{
			return new ProxyInfo
			       	{
			       		HttpProxy = null
			       	};
		}

		public override void EnqueueProxy(ProxyInfo proxyinfo)
		{
			return;
		}

		public override void FailProxy(ProxyInfo proxyInfo)
		{
			return;
		}
	}
}
