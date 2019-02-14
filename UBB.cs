using System;
using System.Text.RegularExpressions;
using System.Collections;

namespace Skyever.Report
{
	/// <summary>
	/// ֧��UBB���룬�ṩUBB��HTML��ת�����ܺͻ��湦��
	/// </summary>
	public class UBB
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		public UBB()
		{
		}

		/// <summary>
		/// ���캯��
		/// </summary>
		/// <param name="MaxItem">��������󻺴���</param>
		public UBB(int MaxItem)
		{
			this.MaxItem = MaxItem;
		}

		int _MaxItem = 0;
		/// <summary>
		/// ���������󻺴���
		/// </summary>
		public int MaxItem
		{
			get { return this._MaxItem;  }
			set { this._MaxItem = value; }
		}

		bool _IsSupportHtmlTag = false;
		/// <summary>
		/// �Ƿ�֧��HTML��ʽ�ı�ǣ����磺[html:div style="width:500px"]�Ұ������찲��[/html:div]
		/// </summary>
		public bool IsSupportHtmlTag
		{
			get { return this._IsSupportHtmlTag;  }
			set { this._IsSupportHtmlTag = value; }
		}

		/// <summary>
		/// ��ȡָ��UBB�ַ�����HTML����
		/// </summary>
		/// <param name="UBBStr"></param>
		/// <returns></returns>
		public string GetHtmlCode(string UBBStr)
		{
			if(UBBStr==null)	return null;

			//�����ʹ�û��棬��ֱ�ӷ���ת�����
			if(this.MaxItem==0)		return	GetHtmlCodeForUBB(UBBStr);

			Hashtable myHashtable = this.myCache.Target as Hashtable;

			if(myHashtable == null)
			{
				myHashtable = new Hashtable();
				this.myCache.Target = myHashtable;
			}

			//��黺�����Ƿ�����ת�����
			int HashCode = UBBStr.GetHashCode();
			string result = myHashtable[HashCode] as string;
			if(result != null)		return result;

			//��ʼת��������ת�����
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
		/// ��ȡָ��UBB�ַ�����HTML����
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
		/// ΪUBB����ṩ�滻�ķ���
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
				#region һЩ�򵥵��滻
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

				#region ��Ϊ���ӵ�HTML�滻
				case "URL":		//��������
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
				case "EMAIL":	//��������
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
				case "COLOR":	//�ı���ɫ
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
				case "FONT":	//�ı�����
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
				case "SIZE":	//�ı��ֺ�
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
				case "IMAGE": case "IMG":	//ͼƬ
					HtmlCode.Append("<IMG src=\"");
					HtmlCode.Append(UBBContent.Replace("\"", "&quot;").Trim());
					HtmlCode.Append("\">");
					break;
				case "CENTER":				//����
					HtmlCode.Append("<center>");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</center>");
					break;
				case "HR":					//����
					HtmlCode.Append("<HR>");
					break;
					#endregion

				#region �˾�Ч��
				case "FLY":		//��������
					HtmlCode.Append("<MARQUEE scrollAmount=3 behavior=alternate width='98%'>");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</MARQUEE>");
					break;
				case "MOVE":	//��������
					HtmlCode.Append("<MARQUEE scrollAmount=3 width='98%'>");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</MARQUEE>");
					break;
				case "GLOW":			//����
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
				case "DROPSHADOW":		//��Ӱ
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
				case "FLIPH":			//�ᷭ
					HtmlCode.Append("<TABLE cellpadding=0 cellspacing=0 style='filter:fliph'>");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</TABLE>");
					break;
				case "FLIPV":			//�ݷ�
					HtmlCode.Append("<TABLE cellpadding=0 cellspacing=0 style='filter:flipv'>");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</TABLE>");
					break;
				case "INVERT":			//��Ƭ
					HtmlCode.Append("<TABLE cellpadding=0 cellspacing=0 style='filter:invert'>");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</TABLE>");
					break;
				case "GRAY":			//�Ҷ�
					HtmlCode.Append("<TABLE cellpadding=0 cellspacing=0 style='filter:gray'>");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</TABLE>");
					break;
				case "BLUR":			//ģ��
					HtmlCode.Append("<TABLE cellpadding=0 cellspacing=0 style='filter:blur(Add=0, direction=6, strength=2, width=50)'>");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</TABLE>");
					break;
				case "FILTER":			//�����˾�Ч�������
					HtmlCode.Append("<TABLE cellpadding=0 cellspacing=0 style='filter: ");
					HtmlCode.Append(UBBArgument.Replace("'", ""));
					HtmlCode.Append(")'>");
					HtmlCode.Append(UBBContent);
					HtmlCode.Append("</TABLE>");
					break;
				#endregion

				#region ע��

				case "REM":	break;

				#endregion

				#region HTML��ʽ�ı��

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
		/// ��ת��UBB���ʱ���涨�Լ���ת���������������null��ʹ��Ĭ�ϵ�ת������
		/// </summary>
		public event OnReplaceUBBTag OnReplaceUBBTag = null;
	}

	/// <summary>
	/// UBB����ת��UBB���ʱ���涨�Լ���ת���������������null��ʹ��Ĭ�ϵ�ת������
	/// </summary>
	public delegate string OnReplaceUBBTag(string UBBTag, string UBBArgument, string UBBContent);
}
