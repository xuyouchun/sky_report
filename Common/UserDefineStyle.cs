using System;

namespace Skyever.Report
{
	#region 用户自定义样式

	/// <summary>
	/// ReportCell/ReportDetail：用户自定义样式
	/// </summary>
	public class UserDefineStyle : ItemStyle
	{
		string _Name = null;
		/// <summary>
		/// 名称
		/// </summary>
		public string Name
		{
			get { return this._Name;  }
			set { this._Name = value; }
		}
	}

	#endregion

	#region 用户自定义样式组
	/// <summary>
	/// ReportCell/ReportDetail：行的样式组
	/// </summary>
	public class UserDefineStyleGroup : ItemGroup
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public UserDefineStyleGroup(){}

		//private System.Collections.Hashtable myHashtable = new System.Collections.Hashtable();
		/// <summary>
		/// 增加样式
		/// </summary>
		/// <param name="NewStyle">样式</param>
		public void Append(UserDefineStyle NewStyle)
		{
			if(NewStyle.Name != null)	this.myHashtable.Add(NewStyle.Name.ToLower(), NewStyle);
		}

		/// <summary>
		/// 删除样式
		/// </summary>
		/// <param name="Name">样式名称</param>
		public void Remove(string Name)
		{
			string Key = Name.ToLower();
			if(this.myHashtable.ContainsKey(Key))	this.myHashtable.Remove(Name.ToLower());
		}

		/// <summary>
		/// 按索引读取样式
		/// </summary>
		public UserDefineStyle this[string Name]
		{
			get
			{
				return this.myHashtable[Name.ToLower()] as UserDefineStyle;
			}
		}

		/// <summary>
		/// 增加字段样式，（方便在ASPX页面中的操作）
		/// </summary>
		public UserDefineStyle Style
		{
			set { this.Append(value); }
		}

		/// <summary>
		/// 增加字段样式，（方便在ASPX页面中的操作）
		/// </summary>
		public UserDefineStyle It
		{
			set { Style = value; }
		}
	}
	#endregion

}
