using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using TaobaoSpider.Model;
using Dapper;

namespace TaobaoSpider.BLL
{
	public class OpsSeller
	{
		private static string SQL_Insert =
			@"INSERT INTO [TaobaoGrab].[dbo].[Seller]([taobaoid],[credit],[starttime],[rmatch],[pmatch],[rservice],[pservice],[rspeed],[pspeed],[refunddays],[refundrate],[complaint],[penalty],[goodrate],[istmall],[pprotect],[psevendays],[pcharge],[preal],[pinvoice])
values (@taobaoid,@credit,@starttime,@rmatch,@pmatch,@rservice,@pservice,@rspeed,@pspeed,@refunddays,@refundrate,@complaint,@penalty,@goodrate,@istmall,@pprotect,@psevendays,@pcharge,@preal,@pinvoice)";

		private static string SQL_Select =
			@"SELECT [sellerid],[taobaoid],[credit],[starttime],[rmatch],[pmatch],[rservice],[pservice],[rspeed],[pspeed],[refunddays],[refundrate],[complaint],[penalty],[goodrate],[istmall],[pprotect],[psevendays],[pcharge],[preal],[pinvoice]
FROM [TaobaoGrab].[dbo].[Seller]
where sellerid = @sellerid";

		public static bool Insert(Seller i)
		{

			SqlConnection conn = Database.GetConnection();
			var id = conn.Query<decimal>(SQL_Insert, i);
			IEnumerator<decimal> e = id.GetEnumerator();

			int? gid = null;
			if (e.MoveNext())
			{
				gid = Convert.ToInt32(e.Current);
				i.SellerId = gid.Value;
			}
			return gid.HasValue;
		}
		public static Seller GetFirstModel(int id)
		{
			SqlConnection conn = Database.GetConnection();
			var items = conn.Query<Seller>(SQL_Select, new
			{
				itemid = id
			});
			var enumerator = items.GetEnumerator();
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
			else
			{
				return null;
			}
		}
	}
}
