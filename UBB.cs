using System;
using System.Text.RegularExpressions;
using System.Collections;

namespace Skyever.Report
{
	/// <summary>
	/// 支持UBB代码，提供UBB到HTML的转换功能和缓存功能
	/// </summary>
	public class UBB
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public UBB()
		{
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="MaxItem">所允许最大缓存数</param>
		public UBB(int MaxItem)
		{
			this.MaxItem = MaxItem;
		}

		int _MaxItem = 0;
		/// <summary>
		/// 所允许的最大缓存数
		/// </summary>
		public int MaxItem
		{
			get { return this._MaxItem;  }
			set { this._MaxItem = value; }
		}

		bool _IsSupportHtmlTag = false;
		/// <summary>
		/// 是否支持HTML格式的标记，形如：[html:div style="width:500px"]我爱北京天安门[/html:div]
		/// </summary>
		public bool IsSupportHtmlTag
		{
			get { return this._IsSupportHtmlTag;  }
			set { this._IsSupportHtmlTag = value; }
		}

		/// <summary>
		/// 获取指定UBB字符串的HTML代码
		/// </summary>
		/// <param name="UBBStr"></param>
		/// <returns></returns>
		public string GetHtmlCode(string UBBStr)
		{
			if(UBBStr==null)	return null;

			//如果不使用缓存，则直接返回转换结果
			if(this.MaxItem==0)		return	GetHtmlCodeForUBB(UBBStr);

			Hashtable myHashtable = this.myCache.Target as Hashtable;

			if(myHashtable == null)
			{
				myHashtable = new Hashtable();
				this.myCache.Target = myHashtable;
			}

			//检查缓存中是否有其转换结果
			int HashCode = UBBStr.GetHashCode();
			string result = myHashtable[HashCode] as string;
			if(result != null)		return result;

			//开始转换并保存转换结果
			result = this.GetHtmlCodeForUBB(UBBStr);
			if(this.MaxItem > 0)
			{
				if(myHashtable.Count >= this.MaxItem)	myHashtable.Clear();
				myHashtable.Add(HashCode, result);
			}

			return result;
		}

		private System.WeakReference myCache = new WeakReference(new Hashtable());

		/// <summary>
		/// 获取指定UBB字符串的HTML代码
		/// </summary>
		private string GetHtmlCodeForUBB(string UBBStr)
		{
			if(UBBStr == null)		return null;
			if(this.IsSupportHtmlTag)	UBBStr = UBBStr.Replace('\n', ' ').Replace('\r', ' ');
			return myRegex.Replace(UBBStr, new MatchEvaluator(this.ReplaceUBBTag));
		}
		private Regex myRegex = new Regex(@"\[([^\s=]+)[^\]]*\][\s\S]*?\[/\1\]|\[[^/]+/\]", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private Regex FindTag = new Regex(@"(?<=\[)[^\s\]=/]+", RegexOptions.Compiled);
		private Regex FirstArgument = new Regex(@"\S+", RegexOptions.Compiled);
		private Regex MatchArgument = new Regex(@"\S+\s*=\s*[^,\s]+", RegexOptions.Compiled | RegexOptions.Multiline);
		/// <summary>
		/// 为UBB标记提供替换的方法
		/// </summary>
		/// <param name="myMatch"></param>
		/// <returns></returns>
		private string ReplaceUBBTag(Match myMatch)
		{
			string Content = myMatch.Value;
			int Length = Content.Length;
			string result = null;

			string UBBTag		= FindTag.Match(Content).Value.ToUpper();
			string UBBContent	= null;
			string UBBArgument  = null;
			if(Content[Length-2]=='/' && Content[Length-1]==']')
			{
				UBBContent  = "";
				UBBArgument = Content.Substring(UBBTag.Length+1, Length-UBBTag.Length-3);
			}
			else
			{
				int begin = Content.IndexOf(']')+1, end = Content.LastIndexOf('[')-1;
				UBBContent  = myRegex.Replace(Content.Substring(begin, end-begin+1), new MatchEvaluator(this.ReplaceUBBTag));
				UBBArgument = Content.Substring(UBBTag.Length+1, begin-UBBTag.Length-2).Trim();
			}
			if(UBBTag == "")	return null;

			if(this.OnReplaceUBBTag != null)	result = this.OnReplaceUBBTag(UBBTag, UBBArgument, UBBContent);
			if(result != null)		return result;

			System.Text.StringBuilder HtmlCode = new System.Text.StringBuilder();
			switch(UBBTag)
			{
				#region 一些简单的替换
				case "B": case "I": case "U": case "STRIKE": case "SUP": case "SUB":
				case "H1": case "H2": case "H3": case "H4": case "H5": case "H6":
					HtmlCode.Append("<");
					HtmlCode.Append(UBBTag);
					HtmlCode.Append(">");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</");
					HtmlCode.Append(UBBTag);
					HtmlCode.Append(">");
					break;
				#endregion

				#region 较为复杂的HTML替换
				case "URL":		//超级链接
					if(UBBArgument == "")
					{
						HtmlCode.Append("<A href=\"");
						HtmlCode.Append(UBBContent.Replace("\"", "&quot;").Trim());
						HtmlCode.Append("\">");
						HtmlCode.Append(UBBContent);
						HtmlCode.Append("</A>");
					}
					else if(UBBArgument[0]=='=')
					{
						HtmlCode.Append("<A href=\"");
						HtmlCode.Append(UBBArgument.Substring(1).Replace("\"", "&quot;").Trim());
						HtmlCode.Append("\">");
						HtmlCode.Append(UBBContent);
						HtmlCode.Append("</A>");
					}
					else return Content;
					break;
				case "EMAIL":	//超级链接
					if(UBBArgument == "")
					{
						HtmlCode.Append("<A href=\"mailto:");
						HtmlCode.Append(UBBContent.Replace("\"", "&quot;").Trim());
						HtmlCode.Append("\">");
						HtmlCode.Append(UBBContent);
						HtmlCode.Append("</A>");
					}
					else if(UBBArgument[0]=='=')
					{
						HtmlCode.Append("<A href=\"mailto:");
						HtmlCode.Append(UBBArgument.Replace("\"", "&quot;").Trim());
						HtmlCode.Append("\">");
						HtmlCode.Append(UBBContent);
						HtmlCode.Append("</A>");
					}
					else return Content;
					break;
				case "COLOR":	//改变颜色
					if(UBBArgument.Length == 0)		return Content;
					if(UBBArgument[0]=='=')
					{
						HtmlCode.Append("<FONT color=");
						HtmlCode.Append(FirstArgument.Match(UBBArgument, 1).Value);
						HtmlCode.Append(">");
						HtmlCode.Append(UBBContent);
						HtmlCode.Append("</FONT>");
					}
					else	return Content;
					break;
				case "FONT":	//改变字体
					if(UBBArgument.Length == 0)		return Content;
					if(UBBArgument[0]=='=')
					{
						HtmlCode.Append("<FONT face=");
						HtmlCode.Append(FirstArgument.Match(UBBArgument,1).Value);
						HtmlCode.Append(">");
						HtmlCode.Append(UBBContent);
						HtmlCode.Append("</FONT>");
					}
					else	return Content;
					break;
				case "SIZE":	//改变字号
					if(UBBArgument.Length == 0)		return Content;
					if(UBBArgument[0]=='=')
					{
						HtmlCode.Append("<FONT size=");
						HtmlCode.Append(FirstArgument.Match(UBBArgument,1).Value);
						HtmlCode.Append(">");
						HtmlCode.Append(UBBContent);
						HtmlCode.Append("</FONT>");
					}
					else	return Content;
					break;
				case "IMAGE": case "IMG":	//图片
					HtmlCode.Append("<IMG src=\"");
					HtmlCode.Append(UBBContent.Replace("\"", "&quot;").Trim());
					HtmlCode.Append("\">");
					break;
				case "CENTER":				//居中
					HtmlCode.Append("<center>");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</center>");
					break;
				case "HR":					//横线
					HtmlCode.Append("<HR>");
					break;
					#endregion

				#region 滤镜效果
				case "FLY":		//飞行文字
					HtmlCode.Append("<MARQUEE scrollAmount=3 behavior=alternate width='98%'>");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</MARQUEE>");
					break;
				case "MOVE":	//滚动文字
					HtmlCode.Append("<MARQUEE scrollAmount=3 width='98%'>");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</MARQUEE>");
					break;
				case "GLOW":			//发光
					if(UBBArgument=="")
					{
						HtmlCode.Append("<TABLE cellpadding=0 cellspacing=0 style='filter:glow(color=yellow, strength=1)'>");
						HtmlCode.Append(UBBContent);
						HtmlCode.Append("</TABLE>");
					}
					else if(UBBArgument[0]==':')
					{
						HtmlCode.Append("<TABLE cellpadding=0 cellspacing=0 style='filter:glow(color=");
						HtmlCode.Append(FirstArgument.Match(UBBArgument, 1).Value);
						HtmlCode.Append(", strength=1)'>");
						HtmlCode.Append(UBBContent);
						HtmlCode.Append("</TABLE>");
					}
					else return Content;
					break;
				case "DROPSHADOW":		//阴影
					if(UBBArgument=="")
					{
						HtmlCode.Append("<TABLE cellpadding=0 cellspacing=0 style='filter:dropshadow(color=#00f0ff)'>");
						HtmlCode.Append(UBBContent);
						HtmlCode.Append("</TABLE>");
					}
					else if(UBBArgument[0]==':')
					{
						HtmlCode.Append("<TABLE cellpadding=0 cellspacing=0 style='filter:dropshadow(color=");
						HtmlCode.Append(FirstArgument.Match(UBBArgument, 1).Value);
						HtmlCode.Append(")'>");
						HtmlCode.Append(UBBContent);
						HtmlCode.Append("</TABLE>");
					}
					else return Content;
					break;
				case "FLIPH":			//横翻
					HtmlCode.Append("<TABLE cellpadding=0 cellspacing=0 style='filter:fliph'>");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</TABLE>");
					break;
				case "FLIPV":			//纵翻
					HtmlCode.Append("<TABLE cellpadding=0 cellspacing=0 style='filter:flipv'>");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</TABLE>");
					break;
				case "INVERT":			//底片
					HtmlCode.Append("<TABLE cellpadding=0 cellspacing=0 style='filter:invert'>");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</TABLE>");
					break;
				case "GRAY":			//灰度
					HtmlCode.Append("<TABLE cellpadding=0 cellspacing=0 style='filter:gray'>");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</TABLE>");
					break;
				case "BLUR":			//模糊
					HtmlCode.Append("<TABLE cellpadding=0 cellspacing=0 style='filter:blur(Add=0, direction=6, strength=2, width=50)'>");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</TABLE>");
					break;
				case "FILTER":			//各种滤镜效果的组合
					HtmlCode.Append("<TABLE cellpadding=0 cellspacing=0 style='filter: ");
					HtmlCode.Append(UBBArgument.Replace("'", ""));
					HtmlCode.Append(")'>");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</TABLE>");
					break;
				#endregion

				#region 注释

				case "REM":	break;

				#endregion

				#region HTML形式的标记

				default:
				{
					if(this.IsSupportHtmlTag && UBBTag.StartsWith("HTML:"))
					{
						string TagName = UBBTag.Substring(5);
						switch(TagName)
						{
							case "STYLE":
								HtmlCode.Append("<span style='display:none'>\\n<");
								HtmlCode.Append(TagName);
								if(UBBArgument != "")
								{
									HtmlCode.Append(" ");
									HtmlCode.Append(UBBArgument);
								}
								HtmlCode.Append(">");
								HtmlCode.Append(UBBContent);
								HtmlCode.Append("</");
								HtmlCode.Append(TagName);
								HtmlCode.Append("></span>");
								break;
							case "BR":	case "INPUT": case "HR":
								HtmlCode.Append("<");
								HtmlCode.Append(TagName);
								if(UBBArgument != "")
								{
									HtmlCode.Append(" ");
									HtmlCode.Append(UBBArgument);
								}
								HtmlCode.Append("/>");
								break;
							default:
								HtmlCode.Append("<");
								HtmlCode.Append(TagName);
								if(UBBArgument != "")
								{
									HtmlCode.Append(" ");
									HtmlCode.Append(UBBArgument);
								}
								HtmlCode.Append(">");
								HtmlCode.Append(UBBContent);
								HtmlCode.Append("</");
								HtmlCode.Append(TagName);
								HtmlCode.Append(">");
								break;
						}
					}
					else	HtmlCode.Append(Content);
					break;
				}
				#endregion
			}

			return HtmlCode.ToString();
		}

		/// <summary>
		/// 在转换UBB标记时，规定自己的转换方法，如果返回null则使用默认的转换方法
		/// </summary>
		public event OnReplaceUBBTag OnReplaceUBBTag = null;
	}

	/// <summary>
	/// UBB：在转换UBB标记时，规定自己的转换方法，如果返回null则使用默认的转换方法
	/// </summary>
	public delegate string OnReplaceUBBTag(string UBBTag, string UBBArgument, string UBBContent);
}
