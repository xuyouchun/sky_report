using System;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	#region �¼�

	/// <summary>
	/// ReportCell��ע������¼�
	/// </summary>
	public class Event : System.Web.UI.HtmlControls.HtmlContainerControl
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		public Event() {}

		string _Name = null;
		/// <summary>
		/// �¼�����
		/// </summary>
		public string Name
		{
			get { return this._Name;  }
			set { this._Name = value; }
		}

		string _Function = null;
		/// <summary>
		/// �¼�Ҫִ�еĺ���
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
		/// ������
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
		/// ����
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

	#region �¼���

	/// <summary>
	/// ReportCell���¼���
	/// </summary>
	public class EventGroup : ItemGroup
	{

		/// <summary>
		/// ���캯��
		/// </summary>
		public EventGroup(){}

		/// <summary>
		/// ���һ��
		/// </summary>
		/// <param name="myEvent"></param>
		public void Append(Event myEvent)
		{
			if(myEvent.Name == null)	return;
			string Key = myEvent.Name.ToLower();
			if(!this.myHashtable.ContainsKey(Key))	myHashtable.Add(Key, myEvent);
		}

		/// <summary>
		/// ɾ��ָ������
		/// </summary>
		/// <param name="Key"></param>
		public void Remove(string Key)
		{
			if(Key == null)	return;
			Key = Key.ToLower();
			if(this.myHashtable.ContainsKey(Key))	myHashtable.Remove(Key);
		}

		/// <summary>
		/// �������������е��¼�
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
		/// ������ASPX�ļ��������
		/// </summary>
		public Event It
		{
			set { this.Append(value); }
		}
	}

	#endregion


}
