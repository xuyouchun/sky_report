using System;
using System.Data;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	/// <summary>
	///	细节屏：功能强大且灵活的对数据库记录进行操作的控件
	/// </summary>
	public class ReportDetail : ReportBase
	{
		#region 读取、插入、更新、删除数据的默认操作

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
					this.ErrorMsg = "数据链接尚未初始化！";
				}
				else
				{
					if(!this.IsPostBack || this.RefreshKeyFlag)		//读取数据
					{
						if(this.IsUpdateState)
						{
							//执行数据的读取
							if(this.CallEventFunction(this.OnBeforeOperate, Operate.Select))	this.Select();
						}
						if(!this.ExplainValues())	return;
					}
					else	//对数据进行插入、更新、删除等
					{
						if(!this.ExplainValues())	return;
						//执行对数据的操作
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
						//如果是更新或删除操作，则首先读取原有的值
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
								this.ErrorMsg = "触发事件 OnBeforeOperate：" + myException.Message;
							}

							try
							{
								if(result)
								{
									//执行对数据的默认操作
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
								this.ErrorMsg = "执行数据" + (myOperate==Operate.Delete?"删除":myOperate==Operate.Insert?"插入":myOperate==Operate.Update?"更新":"重置") + "：" + myException.Message;
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
		/// 将表达式转换为实际的值
		/// </summary>
		private bool ExplainValues()
		{
			//将表达式转换为实际的值
			try
			{
				this.Argument.ExplainValues();
				return true;
			}
			catch(ReportException myExp)
			{
				this.ErrorMsg = "表达式转换：" + myExp.Message;
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

		#region 对数据的操作

		#region 一些基本的操作
		/// <summary>
		/// 是否为更新状态
		/// </summary>
		public bool IsUpdateState
		{
			//原则：检查主键是否都已经赋值，如果是，则为更新状态，若都未赋值，则为插入状态，如果赋值不完整，则抛出异常
			get
			{
				bool result = false;	int index = 0;
				foreach(object item in this.Fields.GetKeyFields())
				{
					object Value = (item as FieldAttrib).GetParameter().Value;
					if(index++ == 0)		result = !(Value is System.Data.SqlTypes.INullable || Value==null);
					bool result1 = !(Value is System.Data.SqlTypes.INullable || Value==null);
					if(result != result1)	this.ErrorMsg = "主键赋值不完整！";
				}
				return result;
			}
		}

		/// <summary>
		/// 增加参数
		/// </summary>
		internal void AppendParameters()
		{
			this.DbCommand.Parameters.Clear();
			switch(this.DbConType)		//根据数据链接类型判断如何增加参数
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
						if(myFieldAttrib == null)	throw new ReportException("无法获取参数" + Item);
						IDbDataParameter myParameter = myFieldAttrib.GetParameter();
						if(!this.DbCommand.Parameters.Contains(myParameter))	this.DbCommand.Parameters.Add(myParameter);
					}
					break;
			}
		}

		/// <summary>
		/// 数据链接类型
		/// </summary>
		private string DbConType = null;

		/// <summary>
		/// 寻找参数
		/// </summary>
		private Regex FindArgument = new Regex(@"'[^']*'|@[^\s,\)\n;]+", RegexOptions.Compiled);
		/// <summary>
		/// 根据寻找到的参数，增加实际的值ss
		/// </summary>
		/// <param name="myMatch"></param>
		/// <returns></returns>
		private string ReplaceSub(Match myMatch)
		{
			string Item = myMatch.Value;
			if(Item[0] != '@')		return Item;

			FieldAttrib myFieldAttrib = this.Fields[Item.TrimStart('@')];
			if(myFieldAttrib == null)	throw new ReportException("无法获取参数" + Item);
			IDbDataParameter myParameter = myFieldAttrib.GetParameter();
			if(this.DbCommand.Parameters.Contains(myParameter))
				this.DbCommand.Parameters.Add(this.CopyParameter(myParameter));
			else this.DbCommand.Parameters.Add(myParameter);
			return "?";
		}

		/// <summary>
		/// 复制一个参数对象
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
		/// 上一个操作受影响的行数
		/// </summary>
		public int Catch
		{
			get { return this._Catch; }
		}

		/// <summary>
		/// 是否为查询语句
		/// </summary>
		Regex IsMatchGetValue = new Regex(@"^\s*\S+(?=\s*=\s*select)", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
		/// <summary>
		/// 分析SQL语句，是存储过程还是SQL，并进行相应的操作
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

		#region 读取数据
		/// <summary>
		/// 读取数据（从数据库）
		/// </summary>
		private void Select()
		{
			//执行数据读取并赋予相应的参数
			string CommandText = this.CurLayoutState.SelectSql;

			//如果未定义SELECT语句，则不执行数据的读取
			if(CommandText == null)	 return;

			if(CommandText.Length>0 && CommandText[0]=='@')		//若第一个字符为'@'，则代表是存储过程
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
						this.ErrorMsg = "读取数据：错误，查询结果不唯一！";
					}
				}
				else	this._Catch = 0;
				myReader.Close();
			}
			catch(Exception e)
			{
				this.ErrorMsg = "读取数据：" + e.Message;
			}
		}
		#endregion

		#region 插入数据
		/// <summary>
		/// 插入数据
		/// </summary>
		private void Insert()
		{
			//赋予相应的插入数据的SQL语句
			string CommandText = this.CurLayoutState.InsertSql;
			if(CommandText == null)	{ this.ErrorMsg = "插入数据：尚未定义相应的 Insert 语句！"; return; }

			CommandText = this.ExplainArgument(CommandText);

			//执行相应的插入数据的操作
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

		#region 更新数据
		/// <summary>
		/// 更新数据
		/// </summary>
		private void Update()
		{
			//赋予相应的SQL语句
			string CommandText = this.CurLayoutState.UpdateSql;
			if(CommandText == null)	{ this.ErrorMsg = "更新数据：尚未定义相应的 Update 语句！"; return; }

			CommandText = this.ExplainArgument(CommandText);

			//执行相应的更新数据的操作
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

			if(operatecount==1 && this.Catch!=1)		//为防止更新错误，而增加的防护措施
			{
				if(this.Catch == 0)		this.ErrorMsg = "更新数据：该项已被删除！";
				else					this.ErrorMsg = "更新数据：严重警告，将更新多条记录！（已回滚）";
				myTrans.Rollback();		return;
			}

			this.DbCommand.CommandType = CommandType.Text;
			this.DbCommand.CommandText = "";

			if(this.CallEventFunction(this.OnAfterOperate, Operate.Update))		myTrans.Commit();
			else	myTrans.Rollback();
			this.DbCommand.Transaction = null;
		}
		#endregion

		#region 删除数据
		/// <summary>
		/// 删除数据
		/// </summary>
		private void Delete()
		{
			//赋予相应的SQL语句
			string CommandText = this.CurLayoutState.DeleteSql;
			if(CommandText == null)	{ this.ErrorMsg = "删除数据：尚未定义相应的 Delete 语句！"; return; }

			CommandText = this.ExplainArgument(CommandText);

			//执行相应的删除数据的操作
			IDbTransaction myTrans = this.DbConnection.BeginTransaction();
			this.DbCommand.Transaction = myTrans;

			int operatecount = 0;
			foreach(Match myMatch in SplitSql.Matches(CommandText))
			{
				string Sql = myMatch.Value.Trim();
				if(Sql.Trim() == "")	continue;
				string Field = this.AttachSql(Sql);
				this.AppendParameters();		//增加参数
				if(Field != "")		this[Field] = this.DbCommand.ExecuteScalar();
				else
				{
					this._Catch = this.DbCommand.ExecuteNonQuery();
					operatecount ++;
				}
			}

			
			if(operatecount==1 && this.Catch!=1)		//为防止更新错误，而增加的防护措施
			{
				if(this.Catch == 0)		this.ErrorMsg = "删除数据：该项已被删除！";
				else					this.ErrorMsg = "删除数据：严重警告，将删除多条记录！（已回滚）";
				myTrans.Rollback();		return;
			}

			this.DbCommand.CommandType = CommandType.Text;
			this.DbCommand.CommandText = "";

			if(this.CallEventFunction(this.OnAfterOperate, Operate.Delete))
			{
				myTrans.Commit();
				//将各字段值清空
				foreach(object Key in this.Fields.GetKeys())		this[Key as string] = null;
			}
			else	myTrans.Rollback();
			this.DbCommand.Transaction = null;
		}
		#endregion

		#region 强制细节屏的操作

		/// <summary>
		/// 强制细节屏执行操作
		/// </summary>
		public override void Execute()
		{
			this.Page_Load(null, null);
			if(this.Content == null)	this.Content = this.GetContent();
		}

		#endregion

		#endregion

		#region 所应用的页面布局

		string _Layout = null;
		/// <summary>
		/// 所应用的页面布局名
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
		/// 当前所用的页面布局
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

		#region 数据链接

		private System.Data.IDbConnection _DbConnection = null;

		/// <summary>
		/// 数据链接对象
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
		/// 命令执行对象
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
		/// 数据链接通道
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

		#region 字段属性组

		private FieldAttribGroup _Fields = null;
		/// <summary>
		/// 字段属性组
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

		#region 页面布局组

		private LayoutStateGroup _Layouts = null;
		/// <summary>
		/// 页面布局组
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

		#region 用户自定义变量组

		private ArgumentGroup _Argument = null;
		/// <summary>
		/// 用户自定义变量组
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

		#region 用户自定义样式组

		private UserDefineStyleGroup _UserDefineStyle;

		/// <summary>
		/// 用户自定义样式组
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

		#region 数据库参数项
		/// <summary>
		/// 数据库参数项
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

		#region Web 窗体设计器生成的代码
		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: 该调用是 ASP.NET Web 窗体设计器所必需的。
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		///		设计器支持所需的方法 - 不要使用代码编辑器
		///		修改此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion

		#region 根据所设定的参数输出细节屏

		#region 输出细节屏之前
		/// <summary>
		/// 在输出之前注释一些细节屏的信息
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender (e);

			#region 注册用于保存客户端操作的控件

			this.Page.RegisterHiddenField(this.ClientID + "_Argument",		"");		//客户端事件的参数
			this.Page.RegisterHiddenField(this.ClientID + "_Operate",		"");		//客户端事件的名字
			this.Page.RegisterHiddenField(this.ClientID + "_Layout",	this.Layout);	//当前的页面布局

			#endregion

			#region 出现错误时，错误文本显示的样式

			string NoContentExceptionStyle = 
@"<style>
	#ReportDetailExceptionStyle A, #ReportDetailExceptionStyle A:visited, #ReportDetailExceptionStyle A:link, #ReportDetailExceptionStyle A:Active { color:blue; padding-top:1px; }
	#ReportDetailExceptionStyle A:hover { color: red; }
</style>";
			this.Page.RegisterClientScriptBlock("ReportDetailExceptionStyle", NoContentExceptionStyle);

			#endregion

			#region 客户端对细节屏的操作

			this.Page.RegisterClientScriptBlock("ReportCell_Menu"	, Script.ReportMenuOperateScript);
			this.Page.RegisterClientScriptBlock("ReportCell_Operate", Script.ReportDetailOperateScript);

			#endregion

			#region 对用户自定义元素的支持
			this.Page.RegisterClientScriptBlock("userdefinecomponent", "<?xml:namespace prefix=userdefinecomponent />\n");
			#endregion
		}

		#endregion

		#region 向客户端输出数据

		Regex ReplaceContentRegex = new Regex(@"\{[^\{\}\n]+\}", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private string ReplaceParameter(Match myMatch)
		{
			string Value = this.Argument[myMatch.Value.Trim('{', '}')];
			if(Value == null)	return myMatch.Value;
			else				return Value;
		}

		/// <summary>
		/// 解释带参数的HTML代码
		/// </summary>
		/// <param name="Content"></param>
		/// <returns></returns>
		internal protected override string ExplainHtml(string Content)
		{
			return this.ExplainArgument(Content).Replace("this.", this.ClientID + ".");
		}

		/// <summary>
		/// 解释带参数的字符串
		/// </summary>
		/// <param name="Content"></param>
		/// <returns></returns>
		internal protected override string ExplainArgument(string Content)
		{
			return ReplaceContentRegex.Replace(Content, new MatchEvaluator(this.ReplaceParameter));
		}
		/// <summary>
		/// 获取向客户端输出的内容
		/// </summary>
		/// <returns></returns>
		protected virtual string GetContent()
		{
			LayoutState myLayout = this.CurLayoutState;
			string Content = null;

			//替换一些用户自定义参数
			Content = this.ExplainHtml(myLayout.InnerHtml.Trim());

			//增加对自定义HTC的支持
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
		/// 向客户端输出的内容
		/// </summary>
		protected string Content = null;


		/// <summary>
		/// 获取样式表
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
		/// 向客户端输出信息
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			LayoutState myLayout = null;
			if(this.ErrorMsg == null)
			{
				//检测布局
				if(this.Layouts == null)
				{
					this.ErrorMsg = "输出页面布局：尚未定义页面布局！";
				}
				myLayout = this.Layouts[this.Layout];
				if(myLayout == null)
				{
					if(this.Layout == null)	this.ErrorMsg = "输出页面布局：尚未定义页面布局，或未给页面布局赋予名字！";
					else					this.ErrorMsg = "输出页面布局：不存在指定的页面布局" + this.Layout;
				}
			}

			if(this.ErrorMsg != null)
			{
				if(this.Request.UserHostAddress=="127.0.0.1" || this.Request.UserHostName==System.Net.Dns.Resolve(System.Net.Dns.GetHostName()).AddressList[0].ToString())
						writer.Write(this.CreateErrorButton(this.ErrorMsg));
				else	writer.Write(this.CreateErrorButton(""));
				return;
			}

			//输出样式表
			writer.Write(this.GetStyle());

			//向客户端输出代码
			writer.Write("<span id=ReportDetail_");
			writer.Write(this.ClientID);
			writer.Write(">");
			
			if(this.Content == null)	this.Content = this.GetContent();
			writer.Write(this.Content);

			writer.Write("</span>");

			//构造一段Javascript脚本，并发送到客户端，用于给各字段赋值
			System.Text.StringBuilder myScript = new System.Text.StringBuilder();
			myScript.Append("<script language = javascript>\n");
			myScript.Append("var ?clientId?_arrValues = new Array(\n".Replace("?clientId?", this.ClientID));
			bool IsExist = false;
			foreach(object Key in this.Fields.GetKeys())
			{
				FieldAttrib myFieldAttrib = this.Fields[Key.ToString()];
				IDbDataParameter myParameter = myFieldAttrib.GetParameter();

				//获取字段值
				object Value = myFieldAttrib.TrueValue;
				if(Tools.IsNull(Value))		Value = null;
				else if(Value is string)	//支持UBB代码的转换
				{
					if(myFieldAttrib.IsSupportUBB && myLayout.IsSupportUBB)		Value = this.UBB.GetHtmlCode(Value as string);
				}
				myScript.Append("new Array('");
				myScript.Append(Key);							//字段名
				myScript.Append("', '");
				myScript.Append(Value==null?null:Value.ToString().Replace("'", @"\'").Replace("\r\n", @"\n"));	//值
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.IsKey);			//是否为主键
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.DbType);			//字段类型
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.Size);			//字段长度
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.IsNullable);		//是否允许为空
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.Verification==null?null:myFieldAttrib.Verification.Replace("'", @"\'").Replace("\r\n", @"\n").Replace(@"\", @"\\"));	//验证码
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.ErrorMsg==null?null:myFieldAttrib.ErrorMsg.Replace("'", @"\'").Replace("\r\n", @"\n"));	//错误信息
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.Title.Replace("'", @"\'").Replace("\r\n", @"\n"));		//别名
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.IsSupportUBB);	//是否支持UBB代码
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.FormatString==null?null:myFieldAttrib.TrueFormatString.Replace("'", @"\'"));	//格式化字符串
				myScript.Append("', '");
				myScript.Append(myFieldAttrib.IsUpdateable);	//是否允许更改
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
		/// 增加对用户自定义组件的支持
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

			//寻找HTC的路径
			string htc = myRegex1.Match(strHtc).Value;
			if(htc=="")		return "";

			//构造HTML字符串
			myStr.Append("\n<?import namespace=userdefinecomponent ");
			myStr.Append("implementation" + htc.Substring(3));
			myStr.Append(" />\n");
			myStr.Append(myRegex2.Replace(strHtc, "userdefinecomponent:"));

			return myStr.ToString();
		}

		#endregion

		#endregion

		#region 以索引形式读取或设置各字段的值

		/// <summary>
		/// 以索引形式读取或设置各字段的值
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
		/// 读取数据库中先前存在的值
		/// </summary>
		/// <param name="FieldName">字段名</param>
		/// <returns></returns>
		public object GetOldValue(string FieldName)
		{
			FieldAttrib myFieldAttrib = this.Fields[FieldName];
			if(myFieldAttrib == null)	return null;
			else						return	myFieldAttrib.OldValue;
		}

		/// <summary>
		/// 读取新赋予的值
		/// </summary>
		/// <param name="FieldName">字段名</param>
		/// <returns></returns>
		public object GetNewValue(string FieldName)
		{
			FieldAttrib myFieldAttrib = this.Fields[FieldName];
			if(myFieldAttrib == null)		return null;
			else							return myFieldAttrib.GetParameter().Value;
		}

		/// <summary>
		/// 设置指定字段的值
		/// </summary>
		/// <param name="FieldName"></param>
		/// <param name="Value"></param>
		/// <returns></returns>
		public void SetNewValue(string FieldName, object Value)
		{
			FieldAttrib myFieldAttrib = this.Fields[FieldName.ToLower()];
			if(myFieldAttrib == null)		throw new ReportException("字段" + FieldName + "在细节屏字段集合中不存在！");
			myFieldAttrib.GetParameter().Value = Value;
			myFieldAttrib.IsUpdated = true;
			if(myFieldAttrib.IsKey)		this.RefreshKeyFlag = true;
		}

		/// <summary>
		/// 记录是否对主键已重新赋值，如果重新赋值，则重新读取所有数据
		/// </summary>
		private bool RefreshKeyFlag = false;

		/// <summary>
		/// 将细节屏复原为添加记录的状态
		/// </summary>
		public void Reset()
		{
			foreach(FieldAttrib myFieldAttrib in this.Fields)
				myFieldAttrib.GetParameter().Value = myFieldAttrib.GetDefaultValue();
		}

		/// <summary>
		/// 在更新、删除记录之前，首先读取数据库中已经存在的值。
		/// </summary>
		private bool GetOldValues()
		{
			//执行数据读取并赋予相应的参数
			string CommandText = this.CurLayoutState.SelectSql;

			//如果未定义SELECT语句，则不执行数据的读取
			if(CommandText == null)	 return false;

			if(CommandText.Length>0 && CommandText[0]=='@')		//若第一个字符为'@'，则代表是存储过程
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
		/// 重置所有的值，只在更新状态和删除状态中才有效
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

		#region 细节屏的事件

		/// <summary>
		/// 在向客户端输出内容之前发生，返回空表示不再向客户端输出内容
		/// </summary>
		public event OnBeforeOutput OnBeforeOutput = null;

		/// <summary>
		/// 在对数据进行读取、插入、更新、删除之前发生，返回false表示不再执行默认操作
		/// </summary>
		public event OnDetailOperate OnBeforeOperate = null;

		/// <summary>
		/// 在对数据进行读取、插入、更新、删除之后发生，此时事务尚未提交，返回false将回滚事务
		/// </summary>
		public event OnDetailOperate OnAfterOperate = null;

		/// <summary>
		/// 从客户端传过来的事件
		/// </summary>
		public event OnDetailCommand OnDetailCommand = null;

		#endregion

		#region 支持UBB代码的缓存对象

		static System.Collections.Hashtable UBBGroup = new System.Collections.Hashtable();

		private UBB _UBB = null;
		/// <summary>
		/// 支持UBB代码的缓存对象
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

		#region IDisposable 成员

		/// <summary>
		/// 由系统自动释放资源
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

		#region 其它功能

		#endregion
	}
}
