using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace TaobaoSpider.BLL
{
	public class Database
	{
		private static string connectionString;
		static Database()
		{
			connectionString = ConfigurationManager.ConnectionStrings["default"].ConnectionString;
		}
		public static SqlConnection GetConnection()
		{
			SqlConnection conn = new SqlConnection(connectionString);
			conn.Open();
			return conn;
		}
	}
}
