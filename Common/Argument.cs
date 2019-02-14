using System;

namespace Skyever.Report
{
	#region 记录变量的每一项

	/// <summary>
	/// 记录变量的每一项，变量是用一对大括号括起来的将来被实际值替换的值
	/// </summary>
	public class Argument : System.Web.UI.HtmlControls.HtmlContainerControl
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public Argument(){}

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="Name"></param>
		/// <param name="Value"></param>
		public Argument(string Name, string Value)
		{
			this.Name = Name.ToLower();
			this.Value = Value;
		}


		internal string _Value = null;
		/// <summary>
		/// 变量的值
		/// </summary>
		public string Value
		{
			get
			{
				if(this._Value==null)	this._Value = this.InnerHtml.Trim();
				if(!this.IsUpdate && this.IsExpression)		this.ExplainValue();
				return this._Value;
			}
			set
			{
				this._Value = value;
				this.IsUpdate = false;
			}
		}

		internal bool IsUpdate = false;

		string _Name = null;
		/// <summary>
		/// 变量的名字
		/// </summary>
		public string Name
		{
			get { return this._Name;  }
			set { this._Name = value; }
		}

		bool _IsExpression = false;
		/// <summary>
		/// 是否为表达式
		/// </summary>
		public bool IsExpression
		{
			get { return this._IsExpression;  }
			set { this._IsExpression = value; }
		}

		ArgumentGroup _Group = null;
		/// <summary>
		/// 所属的变量组
		/// </summary>
		internal ArgumentGroup Group
		{
			get { return this._Group;  }
			set { this._Group = value; }
		}

		/// <summary>
		/// 标记是否正在转换中
		/// </summary>
		bool IsExplaining = false;

		/// <summary>
		/// 解释当前表达式的值
		/// </summary>
		public void ExplainValue()
		{
			if(!this.IsExpression || this.IsUpdate)	return;

			if(this.IsExplaining)	throw new ReportException("出现变量 {" + this.Name + "} 递归引用错误！");

			try
			{
				//解释当前表达式中的参数
				this.IsExplaining = true;

				string Prefix = "EXP";
				if(this._Value.Length>=4 && this._Value[3]==':')
				{
					Prefix = this._Value.Substring(0, 3);
					this._Value = this._Value.Substring(4);
				}
				this._Value = this.Group.ParentReport.ExplainHtml(this._Value);

				switch(Prefix.ToUpper())
				{
					case "SQL":		//是SQL语句
					{
						if(this.Group==null || this.Group.ParentReport==null)	return;
						ReportBase myReport = this.Group.ParentReport;
						if(myReport.DbConnection == null)	return;

						myReport.DbCommand.CommandText = this._Value;
						if(myReport is ReportDetail)	(myReport as ReportDetail).AppendParameters();

						object result = myReport.DbCommand.ExecuteScalar();
						this._Value = result==null?"":result.ToString();
					}
						break;
					case "EXP":		//是普通表达式
						//DoNothing
						break;
					default:
						this._Value = Prefix + this._Value;
						break;
				}
				this.IsUpdate = true;
				this.IsExplaining = false;
			}
			catch(Exception myExp)
			{
				throw new ReportException(myExp.Message, myExp);
			}
		}
	}

	#endregion

	#region 变量的组合

	/// <summary>
	/// 变量的组合
	/// </summary>
	public class ArgumentGroup : ItemGroup
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public ArgumentGroup() {}

		/// <summary>
		/// 添加一项
		/// </summary>
		/// <param name="myArgument"></param>
		public void Append(Argument myArgument)
		{
			string Key = myArgument.Name.ToLower();
			this.myHashtable.Add(Key, myArgument);
			myArgument.Group = this;
		}

		/// <summary>
		/// 删除一项
		/// </summary>
		/// <param name="Key"></param>
		public void Remove(string Key)
		{
			Key = Key.ToLower();
			if(this.myHashtable.ContainsKey(Key))	this.myHashtable.Remove(Key);
		}

		/// <summary>
		/// 方便在客户端添加项
		/// </summary>
		public Argument Option
		{
			set { this.Append(value); }
		}

		/// <summary>
		/// 通过索引查询参数值
		/// </summary>
		public string this[string Key]
		{
			get
			{
				Key = Key.ToLower();
				if(Key=="&lt;" || Key=="<")		return "{";
				if(Key=="&gt;" || Key==">")		return "}";
				Argument myArgument = this.myHashtable[Key] as Argument;
				if(myArgument == null)	return null;
				else					return myArgument.Value;
			}
			set
			{
				Key = Key.ToLower();
				Argument myArgument = this.myHashtable[Key] as Argument;
				if(myArgument == null)
				{
					this.Append(new Argument(Key, value as string));
				}
				else
				{
					if(myArgument.Value != value as string)
					{
						myArgument.Value = value as string;
					}
				}
			}
		}

		/// <summary>
		/// 将表达式转换为实际的值
		/// </summary>
		internal void ExplainValues()
		{
			System.Text.StringBuilder mySql = new System.Text.StringBuilder();
			System.Collections.ArrayList myItem = new System.Collections.ArrayList();
			string Flag = null;

			foreach(Argument item in this.myHashtable.Values)
			{
				if(item.IsUpdate || !item.IsExpression)	continue;
				string Value = item._Value;
				if(Value==null)		return;

				//SQL语句
				if(Value.ToUpper().TrimStart().StartsWith("SQL:"))
				{
					Value = this.ParentReport.ExplainArgument(item._Value);
					string Sql = Value.Substring("SQL:".Length);
					if(Sql.TrimStart().ToUpper().StartsWith("SELECT"))
					{
						mySql.Append(Sql);		mySql.Append(";");
						myItem.Add(item.Name);
						Flag = "SQL";
					}
					else	item.IsUpdate = true;
				}
			}

			try
			{
				switch(Flag)
				{
					case "SQL":
					{
						//执行命令并获取值：ＳＱＬ语句
						if(mySql.Length!=0 && this.ParentReport!=null && this.ParentReport.DbConnection!=null)
						{
							this.ParentReport.DbCommand.CommandText = mySql.ToString();
							if(this.ParentReport is ReportDetail)	(this.ParentReport as ReportDetail).AppendParameters();
							System.Data.IDataReader myReader = this.ParentReport.DbCommand.ExecuteReader();

							foreach(string name in myItem)
							{
								Argument myArgument = this.myHashtable[name.ToLower()] as Argument;
								if(myReader.Read())
								{
									myArgument.Value = myReader[0]==null?"":myReader[0].ToString();
									myArgument.IsUpdate = true;
								}
								if(!myReader.NextResult())	break;
							}

							myReader.Close();
						}
					}
						break;
					default:	//普通表达式
					{
					}
						break;
				}
			}
			catch(Exception myExp)
			{
				throw new ReportException(myExp.Message, myExp);
			}
		}
	}

	#endregion

}
