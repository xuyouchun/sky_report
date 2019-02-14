using System;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	#region �е���ʽ
	/// <summary>
	/// ReportCell�����������ʽ
	/// </summary>
	public class ColStyle : ItemStyle
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		public ColStyle(){}

		/// <summary>
		/// ���캯��
		/// </summary>
		/// <param name="Name">����</param>
		/// <param name="Pattern">��ʽ</param>
		public ColStyle(string Name, string Pattern) : base(Pattern)
		{
			this.Name = Name;
		}

		private string _FormatString = null;
		bool IsUpdateFormatString = false;
		/// <summary>
		/// ��ʽ���ַ���
		/// </summary>
		public string FormatString
		{
			get
			{
				if(this.IsUpdateFormatString == false)
				{
					if(this._FormatString == null)
					{
						string InnerHTML = this.InnerHtml.Trim();
						if(InnerHTML != "")		this._FormatString = InnerHTML;
					}
					if(this._FormatString != null)		this.FormatArguments();
					this.IsUpdateFormatString = true;
				}
				return this._FormatString;
			}
			set
			{
				this._FormatString = value;
				this.IsUpdateFormatString = false;
			}
		}

		private string _Striking = null;
		/// <summary>
		/// ��Ŀ�ı���
		/// </summary>
		public string Striking
		{
			get { return this._Striking;  }
			set
			{
				this._Striking = value;
				if(value==null)		this.arrStriking = null;
				else				this.arrStriking = value.Split('\t', '\n', '\r');
			}
		}

		private string[] arrStriking = null;
		/// <summary>
		/// ��Ŀ�ı���
		/// </summary>
		internal string[] ArrStriking
		{
			get { return this.arrStriking; }
		}


		private string _ClassName = null;
		/// <summary>
		/// ��ʽ�����֣�Ĭ��Ϊ�ֶ���
		/// </summary>
		public string ClassName
		{
			get
			{
				if(this._ClassName == null)		this._ClassName = this.Name;
				return this._ClassName;
			}
			set
			{
				this._ClassName = value;
			}
		}

		private string _Title = null;
		/// <summary>
		/// ����
		/// </summary>
		public string Title
		{
			get
			{
				if(this._Title == null)		this._Title = this.Name;
				return this._Title;
			}
			set { this._Title = value; }
		}

		static Regex ReplaceSpace = new Regex(@"\s+", RegexOptions.Multiline | RegexOptions.Compiled);
		/// <summary>
		/// ֵ
		/// </summary>
		public string Values
		{
			get{ return this._Values; }
			set
			{
				this._Values = value.ToLower();
				this.IsUpdateFormatString = false;
			}
		}
		private string _Values = null;

		/// <summary>
		/// �淶��ʽ���ַ����Ͳ�����
		/// </summary>
		private void FormatArguments()
		{
			if(this._FormatString==null)	return;
			if(this._Values == null)		this._Values = this.Name;
			this.Value = this._Values.Split(',');
			this.argumentcount = this.Value.Length;
			this.myValues = new System.Text.StringBuilder(this._Values);
			this._FormatString = FindUnknownArgument.Replace(this._FormatString, new MatchEvaluator(this.ReplaceUnknownArgument));
			this._Values = myValues.ToString();
			this.Value = ReplaceSpace.Replace(this._Values, "").Split(',');
		}

		static private Regex FindUnknownArgument = new Regex(@"\{\D[^\{\}\n]*\}", RegexOptions.Multiline | RegexOptions.Compiled);

		int argumentcount = 0;
		System.Text.StringBuilder myValues = null;
		private string ReplaceUnknownArgument(Match myMatch)
		{
			string result = myMatch.Value.Trim('{', '}');
			if(ReportCell.IsVariable(result))	return myMatch.Value;

			ArgumentGroup Arguments = this.Group.ParentReport.Argument;
			if(Arguments.IsContainsKey(result))
			{
				string Value = Arguments[result];
				if(Value!="{" && Value!="}")		return Value;
			}

			this.myValues.Append(",");
			this.myValues.Append(result);
			return "{" + (argumentcount++).ToString() + "}";
		}

		private string [] _Value;
		/// <summary>
		/// �������飬��Ϊ��ʽ���ַ����Ĳ���
		/// </summary>
		internal protected string [] Value
		{
			get
			{
				if(this._Value == null)	
				{
					this._Value = new string[] { this.Name };
					this._Values = this.Name;
				}
				return this._Value;
			}
			set	{ this._Value = value; }
		}

		private string _Name = null;
		/// <summary>
		/// �ֶ���
		/// </summary>
		public string Name
		{
			get { return this._Name;  }
			set { this._Name = value; }
		}

		private bool _IsShow = true;
		/// <summary>
		/// �Ƿ���ʾ
		/// </summary>
		public bool IsShow
		{
			get { return this._IsShow;  }
			set { this._IsShow = value; }
		}

		private ColStyleGroup _Group;
		/// <summary>
		/// ������ʽ��
		/// </summary>
		internal ColStyleGroup Group
		{
			get { return this._Group;  }
			set { this._Group = value;}
		}

		/// <summary>
		/// ����������ʽ���ַ���
		/// </summary>
		public string Filter
		{
			get { return this._Filter;  }
			set	{ this._Filter = value;	}
		}
		private string _Filter = null;

		internal int colindex = -1;

		bool _IsSupportUBB = false;
		/// <summary>
		/// �Ƿ�֧��UBB����
		/// </summary>
		public bool IsSupportUBB
		{
			get { return this._IsSupportUBB;  }
			set { this._IsSupportUBB = value; }
		}

		private string _Width = null;
		/// <summary>
		/// ���еĿ��
		/// </summary>
		public string Width
		{
			get { return this._Width;  }
			set { this._Width = value; }
		}

		#region ��ֵͬ�кϲ��йص�����

		bool _IsMergeSameRow = false;
		/// <summary>
		/// �Ƿ�ϲ���ֵͬ��������
		/// </summary>
		public bool IsMergeSameRow
		{
			get { return this._IsMergeSameRow;  }
			set { this._IsMergeSameRow = value; }
		}

		bool _IsMergeNullRow = false;
		/// <summary>
		/// �Ƿ񽫿���Ҳ��Ϊֵͬ��
		/// </summary>
		public bool IsMergeNullRow
		{
			get { return this._IsMergeNullRow;  }
			set { this._IsMergeNullRow = value; }
		}

		bool _IsMergeFollowPrevious = true;
		/// <summary>
		/// �Ƿ���ǰһ��ֵͬ������£����в��ж��Ƿ�Ϊֵͬ
		/// </summary>
		public bool IsMergeFollowPrevious
		{
			get { return this._IsMergeFollowPrevious;  }
			set { this._IsMergeFollowPrevious = value; }
		}

		MergeSameRowMessage _MergeSameRowMessage = null;
		/// <summary>
		/// �����ṩ�ϲ�ֵͬ�е�һЩ��
		/// </summary>
		internal MergeSameRowMessage MergeSameRowMsg
		{
			get
			{
				if(this._MergeSameRowMessage==null)	this._MergeSameRowMessage = new MergeSameRowMessage();
				return this._MergeSameRowMessage;
			}
		}

		/// <summary>
		/// �����ṩ�ϲ�ֵͬ�е�һЩ��Ϣ
		/// </summary>
		internal class MergeSameRowMessage
		{
			/// <summary>
			/// ���ڼ�¼��ֵͬ�е�����
			/// </summary>
			public int RowSpan = 1;
			/// <summary>
			/// ���ڼ�¼��ͬ��ֵ
			/// </summary>
			public string SameValue = null;
			/// <summary>
			/// ���ڼ�¼����HTML������RowSpan����ֵ��λ�ã��Ա���л���
			/// </summary>
			public int Postion = 0;
		}

		#endregion

		private string _GroupTitle = null;
		/// <summary>
		/// Ϊ���ڵļ����ṩͳһ�ı���
		/// </summary>
		public string GroupTitle
		{
			get { return this._GroupTitle;  }
			set { this._GroupTitle = value; }
		}


		private string _EditControl = null;
		/// <summary>
		/// �ڱ༭״̬�£��б༭�ؼ������֣���ѡ inputbox��listbox��checkbox��radiobox��userdefine
		/// </summary>
		public string EditControl
		{
			get
			{
				return this._EditControl;
			}
			set
			{
				if(value==null)	this._EditControl = null;
				else			this._EditControl = value.ToLower().Trim();
			}
		}

		private string _EditControlType = null;
		/// <summary>
		/// �ɱ༭�ؼ������ԣ�������HTML�����ԣ�
		/// </summary>
		public string EditControlType
		{
			get { return this._EditControlType;  }
			set { this._EditControlType = value; }
		}

		private string _EditControlData = null;
		/// <summary>
		/// �ɱ༭�ؼ�����Ϣ
		/// </summary>
		public string EditControlData
		{
			get	{ return this._EditControlData;  }
			set	{ this._EditControlData = value;	}
		}

		private string _DbType = null;
		/// <summary>
		/// �ֶε�����
		/// </summary>
		public string DbType
		{
			get { return this._DbType;  }
			set { this._DbType = value; }
		}
	}
	#endregion

	#region �е���ʽ��
	/// <summary>
	/// ReportCell���е���ʽ��
	/// </summary>
	public class ColStyleGroup : ItemGroup
	{
		//private System.Collections.Hashtable myHashtable = new System.Collections.Hashtable();

		private System.Collections.SortedList mySortList = new System.Collections.SortedList();

		private System.Collections.Hashtable myStyles = new System.Collections.Hashtable();

		/// <summary>
		/// ������ʽ
		/// </summary>
		/// <param name="NewStyle">��ʽ</param>
		public void Append(ColStyle NewStyle)
		{
			if(NewStyle.Name != null)
			{
				NewStyle.Group = this;
				string Key = NewStyle.Name.ToLower();
				if(!this.myHashtable.ContainsKey(Key))
				{
					this.myHashtable.Add(Key, NewStyle);
					this.mySortList.Add(MaxId++, Key);
				}
			}
		}
		int MaxId = 0;

		/// <summary>
		/// ɾ����ʽ
		/// </summary>
		public void Remove(string Name)
		{
			string Key = Name.ToLower();
			if(this.myHashtable.ContainsKey(Key))
			{
				this.myHashtable.Remove(Key);
				int index = this.mySortList.IndexOfValue(Key);
				this.mySortList.Remove(this.mySortList.GetKey(index));
			}
		}

		/// <summary>
		/// ��������ȡ����ʽ��
		/// </summary>
		public ColStyle this[string Name]
		{
			get
			{
				return this.myHashtable[Name.ToLower()] as ColStyle;
			}
		}

		/// <summary>
		/// �����ֶ���ʽ����������ASPXҳ���еĲ�����
		/// </summary>
		public ColStyle It
		{
			set
			{
				if(value.Name!=null) this.Append(value);
			}
		}

		/// <summary>
		/// ���и�Ԫ�ص���ʽ����������ASPXҳ���еĲ�����
		/// </summary>
		public UserDefineStyle Style
		{
			set
			{
				if(value.Name != null)		this.myStyles.Add(value.Name.ToLower(), value);
			}
		}
		
		/// <summary>
		/// ��ȡ��ʽ��ֵ
		/// </summary>
		/// <returns></returns>
		internal System.Collections.ICollection GetStyleKeys()
		{
			return this.myStyles.Keys;
		}

		/// <summary>
		/// ��ȡָ����Ԫ����ʽ
		/// </summary>
		/// <param name="Name"></param>
		/// <returns></returns>
		internal UserDefineStyle GetItemStyle(string Name)
		{
			return this.myStyles[Name.ToLower()] as UserDefineStyle;
		}

		/// <summary>
		/// ��ȡ��ֵ
		/// </summary>
		/// <returns></returns>
		public override System.Collections.ICollection GetKeys()
		{
			return this.mySortList.Values;
		}

		private string _Pattern = "";
		/// <summary>
		/// ���е���ʽ
		/// </summary>
		public string Pattern
		{
			get { return this._Pattern;  }
			set { this._Pattern = value; }
		}

		private bool _IsAutoShow = true;
		/// <summary>
		/// �Ƿ��Զ���ʾ�ֶ�
		/// </summary>
		public bool IsAutoShow
		{
			get { return this._IsAutoShow;  }
			set { this._IsAutoShow = value; }
		}

		private bool _IsEditable = false;
		/// <summary>
		/// �Ƿ�����༭
		/// </summary>
		public bool IsEditable
		{
			get { return this._IsEditable;  }
			set { this._IsEditable = value; }
		}
	}
	#endregion
}
