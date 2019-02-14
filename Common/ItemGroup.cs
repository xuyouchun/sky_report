using System;

namespace Skyever.Report
{
	/// <summary>
	/// ������ϸ��ֲ�����Ļ���
	/// </summary>
	public abstract class ItemGroup : System.Collections.IEnumerable
	{
		/// <summary>
		/// ������ϸ��ֲ�����ļ�����
		/// </summary>
		protected System.Collections.Hashtable myHashtable = new System.Collections.Hashtable();

		/// <summary>
		/// ����е�����
		/// </summary>
		public virtual int Count
		{
			get { return this.myHashtable.Count; }
		}

		/// <summary>
		/// ��ȡ�������
		/// </summary>
		/// <returns></returns>
		public virtual System.Collections.ICollection GetKeys()
		{
			return this.myHashtable.Keys;
		}

		/// <summary>
		/// ��ȡֵ�����
		/// </summary>
		/// <returns></returns>
		public virtual System.Collections.ICollection GetItems()
		{
			return this.myHashtable.Values;
		}

		/// <summary>
		/// ������ϸ�����������
		/// </summary>
		public ReportBase ParentReport
		{
			get { return this._ParentReport;  }
			set { this._ParentReport = value; }
		}

		ReportBase _ParentReport = null;

		/// <summary>
		/// �Ƿ�����ض���
		/// </summary>
		/// <param name="Key"></param>
		/// <returns></returns>
		public bool IsContainsKey(object Key)
		{
			if(Key is string)	return this.myHashtable.ContainsKey( (Key as string).ToLower() );
			else				return this.myHashtable.ContainsKey(Key);
		}

		#region IEnumerable ��Ա

		/// <summary>
		/// ö����
		/// </summary>
		/// <returns></returns>
		public System.Collections.IEnumerator GetEnumerator()
		{
			return this.myHashtable.Values.GetEnumerator();
		}

		#endregion
	}
}
