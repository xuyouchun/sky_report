using System;

namespace Skyever.Report
{
	#region �û��Զ��嵥Ԫ��

	/// <summary>
	/// �û��Զ��嵥Ԫ��
	/// </summary>
	public class UserDefineCell : ItemStyle
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		public UserDefineCell(){}

		string _Name = null;
		/// <summary>
		/// ��Ԫ�������
		/// </summary>
		public string Name
		{
			get { return this._Name;  }
			set { this._Name = value; }
		}

		string _Value = null;
		/// <summary>
		/// ��Ԫ���ֵ�������Ǳ��ʽ��
		/// </summary>
		public string Value
		{
			get { return this._Value;  }
			set { this._Value = value; }
		}
	}

	#endregion

	#region �û��Զ����У���Ԫ�����ϣ�

	/// <summary>
	/// �û��Զ����У���Ԫ�����ϣ�
	/// </summary>
	public class UserDefineRow : ItemGroup
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		public UserDefineRow(){}

		private string _Name = null;
		/// <summary>
		/// �е�����
		/// </summary>
		public string Name
		{
			get { return this._Name;  }
			set { this._Name = value; }
		}

		/// <summary>
		/// ���һ��
		/// </summary>
		/// <param name="myUserDefineCell"></param>
		public void Append(UserDefineCell myUserDefineCell)
		{
			if(myUserDefineCell.Name == null)	return;
			string Key = myUserDefineCell.Name.ToLower();
			if(!this.myHashtable.ContainsKey(Key))	myHashtable.Add(Key, myUserDefineCell);
		}

		/// <summary>
		/// ɾ��ָ������
		/// </summary>
		/// <param name="Key"></param>
		public void Remove(string Key)
		{
			if(Key == null)	return;
			Key = Key.ToLower();
			if(this.myHashtable.ContainsKey(Key))	myHashtable.Remove(Key);
		}

		/// <summary>
		/// �������������е���
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
		/// ������ASPX�ļ��������
		/// </summary>
		public UserDefineCell It
		{
			set { this.Append(value); }
		}

		/// <summary>
		/// ������ASPX�ļ��������
		/// </summary>
		public UserDefineCell Cell
		{
			set { this.Append(value); }
		}

		bool _IsShowAtTop = false;
		/// <summary>
		/// ��ʾ��λ��λ�ã�Ĭ��Ϊ�ڵ׶���ʾ��
		/// </summary>
		public bool IsShowAtTop
		{
			get { return this._IsShowAtTop;  }
			set { this._IsShowAtTop = value; }
		}
	}

	#endregion

	#region �û��Զ����е����

	/// <summary>
	/// �û��Զ����е����
	/// </summary>
	public class UserDefineRowGroup : ItemGroup
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		public UserDefineRowGroup(){}

		/// <summary>
		/// ���һ��
		/// </summary>
		/// <param name="myUserDefineRow"></param>
		public void Append(UserDefineRow myUserDefineRow)
		{
			if(myUserDefineRow.Name == null)	return;
			string Key = myUserDefineRow.Name.ToLower();
			if(!this.myHashtable.ContainsKey(Key))	myHashtable.Add(Key, myUserDefineRow);
		}

		/// <summary>
		/// ɾ��ָ������
		/// </summary>
		/// <param name="Key"></param>
		public void Remove(string Key)
		{
			if(Key == null)	return;
			Key = Key.ToLower();
			if(this.myHashtable.ContainsKey(Key))	myHashtable.Remove(Key);
		}

		/// <summary>
		/// �������������е���
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
		/// ��ʾ��λ��λ�ã�Ĭ��Ϊ�ڵ׶���ʾ��
		/// </summary>
		public bool IsShowAtTop
		{
			get { return this._IsShowAtTop;  }
			set { this._IsShowAtTop = value; }
		}

		/// <summary>
		/// ������ASPX�ļ��������
		/// </summary>
		public UserDefineRow It
		{
			set { this.Append(value); }
		}

		/// <summary>
		/// ������ASPX�ļ��������
		/// </summary>
		public UserDefineRow NewRow
		{
			set { this.Append(value); }
		}

		private string _Pattern = "";
		/// <summary>
		/// �е���ʽ
		/// </summary>
		public string Pattern
		{
			get { return this._Pattern;  }
			set { this._Pattern = value; }
		}
	}

	#endregion

}
