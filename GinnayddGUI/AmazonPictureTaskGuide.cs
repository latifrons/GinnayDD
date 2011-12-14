using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Ginnaydd.Distributed.TaskGuide;
using HtmlAgilityPack;
using HAH = Wimlab.Utilities.HTML.HtmlAgilityHelper;

namespace Ginnaydd.Distributed.ContentProcessor
{
	public class AmazonPictureTaskGuide: AbstractTaskGuide
	{
		
		static Regex GetNodeID = new Regex(@"node=(?<id>\d+)",RegexOptions.Compiled);
		
		
		static Regex PicPathRegex = new Regex(@"setRgAg\(\s*'(?<thumb>.*?)'\s*,\s*'.*?'\s*,\s*'.*?'\s*,\s*'.*?'\s*,\s*'(?<zoom>.*?)'\s*\)\s*;");
		static Regex PicPathRegex2 = new Regex(@"http://ec\d+.images-amazon.com/images/.+?0,0,300,390_.jpg", RegexOptions.Compiled);
		static Regex PicPathRegex3 = new Regex(@"registerImage\(\"".*?\"",\s*?\""(?<img>.+?)\"",\s*?.*\);", RegexOptions.Compiled);
		static Regex PicPathRegex4 = new Regex(@"""large"":\s*\["".+?"",\s*""(?<pic>.+?)""\],",RegexOptions.Compiled);
		static Regex RemoveQid = new Regex("&qid=\\d*");
		static Regex ReplaceSpace = new Regex(@"[\s\n\r]+",RegexOptions.Compiled);

		private AmazonHelper helper;
		private string storePath;
		public const string URL_PREFIX = "http://www.amazon.cn";
		private object logLock = new object();

		public AmazonPictureTaskGuide(string storePath,string connectionString)
		{
			this.storePath = storePath;
			this.helper  =new AmazonHelper();
			helper.StorePath = storePath;
			helper.ConnectionString = connectionString;
			helper.LoadTypes();
//			helper.InitTable();
		}

		public string StorePath
		{
			get { return storePath; }
			set { storePath = value; }
		}

		public override string GetLocalStorePath(Task task)
		{
			if (task.LocalPath != null)
			{
				return task.LocalPath;
			}
			else
			{
				string s;
				helper.GetLocalPath(task, out s);
				task.LocalPath = s;
				return s;
			}
			
		}

		public string GetString(byte[] bytes, Encoding encoding)
		{
			if (encoding != null)
			{
				return encoding.GetString(bytes);
			}
			else
			{
				return Encoding.Default.GetString(bytes);
			}
		}

		public override Task NewTask()
		{
			return new Task();
		}

		public override void SetUpRequest(HttpWebRequest request)
		{
			request.UserAgent = " Mozilla/5.0 (Windows NT 6.1; WOW64; rv:6.0.2) Gecko/20100101 Firefox/6.0.2";
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
			string filePath = GetLocalStorePath(task);
			if (File.Exists(filePath))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public override bool ShouldProcess(Task task)
		{
			AmazonTaskType att = (AmazonTaskType) task.Type;
			if (att == AmazonTaskType.IMAGE
				//|| att == AmazonTaskType.IMAGE_ORI
				)
			{
				return false;
			}
			return true;
		}

		public override bool ShouldStore(Task task)
		{
			return true;
		}


		protected override ContentProcessResult Process(TaskData td)
		{
			Task task = td.Task;
			byte[] bytes = td.Bytes;
			AmazonTaskType att = (AmazonTaskType) task.Type;
			ContentProcessResult result = new ContentProcessResult();
			//set to true, but could fail
			result.Success = true;
			if (att == AmazonTaskType.IMAGE 
				//|| att == AmazonTaskType.IMAGE_ORI
				)
			{
				//DoImage();
			}
			else
			{
				string html = GetString(bytes, null);
				if (!Validate(html))
				{
					result.Success = false;
					return result;
				}
				switch (att)
				{
					case AmazonTaskType.INDEX:
						DoIndex(task, html,result);
						break;
					case AmazonTaskType.CATEGORY:
//						DoCategory(task, html, result);
						break;
					case AmazonTaskType.MORE_CATEGORY:
						DoMoreCategory(task, html, result);
						break;
					case AmazonTaskType.PAGES:
						DoPages(task, html, result,null);
						break;
					case AmazonTaskType.PAGE:
						DoPage(task, html, result);
						break;
				}
			}
			return result;
		}

		private bool Validate(string html)
		{
			if (html.LastIndexOf("</html>", StringComparison.OrdinalIgnoreCase) < 0 ||
				html.IndexOf("amazon", StringComparison.OrdinalIgnoreCase) < 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}


		private void DoIndex(Task task,string html,ContentProcessResult result)
		{
			
			HtmlNode root = GetRoot(html);
			if (root == null)
			{
				TaskFail(task, result);
				return;
			}

			HtmlNodeCollection links = root.SelectNodes(".//div[@id='siteDirectory']//td//a");
			if (links == null)
			{
				return;
			}
			
			foreach (HtmlNode node in links)
			{
				string path = HAH.SafeGetAttributeStringValue(node, "href");
				if (path != null)
				{
					Match m = GetNodeID.Match(path);
					if (m.Success)
					{
						string sid = m.Groups["id"].Value;
						int id;
						if (Int32.TryParse(sid, out id))
						{
							//http://www.amazon.cn/gp/search/ref=sr_hi_1?rh=n%3A658390051&ie=UTF8

							Task t = MakeTask("http://www.amazon.cn/gp/search/ref=sr_hi_1?rh=n%3A" + id + "&ie=UTF8", AmazonTaskType.MORE_CATEGORY, null);
							result.NewTasks.Add(t);
						}
					}
				}
			}
		}

		private void DoMoreCategory(Task task, string html, ContentProcessResult result)
		{
			
			HtmlNode root = GetRoot(html);
			if (root == null || html.Contains("td.refinementContainer"))
			{
				TaskFail(task,result);
				return;
			}
			HtmlNodeCollection navs = root.SelectNodes(".//div[@id='refinements']//ul[@data-typeid='n']/li/a/span[@class='refinementLink']");
			if (navs == null)
			{
				//no more refinements
				DoPages(task, html, result, root);
			}
			else
			{
				foreach (HtmlNode nav in navs)
				{
					HtmlNode link = nav.ParentNode;
					if (link != null)
					{
						string path = HAH.SafeGetAttributeStringValue(link, "href");
						if (path != null)
						{
							result.NewTasks.Add(MakeTask(path,AmazonTaskType.MORE_CATEGORY,null));
						}
					}
				}
			}
		}
		private void DoPage(Task task, string html, ContentProcessResult result)
		{
			
//			HtmlNode root = GetRoot(html);
//			if (root == null)
//			{
//				TaskFail(task,result);
//				return;
//			}
			
			MatchCollection mc;
			if ((mc = PicPathRegex.Matches(html)).Count != 0)
			{
				foreach (Match m in mc)
				{
					string thumbPath = m.Groups["thumb"].Value;
					string zoomPath = m.Groups["zoom"].Value;
					if (!string.IsNullOrEmpty(thumbPath))
					{
						result.NewTasks.Add(MakeTask(thumbPath,AmazonTaskType.IMAGE,task.Context));
					}
//					if (!string.IsNullOrEmpty(zoomPath))
//					{
//						result.NewTasks.Add(MakeTask(thumbPath, AmazonTaskType.IMAGE_ORI, task.Context));
//					}
				}
			}
			else if ((mc = PicPathRegex2.Matches(html)).Count != 0)
			{
				foreach (Match m in mc)
				{
					string thumbPath = m.Value;
					if (!string.IsNullOrEmpty(thumbPath))
					{
						result.NewTasks.Add(MakeTask(thumbPath, AmazonTaskType.IMAGE, task.Context));
					}
				}
			}
			else if ((mc = PicPathRegex3.Matches(html)).Count != 0)
			{
				foreach (Match m in mc)
				{
					string thumbPath = m.Groups["img"].Value;
					if (!string.IsNullOrEmpty(thumbPath))
					{
						result.NewTasks.Add(MakeTask(thumbPath, AmazonTaskType.IMAGE, task.Context));
					}
				}
			}
			else if ((mc = PicPathRegex4.Matches(html)).Count != 0)
			{
				foreach (Match m in mc)
				{
					string picPath = m.Groups["pic"].Value;
					if (!string.IsNullOrEmpty(picPath))
					{
//						Console.WriteLine(picPath);
						result.NewTasks.Add(MakeTask(picPath, AmazonTaskType.IMAGE, task.Context));
					}
				}
			}
			else
			{
				Log("{0} has not picture", task.Url);
				TaskFail(task,result);
			}
		}

		private void DoPages(Task task, string html, ContentProcessResult result, HtmlNode root)
		{
			if (root == null)
			{
				root = GetRoot(html);
				if (root == null)
				{
					TaskFail(task,result);
					return;
				}
			}

			HtmlNodeCollection shops = root.SelectNodes(".//div[@id='rightResultsATF']//a[@class='title']");
			HtmlNode contextNode = root.SelectSingleNode("id('breadCrumb')");
			if (contextNode == null)
			{
				TaskFail(task,result);
				return;
			}
			string context = HttpUtility.HtmlDecode(contextNode.InnerText);
			context = ReplaceSpace.Replace(context, "");
			context = context.Replace('\\', ',');
			context = context.Replace('/', ',');
			context = context.Replace('›', '\\');
			if (shops != null)
			{
				foreach (HtmlNode node in shops)
				{
					string path = HAH.SafeGetAttributeStringValue(node, "href");
					result.NewTasks.Add(MakeTask(path, AmazonTaskType.PAGE,context));
				}
			}
			HtmlNode nextPageLink = root.SelectSingleNode(".//a[@id='pagnNextLink']");
			if (nextPageLink != null)
			{
				string path = HAH.SafeGetAttributeStringValue(nextPageLink, "href");
				if (path != null)
				{
					result.NewTasks.Add(MakeTask(path,AmazonTaskType.PAGES,null));
				}
			}
		}

		private HtmlNode GetRoot(string html)
		{
			HtmlDocument doc = new HtmlDocument();
			
			doc.LoadHtml(html);
			HtmlNode root = doc.DocumentNode;
			return root;
		}

		private void Log(string p, params object[] ds)
		{
			string s = string.Format(p + "\n", ds);
			try
			{
				lock (logLock)
				{
					File.AppendAllText("a.log", s);
				}
			}
			catch {}

		}

		private Task MakeTask(string path, AmazonTaskType taskType, string context)
		{
			Task t = new Task();
			if (!path.StartsWith("http://"))
			{
				//relative path
				t.Url = URL_PREFIX + path;
			}
			else
			{
				//absolute path
				t.Url = path;
			}
			t.Url = RemoveQid.Replace(t.Url, "");
			t.Type = (int)taskType;
			t.Context = context;
			return t;
		}

		private void TaskFail(Task task, ContentProcessResult result)
		{
			result.Success = false;
			result.NewTasks.Clear();
		}
	}
	public enum AmazonTaskType
	{
		//主大类分类
		// http://www.amazon.cn/gp/site-directory/
		INDEX = 0,
		// 初级子类，用于抓取所有“更多XX”的页面
		// http://www.amazon.cn/%E6%9C%8D%E9%A5%B0%E7%AE%B1%E5%8C%85-%E9%85%8D%E4%BB%B6/b/ref=sd_allcat_clo?ie=UTF8&node=2016156051
		CATEGORY = 1,
		//通过左侧“更多XX”进入详细子分类
		//子分类需要递归，通过refinementLink确定是否是子分类，提取refinementLink的父元素a
		//要检查Titile是不是“类别”，或data-typeid='n'
		// http://www.amazon.cn/s/ref=amb_link_29349772_14?ie=UTF8&sort=-launch-date&rh=n%3A2152154051&pf_rd_m=A1AJ19PSB66TGU&pf_rd_s=left-2&pf_rd_r=1PNN86PQ3WM230RMSXKP&pf_rd_t=101&pf_rd_p=60905352&pf_rd_i=2016156051
		MORE_CATEGORY = 2,

		//不存在refinementLink后，进入Pages状态
		//当前页面进入PAGE处理流程，并且加入下一个页面
		PAGES = 3,
		//提取每个商品链接，进入商品页面
		PAGE = 4,
		//单幅图（300*390）
		IMAGE = 5,
		//单幅图（原始）
//		IMAGE_ORI = 6
	}
}
