using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Ginnay.Proxy.BLL;
using Ginnay.ProxySpider.ProxyProviders;

namespace Ginnay.ProxySpider.ProxyProviders
{
	public class WebPageProxyProvider : AbstractProxyProvider
	{
		private string configPath = "plugin/webpageproxyprovider/config.xml";
		private int MAX_PROVIDE = 3000;
		private List<WebPageProxySource> sources = new List<WebPageProxySource>();


		public List<WebPageProxySource> Sources
		{
			get { return sources; }
			set { sources = value; }
		}

		public override List<ProxyInfo> ProvideProxy()
		{
			Dictionary<string, ProxyInfo> proxyInfos = new Dictionary<string,ProxyInfo>();
			foreach (WebPageProxySource src in Sources)
			{
				HttpWebResponse response;
				try
				{
					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(src.URL);
					HtmlHelper.HttpWebRequestNormalSetup(request);
					request.Timeout = 10000;
					request.ReadWriteTimeout = 10000;
					response  = (HttpWebResponse)request.GetResponse();
				}
				catch (Exception)
				{
					continue;
				}
				string html;
				if (HtmlHelper.GetHtml(response, out html))
				{
					MatchCollection mc = src.Regex.Matches(html);
					foreach (Match m in mc)
					{
						string ip = m.Groups["ip"].Value;
						string port = m.Groups["port"].Value;
						int porti;
						if (Int32.TryParse(port, out porti))
						{
							if (src.PortWhiteList.Count == 0 || 
								src.PortWhiteList.Contains(porti))
							{
								proxyInfos[ip] = new ProxyInfo
								                 	{
								                 		HttpProxy = new WebProxy(ip, Int32.Parse(port)),
								                 		//Location = IPLocationSearch.GetIPLocation(ip).country
								                 	};
							}
						}
					}
				}
			}
			//return proxyInfos.GetRange(0,30);
			List<ProxyInfo> proxies = new List<ProxyInfo>(proxyInfos.Values);
			int len = proxies.Count;
			Random r = new Random();
			while (len > MAX_PROVIDE)
			{
				int random = r.Next(len);
				proxies.RemoveAt(random);
				len--;
			}
			return proxies;
		}
		public WebPageProxyProvider()
		{
			ReadConfig();
		}
		public void ReadConfig()
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(configPath);
			XmlNodeList nodelist = doc.SelectNodes("//WebPageProxyProviderInfo");
			if (nodelist == null)
			{
				return;
			}

			foreach (XmlNode node in nodelist)
			{
				WebPageProxySource wpps = new WebPageProxySource();
				if (node.Attributes == null)
				{
					continue;
				}
				XmlAttribute urlX = node.Attributes["URL"];
				XmlAttribute patternX = node.Attributes["Pattern"];
				XmlAttribute whiteX = node.Attributes["PortWhiteList"];
				if (urlX == null || patternX == null || whiteX == null)
				{
					continue;
				}
				wpps.URL = urlX.Value;
				wpps.Pattern = patternX.Value;
				string[] ports = whiteX.Value.Split(new string[] {",",";"},StringSplitOptions.RemoveEmptyEntries);
				foreach (string s in ports)
				{
					int port;
					if (Int32.TryParse(s, out port))
					{
						wpps.PortWhiteList.Add(port);
					}
				}
				sources.Add(wpps);
			}
		}
	}
}
