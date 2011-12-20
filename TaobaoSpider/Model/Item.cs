using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaobaoSpider.Model
{
	public class Item
	{
		private int itemID;
		private int uniqID;
		private string name;
		private double price;
		private double freight;
		private string location;
		private int sellerTaobaoID;
		private string urlLink;
		private int recentDeal;

		public int ItemId
		{
			get { return itemID; }
			set { itemID = value; }
		}

		public int UniqId
		{
			get { return uniqID; }
			set { uniqID = value; }
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public double Price
		{
			get { return price; }
			set { price = value; }
		}

		public double Freight
		{
			get { return freight; }
			set { freight = value; }
		}

		public string Location
		{
			get { return location; }
			set { location = value; }
		}

		public int SellerTaobaoId
		{
			get { return sellerTaobaoID; }
			set { sellerTaobaoID = value; }
		}

		public string UrlLink
		{
			get { return urlLink; }
			set { urlLink = value; }
		}

		public int RecentDeal
		{
			get { return recentDeal; }
			set { recentDeal = value; }
		}

		override public string ToString()
		{
			string str = String.Empty;
			str = String.Concat(str, "ItemId = ", ItemId, "\r\n");
			str = String.Concat(str, "UniqId = ", UniqId, "\r\n");
			str = String.Concat(str, "Name = ", Name, "\r\n");
			str = String.Concat(str, "Price = ", Price, "\r\n");
			str = String.Concat(str, "Freight = ", Freight, "\r\n");
			str = String.Concat(str, "Location = ", Location, "\r\n");
			str = String.Concat(str, "SellerTaobaoId = ", SellerTaobaoId, "\r\n");
			str = String.Concat(str, "UrlLink = ", UrlLink, "\r\n");
			str = String.Concat(str, "RecentDeal = ", RecentDeal, "\r\n");
			return str;
		}
		
	}
}
