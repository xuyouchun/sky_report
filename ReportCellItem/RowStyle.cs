using System;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	#region 行的样式
	/// <summary>
	/// ReportCell：行的样式
	/// </summary>
	public class RowStyle : ItemStyle
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public RowStyle(){}

		/// <summary>
		/// 行的样式
		/// </summary>
		/// <param name="Index">序号</param>
		/// <param name="Pattern">样式</param>
		public RowStyle(int Index, string Pattern) : base(Pattern)
		{
			this.Index = Index;
		}

		private int _Index = 0;
		/// <summary>
		/// 行的序列号
		/// </summary>
		public int Index
		{
			get { return this._Index;  }
			set { this._Index = value; }
		}

		private string _ClassName = null;

		/// <summary>
		/// 行的样式名
		/// </summary>
		public string ClassName
		{
			get
			{
				if(this._ClassName==null)
				{
					this._ClassName = "TR" + this.Index.ToString();
				}
				return this._ClassName; 
			}
			set
			{
				this._ClassName = value;
			}
		}
	}
	#endregion

	#region 行的样式组
	/// <summary>
	/// ReportCell：行的样式组
	/// </summary>
	public class RowStyleGroup : ItemGroup
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public RowStyleGroup(){}

		//private System.Collections.Hashtable myHashtable = new System.Collections.Hashtable();

		/// <summary>
		/// 增加行样式
		/// </summary>
		/// <param name="NewStyle">样式</param>
		public void Append(RowStyle NewStyle)
		{
			if(NewStyle.Index < 0)	return;
			if(NewStyle.Index==0)	NewStyle.Index = this._DifferentRow + 1;
			this.myHashtable.Add(NewStyle.Index, NewStyle);
			if(this._DifferentRow < NewStyle.Index)		this._DifferentRow = NewStyle.Index;
		}

		/// <summary>
		/// 删除行样式
		/// </summary>
		/// <param name="index">序号</param>
		public void Remove(int index)
		{
			if(this.myHashtable.ContainsKey(index))
			{
				this.myHashtable.Remove(index);
				int MaxIndex = 1;
				foreach(RowStyle item in this.myHashtable)
				{
					if(item.Index > MaxIndex)	MaxIndex = item.Index;
				}
				this._DifferentRow = MaxIndex;
			}
		}

		/// <summary>
		/// 按索引读取样式
		/// </summary>
		public RowStyle this[int index]
		{
			get { return this.myHashtable[index] as RowStyle; }
		}


		/// <summary>
		/// 增加字段样式，（方便在ASPX页面中的操作）
		/// </summary>
		public RowStyle Style
		{
			set { this.Append(value); }
		}

		/// <summary>
		/// 增加字段样式，（方便在ASPX页面中操作）
		/// </summary>
		public RowStyle It
		{
			set { this.Style = value; }
		}

		private int _DifferentRow = 0;
		/// <summary>
		/// 规定有多少种不同的样式
		/// </summary>
		public int DifferentRow
		{
			get { return this._DifferentRow==0?1:this._DifferentRow; }
		}

		string _Flag = null;
		/// <summary>
		/// 列的格式化字符串
		/// </summary>
		public string Flag
		{
			get { return this._Flag;  }
			set
			{
				if(value==null)	return;
				this.argumentcount = 0;
				myValues = new System.Text.StringBuilder();
				this._Flag = FindUnknownArgument.Replace(value, new MatchEvaluator(this.ReplaceUnknownArgument));
				if(myValues.Length!=0)	this.Value = myValues.ToString().Split(',');
				else					this.Value = null;
			}
		}
		int argumentcount = 0;
		System.Text.StringBuilder myValues;

		static private Regex FindUnknownArgument = new Regex(@"(?<=\{)[^\s\}\{]*[^\d\{\}]+[^\s\{\}]*(?=\})", RegexOptions.Multiline | RegexOptions.Compiled);
		private string ReplaceUnknownArgument(Match myMatch)
		{
			string result = myMatch.Value;
			if(ReportCell.IsVariable(result))	return result;

			if(argumentcount!=0)	this.myValues.Append(",");
			this.myValues.Append(result);
			return (argumentcount++).ToString();
		}

		/// <summary>
		/// 值字段
		/// </summary>
		public string[] Value
		{
			get{ return this._Value;  }
			set{ this._Value = value; }
		}
		private string[] _Value = null;

		private bool _IsMulCheckable = false;
		/// <summary>
		/// 是否允许多行选择
		/// </summary>
		public bool IsMulCheckable
		{
			get { return this._IsMulCheckable;  }
			set { this._IsMulCheckable = value; }
		}

		private bool _IsShowCheckBox = false;
		/// <summary>
		/// 是否在每行开头显示复选框
		/// </summary>
		public bool IsShowCheckBox
		{
			get { return this._IsShowCheckBox;  }
			set { this._IsShowCheckBox = value; }
		}

		private bool _IsShow = true;
		/// <summary>
		/// 是否显示当前行（在OnItemShow事件中可以控制当前行的显示与否）
		/// </summary>
		public bool IsShow
		{
			get { return this._IsShow;  }
			set { this._IsShow = value; }
		}

		private string _Pattern = null;
		/// <summary>
		/// 各行的样式
		/// </summary>
		public string Pattern
		{
			get { return this._Pattern;  }
			set { this._Pattern = value; }
		}

		private string _Height = null;
		/// <summary>
		/// 指定行的高度
		/// </summary>
		public string Height
		{
			get { return this._Height;  }
			set { this._Height = value; }
		}

		private UserDefineRowGroup myUserDefineRowGroup = null;
		/// <summary>
		/// 用户自定义行的组合
		/// </summary>
		public UserDefineRowGroup UserDefineRow
		{
			get
			{
				if(this.myUserDefineRowGroup==null)		this.myUserDefineRowGroup = new UserDefineRowGroup();
				return this.myUserDefineRowGroup;
			}
			set
			{
				this.myUserDefineRowGroup = value;
			}
		}

		/// <summary>
		/// 添加用户自定义行
		/// </summary>
		public UserDefineRow NewRow
		{
			set
			{
				this.UserDefineRow.Append(value);
			}
		}
	}
	#endregion
}
