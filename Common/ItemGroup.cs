using System;

namespace Skyever.Report
{
	/// <summary>
	/// 用于组合各种参数项的基类
	/// </summary>
	public abstract class ItemGroup : System.Collections.IEnumerable
	{
		/// <summary>
		/// 用于组合各种参数项的集合类
		/// </summary>
		protected System.Collections.Hashtable myHashtable = new System.Collections.Hashtable();

		/// <summary>
		/// 组合中的项数
		/// </summary>
		public virtual int Count
		{
			get { return this.myHashtable.Count; }
		}

		/// <summary>
		/// 获取键的组合
		/// </summary>
		/// <returns></returns>
		public virtual System.Collections.ICollection GetKeys()
		{
			return this.myHashtable.Keys;
		}

		/// <summary>
		/// 获取值的组合
		/// </summary>
		/// <returns></returns>
		public virtual System.Collections.ICollection GetItems()
		{
			return this.myHashtable.Values;
		}

		/// <summary>
		/// 所属的细节屏或汇总屏
		/// </summary>
		public ReportBase ParentReport
		{
			get { return this._ParentReport;  }
			set { this._ParentReport = value; }
		}

		ReportBase _ParentReport = null;

		/// <summary>
		/// 是否包含特定键
		/// </summary>
		/// <param name="Key"></param>
		/// <returns></returns>
		public bool IsContainsKey(object Key)
		{
			if(Key is string)	return this.myHashtable.ContainsKey( (Key as string).ToLower() );
			else				return this.myHashtable.ContainsKey(Key);
		}

		#region IEnumerable 成员

		/// <summary>
		/// 枚举器
		/// </summary>
		/// <returns></returns>
		public System.Collections.IEnumerator GetEnumerator()
		{
			return this.myHashtable.Values.GetEnumerator();
		}

		#endregion
	}
}
