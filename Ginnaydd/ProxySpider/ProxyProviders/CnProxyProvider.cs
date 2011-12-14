using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Ginnay.ProxySpider;
using Ginnay.ProxySpider.ProxyProviders;

namespace Ginnay.ProxySpider.ProxyProviders
{
	public class CnProxyProvider:AbstractProxyProvider
	{
		private const int MAX_PROVIDE = 3000;

		//<td>58.216.168.138<SCRIPT type=text/javascript>document.write(":"+c+z+z+i)</SCRIPT></td>
		//<SCRIPT type="text/javascript">z="3";m="4";k="2";l="9";d="0";b="5";i="7";w="6";r="8";c="1";<SCRIPT>
		private static Regex replaceScriptRegex = new Regex(@"<SCRIPT type=""?text/javascript""?>\n?(?<C>\w+=""\d"";)+\n?</SCRIPT>",RegexOptions.IgnoreCase|RegexOptions.Compiled);
		private static Regex ipRegex = new Regex(@"<td>\n?(?<ip>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\n?<SCRIPT type=""?text/javascript""?>\n?document\.write\("":""(?<port>(\+\w)+)\)\n?</SCRIPT>\n?</td>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		string[] urls = new string[]
		                     	{
		                     		"http://www.cnproxy.com/proxy1.html",
									"http://www.cnproxy.com/proxy2.html",
									"http://www.cnproxy.com/proxy3.html",
									"http://www.cnproxy.com/proxy4.html",
									"http://www.cnproxy.com/proxy5.html",
									"http://www.cnproxy.com/proxy6.html",
									"http://www.cnproxy.com/proxy7.html",
									"http://www.cnproxy.com/proxy8.html",
									"http://www.cnproxy.com/proxy9.html",
									"http://www.cnproxy.com/proxy10.html",
									"http://www.cnproxy.com/proxy10.html",
									"http://www.cnproxy.com/proxyedu1.html",
									"http://www.cnproxy.com/proxyedu2.html"
		                     	};

		

		public override List<ProxyInfo> ProvideProxy()
		{
			Dictionary<string, ProxyInfo> proxyInfos = new Dictionary<string, ProxyInfo>();
			foreach (string url in urls)
			{
				HttpWebResponse response;
				try
				{
					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
					HtmlHelper.HttpWebRequestNormalSetup(request);
					request.Timeout = 10000;
					request.ReadWriteTimeout = 10000;
					response = (HttpWebResponse)request.GetResponse();
				}
				catch (Exception)
				{
					continue;
				}
				string html;
				if (HtmlHelper.GetHtml(response, out html))
				{
					Dictionary<string, int> valueMap = new Dictionary<string, int>();
					{
						Match m = replaceScriptRegex.Match(html);
						if (!m.Success)
						{
							throw new Exception("CnProxy fail");
							continue;
						}
						Group p = m.Groups["C"];

						foreach (Capture cap in p.Captures)
						{
							string v = cap.Value;
							string[] vs = v.Split(new char[] { '\"', '=',';' },StringSplitOptions.RemoveEmptyEntries);
							if (vs.Length != 2)
							{
								throw new Exception("CnProxy fail");
								continue;
							}
							int r;
							if (Int32.TryParse(vs[1], out r))
							{
								valueMap[vs[0]] = r;
							}
						}
					}

					MatchCollection mc = ipRegex.Matches(html);
					foreach (Match m in mc)
					{
						string ip = m.Groups["ip"].Value;
						string port = m.Groups["port"].Value;
						foreach (KeyValuePair<string, int> pair in valueMap)
						{
							port = port.Replace(pair.Key, pair.Value.ToString());
						}
						port = port.Replace("+","");
						int porti;
						if (Int32.TryParse(port, out porti))
						{
							proxyInfos[ip] = new ProxyInfo
							{
								HttpProxy = new WebProxy(ip, porti),
							};
						}
					}
				}
			}
			//return proxyInfos.GetRange(0,30);
			List<ProxyInfo> proxies = new List<ProxyInfo>(proxyInfos.Values);
			int len = proxies.Count;
			Random ran = new Random();
			while (len > MAX_PROVIDE)
			{
				int random = ran.Next(len);
				proxies.RemoveAt(random);
				len--;
			}
			return proxies;
		}
	}
}
