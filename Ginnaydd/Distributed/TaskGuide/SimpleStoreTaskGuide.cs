using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Ginnaydd.Distributed.TaskGuide
{
	public class SimpleStoreTaskGuide: AbstractTaskGuide
	{
		public override string GetLocalStorePath(Task task)
		{
			string url = task.Url;
			url = url.Replace("http://","");
			return Path.Combine("f:/dev/", url);
		}

		public override bool ShouldProcess(Task task)
		{
			return true;
		}

		protected override ContentProcessResult Process(TaskData td)
		{
			throw new NotImplementedException();
		}

		public override bool ShouldDownload(Task task)
		{
			string path = GetLocalStorePath(task);

			if (File.Exists(path))
			{
				return false;
			}
			return true;
		}

		public override Task NewTask()
		{
			return new Task();
		}

		public override void SetUpRequest(HttpWebRequest request)
		{
			throw new NotImplementedException();
		}
	}
}
