using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using TaobaoSpider.Model;
using Dapper;

namespace TaobaoSpider.BLL
{
	public class OpsItem
	{
		#region Static SQL String Memebers
		/// <remarks>This field represents the full SELECT string for the table Item, with the WHERE clause.</remarks>
		internal static string _SQL_Select = "SELECT [itemid], [uniqid], [name], [price], [freight], [location], [sellertaobaoid], [urllink], [recentdeal] FROM [dbo].[Item] WHERE [itemid]=@itemid ";

		/// <remarks>This field represents the full INSERT INTO string for the table Item.</remarks>
		internal static string _SQL_Insert = @"INSERT INTO [TaobaoGrab].[dbo].[Item]
           ([uniqid],[name],[price],[freight],[location],[sellertaobaoid],[urllink],[recentdeal])
VALUES(
@uniqid,@name,@price,@freight,@location,@sellertaobaoid,@urllink,@recentdeal
);
select SCOPE_IDENTITY();
";

		/// <remarks>This field represents the full UPDATE string for the table Item, with the WHERE clause.</remarks>
		internal static string _SQL_Update = "UPDATE [dbo].[Item] SET [itemid] = @itemid, [uniqid] = @uniqid, [name] = @name, [price] = @price, [freight] = @freight, [location] = @location, [sellertaobaoid] = @sellertaobaoid, [urllink] = @urllink, [recentdeal] = @recentdeal WHERE [itemid]=@itemid ";

		/// <remarks>This field represents the DELETE string for the table Item, with the WHERE clause.</remarks>
		internal static string _SQL_Delete = "DELETE FROM [dbo].[Item] WHERE [itemid]=@itemid ";
		#endregion


		public static bool Insert(Item i)
		{
		
			SqlConnection conn = Database.GetConnection();
			var id = conn.Query<decimal>(_SQL_Insert, i);
			IEnumerator<decimal> e = id.GetEnumerator();

			int? gid =null;
			if (e.MoveNext())
			{
				gid = Convert.ToInt32(e.Current);
				i.ItemId = gid.Value;
			}
			return gid.HasValue;
		}
		public static Item GetFirstModel(int id)
		{
			SqlConnection conn = Database.GetConnection();
			var items = conn.Query<Item>(_SQL_Select, new
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
