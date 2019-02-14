using System;
using System.Data;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	#region �ֶε�����
	/// <summary>
	/// ReportDetail���ֶε�����
	/// </summary>
	public class FieldAttrib : System.Web.UI.HtmlControls.HtmlContainerControl
	{
		#region ���캯��
		/// <summary>
		/// ���캯��
		/// </summary>
		public FieldAttrib(){}

		/// <summary>
		/// ���캯��
		/// </summary>
		/// <param name="Name">�ֶ�����</param>
		/// <param name="DbType">�ֶ�����</param>
		public FieldAttrib(string Name, DbType DbType)
		{
			this.Name = Name;
			this.DbType = DbType.ToString();
		}
		#endregion

		#region �ֶ���
		string _Name = null;
		/// <summary>
		/// �ֶ���
		/// </summary>
		public string Name
		{
			get { return this._Name;  }
			set
			{
				if(this._Name != null)	throw new ReportException("�����ظ���FieldAttrib.Name���Ը�ֵ��");
				this._Name = value.ToLower();
			}
		}
		#endregion

		#region �ֶεı���
		string _Title = null;
		/// <summary>
		/// �ֶεı���
		/// </summary>
		public string Title
		{
			get { return this._Title==null? this.Name: this._Title; }
			set { this._Title = value; }
		}
		#endregion

		#region �ֶ�����
		string _DbType = null;
		/// <summary>
		/// �ֶ�����
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
		/// �����ֶ�����
		/// </summary>
		/// <param name="dbType"></param>
		public void SetDbType(System.Enum dbType)
		{
			this.DbType = dbType.ToString();
		}
		#endregion

		#region �ֶγ���
		int _Size = 0;
		/// <summary>
		/// �ֶγ���
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

		#region ��֤��
		string _Verification = null;
		/// <summary>
		/// ��֤�룬��������ʽ��ʽ��ʾ
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

		#region �Զ��������Ϣ
		string _ErrorMsg = null;
		/// <summary>
		/// ������Ϣ������֤�����ʱ�򽫻��ԶԻ�����ʽ��������Ϣ
		/// </summary>
		public string ErrorMsg
		{
			get {	return this._ErrorMsg; }
			set { this._ErrorMsg = value;  }
		}
		#endregion

		#region �Ƿ�Ϊ����
		bool _IsKey = false;
		/// <summary>
		/// �Ƿ�Ϊ����
		/// </summary>
		public bool IsKey
		{
			get { return this._IsKey;  }
			set { this._IsKey = value; }
		}
		#endregion

		#region �Ƿ��������
		bool _IsUpdateable = true;
		/// <summary>
		/// �Ƿ��������
		/// </summary>
		public bool IsUpdateable
		{
			get { return this._IsUpdateable;  }
			set { this._IsUpdateable = value; }
		}
		#endregion

		#region �Ƿ������ֵ
		bool IsUpdateNullable = false;
		bool _IsNullable = true;
		/// <summary>
		/// �Ƿ������ֵ��Ĭ��Ϊ����
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

		#region �ֶε�Ĭ��ֵ
		string _Value = null;
		/// <summary>
		/// �ֶε�Ĭ��ֵ���ַ�����ʽ��ʾ���ṩ��ASPXҳ����ֱ�Ӹ�ֵ�������ڴ���ҳ��ֱ��ʹ��
		/// </summary>
		public string Value
		{
			set
			{
				if(this._Value != null)		throw new ReportException("�����ظ���FieldAttrib.Value��ֵ��");
				this._Value = value;
			}
		}

		/// <summary>
		/// ��ȡĬ��ֵ
		/// </summary>
		/// <returns></returns>
		internal string GetDefaultValue()
		{
			return this._Value;
		}
		#endregion

		#region ����ʵ�������ȡ��ֵ�������͵�ֵ��

		object _TrueValue = null;
		/// <summary>
		/// ��ȡ����ʽ�����е���ֵ
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
		/// ��ȡ�趨ֵ
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

		#region ��ȡ��������
		internal bool IsUpdated = false;

		/// <summary>
		/// ��ȡ��������
		/// </summary>
		/// <returns></returns>
		public IDbDataParameter GetParameter()
		{
			#region ��ȡ��ֵ�������͵�ֵ��
			//�����δ���и���
			if(this.IsUpdated == false)
			{
				this.IsUpdated = true;
				string Value = this.GetValue() as string;
				if(Value != null)
				{
					try
					{
						#region ����ȡ��ֵת��Ϊ�ʺϵ���������
						switch(this.Parameter.DbType)
						{
							case System.Data.DbType.AnsiString:	this.Parameter .Value = Value;						break;
							case System.Data.DbType.Boolean:		//�Բ���ֵ��ת��
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

				//���ת��ʧ�ܣ�������Ӧ���͵Ŀ�ֵ
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
				case "SqlConnection":		//SQL-SERVER��������

					#region �����ȡ��ֵ
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

				default:			//������������
					result = System.DBNull.Value;
					break;
			}
			return result;
		}

		IDbDataParameter _Parameter = null;
		/// <summary>
		/// ���������������ʹ�����������
		/// </summary>
		IDbDataParameter Parameter
		{
			get
			{
				if(this._Parameter == null)
				{
					//���÷��似����̬�����������󣬲��Խӿڵ���ʽ����
					if(this.Group.ParameterType == null)	throw new ReportException("���ڸ������ֶθ�ֵ֮ǰ��ָ���������ӣ�");
					this._Parameter = this.Group.ParameterType.Assembly.CreateInstance(this.Group.ParameterType.FullName) as IDbDataParameter;
					if(this.Name == null)	throw new ReportException("�ֶ����Զ���ȱ�����֣�");
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
							throw new ReportException("��Ч���ֶ����� " + this.DbType);
						}
					}
					//�Ƿ��������Ϊ��
					this._Parameter.GetType().GetMethod("set_IsNullable").Invoke(this._Parameter, new object[]{this.IsNullable});
				}
				return this._Parameter;
			}
		}

		#endregion

		#region �Ƿ�֧��UBB
		bool _IsSupportUBB = false;
		/// <summary>
		/// �Ƿ�֧��UBB����
		/// </summary>
		public bool IsSupportUBB
		{
			get { return this._IsSupportUBB;  }
			set { this._IsSupportUBB = value; }
		}
		#endregion

		#region ����������
		FieldAttribGroup _Group = null;
		/// <summary>
		/// ����������
		/// </summary>
		internal FieldAttribGroup Group
		{
			get { return this._Group;  }
			set { this._Group = value; }
		}
		#endregion

		#region ��¼����ǰ��ֵ
		private object _OldValue = null;
		/// <summary>
		/// ��ǰ��ֵ�����ڸ��¡�ɾ��ʱ�����ݿ�����ǰ���ڵ�ֵ
		/// </summary>
		internal object OldValue
		{
			get { return this._OldValue;  }
			set { this._OldValue = value; }
		}
		#endregion

		#region ��ʽ���ַ���
		private string _FormatString = null;
		private bool IsUpdateFormatString = false;
		/// <summary>
		/// ��¼��׼��ʽ���ַ���{0}-{1}
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

		#region ִ�и�ʽ��
		/// <summary>
		/// ��¼ԭʼ��ʽ���ַ�����{year}-{month}-{day}�ȣ�
		/// </summary>
		internal string TrueFormatString;

		int  argumentindex = 0;
		bool isusedargument = false;
		System.Text.StringBuilder myValues;
		/// <summary>
		/// ִ�и�ʽ��
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
		/// ִ�и�ʽ����ʱ���滻�ַ����ľ������
		/// </summary>
		/// <param name="myMatch"></param>
		/// <returns></returns>
		private string ReplaceArgument(Match myMatch)
		{
			string Key = myMatch.Value.Trim('{', '}');

			//�����Ƿ�Ϊ�Զ��������
			ReportDetail ParentReportDetail = this.Group.ParentReport as ReportDetail;
			if(ParentReportDetail.Argument[Key] != null)
			{
				this.isusedargument = true;
				return ParentReportDetail.Argument[Key];
			}

			//�����ʽ������Ԫ��
			this.myValues.Append(Key);
			this.myValues.Append(",");
			return "{" + (argumentindex++) + "}";
		}

		/// <summary>
		/// ɨ�и�ʽ����ʱ���滻�ַ����ľ���Ҫ��
		/// </summary>
		/// <param name="myMatch"></param>
		/// <returns></returns>
		private string ReplaceArgument2(Match myMatch)
		{
			string Key = myMatch.Value.Trim('{', '}');

			ReportDetail ParentReportDetail = this.Group.ParentReport as ReportDetail;
			//�����Ƿ�Ϊ�Զ��������
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

	#region �ֶ����Ե����

	/// <summary>
	/// ReportDetail���ֶ����Ե����
	/// </summary>
	public class FieldAttribGroup : ItemGroup
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		public FieldAttribGroup(){}

		/// <summary>
		/// ����һ������
		/// </summary>
		/// <param name="newValue">������</param>
		public void Append(FieldAttrib newValue)
		{
			if(newValue.Name != null)
			{
				newValue.Group = this;
				this.myHashtable.Add(newValue.Name.ToLower(), newValue);
			}
		}

		/// <summary>
		/// ɾ��һ������
		/// </summary>
		/// <param name="Name"></param>
		public void Remove(string Name)
		{
			string Key = Name.ToLower();
			if(this.myHashtable.ContainsKey(Key))		this.myHashtable.Remove(Name.ToLower());
		}

		/// <summary>
		/// ��������ȡ����
		/// </summary>
		public FieldAttrib this[string Name]
		{
			get { return this.myHashtable[Name.ToLower()] as FieldAttrib; }
		}

		/// <summary>
		/// ����һ������
		/// </summary>
		public FieldAttrib Field
		{
			set { this.Append(value); }
		}

		/// <summary>
		/// ����һ������
		/// </summary>
		public FieldAttrib It
		{
			set { this.Append(value); }
		}

		System.Type _ConnectionType;
		/// <summary>
		/// �����������
		/// </summary>
		internal System.Type ConnectionType
		{
			get { return this._ConnectionType; }
			set
			{
				this._ConnectionType = value;
				//��������ִ�ж���������жϲ���������
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
		/// �������͵�����
		/// </summary>
		internal System.Type DbType
		{
			get { return this._DbType; }
		}

		System.Type _ParameterType;
		/// <summary>
		/// �������������
		/// </summary>
		internal System.Type ParameterType
		{
			get { return this._ParameterType; }
		}

		/// <summary>
		/// �õ������ֶεļ���
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
