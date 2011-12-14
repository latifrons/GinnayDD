using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ginnay.ProxySpider;

namespace Ginnaydd.Distributed.ProxyPool
{
	public abstract class AbstractProxyPool
	{
		public abstract ProxyInfo DequeueProxy();
		public abstract void EnqueueProxy(ProxyInfo proxyinfo);
		public abstract void FailProxy(ProxyInfo proxyInfo);
	}

}
