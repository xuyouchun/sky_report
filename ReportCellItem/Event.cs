using System;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	#region 事件

	/// <summary>
	/// ReportCell：注册各种事件
	/// </summary>
	public class Event : System.Web.UI.HtmlControls.HtmlContainerControl
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public Event() {}

		string _Name = null;
		/// <summary>
		/// 事件名字
		/// </summary>
		public string Name
		{
			get { return this._Name;  }
			set { this._Name = value; }
		}

		string _Function = null;
		/// <summary>
		/// 事件要执行的函数
		/// </summary>
		public string Function
		{
			get { return this._Function;  }
			set
			{
				if(value==null)		this._Function = null;
				else				this._Function = value.Trim().TrimEnd('(', ')', ' ');
			}
		}

		static Regex FindScriptHead = new Regex(@"^\s*<script[^>]*>(.*?)</script>\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
		static Regex FindLanguage = new Regex(@"<script[^>]*?language\s*=\s*([^>\s]+)[^>]*>", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

		/// <summary>
		/// 函数体
		/// </summary>
		public string FunctionBody
		{
			get
			{
				if(this._FunctionBody==null)
				{
					string myFunc = this.InnerHtml.Trim();
					Match myMatch = FindScriptHead.Match(myFunc);
					if(myMatch.Value != "")
					{
						this._FunctionBody = myMatch.Groups[1].Value.Trim();
						Match Language = FindLanguage.Match(myFunc);
						if(Language.Value != "")	this.Language = Language.Groups[1].Value.Trim('\'', '"', ' ');
					}
					else this._FunctionBody = myFunc;
				}
				return this._FunctionBody;
			}
			set
			{
				this.InnerHtml = value;
				this._FunctionBody = null;
			}
		}

		private string _FunctionBody = null;

		private string _Language = null;

		/// <summary>
		/// 语言
		/// </summary>
		public string Language
		{
			get
			{
				if(this._Language == null)
				{
					this._FunctionBody = FunctionBody;
					if(this._Language==null)	this._Language = "javascript";
				}
				return this._Language;
			}
			set { this._Language = value; }
		}
	}

	#endregion

	#region 事件组

	/// <summary>
	/// ReportCell：事件组
	/// </summary>
	public class EventGroup : ItemGroup
	{

		/// <summary>
		/// 构造函数
		/// </summary>
		public EventGroup(){}

		/// <summary>
		/// 添加一项
		/// </summary>
		/// <param name="myEvent"></param>
		public void Append(Event myEvent)
		{
			if(myEvent.Name == null)	return;
			string Key = myEvent.Name.ToLower();
			if(!this.myHashtable.ContainsKey(Key))	myHashtable.Add(Key, myEvent);
		}

		/// <summary>
		/// 删除指定的项
		/// </summary>
		/// <param name="Key"></param>
		public void Remove(string Key)
		{
			if(Key == null)	return;
			Key = Key.ToLower();
			if(this.myHashtable.ContainsKey(Key))	myHashtable.Remove(Key);
		}

		/// <summary>
		/// 按索引访问组中的事件
		/// </summary>
		public Event this[string Key]
		{
			get
			{
				if(Key==null)	return null;
				Key = Key.ToLower();
				return this.myHashtable[Key] as Event;
			}
		}

		/// <summary>
		/// 方便在ASPX文件中添加项
		/// </summary>
		public Event It
		{
			set { this.Append(value); }
		}
	}

	#endregion


}
