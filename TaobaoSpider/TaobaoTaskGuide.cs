using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using Ginnaydd.Distributed;

namespace TaobaoSpider
{
	public class TaobaoTaskGuide : AbstractTaskGuide
	{
		private string storePath;
		private string connectionString;
		public TaobaoTaskGuide(string storePath, string connectionString)
		{
			this.storePath = storePath;
			this.connectionString = connectionString;
		}
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
			ContentProcessResult cpr  =new ContentProcessResult();
			switch ((TaobaoTaskType)td.Task.Type)
			{
				case TaobaoTaskType.COMBINED_LIST:
					HandleCombinedList(td, cpr);
					break;
				case TaobaoTaskType.PROVIDER_LIST:
					HandleProviderList(td, cpr);
					break;
				case TaobaoTaskType.PROVIDER_RATE:
					HandleProviderRate(td, cpr);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void HandleProviderRate(TaskData td, ContentProcessResult cpr)
		{
			throw new NotImplementedException();
		}

		private void HandleProviderList(TaskData td, ContentProcessResult cpr)
		{
			throw new NotImplementedException();
		}

		private void HandleCombinedList(TaskData td, ContentProcessResult cpr)
		{
			throw new NotImplementedException();
		}
	}

	public enum TaobaoTaskType
	{
		COMBINED_LIST =0,
		PROVIDER_LIST = 1,
		PROVIDER_RATE = 2,


	}
}
