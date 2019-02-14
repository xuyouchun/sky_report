using System;
using System.Data;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	#region 一些专供事件使用的委托

	/// <summary>
	/// ReportCell：在读取数据之前发生，返回非空，则不再启用内置的读取功能
	/// </summary>
	public delegate IDataReader OnReadData(ReportCell sender, IDbCommand DbCommand);

	/// <summary>
	/// ReportCell：从客户端传过来的事件
	/// </summary>
	public delegate void OnCellCommand(ReportCell sender, string CommandName, string Argument);

	/// <summary>
	/// ReportCell：在每行显示之前触发
	/// </summary>
	public delegate void OnItemShow(ReportCell sender);

	#endregion

	#region 样式设置

	/// <summary>
	/// 汇总屏的样式设置
	/// </summary>
	public class ReportCellOption : Option
	{

		/// <summary>
		/// 从XML文件中读取设置
		/// </summary>
		/// <param name="FileName"></param>
		public override void ReadXml(string FileName)
		{

		}

		/// <summary>
		/// 将当前的设置写入XML文件
		/// </summary>
		/// <param name="FileName"></param>
		public override void WriteXml(string FileName)
		{
			
		}
	}

	#endregion
}
