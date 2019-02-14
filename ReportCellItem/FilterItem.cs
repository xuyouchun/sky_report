using System;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	#region 过滤条件
	/// <summary>
	/// ReportCell：过滤条件
	/// </summary>
	public class FilterItem
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public FilterItem() {}

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="Name">过滤条件的名字</param>
		/// <param name="Filter">过滤条件表达式</param>
		public FilterItem(string Name, string Filter)
		{
			this.Name = Name;
			if(this.Name==null)		this.Name = "";
			this.Filter = Filter;
		}

		private string _Name = null;
		/// <summary>
		/// 字段的名字
		/// </summary>
		public string Name
		{
			get { return this._Name;  }
			set { this._Name = value; }
		}

		static Regex FindDefaultValue = new Regex(@"\{.*?\}", RegexOptions.Compiled | RegexOptions.Multiline);

		private string _Filter = null;
		/// <summary>
		/// 过滤条件格式化字符串
		/// </summary>
		public string Filter
		{
			get	{ return this._Filter;  }
			set
			{
				if(this.Group != null)	this.FormatFilterText();
				else					this._Filter = value;
			}
		}

		/// <summary>
		/// 分析处理过滤条件字符串
		/// </summary>
		private void FormatFilterText()
		{
			this._Filter = FindDefaultValue.Replace(this.Filter, new MatchEvaluator(this.FixFilter));
		}

		/// <summary>
		/// 操作过滤条件的项
		/// </summary>
		/// <param name="myMatch"></param>
		/// <returns></returns>
		private string FixFilter(Match myMatch)
		{
			if(myMatch.Value == "{value}")		return this.Name;
			else
			{
				string Item = myMatch.Value.Trim('{', '}', ' ');
				string Name = null, Value = null;
				int index = Item.IndexOf(':');
				if(index != -1)
				{
					Name =  Item.Substring(0, index).TrimEnd();
					Value = Item.Substring(index+1);
				}
				else
				{
					Name = Item;	Value = null;
				}
				if(this.Group.GetFilterValue(Name)==null)		this.Group.SetFilterValue(Name, Value);
				return "{" + Name + '}';
			}
		}

		/// <summary>
		/// 获取当前过滤条件
		/// </summary>
		/// <returns></returns>
		public string GetFilterString()
		{
			if(this.Filter == null)		return null;
			this.IsUsed = true;
			string strFilter = FindDefaultValue.Replace(this.Filter, new MatchEvaluator(GetFilterItemValue));
			if(this.IsUsed)		return strFilter;
			else				return null;
		}

		bool IsUsed = false;

		/// <summary>
		/// 确定过滤条件中每一项的值
		/// </summary>
		/// <param name="myMatch"></param>
		/// <returns></returns>
		private string GetFilterItemValue(Match myMatch)
		{
			FilterGroup myFilterGroup = (this.Group.ParentReport as ReportCell).Filter;
			string Name = myMatch.Value.Trim('{', '}');
			ReportCell myReportCell = this.Group.ParentReport as ReportCell;
			string Value = null;
			if(!myReportCell.IsPostBack)	Value = myFilterGroup.GetFilterValue(Name);
			else
			{
				Value = myFilterGroup.GetFilterValue(Name);
				if(Value==null)	Value = myReportCell.Request.Form[myReportCell.ClientID + "_" + Name];
				if(Value==null)	Value = myReportCell.Request.Form[Name];
				myFilterGroup.SetFilterValue(Name, Value);
			}
			if(Value==null || Value.Trim()=="")	{ this.IsUsed = false;  return ""; }
			else		return Value.Replace("'", "''");
		}

		private FilterGroup _Group;
		/// <summary>
		/// 所属过滤条件组
		/// </summary>
		internal FilterGroup Group
		{
			get { return this._Group;  }
			set
			{
				this._Group = value;
				if(this.Filter!=null)	this.FormatFilterText();
			}
		}
	}

	#endregion

	#region 过滤条件组

	/// <summary>
	/// ReportCell：过滤条件组
	/// </summary>
	public class FilterGroup : ItemGroup
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public FilterGroup() {}

		private System.Collections.ArrayList myFields = new System.Collections.ArrayList();

		private System.Collections.Hashtable myFilterValues
		{
			get { return this.myHashtable; }
		}

		/// <summary>
		/// 按索引读取过滤条件的值
		/// </summary>
		public FilterItem this[string Name]
		{
			get
			{
				if(Name == null)	return null;
				string Key = Name.ToLower();
				foreach(FilterItem item in myFields)
				{
					if(item.Name == Key)	return item;
				}
				return null;
			}
		}

		/// <summary>
		/// 增加一项过滤条件
		/// </summary>
		/// <param name="myFilter"></param>
		public void Append(FilterItem myFilter)
		{
			myFilter.Group = this;
			this.myFields.Add(myFilter);
		}

		/// <summary>
		/// 增加一项过滤条件
		/// </summary>
		/// <param name="Name"></param>
		/// <param name="Filter"></param>
		public void Append(string Name, string Filter)
		{
			if(Name != null)	Name = Name.ToLower();
			FilterItem myFilter = new FilterItem(Name, Filter);
			myFilter.Group = this;
			this.myFields.Add(myFilter);
		}

		/// <summary>
		/// 增加一项过滤条件
		/// </summary>
		/// <param name="Filter"></param>
		public void Append(string Filter)
		{
			this.Append(null, Filter);
		}

		/// <summary>
		/// 设置一项过滤条件，如果设置为false则会增加过滤条件"1=0"，否则会增加过滤条件"1=1"
		/// </summary>
		/// <param name="IsFilter"></param>
		public void Append(bool IsFilter)
		{
			this.Append(null, IsFilter?"1=1":"1=0");
		}

		/// <summary>
		/// 删除过滤条件
		/// </summary>
		/// <param name="Name"></param>
		public void Remove(string Name)
		{
			string Key = Name.ToLower();
			Redo:
				foreach(FilterItem item in this.myFields)
				{
					if(item.Name == Key)
					{
						myFields.Remove(item);	goto Redo;
					}
				}
		}

		/// <summary>
		/// 得到指定名字的过滤条件值
		/// </summary>
		/// <param name="Name"></param>
		/// <returns></returns>
		public string GetFilterValue(string Name)
		{
			//if(this.ParentReportCell!=null)	this.ParentReportCell.RegisterColsFilter();
			Name = Name.ToLower();
			if(!this.myFilterValues.ContainsKey(Name))	return null;
			object Result = this.myFilterValues[Name];
			if(Result == null)
			{
				ReportCell ParentReportCell = this.ParentReport as ReportCell;
				Result = ParentReportCell.Request.Form[ParentReportCell.ClientID + "_" + Name];
				if(Result == null)	Result = ParentReportCell.Request.Form[Name];
			}
			return Result as string;
		}

		/// <summary>
		/// 设置指定名字的过滤项的值
		/// </summary>
		/// <param name="Name"></param>
		/// <param name="Value"></param>
		public void SetFilterValue(string Name, object Value)
		{
			string Key = Name.ToLower();
			if(this.myFilterValues.ContainsKey(Key))	this.myFilterValues[Key] = Value==null?null:Value.ToString();
			else										this.myFilterValues.Add(Key, Value==null?null:Value.ToString());
			this.filter = null;
		}

		/// <summary>
		/// 获取所有过滤条件的集合
		/// </summary>
		/// <returns></returns>
		public System.Collections.ArrayList GetFilters()
		{
			return this.myFields;
		}

		/// <summary>
		/// 得到所有过滤条件值的键
		/// </summary>
		/// <returns></returns>
		public System.Collections.ICollection GetFilterValueKeys()
		{
			return this.myFilterValues.Keys;
		}

		/// <summary>
		/// 过滤条件是否改变
		/// </summary>
		public bool IsFilterChange
		{
			get
			{
				ReportCell ParentReportCell = this.ParentReport as ReportCell;
				if(ParentReportCell == null)		return false;
				if(!ParentReportCell.IsPostBack)	return false;
				return this.GetFilterString() != ParentReportCell.Request.Form[ParentReportCell.ClientID + "_Filter"];
			}
		}

		/// <summary>
		/// 得到过滤条件
		/// </summary>
		/// <returns></returns>
		public string GetFilterString()
		{
			if(this.filter != null)		return this.filter;

			System.Text.StringBuilder myFilter = new System.Text.StringBuilder();
			int index = 0;
			foreach(FilterItem item in this.myFields)
			{
				string strFilter = item.GetFilterString();
				if(strFilter != null)
				{
					if(index++ != 0)	myFilter.Append(" and ");
					myFilter.Append("(");
					myFilter.Append(strFilter);
					myFilter.Append(")");
				}
			}
			this.filter = myFilter.ToString();
			return filter;
		}

		internal string filter = null;

		/// <summary>
		/// 方便在ASPX文件中增加过滤条件
		/// </summary>
		public FilterItem It
		{
			set	{ this.Append(value); }
		}

		/// <summary>
		/// 过滤条件组中的数目
		/// </summary>
		public override int Count
		{
			get { return this.myFields.Count; }
		}
	}

	#endregion

}
