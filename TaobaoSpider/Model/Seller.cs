using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaobaoSpider.Model
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

		public override string ToString()
		{
			return string.Format("SellerId: {0}, TaobaoId: {1}, Credit: {2}, StartTime: {3}, Rmatch: {4}, Pmatch: {5}, Rservice: {6}, Pservice: {7}, Rspeed: {8}, Pspeed: {9}, Refunddays: {10}, Refundrate: {11}, Complaint: {12}, Penalty: {13}, Goodrate: {14}, IsTmall: {15}, Pprotect: {16}, Psevendays: {17}, Pcharge: {18}, Preal: {19}, Pinvoice: {20}", sellerId, taobaoID, credit, startTime, rmatch, pmatch, rservice, pservice, rspeed, pspeed, refunddays, refundrate, complaint, penalty, goodrate, isTmall, pprotect, psevendays, pcharge, preal, pinvoice);
		}
	}
}
