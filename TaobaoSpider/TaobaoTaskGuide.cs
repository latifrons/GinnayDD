using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using Ginnay.ProxySpider;
using Ginnaydd.Distributed;
using HtmlAgilityPack;
using TaobaoSpider.BLL;
using TaobaoSpider.Model;
using HAH = Wimlab.Utilities.HTML.HtmlAgilityHelper;

namespace TaobaoSpider
{
	public class TaobaoTaskGuide : AbstractTaskGuide
	{
		static Regex RegexShopCount = new Regex(@"此款宝贝(?<number>\d+)家店铺在售");
		static Regex RegexRecentSellCount = new Regex(@"最近成交(?<number>\d+)笔");

		static Regex RegexSellerID = new Regex(@"user_number_id=(?<number>\d+)");
		static Regex RegexUniqID = new Regex(@"uniqpid=(?<number>-?\d+)");

		private static string UNIQ_ID = @"uniqID=";

		private static string RateURL = "http://rate.taobao.com/user-rate-#UID#.htm";

		private string storePath;
		private string connectionString;
//		public TaobaoTaskGuide(string storePath, string connectionString)
//		{
//			this.storePath = storePath;
//			this.connectionString = connectionString;
//		}
		public override string GetLocalStorePath(Task task)
		{
			return null;
		}

		public override Task NewTask()
		{
			return new Task();
		}

		public override void SetUpRequest(HttpWebRequest request)
		{
			request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:6.0.2) Gecko/20100101 Firefox/6.0.2";
			request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
			request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
			request.Headers[HttpRequestHeader.AcceptLanguage] = "zh-cn,en-us;q=0.7,en;q=0.3";
			request.Headers[HttpRequestHeader.AcceptCharset] = "ISO-8859-1,utf-8;q=0.7,*;q=0.7";
			request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
			request.ReadWriteTimeout = globalTimeout;
			request.Timeout = globalTimeout;
		}

		public override bool ShouldDownload(Task task)
		{
			return true;
		}

		public override bool ShouldProcess(Task task)
		{
			return true;
		}

		public override bool ShouldStore(Task task)
		{
			return false;
		}

