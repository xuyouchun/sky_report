using System;

namespace Skyever.Report
{
	#region �û��Զ�����ʽ

	/// <summary>
	/// ReportCell/ReportDetail���û��Զ�����ʽ
	/// </summary>
	public class UserDefineStyle : ItemStyle
	{
		string _Name = null;
		/// <summary>
		/// ����
		/// </summary>
		public string Name
		{
			get { return this._Name;  }
			set { this._Name = value; }
		}
	}

	#endregion

	#region �û��Զ�����ʽ��
	/// <summary>
	/// ReportCell/ReportDetail���е���ʽ��
	/// </summary>
	public class UserDefineStyleGroup : ItemGroup
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		public UserDefineStyleGroup(){}

		//private System.Collections.Hashtable myHashtable = new System.Collections.Hashtable();
		/// <summary>
		/// ������ʽ
		/// </summary>
		/// <param name="NewStyle">��ʽ</param>
		public void Append(UserDefineStyle NewStyle)
		{
			if(NewStyle.Name != null)	this.myHashtable.Add(NewStyle.Name.ToLower(), NewStyle);
		}

		/// <summary>
		/// ɾ����ʽ
		/// </summary>
		/// <param name="Name">��ʽ����</param>
		public void Remove(string Name)
		{
			string Key = Name.ToLower();
			if(this.myHashtable.ContainsKey(Key))	this.myHashtable.Remove(Name.ToLower());
		}

		/// <summary>
		/// ��������ȡ��ʽ
		/// </summary>
		public UserDefineStyle this[string Name]
		{
			get
			{
				return this.myHashtable[Name.ToLower()] as UserDefineStyle;
			}
		}

		/// <summary>
		/// �����ֶ���ʽ����������ASPXҳ���еĲ�����
		/// </summary>
		public UserDefineStyle Style
		{
			set { this.Append(value); }
		}

		/// <summary>
		/// �����ֶ���ʽ����������ASPXҳ���еĲ�����
		/// </summary>
		public UserDefineStyle It
		{
			set { Style = value; }
		}
	}
	#endregion

}
