using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Wimlab.Utilities.HTML
{
	public class HtmlAgilityHelper
	{
		public static int? SafeGetSuccessorAttributeIntValue(HtmlNode node, string xpath, string attributeName)
		{
			if (node == null)
			{
				return null;
			}
			HtmlNode successor = node.SelectSingleNode(xpath);
			if (successor != null)
			{
				return SafeGetAttributeIntValue(successor, attributeName);
			}
			else
			{
				return null;
			}
		}
		public static double? SafeGetSuccessorAttributeDoubleValue(HtmlNode node, string xpath, string attributeName)
		{
			if (node == null)
			{
				return null;
			}
			HtmlNode successor = node.SelectSingleNode(xpath);
			if (successor != null)
			{
				return SafeGetAttributeDoubleValue(successor, attributeName);
			}
			else
			{
				return null;
			}
		}
		public static string SafeGetSuccessorAttributeStringValue(HtmlNode node, string xpath,string attributeName)
		{
			if (node == null)
			{
				return null;
			}
			HtmlNode successor = node.SelectSingleNode(xpath);
			if (successor != null)
			{
				return SafeGetAttributeStringValue(successor, attributeName);
			}
			else
			{
				return null;
			}
		}
		public static string SafeGetSuccessorInnerText(HtmlNode node, string xpath)
		{
			if (node == null)
			{
				return null;
			}
			HtmlNode successor = node.SelectSingleNode(xpath);
			if (successor != null)
			{
				return successor.InnerText;
			}
			else
			{
				return null;
			}
		}

		public static int? SafeGetAttributeIntValue(HtmlNode node, string attributeName)
		{
			if (node == null)
			{
				return null;
			}
			string s = SafeGetAttributeStringValue(node, attributeName);
			if (s != null)
			{
				return Int32.Parse(s);
			}
			else
			{
				return null;
			}
		}
		public static double? SafeGetAttributeDoubleValue(HtmlNode node, string attributeName)
		{
			if (node == null)
			{
				return null;
			}
			string s = SafeGetAttributeStringValue(node, attributeName);
			if (s != null)
			{
				return Double.Parse(s);
			}
			else
			{
				return null;
			}
		}
		public static string SafeGetAttributeStringValue(HtmlNode node, string attributeName)
		{
			if (node == null)
			{
				return null;
			}
			HtmlAttribute attr = node.Attributes[attributeName];
			if (attr != null)
			{
				return attr.Value;
			}
			else
			{
				return null;
			}
		}

		private static Regex ClearRegex = new Regex("(&nbsp;|[\\t\\r\\n])+",RegexOptions.Compiled);
		public static string ClearText(string text)
		{
			return ClearText(text, false);
		}
		public static string ClearText(string text,bool removeSpace)
		{
			if (removeSpace)
			{
				text = ClearRegex.Replace(text, "");
				text = text.Replace(" ", "");
			}
			else
			{
				text = ClearRegex.Replace(text, " ");
			}
			
			text = text.Replace("&lt;", "<");
			text = text.Replace("&gt;", ">");
			text = text.Replace("&amp;", "&");
			return text;
		}

		public static string SafeGetSuccessorInnerHtml(HtmlNode node, string xpath)
		{
			if (node == null)
			{
				return null;
			}
			HtmlNode successor = node.SelectSingleNode(xpath);
			if (successor != null)
			{
				return successor.InnerHtml;
			}
			else
			{
				return null;
			}
		}
	}

}
