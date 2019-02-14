using System;

namespace Skyever.Report
{
	#region 页面布局

	/// <summary>
	/// ReportDetail：页面布局
	/// </summary>
	public class LayoutState : System.Web.UI.HtmlControls.HtmlContainerControl
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public LayoutState () {}

		string _Name = null;
		/// <summary>
		/// 名称
		/// </summary>
		public string Name
		{
			get { return this._Name;  }
			set { this._Name = value; }
		}

		#region 执行各种操作时的SQL语句

		string _SelectSql = null;
		/// <summary>
		/// 读取数据时所用的语句
		/// </summary>
		public string SelectSql
		{
			get { return this._SelectSql==null?this.Group.SelectSql:this._SelectSql;  }
			set { this._SelectSql = value; }
		}

		string _InsertSql = null;
		/// <summary>
		/// 执行插入时所用的语句
		/// </summary>
		public string InsertSql
		{
			get { return this._InsertSql==null?this.Group.InsertSql:this._InsertSql;  }
			set { this._InsertSql = value; }
		}

		string _UpdateSql = null;
		/// <summary>
		/// 执行更新时所用的语句
		/// </summary>
		public string UpdateSql
		{
			get { return this._UpdateSql==null?this.Group.UpdateSql:this._UpdateSql;  }
			set { this._UpdateSql = value; }
		}

		string _DeleteSql = null;
		/// <summary>
		/// 执行删除时所用的语句
		/// </summary>
		public string DeleteSql
		{
			get { return this._DeleteSql==null?this.Group.DeleteSql:this._DeleteSql;  }
			set { this._DeleteSql = value; }
		}

		#endregion

		LayoutStateGroup _Group;

		/// <summary>
		/// 属于哪一个组
		/// </summary>
		internal LayoutStateGroup Group
		{
			get { return this._Group;  }
			set { this._Group = value; }
		}

		bool _IsSupportUBB = false;
		/// <summary>
		/// 该布局是否将UBB字段格式化为HTML代码
		/// </summary>
		public bool IsSupportUBB
		{
			get { return this._IsSupportUBB;  }
			set { this._IsSupportUBB = value; }
		}
	}

	#endregion

	#region 页面布局的组合

	/// <summary>
	/// ReportDetail：页面布局的组合
	/// </summary>
	public class LayoutStateGroup : ItemGroup
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public LayoutStateGroup() {}

		/// <summary>
		/// 增加一项布局
		/// </summary>
		/// <param name="NewLayoutState">新布局</param>
		public void Append(LayoutState NewLayoutState)
		{
			if(NewLayoutState.Name==null)
			{
				do { NewLayoutState.Name = "layout" + unchecked(TempIndex++).ToString(); }
				while(this.myHashtable.ContainsKey(NewLayoutState.Name));
			}
			this.myHashtable.Add(NewLayoutState.Name.ToLower(), NewLayoutState);
			NewLayoutState.Group = this;

			if(this._DefaultLayout==null)	this._DefaultLayout = NewLayoutState.Name;
		}

		static int TempIndex = 0;

		/// <summary>
		/// 删除一项布局
		/// </summary>
		/// <param name="Name"></param>
		public void Remove(string Name)
		{
			string Key = Name.ToLower();
			if(this.myHashtable.ContainsKey(Key))		this.myHashtable.Remove(Name.ToLower());
		}

		/// <summary>
		/// 按照索引读取页面布局
		/// </summary>
		public LayoutState this[string Name]
		{
			get 
			{
				if(Name == null)
				{
					if(this.myHashtable.Count > 0)
						foreach(object Value in myHashtable.Values)		return Value as LayoutState;
					return null;
				}
				return this.myHashtable[Name.ToLower()] as LayoutState; 
			}
		}

		/// <summary>
		/// 增加一项布局（方便在ASPX中添加项）
		/// </summary>
		public LayoutState Layout
		{
			set { this.Append(value); }
		}

		/// <summary>
		/// 增加一项布局（方便在ASPX中添加项）
		/// </summary>
		public LayoutState It
		{
			set { this.Append(value); }
		}

		string _DefaultLayout = null;
		/// <summary>
		/// 默认布局
		/// </summary>
		public string DefaultLayout
		{
			get	{ return this._DefaultLayout;  }
			set { this._DefaultLayout = value; }
		}

		#region 执行各种操作时的SQL语句

		string _SelectSql = null;
		/// <summary>
		/// 读取数据时所用的语句
		/// </summary>
		public string SelectSql
		{
			get { return this._SelectSql;  }
			set { this._SelectSql = value; }
		}

		string _InsertSql = null;
		/// <summary>
		/// 执行插入时所用的语句
		/// </summary>
		public string InsertSql
		{
			get { return this._InsertSql;  }
			set { this._InsertSql = value; }
		}

		string _UpdateSql = null;
		/// <summary>
		/// 执行更新时所用的语句
		/// </summary>
		public string UpdateSql
		{
			get { return this._UpdateSql;  }
			set { this._UpdateSql = value; }
		}

		string _DeleteSql = null;
		/// <summary>
		/// 执行删除时所用的语句
		/// </summary>
		public string DeleteSql
		{
			get { return this._DeleteSql;  }
			set { this._DeleteSql = value; }
		}

		#endregion
	}

	#endregion
}
