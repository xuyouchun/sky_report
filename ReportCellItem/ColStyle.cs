using System;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	#region 列的样式
	/// <summary>
	/// ReportCell：报表项的样式
	/// </summary>
	public class ColStyle : ItemStyle
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public ColStyle(){}

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="Name">列名</param>
		/// <param name="Pattern">样式</param>
		public ColStyle(string Name, string Pattern) : base(Pattern)
		{
			this.Name = Name;
		}

		private string _FormatString = null;
		bool IsUpdateFormatString = false;
		/// <summary>
		/// 格式化字符串
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
		/// 醒目的标题
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
		/// 醒目的标题
		/// </summary>
		internal string[] ArrStriking
		{
			get { return this.arrStriking; }
		}


		private string _ClassName = null;
		/// <summary>
		/// 样式表名字，默认为字段名
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
		/// 标题
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
		/// 值
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
		/// 规范格式化字符串和参数表
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
		/// 列名数组，作为格式化字符串的参数
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
		/// 字段名
		/// </summary>
		public string Name
		{
			get { return this._Name;  }
			set { this._Name = value; }
		}

		private bool _IsShow = true;
		/// <summary>
		/// 是否显示
		/// </summary>
		public bool IsShow
		{
			get { return this._IsShow;  }
			set { this._IsShow = value; }
		}

		private ColStyleGroup _Group;
		/// <summary>
		/// 所属样式组
		/// </summary>
		internal ColStyleGroup Group
		{
			get { return this._Group;  }
			set { this._Group = value;}
		}

		/// <summary>
		/// 过滤条件格式化字符串
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
		/// 是否支持UBB代码
		/// </summary>
		public bool IsSupportUBB
		{
			get { return this._IsSupportUBB;  }
			set { this._IsSupportUBB = value; }
		}

		private string _Width = null;
		/// <summary>
		/// 该列的宽度
		/// </summary>
		public string Width
		{
			get { return this._Width;  }
			set { this._Width = value; }
		}

		#region 与同值行合并有关的属性

		bool _IsMergeSameRow = false;
		/// <summary>
		/// 是否合并相同值的相邻行
		/// </summary>
		public bool IsMergeSameRow
		{
			get { return this._IsMergeSameRow;  }
			set { this._IsMergeSameRow = value; }
		}

		bool _IsMergeNullRow = false;
		/// <summary>
		/// 是否将空行也视为同值行
		/// </summary>
		public bool IsMergeNullRow
		{
			get { return this._IsMergeNullRow;  }
			set { this._IsMergeNullRow = value; }
		}

		bool _IsMergeFollowPrevious = true;
		/// <summary>
		/// 是否在前一列同值的情况下，该列才判断是否为同值
		/// </summary>
		public bool IsMergeFollowPrevious
		{
			get { return this._IsMergeFollowPrevious;  }
			set { this._IsMergeFollowPrevious = value; }
		}

		MergeSameRowMessage _MergeSameRowMessage = null;
		/// <summary>
		/// 用于提供合并同值行的一些信
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
		/// 用于提供合并同值行的一些信息
		/// </summary>
		internal class MergeSameRowMessage
		{
			/// <summary>
			/// 用于记录相同值行的数量
			/// </summary>
			public int RowSpan = 1;
			/// <summary>
			/// 用于记录相同的值
			/// </summary>
			public string SameValue = null;
			/// <summary>
			/// 用于记录生成HTML代码中RowSpan属性值的位置，以便进行回填
			/// </summary>
			public int Postion = 0;
		}

		#endregion

		private string _GroupTitle = null;
		/// <summary>
		/// 为相邻的几列提供统一的标题
		/// </summary>
		public string GroupTitle
		{
			get { return this._GroupTitle;  }
			set { this._GroupTitle = value; }
		}


		private string _EditControl = null;
		/// <summary>
		/// 在编辑状态下，列编辑控件的名字，可选 inputbox、listbox、checkbox、radiobox、userdefine
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
		/// 可编辑控件的属性（类似于HTML的属性）
		/// </summary>
		public string EditControlType
		{
			get { return this._EditControlType;  }
			set { this._EditControlType = value; }
		}

		private string _EditControlData = null;
		/// <summary>
		/// 可编辑控件的信息
		/// </summary>
		public string EditControlData
		{
			get	{ return this._EditControlData;  }
			set	{ this._EditControlData = value;	}
		}

		private string _DbType = null;
		/// <summary>
		/// 字段的类型
		/// </summary>
		public string DbType
		{
			get { return this._DbType;  }
			set { this._DbType = value; }
		}
	}
	#endregion

	#region 列的样式组
	/// <summary>
	/// ReportCell：列的样式组
	/// </summary>
	public class ColStyleGroup : ItemGroup
	{
		//private System.Collections.Hashtable myHashtable = new System.Collections.Hashtable();

		private System.Collections.SortedList mySortList = new System.Collections.SortedList();

		private System.Collections.Hashtable myStyles = new System.Collections.Hashtable();

		/// <summary>
		/// 增加样式
		/// </summary>
		/// <param name="NewStyle">样式</param>
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
		/// 删除样式
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
		/// 用索引读取列样式项
		/// </summary>
		public ColStyle this[string Name]
		{
			get
			{
				return this.myHashtable[Name.ToLower()] as ColStyle;
			}
		}

		/// <summary>
		/// 增加字段样式，（方便在ASPX页面中的操作）
		/// </summary>
		public ColStyle It
		{
			set
			{
				if(value.Name!=null) this.Append(value);
			}
		}

		/// <summary>
		/// 列中各元素的样式，（方便在ASPX页面中的操作）
		/// </summary>
		public UserDefineStyle Style
		{
			set
			{
				if(value.Name != null)		this.myStyles.Add(value.Name.ToLower(), value);
			}
		}
		
		/// <summary>
		/// 获取样式键值
		/// </summary>
		/// <returns></returns>
		internal System.Collections.ICollection GetStyleKeys()
		{
			return this.myStyles.Keys;
		}

		/// <summary>
		/// 获取指定列元素样式
		/// </summary>
		/// <param name="Name"></param>
		/// <returns></returns>
		internal UserDefineStyle GetItemStyle(string Name)
		{
			return this.myStyles[Name.ToLower()] as UserDefineStyle;
		}

		/// <summary>
		/// 获取键值
		/// </summary>
		/// <returns></returns>
		public override System.Collections.ICollection GetKeys()
		{
			return this.mySortList.Values;
		}

		private string _Pattern = "";
		/// <summary>
		/// 各列的样式
		/// </summary>
		public string Pattern
		{
			get { return this._Pattern;  }
			set { this._Pattern = value; }
		}

		private bool _IsAutoShow = true;
		/// <summary>
		/// 是否自动显示字段
		/// </summary>
		public bool IsAutoShow
		{
			get { return this._IsAutoShow;  }
			set { this._IsAutoShow = value; }
		}

		private bool _IsEditable = false;
		/// <summary>
		/// 是否允许编辑
		/// </summary>
		public bool IsEditable
		{
			get { return this._IsEditable;  }
			set { this._IsEditable = value; }
		}
	}
	#endregion
}
