using System;

namespace Skyever.Report
{
	#region 用户自定义单元格

	/// <summary>
	/// 用户自定义单元格
	/// </summary>
	public class UserDefineCell : ItemStyle
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public UserDefineCell(){}

		string _Name = null;
		/// <summary>
		/// 单元格的名字
		/// </summary>
		public string Name
		{
			get { return this._Name;  }
			set { this._Name = value; }
		}

		string _Value = null;
		/// <summary>
		/// 单元格的值（可以是表达式）
		/// </summary>
		public string Value
		{
			get { return this._Value;  }
			set { this._Value = value; }
		}
	}

	#endregion

	#region 用户自定义行（单元格的组合）

	/// <summary>
	/// 用户自定义行（单元格的组合）
	/// </summary>
	public class UserDefineRow : ItemGroup
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public UserDefineRow(){}

		private string _Name = null;
		/// <summary>
		/// 行的名字
		/// </summary>
		public string Name
		{
			get { return this._Name;  }
			set { this._Name = value; }
		}

		/// <summary>
		/// 添加一项
		/// </summary>
		/// <param name="myUserDefineCell"></param>
		public void Append(UserDefineCell myUserDefineCell)
		{
			if(myUserDefineCell.Name == null)	return;
			string Key = myUserDefineCell.Name.ToLower();
			if(!this.myHashtable.ContainsKey(Key))	myHashtable.Add(Key, myUserDefineCell);
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
		/// 按索引访问组中的行
		/// </summary>
		public UserDefineCell this[string Key]
		{
			get
			{
				if(Key==null)	return null;
				Key = Key.ToLower();
				return this.myHashtable[Key] as UserDefineCell;
			}
		}

		/// <summary>
		/// 方便在ASPX文件中添加项
		/// </summary>
		public UserDefineCell It
		{
			set { this.Append(value); }
		}

		/// <summary>
		/// 方便在ASPX文件中添加项
		/// </summary>
		public UserDefineCell Cell
		{
			set { this.Append(value); }
		}

		bool _IsShowAtTop = false;
		/// <summary>
		/// 显示的位置位置（默认为在底端显示）
		/// </summary>
		public bool IsShowAtTop
		{
			get { return this._IsShowAtTop;  }
			set { this._IsShowAtTop = value; }
		}
	}

	#endregion

	#region 用户自定义行的组合

	/// <summary>
	/// 用户自定义行的组合
	/// </summary>
	public class UserDefineRowGroup : ItemGroup
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public UserDefineRowGroup(){}

		/// <summary>
		/// 添加一项
		/// </summary>
		/// <param name="myUserDefineRow"></param>
		public void Append(UserDefineRow myUserDefineRow)
		{
			if(myUserDefineRow.Name == null)	return;
			string Key = myUserDefineRow.Name.ToLower();
			if(!this.myHashtable.ContainsKey(Key))	myHashtable.Add(Key, myUserDefineRow);
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
		/// 按索引访问组中的行
		/// </summary>
		public UserDefineRow this[string Key]
		{
			get
			{
				if(Key==null)	return null;
				Key = Key.ToLower();
				return this.myHashtable[Key] as UserDefineRow;
			}
		}

		bool _IsShowAtTop = false;
		/// <summary>
		/// 显示的位置位置（默认为在底端显示）
		/// </summary>
		public bool IsShowAtTop
		{
			get { return this._IsShowAtTop;  }
			set { this._IsShowAtTop = value; }
		}

		/// <summary>
		/// 方便在ASPX文件中添加项
		/// </summary>
		public UserDefineRow It
		{
			set { this.Append(value); }
		}

		/// <summary>
		/// 方便在ASPX文件中添加项
		/// </summary>
		public UserDefineRow NewRow
		{
			set { this.Append(value); }
		}

		private string _Pattern = "";
		/// <summary>
		/// 行的样式
		/// </summary>
		public string Pattern
		{
			get { return this._Pattern;  }
			set { this._Pattern = value; }
		}
	}

	#endregion

}
