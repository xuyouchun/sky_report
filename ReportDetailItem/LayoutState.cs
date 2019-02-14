using System;

namespace Skyever.Report
{
	#region ҳ�沼��

	/// <summary>
	/// ReportDetail��ҳ�沼��
	/// </summary>
	public class LayoutState : System.Web.UI.HtmlControls.HtmlContainerControl
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		public LayoutState () {}

		string _Name = null;
		/// <summary>
		/// ����
		/// </summary>
		public string Name
		{
			get { return this._Name;  }
			set { this._Name = value; }
		}

		#region ִ�и��ֲ���ʱ��SQL���

		string _SelectSql = null;
		/// <summary>
		/// ��ȡ����ʱ���õ����
		/// </summary>
		public string SelectSql
		{
			get { return this._SelectSql==null?this.Group.SelectSql:this._SelectSql;  }
			set { this._SelectSql = value; }
		}

		string _InsertSql = null;
		/// <summary>
		/// ִ�в���ʱ���õ����
		/// </summary>
		public string InsertSql
		{
			get { return this._InsertSql==null?this.Group.InsertSql:this._InsertSql;  }
			set { this._InsertSql = value; }
		}

		string _UpdateSql = null;
		/// <summary>
		/// ִ�и���ʱ���õ����
		/// </summary>
		public string UpdateSql
		{
			get { return this._UpdateSql==null?this.Group.UpdateSql:this._UpdateSql;  }
			set { this._UpdateSql = value; }
		}

		string _DeleteSql = null;
		/// <summary>
		/// ִ��ɾ��ʱ���õ����
		/// </summary>
		public string DeleteSql
		{
			get { return this._DeleteSql==null?this.Group.DeleteSql:this._DeleteSql;  }
			set { this._DeleteSql = value; }
		}

		#endregion

		LayoutStateGroup _Group;

		/// <summary>
		/// ������һ����
		/// </summary>
		internal LayoutStateGroup Group
		{
			get { return this._Group;  }
			set { this._Group = value; }
		}

		bool _IsSupportUBB = false;
		/// <summary>
		/// �ò����Ƿ�UBB�ֶθ�ʽ��ΪHTML����
		/// </summary>
		public bool IsSupportUBB
		{
			get { return this._IsSupportUBB;  }
			set { this._IsSupportUBB = value; }
		}
	}

	#endregion

	#region ҳ�沼�ֵ����

	/// <summary>
	/// ReportDetail��ҳ�沼�ֵ����
	/// </summary>
	public class LayoutStateGroup : ItemGroup
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		public LayoutStateGroup() {}

		/// <summary>
		/// ����һ���
		/// </summary>
		/// <param name="NewLayoutState">�²���</param>
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
		/// ɾ��һ���
		/// </summary>
		/// <param name="Name"></param>
		public void Remove(string Name)
		{
			string Key = Name.ToLower();
			if(this.myHashtable.ContainsKey(Key))		this.myHashtable.Remove(Name.ToLower());
		}

		/// <summary>
		/// ����������ȡҳ�沼��
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
		/// ����һ��֣�������ASPX������
		/// </summary>
		public LayoutState Layout
		{
			set { this.Append(value); }
		}

		/// <summary>
		/// ����һ��֣�������ASPX������
		/// </summary>
		public LayoutState It
		{
			set { this.Append(value); }
		}

		string _DefaultLayout = null;
		/// <summary>
		/// Ĭ�ϲ���
		/// </summary>
		public string DefaultLayout
		{
			get	{ return this._DefaultLayout;  }
			set { this._DefaultLayout = value; }
		}

		#region ִ�и��ֲ���ʱ��SQL���

		string _SelectSql = null;
		/// <summary>
		/// ��ȡ����ʱ���õ����
		/// </summary>
		public string SelectSql
		{
			get { return this._SelectSql;  }
			set { this._SelectSql = value; }
		}

		string _InsertSql = null;
		/// <summary>
		/// ִ�в���ʱ���õ����
		/// </summary>
		public string InsertSql
		{
			get { return this._InsertSql;  }
			set { this._InsertSql = value; }
		}

		string _UpdateSql = null;
		/// <summary>
		/// ִ�и���ʱ���õ����
		/// </summary>
		public string UpdateSql
		{
			get { return this._UpdateSql;  }
			set { this._UpdateSql = value; }
		}

		string _DeleteSql = null;
		/// <summary>
		/// ִ��ɾ��ʱ���õ����
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
