using System;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	/// <summary>
	/// �ṩһЩ�����Ĺ���
	/// </summary>
	public abstract class Tools
	{
		static private Regex _MatchArgument = new Regex(@"\{[^\{\}\n]+\}", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
		/// <summary>
		/// ����ƥ��������е�����
		/// </summary>
		static public Regex MatchArgument
		{
			get { return _MatchArgument; }
		}

		/// <summary>
		/// ��ϼ����е�ֵ���ö��ŷָ�����������Ӧ��SQL����еġ�in������
		/// </summary>
		/// <param name="myICollection">����</param>
		/// <param name="isNeedQuot">�Ƿ���Ҫ��������</param>
		/// <returns></returns>
		static public string GroupValues(System.Collections.ICollection myICollection, bool isNeedQuot)
		{
			System.Text.StringBuilder myValues = new System.Text.StringBuilder();
			foreach(object item in myICollection)
			{
				if(isNeedQuot)
				{
					myValues.Append("'");
					myValues.Append(item.ToString());
					myValues.Append("',");
				}
				else
				{
					myValues.Append(item.ToString());
					myValues.Append(",");
				}
			}
			return myValues.ToString().TrimEnd(',');
		}

		/// <summary>
		/// ��ϳ�SELECT�ؼ��е�OPTION��ʽ
		/// </summary>
		/// <param name="myReader"></param>
		/// <returns></returns>
		static public string GroupOptions(System.Data.IDataReader myReader)
		{
			System.Text.StringBuilder myOptions = new System.Text.StringBuilder();
			int FieldCount = myReader.FieldCount;
			while(myReader.Read())
			{
				myOptions.Append("<option value=\"");
				myOptions.Append(myReader[0].ToString().Replace("\"", "&quot;"));
				myOptions.Append("\">");
				if(FieldCount>1)	myOptions.Append(myReader[1]);
				myOptions.Append("</option>");
			}
			return myOptions.ToString();
		}

		/// <summary>
		/// �ж��Ƿ�Ϊ��ֵ
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		static public bool IsNull(object Value)
		{
			if(Value==null)		return true;
			else if(Value is System.Data.SqlTypes.INullable && (Value as System.Data.SqlTypes.INullable).IsNull)	return true;
			else if(Value is System.DBNull)		return true;
			return false;
		}
	}
}
