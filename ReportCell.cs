using System;
using System.Xml;
using System.Data;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace Skyever.Report
{
	/// <summary>
	///	汇总屏，功能强大且高效的列表显示控件
	/// </summary>
	public class ReportCell : ReportBase
	{
		#region 页面初始化

		bool IsExecutePageLoad = false;
		private void Page_Load(object sender, System.EventArgs e)
		{
			if(this.IsExecutePageLoad)	return;
			this.IsExecutePageLoad = true;

			//将表达式转换为实际的值
			try	{ this.Argument.ExplainValues(); }
			catch(ReportException myExp)
			{
				this.ErrorMsg = "表达式转换：" + myExp.Message;
				return;
			}

			//检查是否已经赋予应有的属性
			if(_SelectSql == null)
			{
				this.ErrorMsg = "未指定数据读取方式！";
				return;
			}

			if(this.DbConnection == null)
			{
				this.ErrorMsg = "数据链接尚未初始化！";
				return;
			}

			if(this.IsPostBack)
			{
				if(this.OnCellCommand != null)
				{
					string CommandName = this.Request.Form[this.ClientID + "_Operate"];
					string Argument	   = this.Request.Form[this.ClientID + "_Argument"];

					// 保存数据
					if(CommandName=="Save")
					{
						this.CommitClientMessage();
					}

					try
					{
						if(CommandName != "")
						{
							this.OnCellCommand(this, CommandName, Argument);
						}
					}
					catch(Exception ex)
					{
						this.ErrorMsg = "触发事件 OnCellCommand：" + ex.Message;
					}
				}
			}
		}

		

		/// <summary>
		/// 强制执行操作，用于解决优先级问题
		/// </summary>
		public override void Execute()
		{
			this.Page_Load(null, null);
			if(this.Content == null)	this.Content = this.GetContent();
			this._IsExecute = true;
		}

		private string ErrorMsg = null;
		#endregion

		#region 根据客户端信息对数据库的插入、更新、删除数据的操作

		/// <summary>
		/// 根据客户端信息对数据库的插入、更新、删除数据的操作
		/// </summary>
		private void CommitClientMessage()
		{
			string ClientMsg = Decode(this.EditMessage);

			System.Collections.Hashtable ClientMsgCollection = new System.Collections.Hashtable();

			System.IO.StringReader xmlStream = new System.IO.StringReader(ClientMsg);
			System.Xml.XmlTextReader xmlReader = new System.Xml.XmlTextReader(xmlStream);
			xmlReader.WhitespaceHandling = WhitespaceHandling.None;
			while(xmlReader.Read())
			{
				if(xmlReader.NodeType==XmlNodeType.Element && xmlReader.Name=="Row")	// 行的节点
				{
					string RowFlag  = xmlReader.GetAttribute("flag");
					CommitMessageItemType RowState = (CommitMessageItemType)System.Enum.Parse(typeof(CommitMessageItemType), xmlReader.GetAttribute("state"), true);
					CommitMessageItem NewItem = new CommitMessageItem(RowFlag, RowState);
					ClientMsgCollection.Add(RowFlag, NewItem);

					while(xmlReader.Read())
					{
						if(xmlReader.NodeType==XmlNodeType.EndElement && xmlReader.Name=="Row")	break;
						if(xmlReader.Name=="Cell")	// 列的节点
						{
							string ColName = xmlReader.GetAttribute("name");
							string Value = null, NewValue = null;

							string DbType = this.ColStyle[ColName].DbType;

							// 读取值
							xmlReader.ReadStartElement();
							Value = xmlReader.ReadString();
							xmlReader.ReadEndElement();

							// 读取新值
							if(RowState==CommitMessageItemType.Update)
							{
								xmlReader.ReadStartElement();
								NewValue = xmlReader.ReadString();
								xmlReader.ReadEndElement();
							}

							NewItem.Append(ColName, Value, NewValue);
						}
					}
				}
			}
			xmlReader.Close();
		}

		static Regex EncodeRegex = new Regex(@"#\w", RegexOptions.Compiled | RegexOptions.Multiline);

		/// <summary>
		/// 对字符串进行解码
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		static string Decode(string msg)
		{
			if(msg==null)	return null;
			return EncodeRegex.Replace(msg, new MatchEvaluator(ReplaceEncodeMsg));
		}

		/// <summary>
		/// 对字符串进行解码所使用的委托参数
		/// </summary>
		/// <param name="myMatch"></param>
		static string ReplaceEncodeMsg(Match myMatch)
		{
			switch(myMatch.Value)
			{
				case "#l":	return "<";
				case "#g":	return ">";
				case "#q":	return "\"";
				case "#a":	return "&";
				case "#n":	return "\r\n";
				case "#x":	return "#";
				default:	return myMatch.Value;
			}
		}

		#region 用于组织编辑信息的类

		/// <summary>
		/// 编辑信息的类型
		/// </summary>
		private enum CommitMessageItemType
		{
			/// <summary>
			/// 无变化
			/// </summary>
			NoChange,
			/// <summary>
			/// 已更改
			/// </summary>
			Update,
			/// <summary>
			/// 新增
			/// </summary>
			Insert,
			/// <summary>
			/// 已删除
			/// </summary>
			Delete
		}

		/// <summary>
		/// 用于组织编辑信息的类
		/// </summary>
		private class CommitMessageItem
		{
			/// <summary>
			/// 构造函数
			/// </summary>
			/// <param name="RowFlag">行标识</param>
			/// <param name="RowState">编辑类型</param>
			public CommitMessageItem(string RowFlag, CommitMessageItemType RowState)
			{
				this._RowFlag  = RowFlag;
				this._RowState = RowState;
			}

			CommitMessageItemType _RowState = CommitMessageItemType.NoChange;
			/// <summary>
			/// 进行的操作
			/// </summary>
			public CommitMessageItemType RowState
			{
				get { return this._RowState;  }
			}

			string _RowFlag = null;
			/// <summary>
			/// 行标识
			/// </summary>
			public string RowFlag
			{
				get { return this._RowFlag;  }
			}

			private System.Collections.Hashtable m_ValueGroup = new System.Collections.Hashtable();

			/// <summary>
			/// 增加一个值项
			/// </summary>
			/// <param name="ColName"></param>
			/// <param name="Value"></param>
			public void Append(string ColName, object Value)
			{
				this.Append(ColName, Value, null);
			}

			/// <summary>
			/// 增加一个值项
			/// </summary>
			/// <param name="ColName"></param>
			/// <param name="Value"></param>
			/// <param name="NewValue"></param>
			public void Append(string ColName, object Value, object NewValue)
			{
				this.m_ValueGroup.Add(ColName, new ItemValue(ColName, Value, NewValue));
			}

			/// <summary>
			/// 删除一个值项
			/// </summary>
			/// <param name="ColName"></param>
			public void Remove(string ColName)
			{
				if(this.m_ValueGroup.ContainsKey(ColName))	this.m_ValueGroup.Remove(ColName);
			}

			/// <summary>
			/// 按索引访问值项
			/// </summary>
			public ItemValue this[string ColName]
			{
				get
				{
					if(!this.m_ValueGroup.ContainsKey(ColName))	 return null;
					else	return this.m_ValueGroup[ColName] as ItemValue;
				}
			}

			#region 值的列表

			/// <summary>
			/// 值的列表
			/// </summary>
			public class ItemValue
			{
				public ItemValue(string ColName, object Value, object NewValue)
				{
					this._ColName  = ColName;
					this._Value    = Value;
					this._NewValue = NewValue;
				}

				string _ColName = null;
				/// <summary>
				/// 字段名
				/// </summary>
				public string ColName
				{
					get { return this._ColName; }
				}

				object _Value = null;
				/// <summary>
				/// 值
				/// </summary>
				public object Value
				{
					get { return this._Value;  }
					set { this._Value = value; }
				}

				object _NewValue = null;
				/// <summary>
				/// 新值
				/// </summary>
				public object NewValue
				{
					get { return this._NewValue;  }
					set { this._NewValue = value; }
				}
			}

			#endregion
		}

		#endregion

		#endregion

		#region 一些基本的属性

		private ColStyleGroup _ColStyle = null;
		/// <summary>
		/// 各列的样式
		/// </summary>
		public ColStyleGroup ColStyle
		{
			get	
			{
				if(this._ColStyle==null)
				{
					this._ColStyle = new ColStyleGroup();
					this._ColStyle.ParentReport = this;
					
				}
				return this._ColStyle;
			}
			set
			{
				this._ColStyle = value;
				this._ColStyle.ParentReport = this;
			}
		}

		private RowStyleGroup _RowStyle = null;
		/// <summary>
		/// 各行的样式
		/// </summary>
		public RowStyleGroup RowStyle
		{
			get
			{
				if(this._RowStyle==null)
				{
					this._RowStyle = new RowStyleGroup();
					this._RowStyle.ParentReport = this;
				}
				return this._RowStyle;
			}
			set
			{
				this._RowStyle = value;
				this._RowStyle.ParentReport = this;
			}
		}

		private FilterGroup _Filter = null;
		/// <summary>
		/// 过滤条件的组合
		/// </summary>
		public FilterGroup Filter
		{
			get
			{
				if(this._Filter == null)
				{
					this._Filter = new FilterGroup();
					this._Filter.ParentReport = this;
				}
				return this._Filter;
			}
			set
			{
				this._Filter = value;
				this._Filter.ParentReport = this;
			}
		}

		private UserDefineStyleGroup _UserDefineStyle = null;
		/// <summary>
		/// 用户自定义的样式
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

		/// <summary>
		/// 数据链接
		/// </summary>
		public override IDbConnection DbConnection
		{
			get
			{
				return this.Adapter.DbConnection;
			}
			set
			{
				this.Adapter.DbConnection = value;
				this.DbConType = value.GetType().Name;
				this.IsConnectionOpen = value.State == ConnectionState.Open;
			}
		}

		private IDbCommand _DbCommand = null;
		/// <summary>
		/// 命令执行对象
		/// </summary>
		public override IDbCommand DbCommand
		{
			get
			{
				if(this._DbCommand == null)
				{
					this._DbCommand = this.DbConnection.CreateCommand();
				}
				if(this.Adapter.DbConnection.State == ConnectionState.Closed)	this.Adapter.DbConnection.Open();
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

		/// <summary>
		/// 记录数据连接在开始的时候是否打开
		/// </summary>
		private bool IsConnectionOpen = false;

		Regex ClearMemo  = new Regex(@"/\*[\s\S]*?\*/|\s+--.*?(?=\n|\z)", RegexOptions.Multiline | RegexOptions.Compiled);
		private string _SelectSql = null;
		private bool IsUpdateSelectSql = false;
		/// <summary>
		/// 用于读取数据的SQL语句
		/// </summary>
		public string SelectSql
		{
			get
			{
				if(this._SelectSql == null)		return null;
				if(this.IsUpdateSelectSql == false)
				{
					if(this._SelectSql[0] == '#')
					{
						try
						{
							this._SelectSql = this.CreateSplitPageSql(this._SelectSql);
						}
						catch(ReportException myExp)
						{
							this.ErrorMsg = "分析 SelectSql 语句：" + myExp.Message;
							this._SelectSql = "";
						}
					}
					this.IsUpdateSelectSql = true;
				}
				return this._SelectSql;
			}
			set
			{
				this._SelectSql = ClearMemo.Replace(value.Trim(), "");
				this.IsUpdateSelectSql = false;
			}
		}

		private string _DeleteSql = null;
		/// <summary>
		/// 用于删除数据的SQL语句
		/// </summary>
		public string DeleteSql
		{
			get { return this._DeleteSql;  }
			set { this._DeleteSql = value; }
		}

		private string _InsertSql = null;
		/// <summary>
		/// 用于插入数据的SQL语句
		/// </summary>
		public string InsertSql
		{
			get { return this._InsertSql;  }
			set { this._InsertSql = value; }
		}

		private string _UpdateSql = null;
		/// <summary>
		/// 用于更新数据的SQL语句
		/// </summary>
		public string UpdateSql
		{
			get { return this._UpdateSql;  }
			set { this._UpdateSql = value; }
		}

		private string filter = "{filter}";
		private string _ItemCountSql = null;
		/// <summary>
		/// 用于读取数据库中有多少条记录
		/// </summary>
		public string ItemCountSql
		{
			get
			{
				if(this._ItemCountSql==null)
				{
					if(this.TableName == null)
					{
						this.SelectSql.ToString();
						if(this.TableName==null)	this.TableName = "";
					}
					if(this.TableName!="")	this._ItemCountSql = " select count(*) from " + this.TableName + " where " + this.filter;
				}
				return this._ItemCountSql;
			}
			set { this._ItemCountSql = value; }
		}

		private string _TableName = null;
		/// <summary>
		/// 表名
		/// </summary>
		public string TableName
		{
			get { return this._TableName;  }
			set { this._TableName = value; }
		}

		/// <summary>
		/// 已经选定的行
		/// </summary>
		public System.Collections.ArrayList SelectedRows
		{
			get
			{
				if(this._SelectedRows == null)
				{
					if(this.IsPostBack)
					{
						string SelectedRowsString = this.Request.Form[this.ClientID + "_SelectedRows"];
						if(SelectedRowsString != null)
						{
							string [] arrSelectedRows = SelectedRowsRegex.Split(SelectedRowsString);
							this._SelectedRows = new System.Collections.ArrayList(arrSelectedRows.Length);
							for(int k=0; k<arrSelectedRows.Length; k++)
								if(arrSelectedRows[k]!="")	this._SelectedRows.Add(arrSelectedRows[k]);
						}
					}
				}
				if(this._SelectedRows==null)	this._SelectedRows = new System.Collections.ArrayList();
				return this._SelectedRows;
			}
		}
		Regex SelectedRowsRegex = new Regex(@"\[selectedrows\]", RegexOptions.Multiline | RegexOptions.Compiled);
		System.Collections.ArrayList _SelectedRows;

		/// <summary>
		/// 获取选定行的字符串
		/// </summary>
		/// <returns></returns>
		private string GetSelectedRows()
		{
			if(this.SelectedRows.Count == 0)	return "";
			System.Text.StringBuilder mySelectRows = new System.Text.StringBuilder();
			foreach(object Value in this._SelectedRows)
			{
				mySelectRows.Append(Value.ToString());
				mySelectRows.Append("[selectedrows]");
			}
			return mySelectRows.ToString().Substring(0, mySelectRows.Length-14);
		}

		private EventGroup _EventGroup = null;
		/// <summary>
		/// 汇总屏事件组
		/// </summary>
		public EventGroup Event
		{
			get
			{
				if(this._EventGroup == null)
				{
					this._EventGroup = new EventGroup();
					this._EventGroup.ParentReport = this;
				}
				return this._EventGroup;
			}
			set
			{
				this._EventGroup = value;
				this._EventGroup.ParentReport = this;
			}
		}

		private string _EditMessage = null;
		/// <summary>
		/// 对汇总屏的编辑信息
		/// </summary>
		private string EditMessage
		{
			get
			{
				if(this._EditMessage==null)
				{
					this._EditMessage = this.IsPostBack?this.Request.Form[this.ClientID + "_EditMessage"] : "";
				}
				return this._EditMessage;
			}
			set
			{
				this._EditMessage = value;
			}
		}

		private string DbConType = null;

		#endregion

		#region 对分页控制的SQL语句的构建

		Regex ExplainSql = new Regex(@"#(?(')'[^']*'|[^#'])*?(?=#|\z)", RegexOptions.Multiline | RegexOptions.Compiled);		//用于解释字符串
		Regex GetTitle   = new Regex(@"(?<=#)\S+", RegexOptions.Compiled);

		private string CreateSplitPageSql(string Sql)
		{
			#region 取得基本的信息

			string TableName = null;		//表名
			string [][]KeyField  = null;	//主键字段
			string Field	 = null;		//要查询的字段
			string Filter	 = null;		//过滤条件
			string Order	 = null;		//排序表达式
			MatchCollection myMatchs = ExplainSql.Matches(this._SelectSql);
			foreach(Match myMatch in myMatchs)
			{
				string Value = myMatch.Value.Trim();
				string Title   = GetTitle.Match(Value).Value.ToLower();
				string Content = Value.Substring(Title.Length + 1).Trim();
				switch(Title)
				{
					case "table":	TableName = Content;	break;
					case "key":	
						string [] arrKey = Content.Split(',');
						KeyField = new string[arrKey.Length][];
						for(int k=0; k<KeyField.Length; k++)
						{
							string [] Item = arrKey[k].Trim().Split(' ', '　', '\n', '\r', '\t');
							KeyField[k] = new string[2];
							KeyField[k][0] = Item[0].Trim();
							if(Item.Length>1)	KeyField[k][1] = Item[1].Trim();
							else				KeyField[k][1] = "int";
						}
						break;
					case "field":	Field = Content;	 break;
					case "filter":	
						if(Content!="")	{ Filter = Content; this.filter = Content; }
						break;
					case "order":
						if(Content!="")	Order = Content;
						break;
				}
			}

			if(TableName == null)
			{
				if(this.TableName==null)	throw new ReportException("错误：未指定表名！");
			}
			else if(this.TableName==null)	this.TableName = TableName;

			//检查是否已指定主键
			if(KeyField == null)		throw new ReportException("错误：未指定主键！");

			if(Field==null || Field=="")
			{
				System.Text.StringBuilder TempStr = new System.Text.StringBuilder();
				for(int k=0; k<KeyField.Length; k++)
				{
					TempStr.Append(KeyField[k][0]);
					TempStr.Append(",");
				}
				Field = TempStr.ToString().TrimEnd(',');
				if(Field == "")	throw new ReportException("错误：未指定要显示的字段！");
			}
			else
			{
				System.Text.StringBuilder TempStr = new System.Text.StringBuilder();
				for(int k=0; k<KeyField.Length; k++)
				{
					if(!Regex.IsMatch(Field, @"(?:^|,)\s*key(?:,|$)".Replace("key", KeyField[k][0]), RegexOptions.IgnoreCase))
					{
						TempStr.Append(KeyField[k][0]);
						TempStr.Append(",");
					}
				}
				if(TempStr.Length > 0)	Field = TempStr.ToString() + Field;
			}

			System.Text.StringBuilder myKeys = new System.Text.StringBuilder();
			for(int k=0; k<KeyField.Length; k++)
			{
				myKeys.Append(KeyField[k][0]);
				if(k!=KeyField.Length-1)	myKeys.Append(",");
			}
			string Keys = myKeys.ToString();

			#endregion

			System.Text.StringBuilder mySql = new System.Text.StringBuilder();

			if(this.PageVolume != 0)
			{
				#region 如果分页

				switch(this.DbConType)
				{
					case "SqlConnection":

						#region 对于SQL-Server的分页功能SQL语句的构建
						if(this.CurPageIndex == 1)
						{
							#region 如果是第一页

							mySql.Append("select top {end} ");
							mySql.Append(Field);
							mySql.Append(" from ");
							mySql.Append(TableName);
							if(Filter != null)
							{
								mySql.Append(" where ");
								mySql.Append(Filter);
							}
							if(Order != null)
							{
								mySql.Append(" order by ");
								mySql.Append(Order);
							}
							break;
							#endregion
						}
						else
						{
							#region 如果是后面的页
							mySql.Append("declare @mytable table(tempflagid int identity");
							foreach(string[] item in KeyField)
							{
								mySql.Append(", Sql_");
								mySql.Append(item[0]);
								mySql.Append(" ");
								mySql.Append(item[1]);
							}
							mySql.Append(");\n");
							mySql.Append("insert into @mytable (");
							for(int k=0; k<KeyField.Length; k++)
							{
								mySql.Append("Sql_");
								mySql.Append(KeyField[k][0]);
								if(k != KeyField.Length-1)	mySql.Append(",");
							}
							mySql.Append(") select top {end} ");
							for(int k=0; k<KeyField.Length; k++)
							{
								mySql.Append(KeyField[k][0]);
								if(k != KeyField.Length-1)	mySql.Append(",");
							}
							mySql.Append(" from ");
							mySql.Append(TableName);
							if(Filter != null)
							{
								mySql.Append(" where ");
								mySql.Append(Filter);
							}
							if(Order != null)
							{
								mySql.Append(" order by ");
								mySql.Append(Order);
							}
							mySql.Append(";\n");
							mySql.Append("select ");
							mySql.Append(Field);
							mySql.Append(" from ");
							mySql.Append(TableName);
							mySql.Append(", @mytable as mytemptable ");
							mySql.Append(" where tempflagid>{begin} ");
							for(int k=0; k<KeyField.Length; k++)
							{
								mySql.Append(" and ");
								mySql.Append(KeyField[k][0]);
								mySql.Append("=mytemptable.Sql_");
								mySql.Append(KeyField[k][0]);
							}
							if(Order != null)
							{
								mySql.Append(" order by ");
								mySql.Append(Order);
							}
							break;
							#endregion
						}
						#endregion

					case "OleDbConnection":  case "OdbcConnection":

						#region 对于OleDb数据源及ODBC分页功能SQL语句的构建

						if(this.CurPageIndex == 1)	//如果是第一页
						{
							#region 如果是第一页
							mySql.Append("select top ");
							mySql.Append(this.PageVolume);
							mySql.Append(" ");
							mySql.Append(Field);
							mySql.Append(" from ");
							mySql.Append(TableName);
							if(Filter != null)
							{
								mySql.Append(" where ");
								mySql.Append(Filter);
							}
							if(Order != null)
							{
								mySql.Append(" order by ");
								mySql.Append(Order);
								mySql.Append(",");
								mySql.Append(Keys);
							}
							#endregion
						}
						else
						{
							#region 如果是其它的页
							if(KeyField.Length == 1)
							{
								#region 如果只有一个主键

								string key = Keys;
								mySql.Append("select ");
								mySql.Append(Field);
								mySql.Append(" from ");
								mySql.Append(TableName);
								mySql.Append(" where ");
								mySql.Append(key);
								mySql.Append(" in ( select ");
								mySql.Append(key);
								mySql.Append(" from ( select top {end} ");
								mySql.Append(key);
								mySql.Append(" from ");
								mySql.Append(TableName);
								if(Filter != null)
								{
									mySql.Append(" where ");
									mySql.Append(Filter);
								}
								if(Order != null)
								{
									mySql.Append(" order by ");
									mySql.Append(Order);
									mySql.Append(",");
									mySql.Append(key);
								}
								mySql.Append(") as temptable where ");
								mySql.Append(key);
								mySql.Append(" not in ( select top {begin} ");
								mySql.Append(key);
								mySql.Append(" from ");
								mySql.Append(TableName);
								if(Filter != null)
								{
									mySql.Append(" where ");
									mySql.Append(Filter);
								}
								if(Order != null)
								{
									mySql.Append(" order by ");
									mySql.Append(Order);
									mySql.Append(",");
									mySql.Append(key);
								}
								mySql.Append("))");
								if(Order != null)
								{
									mySql.Append(" order by ");
									mySql.Append(Order);
									mySql.Append(",");
									mySql.Append(key);
								}
								#endregion
							}
							else
							{
								#region 如果有多个主键（应当避免，效率很低）
								mySql.Append("select ");
								mySql.Append(Field);
								mySql.Append(" from ");
								mySql.Append(TableName);
								mySql.Append(" as mytemptable0 where (select count(*) from (select top {end} ");
								mySql.Append(Keys);
								mySql.Append(" from ");
								mySql.Append(TableName);
								if(Filter != null)
								{
									mySql.Append(" where ");
									mySql.Append(Filter);
								}
								if(Order != null)
								{
									mySql.Append(" order by ");
									mySql.Append(Order);
									mySql.Append(",");
									mySql.Append(Keys);
								}
								mySql.Append(") as mytemptable1 where ");
								for(int k=0; k<KeyField.Length; k++)
								{
									mySql.Append(" mytemptable1.");
									mySql.Append(KeyField[k][0]);
									mySql.Append("=mytemptable0.");
									mySql.Append(KeyField[k][0]);
									if(k!=KeyField.Length-1)	mySql.Append(" and ");
								}
								mySql.Append(") <> 0 and (select count(*) from (select top {begin} ");
								mySql.Append(Keys);
								mySql.Append(" from ");
								mySql.Append(TableName);
								if(Filter != null)
								{
									mySql.Append(" where ");
									mySql.Append(Filter);
								}
								if(Order != null)
								{
									mySql.Append(" order by ");
									mySql.Append(Order);
									mySql.Append(",");
									mySql.Append(Keys);
								}
								mySql.Append(") as mytemptable2 where ");
								for(int k=0; k<KeyField.Length; k++)
								{
									mySql.Append(" mytemptable2.");
									mySql.Append(KeyField[k][0]);
									mySql.Append("=mytemptable0.");
									mySql.Append(KeyField[k][0]);
									if(k!=KeyField.Length-1)	mySql.Append(" and ");
								}
								mySql.Append(") = 0");

								if(Order != null)
								{
									mySql.Append(" order by ");
									mySql.Append(Order);
									mySql.Append(",");
									mySql.Append(Keys);
								}

								#region 代码的形式（已注释）
/*
@"SELECT *
FROM mytable AS TABLE0
WHERE (SELECT COUNT(*)
          FROM (SELECT top 30 id, username
                  FROM mytable where {filter} order by {order} ) AS table1
          WHERE table1.id = TABLE0.id AND table1.username = TABLE0.username) 
      <> 0 AND
          (SELECT COUNT(*)
         FROM (SELECT top 20 id, username
                 FROM mytable where {filter} order by {order} ) AS table2
         WHERE table2.id = table0.id AND table2.username = table0.username) = 0

order by {order}		
"*/
								#endregion

								#endregion
							}
							#endregion
						}
						break;
						#endregion

					case "OracleConnection":

						#region 对Oracle数据源的分页功能SQL语句的构建

						break;
						#endregion
				}
				
				#endregion
			}
			else
			{
				#region 如果不分页

				//select id, username, password from mytable where {filter} order by {order}
				mySql.Append("select ");
				mySql.Append(Field);
				mySql.Append(" from ");
				mySql.Append(TableName);
				if(Filter != null)
				{
					mySql.Append(" where ");
					mySql.Append(Filter);
				}
				if(Order != null)
				{
					mySql.Append(" order by ");
					mySql.Append(Order);
				}

				#endregion
			}
			return mySql.ToString();
		}
		#endregion

		#region 汇总屏的事件

		/// <summary>
		/// 在读取数据之前发生，返回非空，则不再启用控件内置的读取数据的功能
		/// </summary>
		public event OnReadData OnReadData = null;

		/// <summary>
		/// 从客户端传过来的事件
		/// </summary>
		public event OnCellCommand OnCellCommand = null;

		/// <summary>
		/// 在显示每条记录之前触发
		/// </summary>
		public event OnItemShow OnItemShow = null;

		#endregion

		#region 与页操作有关的属性

		int _CurPageIndex = -1;
		/// <summary>
		/// 当前页
		/// </summary>
		public int CurPageIndex
		{
			get
			{
				if(this._CurPageIndex == -1)
				{
					if(this.Filter.IsFilterChange)	this._CurPageIndex = 1;
					else
					{
						if(this.IsPostBack)		this._CurPageIndex = Convert.ToInt32(this.Request.Form[this.ClientID + "_CurPageIndex"]);
						else					this._CurPageIndex = 1;
						if(this.PageVolume==0)	this._CurPageIndex = 1;
						this.CurPageIndex = this._CurPageIndex;
					}
				}
				return this._CurPageIndex;
			}
			set
			{
				if(this.PageCount != 0)
				{
					this._CurPageIndex = value<=0?1:value;
				}
			}
		}

		bool _IsKeepState = false;
		/// <summary>
		/// 是否针对每个用户保持住当前页码
		/// </summary>
		public bool IsKeepState
		{
			get { return this._IsKeepState;  }
			set { this._IsKeepState = value; }
		}

		int _PageCount = 0;
		/// <summary>
		/// 总页数
		/// </summary>
		public int PageCount
		{
			get { return this._PageCount;  }
			set { this._PageCount = value; }
		}

		int _ItemCount = -1;
		/// <summary>
		/// 记录的条数
		/// </summary>
		public int ItemCount
		{
			get
			{
				if(this._ItemCount == -1)
				{
					string strCount = this.Request.Form[this.ClientID + "_ItemCount"];
					if(strCount == null)	this._ItemCount = 0;
					else					this._ItemCount = Convert.ToInt32(strCount);
				}
				return this._ItemCount;
			}
			set
			{
				this._ItemCount = value<0? 0:value;
				if(this.PageVolume > 0)
				{
					this.PageCount = value / this.PageVolume;
					if(value % this.PageVolume != 0)		this.PageCount ++;
				}
				if(this.PageCount==0)	this.PageCount = 1;
			}
		}

		int _PageVolume = 0;

		/// <summary>
		/// 每页显示的条数，设置为0表示不分页
		/// </summary>
		public int PageVolume
		{
			get { return this._PageVolume;  }
			set
			{
				this._PageVolume = value<0? 0:value;
				this.ItemCount = this.ItemCount;
			}
		}


		#endregion

		#region 表格各部分的样式

		private TableStyle _TableStyle = null;
		/// <summary>
		/// 表格框架的样式
		/// </summary>
		public TableStyle TableStyle
		{
			get
			{
				if(this._TableStyle == null)
				{
					this._TableStyle = new TableStyle();
				}
				return this._TableStyle;
			}
			set { this._TableStyle = value; }
		}

		private HeadStyle _HeadStyle;
		/// <summary>
		/// 页眉的样式
		/// </summary>
		public HeadStyle HeadStyle
		{
			get
			{
				if(this._HeadStyle == null)
				{
					this._HeadStyle = new HeadStyle();
					this._HeadStyle.IsShow = false;
				}
				return this._HeadStyle;
			}
			set { this._HeadStyle = value; }
		}

		private PartStyle _FootStyle;
		/// <summary>
		/// 页脚的样式
		/// </summary>
		public PartStyle FootStyle
		{
			get
			{
				if(this._FootStyle == null)
				{
					this._FootStyle = new PartStyle();
					this._FootStyle.IsShow = false;
				}
				return this._FootStyle;
			}
			set { this._FootStyle = value; }
		}

		private PartStyle _BodyStyle;
		/// <summary>
		/// 表体的样式
		/// </summary>
		public PartStyle BodyStyle
		{
			get
			{
				if(this._BodyStyle == null)
				{
					this._BodyStyle = new PartStyle();
					this._BodyStyle.IsShow = true;
				}
				return this._BodyStyle;
			}
			set { this._BodyStyle = value; }
		}

		#endregion

		#region Web 窗体设计器生成的代码
		/// <summary>
		/// 初始化
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

		#region 向客户端输出内容

		private string Content=null, DefaultFunctionScript=null;

		/// <summary>
		/// 在输出HTML之前，注释一些在客户端要用到的脚本
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender (e);

			if(this.Content == null)	this.Content = this.GetContent();
			this._IsExecute = true;
			if(this.ErrorMsg !=  null)	return;

			#region 注册一些隐藏控件，用于保持往返数据

			this.Page.RegisterHiddenField(this.ClientID + "_CurPageIndex",	this.CurPageIndex.ToString());
			this.Page.RegisterHiddenField(this.ClientID + "_PageVolume",	this.PageVolume.ToString());
			this.Page.RegisterHiddenField(this.ClientID + "_ItemCount",		this.ItemCount.ToString());
			this.Page.RegisterHiddenField(this.ClientID + "_PageCount",		this.PageCount.ToString());
			this.Page.RegisterHiddenField(this.ClientID + "_Argument",		"");	//客户端事件的参数
			this.Page.RegisterHiddenField(this.ClientID + "_Operate",		"");	//客户端事件的名字
			this.Page.RegisterHiddenField(this.ClientID + "_Filter",		this.Filter.GetFilterString());
			this.Page.RegisterHiddenField(this.ClientID + "_SelectedRows",	this.GetSelectedRows());
			if(this.ColStyle.IsEditable)	this.Page.RegisterHiddenField(this.ClientID + "_EditMessage", this.EditMessage);
			if(this.DefaultFunctionScript!=null)	this.Page.RegisterStartupScript(this.ClientID + "_DefaultFunctionScript", this.DefaultFunctionScript);

			#endregion

			#region 注册客户端脚本
			this.Page.RegisterClientScriptBlock("ReportCell_Menu",	  Script.ReportMenuOperateScript);
			this.Page.RegisterClientScriptBlock("ReportCell_Operate", Script.ReportCellOperateScript);
			#endregion

			#region 注册在客户端初始完毕后进行操作的一些脚本
			// 设置过滤条件
			System.Text.StringBuilder myStartupScript = new System.Text.StringBuilder();
			myStartupScript.Append("<script language=javascript>\n<!--\n");
			if(this.Filter.Count != 0)
			{
				System.Text.StringBuilder myScript = new System.Text.StringBuilder();
				//为保持过滤条件的值，将其值的脚本数组发送到客户端
				foreach(string Key in this.Filter.GetFilterValueKeys())
				{
					object Value = this.Filter.GetFilterValue(Key);
					myScript.Append("new Array('");
					myScript.Append(Key);
					myScript.Append("','");
					myScript.Append(Value==null?null:Value.ToString().Replace("'", @"\'"));
					myScript.Append("'),\n");
				}
				if(myScript.Length>0)	myScript[myScript.Length-2] = '\n';

				this.Page.RegisterArrayDeclaration(this.ClientID + "_FilterValue", myScript.ToString());
				myStartupScript.Append(string.Concat("_SetFilterValue('", this.ClientID, "');\n"));
			}

			// 反序列化编辑信息
			if(this.ColStyle.IsEditable) myStartupScript.Append(string.Concat("_UnSchemaEditData('", this.ClientID, "');\n"));
			// 设置已选中行的状态
			myStartupScript.Append(string.Concat("_SelectDefaultRows('", this.ClientID, "');\n"));
			// 设置间隔行的样式
			myStartupScript.Append(string.Concat("_ResetRowClassName('", this.ClientID, "');\n"));
			myStartupScript.Append("//-->\n</script>");
			this.Page.RegisterStartupScript(this.ClientID + "_ReportCellStartupScript", myStartupScript.ToString());
			#endregion			
		}

		#region 向客户端输出HTML代码

		/// <summary>
		/// 向客户端输出HTML代码
		/// </summary>
		/// <param name="writer"></param>
		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			if(this.Content==null)	this.Content = this.GetContent();
			this._IsExecute = true;

			//判断是否有错误信息
			if(this.ErrorMsg != null)
			{
				if(this.Request.UserHostAddress=="127.0.0.1" || this.Request.UserHostName==System.Net.Dns.Resolve(System.Net.Dns.GetHostName()).AddressList[0].ToString())
					writer.Write(this.CreateErrorButton(this.ErrorMsg));
				else	writer.Write(this.CreateErrorButton(""));
				return;
			}

			//写入样式表
			writer.Write(this.GetStyle());

			//写入内容
			writer.Write("<SPAN id=ReportCell_" + this.ClientID + ">");
			writer.Write(this.Content);
			writer.Write("</SPAN>");

			System.Text.StringBuilder myScript = new System.Text.StringBuilder();
			myScript.Append("<script language=javascript>try{var ");
			myScript.Append(this.ClientID);
			myScript.Append(" = new ReportCellOperate('");
			myScript.Append(this.ClientID);
			myScript.Append("');_AttachReportCellEvent('");
			myScript.Append(this.ClientID);
			myScript.Append("')}catch(e){}</script>");

			writer.Write(myScript.ToString());
		}

		#endregion

		#endregion

		#region 构建导航条

		/// <summary>
		/// 用于寻找表示变量的字符串
		/// </summary>
		protected Regex ReplaceContentRegex = new Regex(@"\{.*?\}", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
		Regex ReplaceKh = new Regex(@"\(.*?\)",  RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
		/// <summary>
		/// 替换一些参数
		/// </summary>
		/// <param name="myMatch"></param>
		/// <returns></returns>
		protected virtual string ReplaceParameter(Match myMatch)
		{
			switch(ReplaceKh.Replace(myMatch.Value.ToLower(), ""))
			{
				case "{curpageindex}":	if(!this.IsGetPageInfoFinish)	this.GetPageInfo(); return this.CurPageIndex.ToString();
				case "{pagevolume}":	if(!this.IsGetPageInfoFinish)	this.GetPageInfo(); return this.PageVolume.ToString();
				case "{pagecount}":		if(!this.IsGetPageInfoFinish)	this.GetPageInfo(); return this.PageCount.ToString();
				case "{itemcount}":		if(!this.IsGetPageInfoFinish)	this.GetPageInfo(); return this.ItemCount.ToString();
				case "{navigation}":	return this.GetNavigation(myMatch.Value);
				case "{pagehandle}":	return this.GetPageHandle(myMatch.Value);
				case "{pagemessage}":	return this.GetPageMessage(myMatch.Value);
				case "{begin}":	
					if(this.PageVolume == 0)	return "0";
					else						return (this.PageVolume * (this.CurPageIndex-1)).ToString();
				case "{end}":
					if(this.PageVolume == 0)	return this.ItemCount.ToString();
					else						return (this.PageVolume * this.CurPageIndex).ToString();
				case "{filter}":
					string filter = this.Filter.GetFilterString();
					if(filter == "")	return " 1=1 ";
					else				return filter;
				default:
					string Key = myMatch.Value.Trim('{', '}');
					if(this.Argument.IsContainsKey(Key))	return this.Argument[Key];
					break;
			}
			return myMatch.Value;
		}

		/// <summary>
		/// 构建导航条
		/// </summary>
		/// <param name="Argument"></param>
		/// <returns></returns>
		string GetNavigation(string Argument)
		{
			if(this.NavigationHashtable[Argument] != null)		return this.NavigationHashtable[Argument] as string;

			if(!this.IsGetPageInfoFinish)	this.GetPageInfo(); 

			string Argu = ReplaceKh.Match(Argument).Value.Trim('(', ')').Trim();
			int count = 0;
			if(Argu == "")	 { count = 10; Argu += "(10)"; }
			else
			{
				try		{ count = Convert.ToInt32(Argu); }
				catch	{ count = 10;					 }
			}
			int Count = count;
			System.Text.StringBuilder myNavi = new System.Text.StringBuilder();
			myNavi.Append("<span class=Navigation reportcell='");
			myNavi.Append(this.ClientID);
			myNavi.Append("' onclick='javascript:_OnNavigation()'>");
			if(this.PageCount > count)
			{
				myNavi.Append("<A href='javascript:void(0)' title='左移导航栏'>＜</A>");
			}
			else	Count = this.PageCount;
			int pagecount = this.PageCount;
			int curpageindex = this.CurPageIndex;
			int begin =  curpageindex - Count / 2;
			int end   = begin + Count - 1;
			if(begin < 1)		{ end += 1-begin; begin = 1; }
			if(end > pagecount)	{ begin -= end-pagecount; end = pagecount; }
			for(int index=begin; index<=end; index++)
			{
				myNavi.Append("<A href='javascript:void(0)' ");
				if(curpageindex == index)	myNavi.Append("class = CurPage");
				myNavi.Append(">&nbsp;");
				myNavi.Append(index);
				myNavi.Append("&nbsp;</A>");
			}
			if(this.PageCount > count)
			{
				myNavi.Append("<A href='javascript:void(0)' title='右移导航栏'>＞</A>");
			}
			myNavi.Append("</span>");
			string myNaviString = myNavi.ToString();
			this.NavigationHashtable.Add(Argument, myNaviString);
			return myNaviString;
		}

		//用于缓存导航栏
		private System.Collections.Hashtable NavigationHashtable = new System.Collections.Hashtable();

		/// <summary>
		/// 构建页导航按钮
		/// </summary>
		/// <returns></returns>
		string GetPageHandle(string Argument)
		{
			string Argu = ReplaceKh.Match(Argument).Value.Trim('(', ')').Trim();
			if(Argu=="")	{ Argu = "0"; Argument += "(0)"; }
			if(!this.NavigationHashtable.ContainsKey(Argument))
			{
				System.Text.StringBuilder myHandle = new System.Text.StringBuilder();

				switch(Argu)
				{
					case "0":
						myHandle.Append(string.Concat("<A class=PageHandle href='javascript:", this.ClientID, ".FirstPage()'>首页</A>&nbsp;"));
						myHandle.Append(string.Concat("<A class=PageHandle href='javascript:", this.ClientID, ".PreviousPage()'>前页</A>&nbsp;"));
						myHandle.Append(string.Concat("<A class=PageHandle href='javascript:", this.ClientID, ".NextPage()'>后页</A>&nbsp;"));
						myHandle.Append(string.Concat("<A class=PageHandle href='javascript:", this.ClientID, ".LastPage()'>末页</A>"));
						break;
					case "1":
						myHandle.Append(string.Concat("<A class=PageHandle title='首页' style='font-family:Webdings' href='javascript:", this.ClientID, ".FirstPage()'>9</A>&nbsp;"));
						myHandle.Append(string.Concat("<A class=PageHandle title='前页' style='font-family:Webdings' href='javascript:", this.ClientID, ".PreviousPage()'>7</A>&nbsp;"));
						myHandle.Append(string.Concat("<A class=PageHandle title='后页' style='font-family:Webdings' href='javascript:", this.ClientID, ".NextPage()'>8</A>&nbsp;"));
						myHandle.Append(string.Concat("<A class=PageHandle title='末页' style='font-family:Webdings' href='javascript:", this.ClientID, ".LastPage()'>:</A>"));
						break;
					case "2":
						myHandle.Append(string.Concat("<A class=PageHandle title='首页' style='font-family:Wingdings 3' href='javascript:", this.ClientID, ".FirstPage()'>)</A>&nbsp;"));
						myHandle.Append(string.Concat("<A class=PageHandle title='前页' style='font-family:Wingdings 3' href='javascript:", this.ClientID, ".PreviousPage()'>!</A>&nbsp;"));
						myHandle.Append(string.Concat("<A class=PageHandle title='后页' style='font-family:Wingdings 3' href='javascript:", this.ClientID, ".NextPage()'>&quot;</A>&nbsp;"));
						myHandle.Append(string.Concat("<A class=PageHandle title='末页' style='font-family:Wingdings 3' href='javascript:", this.ClientID, ".LastPage()'>*</A>"));
						break;
				}

				this.NavigationHashtable.Add(Argument, myHandle.ToString());
			}
			return this.NavigationHashtable[Argument] as string;
		}

		/// <summary>
		/// 构建页信息
		/// </summary>
		/// <returns></returns>
		string GetPageMessage(string Argument)
		{
			if(!this.IsGetPageInfoFinish)	this.GetPageInfo();

			string Argu = ReplaceKh.Match(Argument).Value.Trim('(', ')').Trim();
			if(Argu=="")	{ Argu = "0"; Argument += "(0)"; }

			if(!this.NavigationHashtable.ContainsKey(Argument))
			{
				System.Text.StringBuilder myHandle = new System.Text.StringBuilder();

				switch(Argu)
				{
					case "0":	// 1/10		10/100
						myHandle.Append(string.Concat("<span class=PageMessage>", this.CurPageIndex.ToString(), "</span>/<span class=PageMessage>", this.PageCount.ToString(), "</span>"));
						myHandle.Append("&nbsp;");
						myHandle.Append(string.Concat("<span class=PageMessage>", this.PageVolume.ToString(), "</span>/<span class=PageMessage>", this.ItemCount.ToString(), "</span>"));
						break;
					case "1":	// 1/10
						myHandle.Append(string.Concat("<span class=PageMessage>", this.CurPageIndex.ToString(), "</span>/<span class=PageMessage>", this.PageCount.ToString(), "</span>"));
						break;
					case "2":	// 10/100
						myHandle.Append(string.Concat("<span class=PageMessage>", this.PageVolume.ToString(), "</span>/<span class=PageMessage>", this.ItemCount.ToString(), "</span>"));
						break;
					case "3":	// 第1页共10页	每页10项共100项
						myHandle.Append(string.Concat("第<span class=PageMessage>", this.CurPageIndex.ToString(), "</span>页&nbsp;"));
						myHandle.Append(string.Concat("共<span class=PageMessage>", this.PageCount.ToString(), "</span>页&nbsp;"));
						myHandle.Append(string.Concat("每页<span class=PageMessage>", this.PageVolume.ToString(), "</span>项&nbsp;"));
						myHandle.Append(string.Concat("共<span class=PageMessage>", this.ItemCount.ToString(), "</span>项"));
						break;
					case "4":	// 第1页共10页
						myHandle.Append(string.Concat("第<span class=PageMessage>", this.CurPageIndex.ToString(), "</span>页&nbsp;"));
						myHandle.Append(string.Concat("共<span class=PageMessage>", this.PageCount.ToString(), "</span>页"));
						break;
					case "5":	// 每页10项共100项
						myHandle.Append(string.Concat("每页<span class=PageMessage>", this.PageVolume.ToString(), "</span>项&nbsp;"));
						myHandle.Append(string.Concat("共<span class=PageMessage>", this.ItemCount.ToString(), "</span>项"));
						break;
				}

				this.NavigationHashtable.Add(Argument, myHandle.ToString());
			}
			return this.NavigationHashtable[Argument] as string;
		}

		#endregion

		#region 获取页信息

		bool IsGetPageInfoFinish = false;
		/// <summary>
		/// 获取页信息
		/// </summary>
		private void GetPageInfo()
		{
			this.IsGetPageInfoFinish = true;
			if(this.ItemCountSql != null)
			{
				IDbCommand myIDbCommand = this.DbCommand;
				myIDbCommand.CommandText = ReplaceContentRegex.Replace(this.ItemCountSql, new MatchEvaluator(ReplaceParameter));
				this.ItemCount = (int)myIDbCommand.ExecuteScalar();
				myIDbCommand.CommandText = "";
			}
		}

		#endregion

		#region 获取内容
		/// <summary>
		/// 获取内容
		/// </summary>
		protected virtual string GetContent()
		{
			if(this.ErrorMsg != null)			return "";
			if(this.TableStyle.IsShow==false)	return "";

			//读取数据
			IDataReader myReader = null;

			IDbCommand myIDbCommand = this.DbCommand;

			string SelectSql = this.SelectSql;

			try
			{
				try
				{
					if(this.OnReadData != null)
					{
						foreach(OnReadData EventItem in this.OnReadData.GetInvocationList())
						{
							IDataReader Result = EventItem(this, myIDbCommand);
							if(Result != null)	myReader = Result;
						}
					}
				}
				catch(Exception e)
				{
					this.ErrorMsg = "触发事件 OnReadData ：" + e.Message;
					return "";
				}

				//首先读取分页信息
				if(!this.IsGetPageInfoFinish)	this.GetPageInfo();

				if(myReader == null)
				{
					string Sql = ReplaceContentRegex.Replace(SelectSql, new MatchEvaluator(ReplaceParameter));
					if(SelectSql.Length>0 && SelectSql[0]=='@')	//说明是存储过程
					{
						myIDbCommand.CommandText = Sql.Substring(1);
						myIDbCommand.CommandType = CommandType.StoredProcedure;
					}
					else
					{
						myIDbCommand.CommandText = Sql;
						myIDbCommand.CommandType = CommandType.Text;
					}
					try
					{
						myReader = myIDbCommand.ExecuteReader();
					}
					catch(Exception e)
					{
						this.ErrorMsg = "建立数据链接通道：" + e.Message;
						return "";
					}
				}

				this.myReader = myReader;
				string result = this.CreateHtml(myReader);
				this.myReader = null;
				return result;
			}
			catch(Exception e)
			{
				this.ErrorMsg = "创建HTML：" + e.Message;
				return "";
			}
			finally
			{
				if(myReader!=null)	myReader.Close();
				//提交事务
				if(myIDbCommand.Transaction != null)
				{
					try
					{
						myIDbCommand.Transaction.Commit();
						myIDbCommand.Transaction = null;
					}
					catch{}
				}
				if(!this.IsConnectionOpen)	this.DbConnection.Close();
			}
		}

		#region 构建客户端HTML代码

		/// <summary>
		/// 解释带参数的HTML代码
		/// </summary>
		/// <param name="Content"></param>
		/// <returns></returns>
		internal protected override string ExplainHtml(string Content)
		{
			if(Content==null)	return null;
			return this.ExplainArgument(Content).Replace("this.", this.ClientID + ".");
		}

		/// <summary>
		/// 解释带参数的字符串
		/// </summary>
		/// <param name="Content"></param>
		/// <returns></returns>
		internal protected override string ExplainArgument(string Content)
		{
			if(Content==null)	return null;
			return ReplaceContentRegex.Replace(Content, new MatchEvaluator(ReplaceParameter));
		}

		System.Data.IDataReader myReader = null;

		/// <summary>
		/// 在显示每条记录的时候，读取当前行要显示的记录
		/// </summary>
		public object this[string FieldName]
		{
			get
			{
				if(myReader == null)	return null;
				try
				{
					FieldName = FieldName.ToLower();
					if(MyValues.ContainsKey(FieldName))	return this.MyValues[FieldName];
					else								return this.myReader[FieldName];
				}
				catch
				{
					return null;
				}
			}
			set
			{
				FieldName = FieldName.ToLower();
				if(this.MyValues.ContainsKey(FieldName)) this.MyValues.Add(FieldName, value);
				else									 this.MyValues[FieldName] = value;
			}
		}

		/// <summary>
		/// 记录新值
		/// </summary>
		private System.Collections.Hashtable MyValues = new System.Collections.Hashtable();

		/// <summary>
		/// 构建HTML代码
		/// </summary>
		/// <param name="myReader"></param>
		/// <returns></returns>
		protected virtual string CreateHtml(IDataReader myReader)
		{
			System.Text.StringBuilder myContent = new System.Text.StringBuilder();
			int ColCounter = 0;		//记录应该显示的列的总数，除去隐藏列
			int length = 0;			//记录所有列的总数

			string TRStyle = null, TDStyle = null, THStyle = null;
			if(this.UserDefineStyle["#tr#"]!=null)	TRStyle = this.UserDefineStyle["#tr#"].Pattern;
			if(this.UserDefineStyle["#td#"]!=null)	TDStyle = this.UserDefineStyle["#td#"].Pattern;
			if(this.UserDefineStyle["#th#"]!=null)	THStyle = this.UserDefineStyle["#th#"].Pattern;

			#region 构建列的显示形式

			string ID = "ReportCell_" + this.ClientID;
			ColStyle [] myColStyle = null;
			bool IsGroupTitle = false;

			//若是自动显示
			if(this.ColStyle.IsAutoShow)
			{
				length = myReader.FieldCount;
				myColStyle = new ColStyle[length];

				for(int index=0; index<length; index++)
				{
					string name = myReader.GetName(index);
					ColStyle myStyle = this.ColStyle[name.ToLower()];
					if(myStyle == null)			myStyle = new ColStyle();
					if(myStyle.Name == null)	myStyle.Name  = name;
					if(myStyle.Title == null)	myStyle.Title = name;
					myStyle.FormatString = this.ExplainHtml(myStyle.FormatString);
					myStyle.colindex = index;
					myColStyle[index] = myStyle;
					if(myStyle.IsShow)				ColCounter ++;
					if(myStyle.GroupTitle!=null)	IsGroupTitle = true;
				}
			}
			else	//若是自定义字段显示
			{
				length = this.ColStyle.Count;
				myColStyle = new ColStyle[length];
				int index = 0;
				foreach(string Key in ColStyle.GetKeys())
				{
					ColStyle myStyle = this.ColStyle[Key.ToLower()];
					try		{	myStyle.colindex = myReader.GetOrdinal(Key);	}
					catch	{	myStyle.colindex = -1;							}
					myStyle.FormatString = this.ExplainHtml(myStyle.FormatString);
					myColStyle[index] = myStyle;
					index ++;
					if(myStyle.IsShow)	ColCounter ++;
					if(myStyle.GroupTitle!=null)	IsGroupTitle = true;
				}
			}

			if(this.RowStyle.IsShowCheckBox)	ColCounter ++;

			#endregion

			#region 在可编辑状态下，注册每列的编辑控件的信息
			if(this.ColStyle.IsEditable)
			{
				System.Text.StringBuilder myScript = new System.Text.StringBuilder();
				myScript.Append("<script language=javascript>var " + this.ClientID + "_ColStyle = new Array(\n ");
				foreach(ColStyle myStyle in myColStyle)
				{
					myScript.Append("new Array('");
					myScript.Append(myStyle.EditControl);
					myScript.Append("','");
					myStyle.EditControlType = ExplainHtml(myStyle.EditControlType);
					myScript.Append(myStyle.EditControlType==null?null:myStyle.EditControlType.Replace("'", @"\'"));
					myScript.Append("','");
					myStyle.EditControlData = ExplainHtml(myStyle.EditControlData);
					myScript.Append(myStyle.EditControlData==null?null:myStyle.EditControlData.Replace("'", @"\'"));
					myScript.Append("','");
					myScript.Append(myStyle.Name.Replace("'", @"\'"));
					myScript.Append("','");
					myScript.Append(myStyle.ClassName==null?null:myStyle.ClassName.Replace("'", @"\'"));
					myScript.Append("','");
					myScript.Append(myStyle.Width);
					myScript.Append("'),\n");
				}
				myScript[myScript.Length-2] = '\n';
				myScript.Append(");\n var " + this.ClientID + "_RowStyle = new Array(\n ");
				myScript.Append(this.RowStyle.DifferentRow + ",\n");
				foreach(RowStyle myStyle in this.RowStyle)
				{
					myScript.Append("new Array(");
					myScript.Append(myStyle.Index);
					myScript.Append(",'");
					myScript.Append(myStyle.ClassName==null?null:myStyle.ClassName.Replace("'", @"\'"));
					myScript.Append("'),\n");
				}
				myScript[myScript.Length-2] = '\n';
				myScript.Append(");\n var " + this.ClientID + "_EditMessage=new Array();</script>");
				this.Page.RegisterClientScriptBlock(this.ClientID + "_EditControlMessage", myScript.ToString());
			}
			#endregion

			// TABLE 外框
			myContent.Append("<TABLE");

			#region 增加自定义事件

			if(this.Event.Count>0)
			{
				System.Text.StringBuilder DefaultFunctionScript = new System.Text.StringBuilder();
				DefaultFunctionScript.Append("<script language=javascript>\n<!--\n");
				foreach(Event myEvent in this.Event)		//增加事件
				{
					myContent.Append(myEvent.Name.ToLower());
					myContent.Append("='");
					if(myEvent.Function!=null)
					{
						myContent.Append(myEvent.Function);
						myContent.Append("()");
					}
					else
					{
						myContent.Append("SkyeverReportCellDefaultFunction_");
						myContent.Append(myEvent.Name);
						myContent.Append("()");

						//注册事件函数
						DefaultFunctionScript.Append("var ");
						DefaultFunctionScript.Append("SkyeverReportCellDefaultFunction_");
						DefaultFunctionScript.Append(myEvent.Name);
						DefaultFunctionScript.Append(" = new Function('");
						DefaultFunctionScript.Append(myEvent.Language);
						DefaultFunctionScript.Append(":");
						DefaultFunctionScript.Append(myEvent.FunctionBody.Replace("'", @"\'").Replace("\r\n", @"\n"));
						DefaultFunctionScript.Append(";');\n");
					}
					myContent.Append("' ");
				}
				DefaultFunctionScript.Append("\n//-->\n</script>");
				this.DefaultFunctionScript = DefaultFunctionScript.ToString();
			}
			#endregion

			#region 增加TABLE元素的其它一些属性

			if(this.UserDefineStyle["#table#"] != null)	myContent.Append(this.UserDefineStyle["#table#"].Pattern);
			myContent.Append(this.TableStyle.IsCollapse?" style='border-collapse:collapse'" : " style='border-collapse:separate'");
			myContent.Append(this.TableStyle.IsAutoLayout?" style='table-layout:auto'" : " style='table-layout:fixed'");
			if(this.TableStyle.Align!=null)	myContent.Append(" align=" + this.TableStyle.Align);
			if(this.TableStyle.Width!=null)	myContent.Append(" width=" + this.TableStyle.Width);
			if(this.RowStyle.Height!=null)	myContent.Append(" rowWidth=" + this.RowStyle.Height);
			myContent.Append(" border=" + this.TableStyle.Border);
			myContent.Append(" cellpadding=" + this.TableStyle.CellPadding);
			myContent.Append(" cellspacing=" + this.TableStyle.CellSpacing);
			if(this.TableStyle.ClassName!=null)	myContent.Append(" class=" + this.TableStyle.ClassName);
			myContent.Append(" id=");
			myContent.Append(ID);
			myContent.Append("_Table isMulCheckable="	+ this.RowStyle.IsMulCheckable);
			myContent.Append(" isShowCheckBox="			+ this.RowStyle.IsShowCheckBox);
			myContent.Append(" isSelectable="			+ this.TableStyle.IsSelectable);
			myContent.Append(" isEditable="				+ this.ColStyle.IsEditable);
			myContent.Append(" operate=");
			myContent.Append(this.ClientID);
			myContent.Append(">");

			#endregion

			#region 写入列描述

			myContent.Append("<COLGROUP>");

			if(this.RowStyle.IsShowCheckBox)
			{
				myContent.Append("<COL class=CHECKEDFLAG>");
			}

			foreach(ColStyle item in myColStyle)
			{
				myContent.Append("<COL class=");
				myContent.Append(item.ClassName);
				if(item.EditControl=="textarea" || item.EditControl=="listbox")	myContent.Append(" valign=top");
				myContent.Append(">");
			}

			myContent.Append("</COLGROUP>");

			#endregion

			#region 表格的标题

			string Caption = this.HeadStyle.Caption;
			if(Caption != null)
			{
				myContent.Append("<CAPTION ");
				if(this.UserDefineStyle["#caption#"] != null)	myContent.Append(this.UserDefineStyle["#caption#"].Pattern);
				myContent.Append(">");
				myContent.Append(ExplainHtml(Caption));
				myContent.Append("</CAPTION>");
			}

			#endregion

			#region 写入标头内容

			if(this.HeadStyle.IsShow || this.ColStyle.IsAutoShow)
			{
				// THEAD 标题
				myContent.Append("<THEAD valign=middle ");
				if(this.UserDefineStyle["#thead#"] != null)	myContent.Append(this.UserDefineStyle["#thead#"].Pattern);
				myContent.Append(">");

				// 表头的内容
				string HeadText = this.HeadStyle.HeadText;
				if(HeadText != null)
				{
					myContent.Append("<TR class=HEAD ");
					myContent.Append(TRStyle);
					myContent.Append("><TH ");
					myContent.Append(TDStyle);
					myContent.Append(" colspan=");
					myContent.Append(ColCounter);
					myContent.Append(">");
					myContent.Append(ExplainHtml(HeadText));
					myContent.Append("</TH></TR>");
				}

				// 显示标题
				if(this.HeadStyle.IsShowTitle)
				{
					string GroupTitle = null;
					int position = 0, colspan = 1;
					myContent.Append("<TR");	myContent.Append(TRStyle);	myContent.Append(" class=TITLE>");

					// 显示标头的复选框
					if(this.RowStyle.IsShowCheckBox)
					{
						myContent.Append("<TH "); myContent.Append(THStyle);
						myContent.Append(" align=center valign=middle class=CHECKEDFLAG isCheckBox=true");
						if(IsGroupTitle)	myContent.Append(" rowspan=2");
						myContent.Append(">");
						if(this.RowStyle.IsMulCheckable)
						{
							myContent.Append("<INPUT type=checkbox onclick='this.checked?");
							myContent.Append(string.Concat(this.ClientID, ".SelectAllRows():", this.ClientID, ".UnSelectAllRows()'"));
							myContent.Append(string.Concat(" id=", this.ClientID, "_SelectedAllFlag"));
							myContent.Append(" title=全部选中/全部取消 style='width:16px;height:16px'>");
						}
						else
						{
							myContent.Append(string.Concat("<INPUT type=radio name=", this.ClientID, "_CHECKEDFLAG onclick='"));
							myContent.Append(string.Concat(this.ClientID, ".UnSelectAllRows()'"));
							myContent.Append(string.Concat(" id=", this.ClientID, "_SelectedAllFlag"));
							myContent.Append(" title=选中/取消 style='width:16px;height:16px'>");
						}
						myContent.Append("</TH>");
					}

					// 显示每列的标头
					for(int index=0; index<length; index++)
					{
						ColStyle myStyle = myColStyle[index];
						if(myStyle.IsShow == true)
						{
							if(IsGroupTitle)	// 如果是组合标题
							{
								string Title = null;
								if(myStyle.GroupTitle!=null && GroupTitle!=null && (GroupTitle==myStyle.GroupTitle || myStyle.GroupTitle==""))
								{	// 组合上一个标题
									int position2 = position;
									foreach(char ch in (++colspan).ToString())
									{
										myContent[position2++] = ch;
									}
									continue;
								}

								myContent.Append("<TH ");
								myContent.Append(THStyle);
								if(myStyle.GroupTitle==null)
								{
									myContent.Append(" class=" + myStyle.ClassName);
									myContent.Append(" rowspan=2");
									GroupTitle = null;
									Title = myStyle.Title;
								}
								else
								{
									myContent.Append(" colspan=");
									position = myContent.Length;
									myContent.Append("1   ");
									if(myStyle.GroupTitle!="" || GroupTitle==null)	GroupTitle = myStyle.GroupTitle;
									Title = myStyle.GroupTitle;
									colspan = 1;
								}
								myContent.Append(">");
								myContent.Append(Title);
								myContent.Append("</TH>");
							}
							else
							{
								myContent.Append("<TH ");
								myContent.Append(THStyle);
								myContent.Append(" class=" + myStyle.ClassName);
								myContent.Append(">");
								myContent.Append(myStyle.Title);
								myContent.Append("</TH>");
							}
						}
					}
					myContent.Append("</TR>");

					if(IsGroupTitle)
					{
						myContent.Append("<TR ");	myContent.Append(TRStyle);	myContent.Append(" class=TITLE>");
						for(int index=0; index<length; index++)
						{
							ColStyle myStyle = myColStyle[index];
							if(myStyle.IsShow==true && myStyle.GroupTitle!=null)
							{
								myContent.Append("<TH ");
								myContent.Append(THStyle);
								myContent.Append(" class=" + myStyle.ClassName);
								myContent.Append(">");
								myContent.Append(myStyle.Title);
								myContent.Append("</TH>");
							}
						}
						myContent.Append("</TR>");
					}
				}
				myContent.Append("</HEAD>");
			}

			#endregion

			#region 写入表体内容

			if(this.BodyStyle==null || this.BodyStyle.IsShow)
			{
				int DifferentRow = this.RowStyle.DifferentRow;
				//写入内容
				myContent.Append("<TBODY ");
				if(this.UserDefineStyle["#tbody#"] != null)		myContent.Append(this.UserDefineStyle["#tbody#"].Pattern);
				myContent.Append(">");

				int rowindex = 0;
				while(myReader.Read())
				{
					this.MyValues.Clear();

					//激发事件
					this.RowStyle.IsShow = true;
					this.FireOnItemShowEvent();

					//若已关闭该行的显示状态，则继续下一行的显示
					if(!this.RowStyle.IsShow)	continue;

					//计算间隔行的样式
					RowStyle myRowStyle = this.RowStyle[rowindex % DifferentRow + 1];
					myContent.Append("<TR");	myContent.Append(TRStyle);	myContent.Append(" class=");
					if(myRowStyle == null)		myContent.Append("TR" + (rowindex % DifferentRow + 1));
					else						myContent.Append(myRowStyle.ClassName);

					//加入行标识
					myContent.Append(" flag=\"");
					string RowFlag = null;
					if(this.RowStyle.Flag != null)
					{
						try
						{
							if(this.RowStyle.Value != null)
							{
								object[] Params = new object[this.RowStyle.Value.Length];
								for(int k=0; k<Params.Length; k++)
								{
									Params[k] = this[this.RowStyle.Value[k]];
								}
								RowFlag = string.Format(this.RowStyle.Flag, Params);
							}
							else	RowFlag = this.RowStyle.Flag;
						}
						catch {}
					}
					else	RowFlag = (rowindex+1).ToString();
					myContent.Append(RowFlag.Replace("\"", "&quot;"));
					myContent.Append("\">");

					//写入每行标头复选框
					if(this.RowStyle.IsShowCheckBox)
					{
						myContent.Append("<TD align=center valign=middle class=CHECKEDFLAG isCheckBox=true><INPUT ");
						myContent.Append(this.RowStyle.IsMulCheckable?"type=checkbox":("type=radio name=" + this.ClientID + "_CHECKEDFLAG"));
						myContent.Append(string.Concat(" onclick='", this.ClientID, ".SelectRow(this, this.checked, false)'"));
						myContent.Append(" style='width:16px;height:16px'></TD> ");
					}

					//写入具体内容
					for(int itemindex=0; itemindex<length; itemindex++)
					{
						ColStyle CurColStyle = myColStyle[itemindex];
						if(CurColStyle.IsShow == false)	continue;
						int colindex = CurColStyle.colindex;
						bool isEditState = this.ColStyle.IsEditable && CurColStyle.EditControl!=null && CurColStyle.colindex!=-1;

						string ContentHtml = "";

						//不带格式化的内容
						if(CurColStyle.FormatString==null || isEditState)
						{
							object Content = this[CurColStyle.Name];
							string[] ArrStriking = myColStyle[itemindex].ArrStriking;
							if(Content!=null && !(Content is System.DBNull) && !Content.Equals(string.Empty))
							{
								if(Content is string && !isEditState)
								{
									ContentHtml = Content as string;
									//替换醒目文字
									if(ArrStriking != null)
									{
										foreach(string Striking in ArrStriking)
											ContentHtml = ContentHtml.Replace(Striking, "<font class=Striking>" + Striking + "</font>");
									}

									//替换UBB
									if(CurColStyle.IsSupportUBB)
									{
										ContentHtml = this.UBB.GetHtmlCode(ContentHtml).Replace("\r\n", "<br>");
									}
								}
								else ContentHtml = Content.ToString();
							}
						}
						else	//带格式化的内容
						{
							string [] ColNames = CurColStyle.Value;
							if(ColNames != null)
							{
								//用于显示醒目文字的内容
								string[] ArrStriking = CurColStyle.ArrStriking;

								//构建参数数组
								object [] Params = new object[ColNames.Length];
								for(int k=0; k<Params.Length; k++)
								{
									object ItemValue = null;
									try	
									{
										switch(ColNames[k].ToLower())
										{
											case "rowindex":  ItemValue = rowindex  + 1;			break;
											case "colindex":  ItemValue = itemindex + 1;			break;
											case "itemindex": ItemValue = this.PageVolume * (this.CurPageIndex-1) + rowindex + 1; break;
											default:
												ItemValue = this.Argument[ColNames[k]];
												if(ItemValue == null)	ItemValue = this[ColNames[k]];
												break;
										}
										if(ItemValue!=null && ItemValue is string)
										{
											//构建显示醒目的字符串
											if(ArrStriking != null)
											{
												foreach(string Striking in ArrStriking)
													ItemValue = (ItemValue as string).Replace(Striking, "<font class=Striking>" + Striking + "</font>");
											}
										}
									}
									catch	{  ItemValue = "Error!"; }

									if(ItemValue != null)	Params[k] = ItemValue.ToString();
								}
								//执行字符串的格式化
								ContentHtml = string.Format(CurColStyle.FormatString, Params);
								if(CurColStyle.IsSupportUBB)	ContentHtml = this.UBB.GetHtmlCode(ContentHtml).Replace("\r\n", "<br>");
							}
							else	ContentHtml = CurColStyle.FormatString;
						}

						//填充TD元素
						if(CurColStyle.IsMergeSameRow && (CurColStyle.IsMergeNullRow || ContentHtml!="") )
						{
							Report.ColStyle.MergeSameRowMessage MergeSameRowMsg = CurColStyle.MergeSameRowMsg;
							// 若是同值
							if(MergeSameRowMsg.SameValue==ContentHtml &&
								(!CurColStyle.IsMergeFollowPrevious || itemindex==0 || myColStyle[itemindex-1].MergeSameRowMsg.RowSpan>1) )
							{
								int index = MergeSameRowMsg.Postion;
								foreach(char ch in (++MergeSameRowMsg.RowSpan).ToString())
								{
									myContent[index++] = ch;
								}
							}
							else	// 发现不同值行
							{
								myContent.Append("<TD rowspan=");
								MergeSameRowMsg.Postion = myContent.Length;
								myContent.Append("1    ");
								myContent.Append(TDStyle);
								myContent.Append(" class=");	myContent.Append(CurColStyle.ClassName);
								if(CurColStyle.Width!=null)		myContent.Append("width=" + CurColStyle.Width);
								myContent.Append(">");
								this.WriteCellContent(myContent, ContentHtml, CurColStyle, RowFlag);
								myContent.Append("</TD>");
								MergeSameRowMsg.SameValue = ContentHtml;
								MergeSameRowMsg.RowSpan = 1;
							}
						}
						else
						{
							myContent.Append("<TD ");		myContent.Append(TDStyle);
							myContent.Append(" class=");	myContent.Append(CurColStyle.ClassName);
							myContent.Append(">");
							this.WriteCellContent(myContent, ContentHtml, CurColStyle, RowFlag);
							myContent.Append("</TD>");
						}
					}
					myContent.Append("</TR>");
					rowindex ++;
				}

				myContent.Append("</TBODY>");
			}

			#endregion

			#region 写入页脚内容

			//写入页脚

			if(this.FootStyle.IsShow)
			{
				myContent.Append("<TFOOT ");
				if(this.UserDefineStyle["#tfoot#"] != null)		myContent.Append(this.UserDefineStyle["#tfoot#"].Pattern);
				myContent.Append("><TR ");	myContent.Append(TRStyle);	myContent.Append(">");
				myContent.Append("<TD ");	myContent.Append(TDStyle);	myContent.Append(" colspan=");
				myContent.Append(ColCounter);
				myContent.Append(">");

				//在此写入页脚的内容
				myContent.Append(this.ExplainHtml(this.FootStyle.InnerHtml.Trim()));

				myContent.Append("</TR></TFOOT>");
			}

			#endregion

			myContent.Append("</TABLE>\n");

			return myContent.ToString();
		}

		#endregion

		/// <summary>
		/// 写入客户端 TD 值
		/// </summary>
		/// <param name="myContent"></param>
		/// <param name="ContentHtml"></param>
		/// <param name="CurColStyle"></param>
		/// <param name="RowFlag"></param>
		private void WriteCellContent(System.Text.StringBuilder myContent, string ContentHtml, ColStyle CurColStyle, string RowFlag)
		{
			// 如果当前允许编辑
			if(this.ColStyle.IsEditable && CurColStyle.EditControl!=null && CurColStyle.colindex!=-1)
			{
				myContent.Append("<script language=javascript>_W19L('");
				myContent.Append(this.ClientID);											// ClientID
				myContent.Append("','");
				myContent.Append(ContentHtml==null?null:ContentHtml.Replace("'", @"\'"));	// 值
				myContent.Append("', ");
				myContent.Append(CurColStyle.colindex);										// 列索引
				myContent.Append(" ,'");
				myContent.Append(RowFlag==null?null:RowFlag.Replace("'", @"\'"));			// 行标识
				myContent.Append("')</script>");
			}
			else myContent.Append(ContentHtml==""?"&nbsp;":ContentHtml);
		}

		/// <summary>
		/// 激发OnItemShow事件
		/// </summary>
		protected void FireOnItemShowEvent()
		{
			if(this.OnItemShow != null)		this.OnItemShow(this);
		}

		#endregion

		#region 获取样式表
		/// <summary>
		/// 获取样式表
		/// </summary>
		/// <returns></returns>
		private string GetStyle()
		{
			string ID  = "#ReportCell_" + this.ClientID + " ";
			string ID2 = ID + ".";
			System.Text.StringBuilder myStyle = new System.Text.StringBuilder();
			myStyle.Append("\n<style type=text/css id=");
			myStyle.Append(ID);
			myStyle.Append(">\n<!--\n");

			//各列的样式
			foreach(ColStyle Style in this.ColStyle)
			{
				if(Style.Pattern!=null)
				{
					myStyle.Append(ID);
					myStyle.Append(" TBODY .");
					myStyle.Append(Style.Name);
					myStyle.Append("{");
					myStyle.Append(Style.Pattern);
					myStyle.Append("}\n");
				}

				//列的宽度
				if(Style.Width!=null)
				{
					myStyle.Append(ID2);
					myStyle.Append(Style.Name);
					myStyle.Append("{");
					myStyle.Append("width:");
					myStyle.Append(Style.Width);
					myStyle.Append("}\n");
				}
			}

			//列的样式
			myStyle.Append(ID);
			myStyle.Append(" TBODY TD {");
			myStyle.Append(this.ColStyle.Pattern);
			myStyle.Append("}\n");

			//列元素的样式
			foreach(string Key in this.ColStyle.GetStyleKeys())
			{
				UserDefineStyle Style = this.ColStyle.GetItemStyle(Key);
				myStyle.Append(ID);
				myStyle.Append(" TBODY ");
				if(Style.Name[0]!='.')	myStyle.Append(".");
				myStyle.Append(Regex.Replace(Style.Name, @",\s*\.?", string.Concat(",", ID, " TBODY ."), RegexOptions.Multiline));
				myStyle.Append("{");
				myStyle.Append(Style.Pattern);
				myStyle.Append("}\n");
			}

			//间隔行的样式
			foreach(RowStyle Style in this.RowStyle)
			{
				myStyle.Append(ID2);
				myStyle.Append(Style.ClassName);
				myStyle.Append("{");
				myStyle.Append(Style.Pattern);
				myStyle.Append("}\n");
			}

			//行的样式
			if(this.RowStyle.Pattern!=null)
			{
				myStyle.Append(ID);
				myStyle.Append(" TBODY TR {");
				myStyle.Append(this.RowStyle.Pattern);
				myStyle.Append("}\n");
			}

			//表格的样式
			if(this.TableStyle.Pattern!=null)
			{
				myStyle.Append(ID.TrimEnd());
				myStyle.Append("_Table{");
				myStyle.Append(this.TableStyle.Pattern);
				myStyle.Append("}\n");
			}

			//表头的样式
			if(this.HeadStyle.Pattern!=null)
			{
				myStyle.Append(ID);
				myStyle.Append("THEAD");
				myStyle.Append("{");
				myStyle.Append(this.HeadStyle.Pattern);
				myStyle.Append("}\n");
			}

			//表体的样式
			if(this.BodyStyle.Pattern!=null)
			{
				myStyle.Append(ID);
				myStyle.Append("TBODY");
				myStyle.Append("{");
				myStyle.Append(this.BodyStyle.Pattern);
				myStyle.Append("}\n");
			}

			//页脚的样式
			if(this.FootStyle.Pattern!=null)
			{
				myStyle.Append(ID);
				myStyle.Append("TFOOT");
				myStyle.Append("{");
				myStyle.Append(this.FootStyle.Pattern);
				myStyle.Append("}\n");
			}

			//编辑状态下，编辑控件的默认样式
			if(this.ColStyle.IsEditable)
			{
				myStyle.Append(ID);
				myStyle.Append("TBODY INPUT{border-width:0px;background-color:transparent;}\n");
				myStyle.Append(ID);
				myStyle.Append("TBODY TEXTAREA{border-width:0px;background-color:transparent;overflow:auto;}\n");
			}

			//用户自定义样式
			foreach(UserDefineStyle Style in this.UserDefineStyle)
			{
				string myKey = Style.Name.Trim();
				if(myKey[0]=='#' && myKey[myKey.Length-1]=='#')	continue;
				myStyle.Append(ID);
				myStyle.Append(Style.Name.Replace(",", ","+ID).Replace("list:", "tbody .list"));
				myStyle.Append("{");
				myStyle.Append(Style.Pattern);
				myStyle.Append("}\n");
			}

			//表格的边框颜色
			if(this.TableStyle.BorderColor!=null)
			{
				myStyle.Append(ID);
				myStyle.Append("TABLE,");
				myStyle.Append(ID);
				myStyle.Append("TD{ border-color:");
				myStyle.Append(this.TableStyle.BorderColor);
				myStyle.Append("}\n");
			}

			//行的高度
			if(this.RowStyle.Height != null)
			{
				myStyle.Append(ID);
				myStyle.Append("TBODY TR{");
				myStyle.Append("height:");
				myStyle.Append(this.RowStyle.Height);
				myStyle.Append("}\n");
			}

			//行复选框
			if(!this.TableStyle.IsAutoLayout && this.RowStyle.IsShowCheckBox)
			{
				myStyle.Append(ID);
				myStyle.Append(".CHECKEDFLAG{width:18px;padding:0px;}\n");
			}

			//滚动条的默认样式
			myStyle.Append(ID);
			myStyle.Append("{SCROLLBAR-FACE-COLOR:#fffeef;SCROLLBAR-HIGHLIGHT-COLOR:#c1c1bb;SCROLLBAR-SHADOW-COLOR:#c1c1bb;");
			myStyle.Append("SCROLLBAR-3DLIGHT-COLOR:#ebebe4;SCROLLBAR-ARROW-COLOR:#cacab7;SCROLLBAR-TRACK-COLOR:#f4f4f0;SCROLLBAR-DARKSHADOW-COLOR:#ebebe4;}\n");

			myStyle.Append("//-->\n</style>\n");
			return myStyle.ToString();
		}
		#endregion

		#region 保持住页面的状态

		/// <summary>
		/// ReportCell：保持住页面的状态
		/// </summary>
		class KeepPageState
		{
			public int CurPageIndex = 0;
			public FilterGroup Filter = null;
			public bool IsEmpty = true;
		}

		private KeepPageState PageState
		{
			get
			{
				if(!this.IsKeepState)	return null;
				string Key = this.Page.ClientID + this.ClientID + "state";
				if(this.Session[Key] == null)		this.Session[Key] = new KeepPageState();
				return this.Session[Key] as KeepPageState;
			}
		}

		/// <summary>
		/// 在卸载页面之前保存住当前的状态
		/// </summary>
		/// <param name="e"></param>
		protected override void OnUnload(EventArgs e)
		{
			base.OnUnload (e);
			if(this.IsKeepState)
			{
				KeepPageState myState = this.PageState;
				myState.Filter = this.Filter;
				myState.CurPageIndex = this.CurPageIndex;
				myState.IsEmpty = false;
			}
		}

		/// <summary>
		/// 在刚加载时，恢复其状态
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);
			KeepPageState myState = this.PageState;
			if(this.IsKeepState && !this.IsPostBack && !myState.IsEmpty)
			{
				this.Filter = myState.Filter;
				this._CurPageIndex = myState.CurPageIndex;
			}
			else
			{
				this.RegisterColsFilter();
			}
		}

		/// <summary>
		/// 将字段上的过滤条件添加到组
		/// </summary>
		internal void RegisterColsFilter()
		{
			if(this.IsUpdateFilter==true)	return;

			foreach(string Key in this.ColStyle.GetKeys())
			{
				ColStyle myColStyle = this.ColStyle[Key];
				if(myColStyle.Filter != null)
				{
					this.Filter.Append(new FilterItem(myColStyle.Name, myColStyle.Filter));
				}
			}
			this.IsUpdateFilter = true;
		}

		bool IsUpdateFilter = false;

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
						this._UBB = new UBB(0);
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
		public override void  Dispose()
		{
			base.Dispose();
			if(this.Adapter.DbConnection != null)
			{
				if(!this.IsConnectionOpen)	this.Adapter.DbConnection.Close();
			}
		}

		#endregion

		#region  其它功能

		static System.Collections.Specialized.StringCollection Variables = new System.Collections.Specialized.StringCollection();

		/// <summary>
		/// 检查一个参数是否为内置的变量
		/// </summary>
		/// <param name="Name"></param>
		/// <returns></returns>
		static public bool IsVariable(string Name)
		{
			if(Variables.Count==0)	Variables.AddRange(new string[] {"begin", "end", "navigation", "filter", "curpageindex", 
										"pagecount", "itemcount", "pagevolume", "pagemessage", "pagehandle" /*, "rowindex", "colindex", "itemindex" */ });
			if(Name == null)	return false;
			return Variables.Contains(Name.ToLower().Split('(')[0].Trim());
		}

		#endregion
	}
}
