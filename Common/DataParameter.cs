using System;
using System.Data;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	#region 参数的组合

	/// <summary>
	/// 参数的组合
	/// </summary>
	internal class DataParameterGroup : ItemGroup
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public DataParameterGroup(IDbConnection DbConnection)
		{
			this.ParameterType = DbConnection.CreateCommand().CreateParameter().GetType();

			foreach(System.Reflection.PropertyInfo myInfo in this.ParameterType.GetProperties())
			{
				if(myInfo.Name.EndsWith("DbType") && myInfo.Name!="DbType")
				{
					this.DbTypePropertyInfo = myInfo;
					break;
				}
			}
			if(this.DbTypePropertyInfo == null)		this.DbTypePropertyInfo = this.ParameterType.GetProperty("DbType");
		}

		private System.Type ParameterType = null;
		private System.Reflection.PropertyInfo DbTypePropertyInfo = null;

		/// <summary>
		/// 根据列名字获得参数
		/// </summary>
		public DataParameter this[string ColName]
		{
			get
			{
				if(ColName==null || ColName=="")	return null;
				ColName = ColName.ToLower().Trim('@');
				object result = this.myHashtable[ColName];
				if(result==null)
				{
					if(this.ParentReport is ReportCell)
					{
						ReportCell myReportCell = this.ParentReport as ReportCell;
						if(myReportCell.ColStyle.IsContainsKey(ColName))
						{
							ColStyle item = myReportCell.ColStyle[ColName];
							this.Append(ColName, null, item.DbType);
						}
					}
					else if(this.ParentReport is ReportDetail)	// 如果是细节屏
					{
//						ReportDetail myReportDetail = this.ParentReport as ReportDetail;
//						if(myReportDetail.Fields.IsContainsKey(ColName))
//						{
//							FieldAttrib item = myReportDetail.Fields[ColName];
//							this.Append(ColName, item.v
//						}
					}
					result = this.myHashtable[ColName];
				}

				return result as DataParameter;
			}
		}

		/// <summary>
		/// 增加一项
		/// </summary>
		/// <param name="Name">名称</param>
		/// <param name="Value">值</param>
		/// <param name="Type">类型</param>
		public void Append(string Name, object Value, object ValueType)
		{
			if(Name==null)	return;
			Name = Name.ToLower().Trim('@');
			DataParameter parameter = new DataParameter(Name, Value, ValueType, this.ParameterType, this.DbTypePropertyInfo);

			if(!this.myHashtable.ContainsKey(Name))	this.myHashtable.Add(Name, parameter);
		}

		/// <summary>
		/// 移除一项
		/// </summary>
		/// <param name="Name">名称</param>
		public void Remove(string Name)
		{
			if(Name==null)	return;
			Name = Name.ToLower().Trim('@');

			if(this.myHashtable.ContainsKey(Name))	this.myHashtable.Remove(Name);
		}
	}

	#endregion

	#region 参数项（类）

	/// <summary>
	/// 参数项
	/// </summary>
	internal class DataParameter
	{
		static DataParameter()
		{
			foreach(string DbTypeName in System.Enum.GetNames(typeof(System.Data.DbType)))
			{
				DbTypeCollection.Add(DbTypeName.ToLower(), System.Enum.Parse(typeof(System.Data.DbType), DbTypeName));
			}
		}

		static System.Collections.Hashtable DbTypeCollection = new System.Collections.Hashtable();

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="Name">参数名（大小写无关）</param>
		/// <param name="Value">值</param>
		/// <param name="ValueType">值的类型（可以是DbType枚举或该数据连接的特有类型）</param>
		/// <param name="ParameterType">参数的类型</param>
		/// <param name="DbType">该数据连接的特有类型</param>
		public DataParameter(string Name, object Value, object ValueType, System.Type ParameterType, System.Reflection.PropertyInfo DbTypePropertyInfo)
		{
			this._Name = Name.ToLower().Trim('@');
			this._Parameter = ParameterType.Assembly.CreateInstance(ParameterType.FullName, true) as IDataParameter;
			this._Parameter.ParameterName = "@" + this._Name;

			this.DbTypePropertyInfo = DbTypePropertyInfo;
			System.Type DbType = DbTypePropertyInfo.PropertyType;
			string DbTypeName  = DbTypePropertyInfo.Name;

			if(ValueType==null)
			{
				this._Parameter.DbType = System.Data.DbType.String;
			}
			else if(ValueType is System.Data.DbType)		// 若是基础数据类型
			{
				this._Parameter.DbType = (System.Data.DbType)ValueType;
			}
			else if(ValueType.GetType() == DbType)		// 若是数据库自身的类型
			{
				ParameterType.GetProperty(DbTypeName).SetValue(this._Parameter, ValueType, null);
			}
			else if(ValueType is string)			// 若是字符串类型
			{
				ValueType = (ValueType as string).ToLower();
				if(DbTypeCollection.ContainsKey(ValueType))
				{
					this._Parameter.DbType = (System.Data.DbType)DbTypeCollection[ValueType];
				}
				else
				{
					ParameterType.GetProperty(DbTypeName).SetValue(this._Parameter, System.Enum.Parse(DbType, ValueType as string, true), null);
				}
			}

			this.Value = Value;
		}

		System.Reflection.PropertyInfo DbTypePropertyInfo = null;

		string _Name = null;
		/// <summary>
		/// 参数的名字
		/// </summary>
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				this._Name = Name.ToLower().Trim('@');
				this._Parameter.ParameterName = "@" + this._Name;
			}
		}

		/// <summary>
		/// 参数的值
		/// </summary>
		public object Value
		{
			get { return this._Parameter.Value; }
			set
			{
				object _Value = null;
				if(value == null)	_Value = null;
				else
				{
					try
					{
						#region 根据数据类型进行转换

						switch(this._Parameter.DbType)
						{
								// 字符串
							case DbType.AnsiString:
							case DbType.AnsiStringFixedLength:
							case DbType.Guid:
							case DbType.String:
							case DbType.StringFixedLength:
								_Value = Convert.ToString(value);
								break;

								// 布尔值
							case DbType.Boolean:
								_Value = Convert.ToBoolean(value);
								break;

								// 字符型
							case DbType.Byte:
								_Value = Convert.ToByte(value);
								break;

								// 货币型 Decimal型
							case DbType.Decimal:
							case DbType.Currency:
								_Value = Convert.ToDecimal(value);
								break;

								// 日期时间型
							case DbType.Date:
							case DbType.Time:
							case DbType.DateTime:
								_Value = Convert.ToDateTime(value);
								break;

								// Double型
							case DbType.Double:
								_Value = Convert.ToDouble(value);
								break;

								// Int16型
							case DbType.Int16:
								_Value = Convert.ToInt16(value);
								break;

								// Int32型
							case DbType.Int32:
								_Value = Convert.ToInt32(value);
								break;

								// Int64型
							case DbType.Int64:
								_Value = Convert.ToInt64(value);
								break;

								// SByte型
							case DbType.SByte:
								_Value = Convert.ToSByte(value);
								break;

								// Single型
							case DbType.Single:
								_Value = Convert.ToSingle(value);
								break;

								// 其余的数据类型
							case DbType.Object: case DbType.VarNumeric:
								_Value = value;
								break;

							default:
								_Value = value;
								break;
						}
						#endregion

						this._Parameter.Value = _Value;
					}
					catch
					{
						_Value = null;
					}
				}

				if(_Value==null)	this._Parameter.Value = this.GetNull();
				else				this._Parameter.Value = _Value;
			}
		}

		#region 根据数据库类型获取空值

		private object GetNull()
		{
			object result = null;
			switch(this.DbTypePropertyInfo.Name)
			{
				case "SqlDbType":		//SQL-SERVER数据连接

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

		#endregion

		/// <summary>
		/// 字段的类型
		/// </summary>
		public System.Data.DbType DbType
		{
			get { return this._Parameter.DbType; }
		}

		/// <summary>
		/// 参数
		/// </summary>
		public IDataParameter Parameter
		{
			get { return this._Parameter; }
		}

		IDataParameter _Parameter = null;
	}

	#endregion
}
