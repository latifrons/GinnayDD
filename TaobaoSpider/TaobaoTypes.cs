using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaobaoSpider
{
	public class Seller
	{
		private int sellerId;
		private int taobaoID;
		private int? credit;
		private DateTime? startTime;
		private double rmatch;
		private double pmatch;
		private double rservice;
		private double pservice;
		private double rspeed;
		private double pspeed;
		private int refunddays;
		private double refundrate;
		private int complaint;
		private int penalty;
		private double? goodrate;
		private bool isTmall;
		private bool? pprotect;
		private bool? psevendays;
		private bool? pcharge;
		private bool? preal;
		private bool? pinvoice;

		public int SellerId
		{
			get { return sellerId; }
			set { sellerId = value; }
		}

		public int TaobaoId
		{
			get { return taobaoID; }
			set { taobaoID = value; }
		}

		public int? Credit
		{
			get { return credit; }
			set { credit = value; }
		}

		public DateTime? StartTime
		{
			get { return startTime; }
			set { startTime = value; }
		}

		public double Rmatch
		{
			get { return rmatch; }
			set { rmatch = value; }
		}

		public double Pmatch
		{
			get { return pmatch; }
			set { pmatch = value; }
		}

		public double Rservice
		{
			get { return rservice; }
			set { rservice = value; }
		}

		public double Pservice
		{
			get { return pservice; }
			set { pservice = value; }
		}

		public double Rspeed
		{
			get { return rspeed; }
			set { rspeed = value; }
		}

		public double Pspeed
		{
			get { return pspeed; }
			set { pspeed = value; }
		}

		public int Refunddays
		{
			get { return refunddays; }
			set { refunddays = value; }
		}

		public double Refundrate
		{
			get { return refundrate; }
			set { refundrate = value; }
		}

		public int Complaint
		{
			get { return complaint; }
			set { complaint = value; }
		}

		public int Penalty
		{
			get { return penalty; }
			set { penalty = value; }
		}

		public double? Goodrate
		{
			get { return goodrate; }
			set { goodrate = value; }
		}

		public bool IsTmall
		{
			get { return isTmall; }
			set { isTmall = value; }
		}

		public bool? Pprotect
		{
			get { return pprotect; }
			set { pprotect = value; }
		}

		public bool? Psevendays
		{
			get { return psevendays; }
			set { psevendays = value; }
		}

		public bool? Pcharge
		{
			get { return pcharge; }
			set { pcharge = value; }
		}

		public bool? Preal
		{
			get { return preal; }
			set { preal = value; }
		}

		public bool? Pinvoice
		{
			get { return pinvoice; }
			set { pinvoice = value; }
		}
	}
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
	}
}
