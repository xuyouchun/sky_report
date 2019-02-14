using System;
using System.Data;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	/// <summary>
	///	ϸ����������ǿ�������Ķ����ݿ��¼���в����Ŀؼ�
	/// </summary>
	public class ReportDetail : ReportBase
	{
		#region ��ȡ�����롢���¡�ɾ�����ݵ�Ĭ�ϲ���

		string ErrorMsg = null;
		bool IsExecutePageLoad = false;
		private void Page_Load(object sender, System.EventArgs e)
		{
			if(this.IsExecutePageLoad)		return;
			this.IsExecutePageLoad = true;

			if(this.CurLayoutState==null)	return;

			try
			{
				if(this.DbConnection == null)
				{
					this.ErrorMsg = "����������δ��ʼ����";
				}
				else
				{
					if(!this.IsPostBack || this.RefreshKeyFlag)		//��ȡ����
					{
						if(this.IsUpdateState)
						{
							//ִ�����ݵĶ�ȡ
							if(this.CallEventFunction(this.OnBeforeOperate, Operate.Select))	this.Select();
						}
						if(!this.ExplainValues())	return;
					}
					else	//�����ݽ��в��롢���¡�ɾ����
					{
						if(!this.ExplainValues())	return;
						//ִ�ж����ݵĲ���
						Operate myOperate = Operate.None;
						try
						{
							string strOperate = this.Request.Form[this.ClientID + "_Operate"];
							if(strOperate != "")	myOperate = (Operate)System.Enum.Parse(typeof(Operate), strOperate, false);
							if(myOperate==Operate.Update || myOperate==Operate.Delete)
							{
								this.GetOldValues();
							}
						}
						catch
						{
							myOperate = Operate.None;
						}
						//����Ǹ��»�ɾ�������������ȶ�ȡԭ�е�ֵ
						if(myOperate != Operate.None)
						{
							if(this.DbConnection.State == ConnectionState.Closed)	this.DbConnection.Open();
							bool result = false;
							try
							{
								result = this.CallEventFunction(this.OnBeforeOperate, myOperate);
							}
							catch(Exception myException)
							{
								this.ErrorMsg = "�����¼� OnBeforeOperate��" + myException.Message;
							}

							try
							{
								if(result)
								{
									//ִ�ж����ݵ�Ĭ�ϲ���
									switch(myOperate)
									{
										case Operate.Delete:	this.Delete();		break;
										case Operate.Insert:	this.Insert();		break;
										case Operate.Update:	this.Update();		break;
										case Operate.Reset:		this.Reset();		break;
									}
								}
							}
							catch(Exception myException)
							{
								this.ErrorMsg = "ִ������" + (myOperate==Operate.Delete?"ɾ��":myOperate==Operate.Insert?"����":myOperate==Operate.Update?"����":"����") + "��" + myException.Message;
							}
						}
						else
						{
							string CommandName = this.Request.Form[this.ClientID + "_Operate"];
							string Argument	   = this.Request.Form[this.ClientID + "_Argument"];
							if(CommandName!="" && this.OnDetailCommand!=null)	this.OnDetailCommand(this, CommandName, Argument);
						}
					}
				}
			}
			finally
			{
				if(this.DbConnection!=null)
				{
					if(this.DbCommand.Transaction != null)
					{
						try
						{
							this.DbCommand.Transaction.Commit();
							this.DbCommand.Transaction = null;
						}
						catch {}
					}
					if(!this.IsConnectionOpen)	this.DbConnection.Close();
				}
			}
			this._IsExecute = true;
		}


		/// <summary>
		/// �����ʽת��Ϊʵ�ʵ�ֵ
		/// </summary>
		private bool ExplainValues()
		{
			//�����ʽת��Ϊʵ�ʵ�ֵ
			try
			{
				this.Argument.ExplainValues();
				return true;
			}
			catch(ReportException myExp)
			{
				this.ErrorMsg = "���ʽת����" + myExp.Message;
				return false;
			}
		}

		private bool CallEventFunction(OnDetailOperate OperateEvent, Operate operate)
		{
			if(OperateEvent == null)	return true;

			bool result = true;
			foreach(OnDetailOperate EventItem in OperateEvent.GetInvocationList())
			{
				if(EventItem(this, operate)==false)	 result = false;
			}

			return result;
		}

		#endregion

		#region �����ݵĲ���

		#region һЩ�����Ĳ���
		/// <summary>
		/// �Ƿ�Ϊ����״̬
		/// </summary>
		public bool IsUpdateState
		{
			//ԭ�򣺼�������Ƿ��Ѿ���ֵ������ǣ���Ϊ����״̬������δ��ֵ����Ϊ����״̬�������ֵ�����������׳��쳣
			get
			{
				bool result = false;	int index = 0;
				foreach(object item in this.Fields.GetKeyFields())
				{
					object Value = (item as FieldAttrib).GetParameter().Value;
					if(index++ == 0)		result = !(Value is System.Data.SqlTypes.INullable || Value==null);
					bool result1 = !(Value is System.Data.SqlTypes.INullable || Value==null);
					if(result != result1)	this.ErrorMsg = "������ֵ��������";
				}
				return result;
			}
		}

		/// <summary>
		/// ���Ӳ���
		/// </summary>
		internal void AppendParameters()
		{
			this.DbCommand.Parameters.Clear();
			switch(this.DbConType)		//�����������������ж�������Ӳ���
			{
				case "OdbcConnection": case "OleDbConnection":
					this.DbCommand.CommandText = FindArgument.Replace(this.DbCommand.CommandText, new MatchEvaluator(this.ReplaceSub));
					break;
				default:
					foreach(Match myMatch in FindArgument.Matches(this.DbCommand.CommandText))
					{
						string Item = myMatch.Value;
						if(Item[0] != '@')	continue;
						FieldAttrib myFieldAttrib = this.Fields[Item.TrimStart('@')];
						if(myFieldAttrib == null)	throw new ReportException("�޷���ȡ����" + Item);
						IDbDataParameter myParameter = myFieldAttrib.GetParameter();
						if(!this.DbCommand.Parameters.Contains(myParameter))	this.DbCommand.Parameters.Add(myParameter);
					}
					break;
			}
		}

		/// <summary>
		/// ������������
		/// </summary>
		private string DbConType = null;

		/// <summary>
		/// Ѱ�Ҳ���
		/// </summary>
		private Regex FindArgument = new Regex(@"'[^']*'|@[^\s,\)\n;]+", RegexOptions.Compiled);
		/// <summary>
		/// ����Ѱ�ҵ��Ĳ���������ʵ�ʵ�ֵss
		/// </summary>
		/// <param name="myMatch"></param>
		/// <returns></returns>
		private string ReplaceSub(Match myMatch)
		{
			string Item = myMatch.Value;
			if(Item[0] != '@')		return Item;

			FieldAttrib myFieldAttrib = this.Fields[Item.TrimStart('@')];
			if(myFieldAttrib == null)	throw new ReportException("�޷���ȡ����" + Item);
			IDbDataParameter myParameter = myFieldAttrib.GetParameter();
			if(this.DbCommand.Parameters.Contains(myParameter))
				this.DbCommand.Parameters.Add(this.CopyParameter(myParameter));
			else this.DbCommand.Parameters.Add(myParameter);
			return "?";
		}

		/// <summary>
		/// ����һ����������
		/// </summary>
		/// <param name="myIDbDataParameter"></param>
		/// <returns></returns>
		private IDbDataParameter CopyParameter(IDbDataParameter myIDbDataParameter)
		{
			IDbDataParameter newIDbDataParameter = this.DbCommand.CreateParameter();
			foreach(System.Reflection.PropertyInfo item in myIDbDataParameter.GetType().GetProperties())
			{
				item.SetValue(newIDbDataParameter, item.GetValue(myIDbDataParameter, null), null);
			}
			return newIDbDataParameter;
		}



		private int _Catch = 0;
		/// <summary>
		/// ��һ��������Ӱ�������
		/// </summary>
		public int Catch
		{
			get { return this._Catch; }
		}

		/// <summary>
		/// �Ƿ�Ϊ��ѯ���
		/// </summary>
		Regex IsMatchGetValue = new Regex(@"^\s*\S+(?=\s*=\s*select)", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
		/// <summary>
		/// ����SQL��䣬�Ǵ洢���̻���SQL����������Ӧ�Ĳ���
		/// </summary>
		/// <param name="Sql"></param>
		/// <returns></returns>
		private string AttachSql(string Sql)
		{
			string Field = IsMatchGetValue.Match(Sql).Value.Trim().Trim('@');
			if(Field != "")		Sql = Sql.Substring(Sql.IndexOf('=')+1).Trim();
			else				Sql = Sql.Trim();
			if(Sql.Length>0 && Sql[0]=='@')
			{
				this.DbCommand.CommandType = CommandType.StoredProcedure;
				this.DbCommand.CommandText = Sql.Substring(1);
			}
			else
			{
				this.DbCommand.CommandType = CommandType.Text;
				this.DbCommand.CommandText = Sql;
			}
			return Field;
		}

		Regex SplitSql = new Regex(@"[^;][^';]*(?(')'[^']'|[^';])*?(?=;|\z)", RegexOptions.Multiline | RegexOptions.Compiled);

		#endregion

		#region ��ȡ����
		/// <summary>
		/// ��ȡ���ݣ������ݿ⣩
		/// </summary>
		private void Select()
		{
			//ִ�����ݶ�ȡ��������Ӧ�Ĳ���
			string CommandText = this.CurLayoutState.SelectSql;

			//���δ����SELECT��䣬��ִ�����ݵĶ�ȡ
			if(CommandText == null)	 return;

			if(CommandText.Length>0 && CommandText[0]=='@')		//����һ���ַ�Ϊ'@'��������Ǵ洢����
			{
				this.DbCommand.CommandText = CommandText.Substring(1);
				this.DbCommand.CommandType = CommandType.StoredProcedure;
			}
			else
			{
				this.DbCommand.CommandText = CommandText;
				this.DbCommand.CommandType = CommandType.Text;
			}

			this.DbCommand.CommandText = this.ExplainArgument(this.DbCommand.CommandText);

			try
			{
				this.AppendParameters();
				IDataReader myReader = this.DbCommand.ExecuteReader();
				this.DbCommand.CommandType = CommandType.Text;
				if(myReader.Read())
				{
					this._Catch = 1;
					for(int index=0; index<myReader.FieldCount; index++)
					{
						FieldAttrib myFieldAttrib = this.Fields[myReader.GetName(index)];
						if(myFieldAttrib == null)
						{
							myFieldAttrib = new FieldAttrib(myReader.GetName(index), System.Data.DbType.String);
							this.Fields.Append(myFieldAttrib);
						}
						myFieldAttrib.GetParameter().Value = myReader[index];
					}
					this.CallEventFunction(this.OnAfterOperate, Operate.Select);
					if(myReader.Read())
					{
						this._Catch = 2;
						this.ErrorMsg = "��ȡ���ݣ����󣬲�ѯ�����Ψһ��";
					}
				}
				else	this._Catch = 0;
				myReader.Close();
			}
			catch(Exception e)
			{
				this.ErrorMsg = "��ȡ���ݣ�" + e.Message;
			}
		}
		#endregion

		#region ��������
		/// <summary>
		/// ��������
		/// </summary>
		private void Insert()
		{
			//������Ӧ�Ĳ������ݵ�SQL���
			string CommandText = this.CurLayoutState.InsertSql;
			if(CommandText == null)	{ this.ErrorMsg = "�������ݣ���δ������Ӧ�� Insert ��䣡"; return; }

			CommandText = this.ExplainArgument(CommandText);

			//ִ����Ӧ�Ĳ������ݵĲ���
			IDbTransaction myTrans = this.DbConnection.BeginTransaction();
			this.DbCommand.Transaction = myTrans;

			foreach(Match myMatch in SplitSql.Matches(CommandText))
			{
				string Sql = myMatch.Value.Trim();
				if(Sql == "")	continue;
				string Field = this.AttachSql(Sql);
				this.AppendParameters();
				if(Field != "")		this[Field] = this.DbCommand.ExecuteScalar();
				else				this._Catch = this.DbCommand.ExecuteNonQuery();
			}

			this.DbCommand.CommandType = CommandType.Text;
			this.DbCommand.CommandText = "";

			if(this.CallEventFunction(this.OnAfterOperate, Operate.Insert))		myTrans.Commit();
			else	myTrans.Rollback();
			this.DbCommand.Transaction = null;
		}
		#endregion

		#region ��������
		/// <summary>
		/// ��������
		/// </summary>
		private void Update()
		{
			//������Ӧ��SQL���
			string CommandText = this.CurLayoutState.UpdateSql;
			if(CommandText == null)	{ this.ErrorMsg = "�������ݣ���δ������Ӧ�� Update ��䣡"; return; }

			CommandText = this.ExplainArgument(CommandText);

			//ִ����Ӧ�ĸ������ݵĲ���
			IDbTransaction myTrans = this.DbConnection.BeginTransaction();
			this.DbCommand.Transaction = myTrans;

			int operatecount = 0;
			foreach(Match myMatch in SplitSql.Matches(CommandText))
			{
				string Sql = myMatch.Value.Trim();
				if(Sql.Trim() == "")	continue;
				string Field = this.AttachSql(Sql);
				this.AppendParameters();
				if(Field != "")		this[Field] = this.DbCommand.ExecuteScalar();
				else
				{
					this._Catch = this.DbCommand.ExecuteNonQuery();
					operatecount ++;
				}
			}

			if(operatecount==1 && this.Catch!=1)		//Ϊ��ֹ���´��󣬶����ӵķ�����ʩ
			{
				if(this.Catch == 0)		this.ErrorMsg = "�������ݣ������ѱ�ɾ����";
				else					this.ErrorMsg = "�������ݣ����ؾ��棬�����¶�����¼�����ѻع���";
				myTrans.Rollback();		return;
			}

			this.DbCommand.CommandType = CommandType.Text;
			this.DbCommand.CommandText = "";

			if(this.CallEventFunction(this.OnAfterOperate, Operate.Update))		myTrans.Commit();
			else	myTrans.Rollback();
			this.DbCommand.Transaction = null;
		}
		#endregion

		#region ɾ������
		/// <summary>
		/// ɾ������
		/// </summary>
		private void Delete()
		{
			//������Ӧ��SQL���
			string CommandText = this.CurLayoutState.DeleteSql;
			if(CommandText == null)	{ this.ErrorMsg = "ɾ�����ݣ���δ������Ӧ�� Delete ��䣡"; return; }

			CommandText = this.ExplainArgument(CommandText);

			//ִ����Ӧ��ɾ�����ݵĲ���
			IDbTransaction myTrans = this.DbConnection.BeginTransaction();
			this.DbCommand.Transaction = myTrans;

			int operatecount = 0;
			foreach(Match myMatch in SplitSql.Matches(CommandText))
			{
				string Sql = myMatch.Value.Trim();
				if(Sql.Trim() == "")	continue;
				string Field = this.AttachSql(Sql);
				this.AppendParameters();		//���Ӳ���
				if(Field != "")		this[Field] = this.DbCommand.ExecuteScalar();
				else
				{
					this._Catch = this.DbCommand.ExecuteNonQuery();
					operatecount ++;
				}
			}

			
			if(operatecount==1 && this.Catch!=1)		//Ϊ��ֹ���´��󣬶����ӵķ�����ʩ
			{
				if(this.Catch == 0)		this.ErrorMsg = "ɾ�����ݣ������ѱ�ɾ����";
				else					this.ErrorMsg = "ɾ�����ݣ����ؾ��棬��ɾ��������¼�����ѻع���";
				myTrans.Rollback();		return;
			}

			this.DbCommand.CommandType = CommandType.Text;
			this.DbCommand.CommandText = "";

			if(this.CallEventFunction(this.OnAfterOperate, Operate.Delete))
			{
				myTrans.Commit();
				//�����ֶ�ֵ���
				foreach(object Key in this.Fields.GetKeys())		this[Key as string] = null;
			}
			else	myTrans.Rollback();
			this.DbCommand.Transaction = null;
		}
		#endregion

		#region ǿ��ϸ�����Ĳ���

		/// <summary>
		/// ǿ��ϸ����ִ�в���
		/// </summary>
		public override void Execute()
		{
			this.Page_Load(null, null);
			if(this.Content == null)	this.Content = this.GetContent();
		}

		#endregion

		#endregion

		#region ��Ӧ�õ�ҳ�沼��

		string _Layout = null;
		/// <summary>
		/// ��Ӧ�õ�ҳ�沼����
		/// </summary>
		public string Layout
		{
			get
			{
				if(this._Layout==null)
				{
					if(this.IsPostBack)	this._Layout = this.Request.Form[this.ClientID + "_Layout"];
					else				this._Layout = this.Layouts.DefaultLayout;
				}
				return this._Layout;
			}
			set	{ this._Layout = value; }
		}

		/// <summary>
		/// ��ǰ���õ�ҳ�沼��
		/// </summary>
		public LayoutState CurLayoutState
		{
			get
			{
				if(this._Layouts == null)	this._Layouts = new LayoutStateGroup();
				return this.Layouts[this.Layout];
			}
		}

		#endregion

		#region ��������

		private System.Data.IDbConnection _DbConnection = null;

		/// <summary>
		/// �������Ӷ���
		/// </summary>
		public override System.Data.IDbConnection DbConnection
		{
			get { return this._DbConnection; }
			set 
			{
				this.Adapter.DbConnection = value;
				this.Fields.ConnectionType = value.GetType();
				this.DbConType = value.GetType().Name;
				this.IsConnectionOpen = value.State == ConnectionState.Open;
			}
		}
		private bool IsConnectionOpen = false;

		private System.Data.IDbCommand _DbCommand = null;
		/// <summary>
		/// ����ִ�ж���
		/// </summary>
		public override System.Data.IDbCommand DbCommand
		{
			get
			{
				if(this._DbCommand == null)
				{
					this._DbCommand = this.DbConnection.CreateCommand();
				}
				if(this._DbConnection.State == ConnectionState.Closed)	this._DbConnection.Open();
				return this._DbCommand;
			}
		}

		/// <summary>
		/// ��������ͨ��
		/// </summary>
		public override DataAdapter Adapter
		{
			get
			{
				return this._Adapter;
			}
			set
			{
				this._Adapter = value;
			}
		}

		private DataAdapter _Adapter = new DataAdapter();

		#endregion

		#region �ֶ�������

		private FieldAttribGroup _Fields = null;
		/// <summary>
		/// �ֶ�������
		/// </summary>
		public FieldAttribGroup Fields
		{
			get
			{
				if(this._Fields == null)
				{
					this._Fields = new FieldAttribGroup();
					this._Fields.ParentReport = this;
				}
				return this._Fields;
			}
			set
			{
				this._Fields = value;
				this._Fields.ParentReport = this;
			}
		}

		#endregion

		#region ҳ�沼����

		private LayoutStateGroup _Layouts = null;
		/// <summary>
		/// ҳ�沼����
		/// </summary>
		public LayoutStateGroup Layouts
		{
			get
			{
				if(this._Layouts==null)
				{
					this._Layouts = new LayoutStateGroup();
					this._Layouts.ParentReport = this;
				}
				return this._Layouts;
			}
			set
			{
				this._Layouts = value;
				this._Layouts.ParentReport = this;
			}
		}

		#endregion

		#region �û��Զ��������

		private ArgumentGroup _Argument = null;
		/// <summary>
		/// �û��Զ��������
		/// </summary>
		public override ArgumentGroup Argument
		{
			get
			{
				if(this._Argument == null)
				{
					this._Argument = new ArgumentGroup();
					this._Argument.ParentReport = this;
				}
				return this._Argument;
			}
			set
			{
				this._Argument = value;
				this._Argument.ParentReport = this;
			}
		}

		#endregion

		#region �û��Զ�����ʽ��

		private UserDefineStyleGroup _UserDefineStyle;

		/// <summary>
		/// �û��Զ�����ʽ��
		/// </summary>
		public UserDefineStyleGroup UserDefineStyle
		{
			get
			{
				if(this._UserDefineStyle==null)
				{
					this._UserDefineStyle = new UserDefineStyleGroup();
					this._UserDefineStyle.ParentReport = this;
				}
				return this._UserDefineStyle;
			}
			set
			{
				this._UserDefineStyle = value;
				this._UserDefineStyle.ParentReport = this;
			}
		}

		#endregion

		#region ���ݿ������
		/// <summary>
		/// ���ݿ������
		/// </summary>
		internal DataParameterGroup DataParameter
		{
			get
			{
				if(this._DataParameter==null)
				{
					this._DataParameter = new DataParameterGroup(this.DbConnection);
					this._DataParameter.ParentReport = this;
				}
				return this._DataParameter;
			}
		}

		private DataParameterGroup _DataParameter = null;

		#endregion

		#region Web ������������ɵĴ���
		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: �õ����� ASP.NET Web ���������������ġ�
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		///		�����֧������ķ��� - ��Ҫʹ�ô���༭��
		///		�޸Ĵ˷��������ݡ�
		/// </summary>
		private void InitializeComponent()
		{
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion

		#region �������趨�Ĳ������ϸ����

		#region ���ϸ����֮ǰ
		/// <summary>
		/// �����֮ǰע��һЩϸ��������Ϣ
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender (e);

			#region ע�����ڱ���ͻ��˲����Ŀؼ�

			this.Page.RegisterHiddenField(this.ClientID + "_Argument",		"");		//�ͻ����¼��Ĳ���
			this.Page.RegisterHiddenField(this.ClientID + "_Operate",		"");		//�ͻ����¼�������
			this.Page.RegisterHiddenField(this.ClientID + "_Layout",	this.Layout);	//��ǰ��ҳ�沼��

			#endregion

			#region ���ִ���ʱ�������ı���ʾ����ʽ

			string NoContentExceptionStyle = 
@"<style>
	#ReportDetailExceptionStyle A, #ReportDetailExceptionStyle A:visited, #ReportDetailExceptionStyle A:link, #ReportDetailExceptionStyle A:Active { color:blue; padding-top:1px; }
	#ReportDetailExceptionStyle A:hover { color: red; }
</style>";
			this.Page.RegisterClientScriptBlock("ReportDetailExceptionStyle", NoContentExceptionStyle);

			#endregion

			#region �ͻ��˶�ϸ�����Ĳ���

			this.Page.RegisterClientScriptBlock("ReportCell_Menu"	, Script.ReportMenuOperateScript);
			this.Page.RegisterClientScriptBlock("ReportCell_Operate", Script.ReportDetailOperateScript);

			#endregion

			#region ���û��Զ���Ԫ�ص�֧��
			this.Page.RegisterClientScriptBlock("userdefinecomponent", "<?xml:namespace prefix=userdefinecomponent />\n");
			#endregion
		}

		#endregion

		#region ��ͻ����������

		Regex ReplaceContentRegex = new Regex(@"\{[^\{\}\n]+\}", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private string ReplaceParameter(Match myMatch)
		{
			string Value = this.Argument[myMatch.Value.Trim('{', '}')];
			if(Value == null)	return myMatch.Value;
			else				return Value;
		}

		/// <summary>
		/// ���ʹ�������HTML����
		/// </summary>
		/// <param name="Content"></param>
		/// <returns></returns>
		internal protected override string ExplainHtml(string Content)
		{
			return this.ExplainArgument(Content).Replace("this.", this.ClientID + ".");
		}

		/// <summary>
		/// ���ʹ��������ַ���
		/// </summary>
		/// <param name="Content"></param>
		/// <returns></returns>
		internal protected override string ExplainArgument(string Content)
		{
			return ReplaceContentRegex.Replace(Content, new MatchEvaluator(this.ReplaceParameter));
		}
		/// <summary>
		/// ��ȡ��ͻ������������
		/// </summary>
		/// <returns></returns>
		protected virtual string GetContent()
		{
			LayoutState myLayout = this.CurLayoutState;
			string Content = null;

			//�滻һЩ�û��Զ������
			Content = this.ExplainHtml(myLayout.InnerHtml.Trim());

			//���Ӷ��Զ���HTC��֧��
			Content = this.AddUserDefineComponent(Content);

			if(this.OnBeforeOutput != null)
			{
				this.DbCommand.CommandText = "";
				this.DbCommand.CommandType = CommandType.Text;
				this.OnBeforeOutput(this, ref Content);
			}
			return Content;
		}

		/// <summary>
		/// ��ͻ������������
		/// </summary>
		protected string Content = null;


		/// <summary>
		/// ��ȡ��ʽ��
		/// </summary>
		/// <returns></returns>
		private string GetStyle()
		{
			string ID  = "#ReportDetail_" + this.ClientID + " ";
			string CurLayoutName = this.CurLayoutState.Name.ToLower();

			System.Text.StringBuilder myStyle = new System.Text.StringBuilder();
			myStyle.Append("<style type=text/css>");
			foreach(string Key in this.UserDefineStyle.GetKeys())
			{
				string Name = Key;
				if(Name.StartsWith("$"))
				{
					int start = Name.IndexOfAny(new char[]{':', ' ', '\t', '\r', '\n'});
					if(Name.Substring(1, start-1) == CurLayoutName)	Name = Name.Substring(start+1);
					else											continue;
				}
				string Pattern = this.UserDefineStyle[Key].Pattern;
				myStyle.Append(ID);
				myStyle.Append(Name);
				myStyle.Append("{");
				myStyle.Append(Pattern);
				myStyle.Append("}");
			}
			myStyle.Append("</style>");
			return myStyle.ToString();
		}


		/// <summary>
		/// ��ͻ��������Ϣ
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			LayoutState myLayout = null;
			if(this.ErrorMsg == null)
			{
				//��Ⲽ��
				if(this.Layouts == null)
				{
					this.ErrorMsg = "���ҳ�沼�֣���δ����ҳ�沼�֣�";
				}
				myLayout = this.Layouts[this.Layout];
				if(myLayout == null)
				{
					if(this.Layout == null)	this.ErrorMsg = "���ҳ�沼�֣���δ����ҳ�沼�֣���δ��ҳ�沼�ָ������֣�";
					else					this.ErrorMsg = "���ҳ�沼�֣�������ָ����ҳ�沼��" + this.Layout;
				}
			}

			if(this.ErrorMsg != null)
			{
				if(this.Request.UserHostAddress=="127.0.0.1" || this.Request.UserHostName==System.Net.Dns.Resolve(System.Net.Dns.GetHostName()).AddressList[0].ToString())
						writer.Write(this.CreateErrorButton(this.ErrorMsg));
				else	writer.Write(this.CreateErrorButton(""));
				return;
			}

			//�����ʽ��
			writer.Write(this.GetStyle());

			//��ͻ����������
			writer.Write("<span id=ReportDetail_");
			writer.Write(this.ClientID);
			writer.Write(">");
			
			if(this.Content == null)	this.Content = this.GetContent();
			writer.Write(this.Content);

			writer.Write("</span>");

			//����һ��Javascript�ű��������͵��ͻ��ˣ����ڸ����ֶθ�ֵ
			System.Text.StringBuilder myScript = new System.Text.StringBuilder();
			myScript.Append("<script language = javascript>\n");
			myScript.Append("var ?clientId?_arrValues = new Array(\n".Replace("?clientId?", this.ClientID));
			bool IsExist = false;
			foreach(object Key in this.Fields.GetKeys())
			{
				FieldAttrib myFieldAttrib = this.Fields[Key.ToString()];
				IDbDataParameter myParameter = myFieldAttrib.GetParameter();

				//��ȡ�ֶ�ֵ
				object Value = myFieldAttrib.TrueValue;
				if(Tools.IsNull(Value))		Value = null;
				else if(Value is string)	//֧��UBB�����ת��
				{
					if(myFieldAttrib.IsSupportUBB && myLayout.IsSupportUBB)		Value = this.UBB.GetHtmlCode(Value as string);
				}
				myScript.Append("new Array('");
				myScript.Append(Key);							//�ֶ���
				myScript.Append("', '");
				myScript.Append(Value==null?null:Value.ToString().Replace("'", @"\'").Replace("\r\n", @"\n"));	//ֵ
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.IsKey);			//�Ƿ�Ϊ����
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.DbType);			//�ֶ�����
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.Size);			//�ֶγ���
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.IsNullable);		//�Ƿ�����Ϊ��
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.Verification==null?null:myFieldAttrib.Verification.Replace("'", @"\'").Replace("\r\n", @"\n").Replace(@"\", @"\\"));	//��֤��
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.ErrorMsg==null?null:myFieldAttrib.ErrorMsg.Replace("'", @"\'").Replace("\r\n", @"\n"));	//������Ϣ
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.Title.Replace("'", @"\'").Replace("\r\n", @"\n"));		//����
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.IsSupportUBB);	//�Ƿ�֧��UBB����
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.FormatString==null?null:myFieldAttrib.TrueFormatString.Replace("'", @"\'"));	//��ʽ���ַ���
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.IsUpdateable);	//�Ƿ��������
				myScript.Append("'),\n");
				IsExist = true;
			}
			if(IsExist==true)	myScript[myScript.Length-2] = '\n';
			myScript.Append(");\n");
			myScript.Append("try{var ?clientId? = new ReportDetailOperate('?clientId?', ?clientId?_arrValues);\n".Replace("?clientId?", this.ClientID));
			myScript.Append("ReportDetailSetValue('?clientId?', ?clientId?_arrValues);\n".Replace("?clientId?", this.ClientID));
			myScript.Append("}catch(e){}</script>\n");

			writer.Write(myScript.ToString());
		}

		/// <summary>
		/// ���Ӷ��û��Զ��������֧��
		/// </summary>
		/// <param name="InnerHtml"></param>
		/// <returns></returns>
		private string AddUserDefineComponent(string InnerHtml)
		{
			MatchEvaluator myMatchEvaluator = new MatchEvaluator(ReplaceOperate);
			Regex myRegex = new Regex(@"<(htc:\S+)[^>]*?></\1>|<htc:[^>]*?/>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
			return myRegex.Replace(InnerHtml, myMatchEvaluator);
		}

		Regex myRegex1 = new Regex(@"htc\s*=\s*\S+?(?=\s|/>|>)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
		Regex myRegex2 = new Regex(@"htc:", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

		private string ReplaceOperate(Match myMatch)
		{
			string strHtc = myMatch.Value;
			System.Text.StringBuilder myStr = new System.Text.StringBuilder();

			//Ѱ��HTC��·��
			string htc = myRegex1.Match(strHtc).Value;
			if(htc=="")		return "";

			//����HTML�ַ���
			myStr.Append("\n<?import namespace=userdefinecomponent ");
			myStr.Append("implementation" + htc.Substring(3));
			myStr.Append(" />\n");
			myStr.Append(myRegex2.Replace(strHtc, "userdefinecomponent:"));

			return myStr.ToString();
		}

		#endregion

		#endregion

		#region ��������ʽ��ȡ�����ø��ֶε�ֵ

		/// <summary>
		/// ��������ʽ��ȡ�����ø��ֶε�ֵ
		/// </summary>
		public virtual object this[string FieldName]
		{
			get
			{
				return this.GetNewValue(FieldName);
			}
			set
			{
				this.SetNewValue(FieldName, value);
			}
		}

		/// <summary>
		/// ��ȡ���ݿ�����ǰ���ڵ�ֵ
		/// </summary>
		/// <param name="FieldName">�ֶ���</param>
		/// <returns></returns>
		public object GetOldValue(string FieldName)
		{
			FieldAttrib myFieldAttrib = this.Fields[FieldName];
			if(myFieldAttrib == null)	return null;
			else						return	myFieldAttrib.OldValue;
		}

		/// <summary>
		/// ��ȡ�¸����ֵ
		/// </summary>
		/// <param name="FieldName">�ֶ���</param>
		/// <returns></returns>
		public object GetNewValue(string FieldName)
		{
			FieldAttrib myFieldAttrib = this.Fields[FieldName];
			if(myFieldAttrib == null)		return null;
			else							return myFieldAttrib.GetParameter().Value;
		}

		/// <summary>
		/// ����ָ���ֶε�ֵ
		/// </summary>
		/// <param name="FieldName"></param>
		/// <param name="Value"></param>
		/// <returns></returns>
		public void SetNewValue(string FieldName, object Value)
		{
			FieldAttrib myFieldAttrib = this.Fields[FieldName.ToLower()];
			if(myFieldAttrib == null)		throw new ReportException("�ֶ�" + FieldName + "��ϸ�����ֶμ����в����ڣ�");
			myFieldAttrib.GetParameter().Value = Value;
			myFieldAttrib.IsUpdated = true;
			if(myFieldAttrib.IsKey)		this.RefreshKeyFlag = true;
		}

		/// <summary>
		/// ��¼�Ƿ�����������¸�ֵ��������¸�ֵ�������¶�ȡ��������
		/// </summary>
		private bool RefreshKeyFlag = false;

		/// <summary>
		/// ��ϸ������ԭΪ��Ӽ�¼��״̬
		/// </summary>
		public void Reset()
		{
			foreach(FieldAttrib myFieldAttrib in this.Fields)
				myFieldAttrib.GetParameter().Value = myFieldAttrib.GetDefaultValue();
		}

		/// <summary>
		/// �ڸ��¡�ɾ����¼֮ǰ�����ȶ�ȡ���ݿ����Ѿ����ڵ�ֵ��
		/// </summary>
		private bool GetOldValues()
		{
			//ִ�����ݶ�ȡ��������Ӧ�Ĳ���
			string CommandText = this.CurLayoutState.SelectSql;

			//���δ����SELECT��䣬��ִ�����ݵĶ�ȡ
			if(CommandText == null)	 return false;

			if(CommandText.Length>0 && CommandText[0]=='@')		//����һ���ַ�Ϊ'@'��������Ǵ洢����
			{
				this.DbCommand.CommandText = CommandText.Substring(1);
				this.DbCommand.CommandType = CommandType.StoredProcedure;
			}
			else
			{
				this.DbCommand.CommandText = CommandText;
				this.DbCommand.CommandType = CommandType.Text;
			}

			IDataReader myReader = null;
			try
			{
				this.AppendParameters();
				myReader = this.DbCommand.ExecuteReader();
				if(myReader.Read())
				{
					for(int index=0; index<myReader.FieldCount; index++)
					{
						string Key = myReader.GetName(index);
						FieldAttrib myFieldAttrib = this.Fields[Key];
						myFieldAttrib.OldValue = myReader[Key];
					}
				}
			}
			catch
			{
				return false;
			}
			finally
			{
				myReader.Close();
			}
			return true;
		}

		/// <summary>
		/// �������е�ֵ��ֻ�ڸ���״̬��ɾ��״̬�в���Ч
		/// </summary>
		public void ResetFieldValues()
		{
			if(this.IsPostBack && this.IsUpdateState)
			{
				foreach(string Key in this.Fields.GetKeys())
				{
					FieldAttrib myFieldAttrib = this.Fields[Key];
					if(!myFieldAttrib.IsKey)	myFieldAttrib.GetParameter().Value = myFieldAttrib.OldValue;
				}
			}
		}

		#endregion

		#region ϸ�������¼�

		/// <summary>
		/// ����ͻ����������֮ǰ���������ؿձ�ʾ������ͻ����������
		/// </summary>
		public event OnBeforeOutput OnBeforeOutput = null;

		/// <summary>
		/// �ڶ����ݽ��ж�ȡ�����롢���¡�ɾ��֮ǰ����������false��ʾ����ִ��Ĭ�ϲ���
		/// </summary>
		public event OnDetailOperate OnBeforeOperate = null;

		/// <summary>
		/// �ڶ����ݽ��ж�ȡ�����롢���¡�ɾ��֮��������ʱ������δ�ύ������false���ع�����
		/// </summary>
		public event OnDetailOperate OnAfterOperate = null;

		/// <summary>
		/// �ӿͻ��˴��������¼�
		/// </summary>
		public event OnDetailCommand OnDetailCommand = null;

		#endregion

		#region ֧��UBB����Ļ������

		static System.Collections.Hashtable UBBGroup = new System.Collections.Hashtable();

		private UBB _UBB = null;
		/// <summary>
		/// ֧��UBB����Ļ������
		/// </summary>
		public UBB UBB
		{
			get
			{
				if(this._UBB == null)
				{
					this._UBB = UBBGroup[this.Page.ClientID + this.ClientID] as UBB;
					if(this._UBB == null)
					{
						this._UBB = new UBB(100);
						UBBGroup.Add(this.Page.ClientID + this.ClientID, this._UBB);
					}
				}
				return this._UBB;
			}
		}

		#endregion

		#region IDisposable ��Ա

		/// <summary>
		/// ��ϵͳ�Զ��ͷ���Դ
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();
			if(this._DbConnection != null)
			{
				if(!this.IsConnectionOpen)	this._DbConnection.Close();
			}
		}

		#endregion

		#region ��������

		#endregion
	}
}