		protected override ContentProcessResult Process(TaskData td)
		{
			ContentProcessResult cpr = new ContentProcessResult();
			TaskProcess tp = new TaskProcess();
			tp.TaskData = td;
			tp.CpResult = cpr;
			
			switch ((TaobaoTaskType)td.Task.Type)
			{
				case TaobaoTaskType.COMBINED_LIST:
					HandleCombinedList(tp);
					break;
				case TaobaoTaskType.PROVIDER_LIST:
					HandleProviderList(tp);
					break;
				case TaobaoTaskType.PROVIDER_RATE:
					HandleProviderRate(tp);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			return cpr;
		}

		private void HandleProviderRate(TaskProcess tp)
		{
			throw new NotImplementedException();
		}

		private void HandleProviderList(TaskProcess tp)
		{
			throw new NotImplementedException();
		}

		private void HandleCombinedList(TaskProcess tp)
		{
			string html = Encoding.Default.GetString(tp.TaskData.Bytes);
			if (!Validate(html))
			{
				tp.CpResult.Success = false;
				return;
			}

			HtmlDocument doc = new HtmlDocument();
			HtmlNode root = GetRoot(html);
			if (root == null)
			{
				FailProcess(tp);
				return;
			}

			HtmlNodeCollection list = root.SelectNodes(@"//div[@id='list-content']//li[@class='list-item']");
			if (list == null)
			{
				FailProcess(tp);
				return;
			}
			bool success = true;

			foreach (HtmlNode node in list)
			{
				//only 1 shop?
				string count = HAH.SafeGetSuccessorInnerText(node, @".//div[@class='legend2']/a");
				if (string.IsNullOrEmpty(count))
				{
					LogMissing("count", count);
				}
				else
				{
					Match m = RegexShopCount.Match(count);
					if (m.Success)
					{
						int c = Int32.Parse(m.Groups["number"].Value);
						if (c == 1)
						{
							//single shop
							success &= HandleCombinedListSingleShop(tp, node);
						}
						else
						{
							success &= HandleCombinedListMultiShops(tp, node);
						}
					}
					else
					{
						LogMissing("count", count);
						success = false;
					}
				}
				if (!success)
				{
					break;
				}
			}
			tp.CpResult.Success = success;
		}

		private bool Validate(string html)
		{
			return html.Contains("taobao") && html.LastIndexOf("/html") != -1;
		}


		private bool HandleCombinedListSingleShop(TaskProcess tp, HtmlNode node)
		{
			#region build an item
			Item i = new Item();

			string freight = HAH.SafeGetSuccessorInnerText(node, @".//li[@class='shipment']/span[@class='fee']");
			if (freight != null && freight.Length > 3)
			{
				//运费：8.00
				string freightD = freight.Substring(3);
				double d;
				if (double.TryParse(freightD, out d))
				{
					i.Freight = d;
				}
				else
				{
					LogMissing("freight", freightD);
					return false;
				}
			}
			else
			{
				LogMissing("freight", freight);
				return false;
			}

			i.Name = HAH.SafeGetSuccessorAttributeStringValue(node, @".//a[@class='EventCanSelect']", "title");
			if (i.Name == null)
			{
				LogMissing("Name", node.InnerHtml);
				return false;
			}
			i.Location = HAH.SafeGetSuccessorInnerText(node, @".//li[@class='shipment']/span[@class='loc']");
			if (i.Location == null)
			{
				LogMissing("Location", node.InnerHtml);
				return false;
			}
			string price = HAH.SafeGetSuccessorInnerText(node, @".//li[@class='price']/em");
			if (!string.IsNullOrEmpty(price))
			{
				//359.00
				double d;
				if (double.TryParse(price, out d))
				{
					i.Price = d;
				}
				else
				{
					LogMissing("price", price);
					return false;
				}
			}
			else
			{
				LogMissing("price", price);
				return false;
			}

			i.RecentDeal = 0;
			string recentDeal = HAH.SafeGetSuccessorInnerText(node, @".//li[@class='price']/span");

			if (!string.IsNullOrEmpty(recentDeal))
			{
				Match m = RegexRecentSellCount.Match(recentDeal);
				if (m.Success)
				{
					i.RecentDeal = Int32.Parse(m.Groups["number"].Value);
				}
			}

			string sellerID = HAH.SafeGetSuccessorAttributeStringValue(node, @".//li[@class='seller']/a", "href");

			if (!string.IsNullOrEmpty(sellerID))
			{
				Match m = RegexSellerID.Match(sellerID);
				if (m.Success)
				{
					i.SellerTaobaoId = Int32.Parse(m.Groups["number"].Value);
				}
				else
				{
					LogMissing("SellerID", sellerID);
					return false;
				}
			}
			else
			{
				LogMissing("SellerID", node.InnerHtml);
				return false;
			}

			i.UniqId = 0;
			i.UrlLink = HAH.SafeGetSuccessorAttributeStringValue(node, ".//a[@class='EventCanSelect']", "href");

			OpsItem.Insert(i);
			#endregion
			#region build new task
			//Seller
			tp.CpResult.NewTasks.Add(new Task
			                         	{
											Url = RateURL.Replace("#UID#",i.SellerTaobaoId.ToString()),
											Type = (int)TaobaoTaskType.PROVIDER_RATE,
			                         	});
			#endregion

			return true;
		}
		private bool HandleCombinedListMultiShops(TaskProcess tp, HtmlNode node)
		{
			string link = HAH.SafeGetSuccessorAttributeStringValue(node, @".//div[@class='legend2']/a", "href");
			if (link == null)
			{
				LogMissing("URL",node.InnerHtml);
				return false;
			}
			Match m = RegexUniqID.Match(link);
			
			if (m.Success)
			{
				string uniqIDs = m.Groups["number"].Value;
				tp.CpResult.NewTasks.Add(new Task
				{
					Type = (int)TaobaoTaskType.PROVIDER_LIST,
					Url = FixRelativeURL(link,tp.TaskData.Task.Host),
					Context = UNIQ_ID + uniqIDs,
				});
			}
			return true;
		}

		private string FixRelativeURL(string url, string host)
		{
			if (url.StartsWith("http://"))
			{
				return url;
			}
			return "http://" + host + url;
		}
		private void FailProcess(TaskProcess tp)
		{
			tp.CpResult.Success = false;
		}
		private HtmlNode GetRoot(string html)
		{
			HtmlDocument doc = new HtmlDocument();

			doc.LoadHtml(html);
			HtmlNode root = doc.DocumentNode;
			return root;
		}
		private void LogMissing(string attribute, string textParse)
		{
			Console.Error.WriteLine("Missing {0} in {1}", attribute, textParse);
		}
	}

	public class TaskProcess
	{
		private TaskData taskData;
		private HtmlNode htmlRoot;
		private ContentProcessResult cpResult;

		public TaskData TaskData
		{
			get { return taskData; }
			set { taskData = value; }
		}

		public HtmlNode HtmlRoot
		{
			get { return htmlRoot; }
			set { htmlRoot = value; }
		}

		public ContentProcessResult CpResult
		{
			get { return cpResult; }
			set { cpResult = value; }
		}
	}

	public enum TaobaoTaskType
	{
		COMBINED_LIST = 0,
		PROVIDER_LIST = 1,
		PROVIDER_RATE = 2,
	}
}
