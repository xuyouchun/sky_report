using System;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	/// <summary>
	/// 提供一些公共的功能
	/// </summary>
	public abstract class Tools
	{
		static private Regex _MatchArgument = new Regex(@"\{[^\{\}\n]+\}", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
		/// <summary>
		/// 用于匹配大括号中的内容
		/// </summary>
		static public Regex MatchArgument
		{
			get { return _MatchArgument; }
		}

		/// <summary>
		/// 组合集合中的值，用逗号分隔开来，以适应于SQL语句中的“in”条件
		/// </summary>
		/// <param name="myICollection">集合</param>
		/// <param name="isNeedQuot">是否需要加上引号</param>
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
		/// 组合成SELECT控件中的OPTION格式
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
		/// 判断是否为空值
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
