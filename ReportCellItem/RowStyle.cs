using System;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	#region �е���ʽ
	/// <summary>
	/// ReportCell���е���ʽ
	/// </summary>
	public class RowStyle : ItemStyle
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		public RowStyle(){}

		/// <summary>
		/// �е���ʽ
		/// </summary>
		/// <param name="Index">���</param>
		/// <param name="Pattern">��ʽ</param>
		public RowStyle(int Index, string Pattern) : base(Pattern)
		{
			this.Index = Index;
		}

		private int _Index = 0;
		/// <summary>
		/// �е����к�
		/// </summary>
		public int Index
		{
			get { return this._Index;  }
			set { this._Index = value; }
		}

		private string _ClassName = null;

		/// <summary>
		/// �е���ʽ��
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

	#region �е���ʽ��
	/// <summary>
	/// ReportCell���е���ʽ��
	/// </summary>
	public class RowStyleGroup : ItemGroup
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		public RowStyleGroup(){}

		//private System.Collections.Hashtable myHashtable = new System.Collections.Hashtable();

		/// <summary>
		/// ��������ʽ
		/// </summary>
		/// <param name="NewStyle">��ʽ</param>
		public void Append(RowStyle NewStyle)
		{
			if(NewStyle.Index < 0)	return;
			if(NewStyle.Index==0)	NewStyle.Index = this._DifferentRow + 1;
			this.myHashtable.Add(NewStyle.Index, NewStyle);
			if(this._DifferentRow < NewStyle.Index)		this._DifferentRow = NewStyle.Index;
		}

		/// <summary>
		/// ɾ������ʽ
		/// </summary>
		/// <param name="index">���</param>
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
		/// ��������ȡ��ʽ
		/// </summary>
		public RowStyle this[int index]
		{
			get { return this.myHashtable[index] as RowStyle; }
		}


		/// <summary>
		/// �����ֶ���ʽ����������ASPXҳ���еĲ�����
		/// </summary>
		public RowStyle Style
		{
			set { this.Append(value); }
		}

		/// <summary>
		/// �����ֶ���ʽ����������ASPXҳ���в�����
		/// </summary>
		public RowStyle It
		{
			set { this.Style = value; }
		}

		private int _DifferentRow = 0;
		/// <summary>
		/// �涨�ж����ֲ�ͬ����ʽ
		/// </summary>
		public int DifferentRow
		{
			get { return this._DifferentRow==0?1:this._DifferentRow; }
		}

		string _Flag = null;
		/// <summary>
		/// �еĸ�ʽ���ַ���
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
		/// ֵ�ֶ�
		/// </summary>
		public string[] Value
		{
			get{ return this._Value;  }
			set{ this._Value = value; }
		}
		private string[] _Value = null;

		private bool _IsMulCheckable = false;
		/// <summary>
		/// �Ƿ��������ѡ��
		/// </summary>
		public bool IsMulCheckable
		{
			get { return this._IsMulCheckable;  }
			set { this._IsMulCheckable = value; }
		}

		private bool _IsShowCheckBox = false;
		/// <summary>
		/// �Ƿ���ÿ�п�ͷ��ʾ��ѡ��
		/// </summary>
		public bool IsShowCheckBox
		{
			get { return this._IsShowCheckBox;  }
			set { this._IsShowCheckBox = value; }
		}

		private bool _IsShow = true;
		/// <summary>
		/// �Ƿ���ʾ��ǰ�У���OnItemShow�¼��п��Կ��Ƶ�ǰ�е���ʾ���
		/// </summary>
		public bool IsShow
		{
			get { return this._IsShow;  }
			set { this._IsShow = value; }
		}

		private string _Pattern = null;
		/// <summary>
		/// ���е���ʽ
		/// </summary>
		public string Pattern
		{
			get { return this._Pattern;  }
			set { this._Pattern = value; }
		}

		private string _Height = null;
		/// <summary>
		/// ָ���еĸ߶�
		/// </summary>
		public string Height
		{
			get { return this._Height;  }
			set { this._Height = value; }
		}

		private UserDefineRowGroup myUserDefineRowGroup = null;
		/// <summary>
		/// �û��Զ����е����
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
		/// ����û��Զ�����
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
