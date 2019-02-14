using System;
using System.Data;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	#region 字段的属性
	/// <summary>
	/// ReportDetail：字段的属性
	/// </summary>
	public class FieldAttrib : System.Web.UI.HtmlControls.HtmlContainerControl
	{
		#region 构造函数
		/// <summary>
		/// 构造函数
		/// </summary>
		public FieldAttrib(){}

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="Name">字段名字</param>
		/// <param name="DbType">字段类型</param>
		public FieldAttrib(string Name, DbType DbType)
		{
			this.Name = Name;
			this.DbType = DbType.ToString();
		}
		#endregion

		#region 字段名
		string _Name = null;
		/// <summary>
		/// 字段名
		/// </summary>
		public string Name
		{
			get { return this._Name;  }
			set
			{
				if(this._Name != null)	throw new ReportException("不能重复给FieldAttrib.Name属性赋值！");
				this._Name = value.ToLower();
			}
		}
		#endregion

		#region 字段的别名
		string _Title = null;
		/// <summary>
		/// 字段的别名
		/// </summary>
		public string Title
		{
			get { return this._Title==null? this.Name: this._Title; }
			set { this._Title = value; }
		}
		#endregion

		#region 字段类型
		string _DbType = null;
		/// <summary>
		/// 字段类型
		/// </summary>
		public string DbType
		{
			get { return this._DbType;  }
			set 
			{ 
				this._DbType = value; 
				if(this._Parameter!=null)
				{
					try
					{
						System.Enum dbType = (System.Enum)System.Enum.Parse(this.Group.DbType, this.DbType, true);
						this._Parameter.GetType().GetProperty(this.Group.DbType.Name).SetValue(this._Parameter, dbType, null);
					}
					catch
					{
						System.Enum dbType = (System.Enum)System.Enum.Parse(typeof(System.Data.DbType), this.DbType, true);
						this._Parameter.GetType().GetProperty("DbType").SetValue(this._Parameter, dbType, null);
					}
				}
			}
		}

		/// <summary>
		/// 设置字段类型
		/// </summary>
		/// <param name="dbType"></param>
		public void SetDbType(System.Enum dbType)
		{
			this.DbType = dbType.ToString();
		}
		#endregion

		#region 字段长度
		int _Size = 0;
		/// <summary>
		/// 字段长度
		/// </summary>
		public int Size
		{
			get { return this._Size;  }
			set 
			{ 
				this._Size = value; 
				if(this._Parameter!=null)	this._Parameter.Size = this._Size;
			}
		}
		#endregion

		#region 验证码
		string _Verification = null;
		/// <summary>
		/// 验证码，以正则表达式形式表示
		/// </summary>
		public string Verification
		{
			get 
			{
				if(this._Verification == null)
				{
					switch(this.Parameter.DbType)
					{
						case System.Data.DbType.Int16: case System.Data.DbType.Int32: case System.Data.DbType.Int64: case System.Data.DbType.SByte:
							this._Verification = @"^[1-9-][0-9]*?$";
							break;
						case System.Data.DbType.UInt16: case System.Data.DbType.UInt32: case System.Data.DbType.UInt64: case System.Data.DbType.Byte:
							this._Verification = @"^[1-9][0-9]*?$";
							break;
						case System.Data.DbType.Currency: case System.Data.DbType.Decimal: case System.Data.DbType.Double:
							this._Verification = @"^(?:\+|-)?\d+(?:\.\d+)?$";
							break;
						case System.Data.DbType.Time: case System.Data.DbType.DateTime: case System.Data.DbType.Date:
							this._Verification = @"^\d{1,4}-\d{1,2}-\d{1,2}(?:\s+\d{1,2}:\d{1,2}:\d{1,2})?$";
							break;
					}
				}
				return this._Verification;  
			}
			set { this._Verification = value; }
		}
		#endregion

		#region 自定义错误信息
		string _ErrorMsg = null;
		/// <summary>
		/// 错误信息，当验证错误的时候将会以对话框形式弹出该消息
		/// </summary>
		public string ErrorMsg
		{
			get {	return this._ErrorMsg; }
			set { this._ErrorMsg = value;  }
		}
		#endregion

		#region 是否为主键
		bool _IsKey = false;
		/// <summary>
		/// 是否为主键
		/// </summary>
		public bool IsKey
		{
			get { return this._IsKey;  }
			set { this._IsKey = value; }
		}
		#endregion

		#region 是否允许更改
		bool _IsUpdateable = true;
		/// <summary>
		/// 是否允许更改
		/// </summary>
		public bool IsUpdateable
		{
			get { return this._IsUpdateable;  }
			set { this._IsUpdateable = value; }
		}
		#endregion

		#region 是否允许空值
		bool IsUpdateNullable = false;
		bool _IsNullable = true;
		/// <summary>
		/// 是否允许空值，默认为允许
		/// </summary>
		public bool IsNullable
		{
			get
			{
				if(!this.IsUpdateNullable)
				{
					this._IsNullable = !this.IsKey;
				}
				return this._IsNullable;
			}
			set
			{
				this.IsUpdateNullable = true;
				this._IsNullable = value;
			}
		}
		#endregion

		#region 字段的默认值
		string _Value = null;
		/// <summary>
		/// 字段的默认值，字符串形式表示，提供在ASPX页面中直接赋值，请勿在代码页中直接使用
		/// </summary>
		public string Value
		{
			set
			{
				if(this._Value != null)		throw new ReportException("不能重复给FieldAttrib.Value赋值！");
				this._Value = value;
			}
		}

		/// <summary>
		/// 获取默认值
		/// </summary>
		/// <returns></returns>
		internal string GetDefaultValue()
		{
			return this._Value;
		}
		#endregion

		#region 根据实际情况获取真值（带类型的值）

		object _TrueValue = null;
		/// <summary>
		/// 获取带格式化序列的真值
		/// </summary>
		internal object TrueValue
		{
			get
			{
				if(this._TrueValue == null)
				{
					this._TrueValue = this.GetParameter().Value;
					if(this.FormatString!=null && this.GetParameter().Value!=null)	//  1979-08-20		{year}-{month}-{day}
					{
						Regex ReplaceArgu = new Regex(@"\{\d+\}", RegexOptions.Compiled | RegexOptions.Multiline);
						string strRegex = ReplaceArgu.Replace(this.FormatString, @"(\S+?)").Replace(" ", @"\s") + @"(?=\s|$)";
						Regex myRegex  = new Regex(strRegex, RegexOptions.Multiline);
						string Value   = this.GetParameter().Value.ToString();
						Match myMatch = myRegex.Match(Value);
						string result = myMatch.Value;
						GroupCollection myGroup = myMatch.Groups;
						System.Text.StringBuilder myValues = new System.Text.StringBuilder();
						for(int index=1; index<myGroup.Count; index++)
						{
							myValues.Append(myGroup[index].Value);
							if(index!=myGroup.Count)	myValues.Append("[valueitem]");
						}
						this._TrueValue = myValues.ToString();
					}
				}
				return this._TrueValue;
			}
		}

		/// <summary>
		/// 获取设定值
		/// </summary>
		/// <returns></returns>
		private object GetValue()
		{
			string Value = this._Value;
			ReportDetail ParentReportDetail = this.Group.ParentReport as ReportDetail;
			if(ParentReportDetail.IsPostBack)
			{
				Value = ParentReportDetail.Request.Form[ParentReportDetail.ClientID + "_" + this.Name];
				if(Value==null)
				{
					Value = ParentReportDetail.Request.Form[this.Name];
					if(Value==null)
					{
						Value = ParentReportDetail.Request.Form["htc_" + this.Name];
						if(Value==null)		Value = ParentReportDetail.Request.Form["htc_" + ParentReportDetail.ClientID + "_" + this.Name];
					}
				}
			}
			if(Value==null && this.FormatString!=null)
			{
				System.Text.StringBuilder myTrueValues = new System.Text.StringBuilder();
				if(ParentReportDetail.IsPostBack && this.Values!=null)
				{
					ReportDetail myReportDetail = ParentReportDetail;
					string [] FieldValues = new string [this.Values.Length];
					for(int index=0; index<this.Values.Length; index++)
					{
						string FieldName = this.Values[index];
						string FieldValue = myReportDetail.Request.Form[myReportDetail.ClientID + "_" + FieldName];
						if(FieldValue == null)
						{
							FieldValue = myReportDetail.Request.Form[FieldName];
							if(FieldValue == null)
							{
								FieldValue = myReportDetail.Request.Form["htc_" + this.Name];
								if(FieldValue == null)	FieldValue = myReportDetail.Request.Form["htc_" + myReportDetail.ClientID + "_" + this.Name];
							}
						}
						FieldValues[index] = FieldValue;
						myTrueValues.Append(FieldValue);
						if(index!=this.Values.Length)	myTrueValues.Append("[valueitem]");
					}
					this._TrueValue = myTrueValues.ToString();
					return string.Format(this.FormatString, FieldValues);
				}
			}
			return Value;
		}
		#endregion

		#region 获取参数对象
		internal bool IsUpdated = false;

		/// <summary>
		/// 获取参数对象
		/// </summary>
		/// <returns></returns>
		public IDbDataParameter GetParameter()
		{
			#region 读取真值（带类型的值）
			//如果尚未进行更新
			if(this.IsUpdated == false)
			{
				this.IsUpdated = true;
				string Value = this.GetValue() as string;
				if(Value != null)
				{
					try
					{
						#region 将读取的值转换为适合的数据类型
						switch(this.Parameter.DbType)
						{
							case System.Data.DbType.AnsiString:	this.Parameter .Value = Value;						break;
							case System.Data.DbType.Boolean:		//对布尔值的转换
								if(Value=="0" || Value=="")		this.Parameter.Value = false;
								else if(Value == "1")			this.Parameter.Value = true;
								else	this.Parameter.Value = Convert.ToBoolean(Value);	break;
							case System.Data.DbType.Byte:		this.Parameter.Value = Convert.ToByte(Value);		break;
							case System.Data.DbType.Currency:
							case System.Data.DbType.Decimal:	this.Parameter.Value = Convert.ToDecimal(Value);	break;
							case System.Data.DbType.Date: 
							case System.Data.DbType.Time:
							case System.Data.DbType.DateTime:	this.Parameter.Value = Convert.ToDateTime(Value);	break;
							case System.Data.DbType.Double:		this.Parameter.Value = Convert.ToDouble(Value);		break;
							case System.Data.DbType.Int16:		this.Parameter.Value = Convert.ToInt16(Value);		break;
							case System.Data.DbType.Int32:		this.Parameter.Value = Convert.ToInt32(Value);		break;
							case System.Data.DbType.Int64:		this.Parameter.Value = Convert.ToInt64(Value);		break;
							case System.Data.DbType.SByte:		this.Parameter.Value = Convert.ToSByte(Value);		break;
							case System.Data.DbType.Single:		this.Parameter.Value = Convert.ToSingle(Value);		break;
							case System.Data.DbType.UInt16:		this.Parameter.Value = Convert.ToUInt16(Value);		break;
							case System.Data.DbType.UInt32:		this.Parameter.Value = Convert.ToUInt32(Value);		break;
							case System.Data.DbType.UInt64:		this.Parameter.Value = Convert.ToInt64(Value);		break;
							default:							this.Parameter.Value = Value;						break;
						}
						#endregion
					}
					catch
					{
						this.Parameter.Value = null;
					}
				}

				//如果转换失败，则赋予相应类型的空值
				if(this.Parameter.Value==null)
				{
					this.Parameter.Value = this.GetNull();
				}
			}
			#endregion

			return this.Parameter;
		}

		private object GetNull()
		{
			object result = null;
			switch(this.Group.ConnectionType.Name)
			{
				case "SqlConnection":		//SQL-SERVER数据连接

					#region 对其获取空值
				switch(this.Parameter.DbType)
				{
					case System.Data.DbType.AnsiString:	result = System.Data.SqlTypes.SqlString.Null;		break;
					case System.Data.DbType.Boolean:	result = System.Data.SqlTypes.SqlBoolean.Null;		break;
					case System.Data.DbType.Byte:		result = System.Data.SqlTypes.SqlByte.Null;			break;
					case System.Data.DbType.Currency:	result = System.Data.SqlTypes.SqlMoney.Null;		break;
					case System.Data.DbType.Decimal:	result = System.Data.SqlTypes.SqlDecimal.Null;		break;
					case System.Data.DbType.Date: 
					case System.Data.DbType.Time:
					case System.Data.DbType.DateTime:	result = System.Data.SqlTypes.SqlDateTime.Null;		break;
					case System.Data.DbType.Double:		result = System.Data.SqlTypes.SqlDouble.Null;		break;
					case System.Data.DbType.Int16:		result = System.Data.SqlTypes.SqlInt16.Null;		break;
					case System.Data.DbType.Int32:		result = System.Data.SqlTypes.SqlInt32.Null;		break;
					case System.Data.DbType.Int64:		result = System.Data.SqlTypes.SqlInt64.Null;		break;
					case System.Data.DbType.SByte:		result = System.Data.SqlTypes.SqlByte.Null;			break;
					case System.Data.DbType.Single:		result = System.Data.SqlTypes.SqlSingle.Null;		break;
					case System.Data.DbType.UInt16:		result = System.Data.SqlTypes.SqlInt16.Null;		break;
					case System.Data.DbType.UInt32:		result = System.Data.SqlTypes.SqlInt32.Null;		break;
					case System.Data.DbType.UInt64:		result = System.Data.SqlTypes.SqlInt64.Null;		break;
					default:							result = System.Data.SqlTypes.SqlString.Null;		break;
				}
					#endregion

					break;

				default:			//其它数据连接
					result = System.DBNull.Value;
					break;
			}
			return result;
		}

		IDbDataParameter _Parameter = null;
		/// <summary>
		/// 根据数据链接类型创建参数对象
		/// </summary>
		IDbDataParameter Parameter
		{
			get
			{
				if(this._Parameter == null)
				{
					//利用反射技术动态创建参数对象，并以接口的形式返回
					if(this.Group.ParameterType == null)	throw new ReportException("请在给各个字段赋值之前，指定数据链接！");
					this._Parameter = this.Group.ParameterType.Assembly.CreateInstance(this.Group.ParameterType.FullName) as IDbDataParameter;
					if(this.Name == null)	throw new ReportException("字段属性对象缺少名字！");
					this._Parameter.ParameterName = "@" + this.Name;
					if(this.Size != 0)		this._Parameter.Size = this.Size;
					if(this.DbType != null)
					{
						try
						{
							try
							{
								System.Enum dbType = (System.Enum)System.Enum.Parse(this.Group.DbType, this.DbType, true);
								this._Parameter.GetType().GetProperty(this.Group.DbType.Name).SetValue(this._Parameter, dbType, null);
							}
							catch
							{
								System.Enum dbType = (System.Enum)System.Enum.Parse(typeof(System.Data.DbType), this.DbType, true);
								this._Parameter.GetType().GetProperty("DbType").SetValue(this._Parameter, dbType, null);
							}
						}
						catch
						{
							throw new ReportException("无效的字段类型 " + this.DbType);
						}
					}
					//是否允许参数为空
					this._Parameter.GetType().GetMethod("set_IsNullable").Invoke(this._Parameter, new object[]{this.IsNullable});
				}
				return this._Parameter;
			}
		}

		#endregion

		#region 是否支持UBB
		bool _IsSupportUBB = false;
		/// <summary>
		/// 是否支持UBB代码
		/// </summary>
		public bool IsSupportUBB
		{
			get { return this._IsSupportUBB;  }
			set { this._IsSupportUBB = value; }
		}
		#endregion

		#region 所属参数组
		FieldAttribGroup _Group = null;
		/// <summary>
		/// 所属参数组
		/// </summary>
		internal FieldAttribGroup Group
		{
			get { return this._Group;  }
			set { this._Group = value; }
		}
		#endregion

		#region 记录更新前的值
		private object _OldValue = null;
		/// <summary>
		/// 先前的值，即在更新、删除时，数据库中先前存在的值
		/// </summary>
		internal object OldValue
		{
			get { return this._OldValue;  }
			set { this._OldValue = value; }
		}
		#endregion

		#region 格式化字符串
		private string _FormatString = null;
		private bool IsUpdateFormatString = false;
		/// <summary>
		/// 记录标准格式化字符串{0}-{1}
		/// </summary>
		public string FormatString
		{
			get
			{
				if(this._FormatString == null)
				{
					this._FormatString = this.InnerHtml.Trim();
					if(this._FormatString == "")	this._FormatString = null;
				}
				if(this.IsUpdateFormatString==false && this._FormatString!=null)
				{
					this.IsUpdateFormatString = true;
					this.SelectArguments();
				}
				return this._FormatString;
			}
			set
			{
				this._FormatString = value;
				this.IsUpdateFormatString = false;
			}
		}
		#endregion

		#region 执行格式化
		/// <summary>
		/// 记录原始格式化字符串（{year}-{month}-{day}等）
		/// </summary>
		internal string TrueFormatString;

		int  argumentindex = 0;
		bool isusedargument = false;
		System.Text.StringBuilder myValues;
		/// <summary>
		/// 执行格式化
		/// </summary>
		private void SelectArguments()
		{
			this.argumentindex = 0;
			this.isusedargument = false;
			this.myValues = new System.Text.StringBuilder();
			this._FormatString = this.ReplaceArguments.Replace(this.TrueFormatString=this._FormatString.Replace("\r\n", ""), new MatchEvaluator(this.ReplaceArgument));
			if(this.isusedargument)	this.TrueFormatString = this.ReplaceArguments.Replace(this.TrueFormatString, new MatchEvaluator(this.ReplaceArgument2));
			if(this.myValues.Length > 0)	this.Values = this.myValues.ToString().Trim(',').Split(',');
			else							this.Values = null;
		}
		private Regex ReplaceArguments = new Regex(@"\{\S+?\}", RegexOptions.Multiline | RegexOptions.Compiled);

		/// <summary>
		/// 执行格式化的时候，替换字符串的具体操作
		/// </summary>
		/// <param name="myMatch"></param>
		/// <returns></returns>
		private string ReplaceArgument(Match myMatch)
		{
			string Key = myMatch.Value.Trim('{', '}');

			//检验是否为自定义参数。
			ReportDetail ParentReportDetail = this.Group.ParentReport as ReportDetail;
			if(ParentReportDetail.Argument[Key] != null)
			{
				this.isusedargument = true;
				return ParentReportDetail.Argument[Key];
			}

			//赋予格式化序列元素
			this.myValues.Append(Key);
			this.myValues.Append(",");
			return "{" + (argumentindex++) + "}";
		}

		/// <summary>
		/// 扫行格式化的时候，替换字符串的具体要求
		/// </summary>
		/// <param name="myMatch"></param>
		/// <returns></returns>
		private string ReplaceArgument2(Match myMatch)
		{
			string Key = myMatch.Value.Trim('{', '}');

			ReportDetail ParentReportDetail = this.Group.ParentReport as ReportDetail;
			//检验是否为自定义参数。
			if(ParentReportDetail.Argument[Key] != null)
			{
				this.isusedargument = true;
				return ParentReportDetail.Argument[Key];
			}
			else return myMatch.Value;
		}

		private string [] Values = null;
		#endregion
	}
	#endregion

	#region 字段属性的组合

	/// <summary>
	/// ReportDetail：字段属性的组合
	/// </summary>
	public class FieldAttribGroup : ItemGroup
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public FieldAttribGroup(){}

		/// <summary>
		/// 增加一个属性
		/// </summary>
		/// <param name="newValue">新属性</param>
		public void Append(FieldAttrib newValue)
		{
			if(newValue.Name != null)
			{
				newValue.Group = this;
				this.myHashtable.Add(newValue.Name.ToLower(), newValue);
			}
		}

		/// <summary>
		/// 删除一个属性
		/// </summary>
		/// <param name="Name"></param>
		public void Remove(string Name)
		{
			string Key = Name.ToLower();
			if(this.myHashtable.ContainsKey(Key))		this.myHashtable.Remove(Name.ToLower());
		}

		/// <summary>
		/// 按索引读取属性
		/// </summary>
		public FieldAttrib this[string Name]
		{
			get { return this.myHashtable[Name.ToLower()] as FieldAttrib; }
		}

		/// <summary>
		/// 增加一个属性
		/// </summary>
		public FieldAttrib Field
		{
			set { this.Append(value); }
		}

		/// <summary>
		/// 增加一个属性
		/// </summary>
		public FieldAttrib It
		{
			set { this.Append(value); }
		}

		System.Type _ConnectionType;
		/// <summary>
		/// 数据链接类别
		/// </summary>
		internal System.Type ConnectionType
		{
			get { return this._ConnectionType; }
			set
			{
				this._ConnectionType = value;
				//根据命令执行对象的条件判断参数的类型
				this._ParameterType = this._ConnectionType.GetMethod("CreateCommand").ReturnType.GetMethod("CreateParameter").ReturnType;
				foreach(System.Reflection.PropertyInfo myInfo in this._ParameterType.GetProperties())
				{
					if(myInfo.Name.EndsWith("DbType") && myInfo.Name!="DbType")
					{
						this._DbType = myInfo.PropertyType;
						break;
					}
				}
				if(this._DbType == null)		this._DbType = typeof(System.Data.DbType);
			}
		}
		System.Type _DbType;
		/// <summary>
		/// 参数类型的类型
		/// </summary>
		internal System.Type DbType
		{
			get { return this._DbType; }
		}

		System.Type _ParameterType;
		/// <summary>
		/// 参数对象的类型
		/// </summary>
		internal System.Type ParameterType
		{
			get { return this._ParameterType; }
		}

		/// <summary>
		/// 得到主键字段的集合
		/// </summary>
		/// <returns></returns>
		public System.Collections.ArrayList GetKeyFields()
		{
			System.Collections.ArrayList myKeys = new System.Collections.ArrayList();
			foreach(System.Collections.DictionaryEntry Item in myHashtable)
			{
				if((Item.Value as FieldAttrib).IsKey)	myKeys.Add(Item.Value);
			}
			return myKeys;
		}
	}

	#endregion
}
