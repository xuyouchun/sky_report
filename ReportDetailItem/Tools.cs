using System;
using System.Data;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	#region 一些专供事件使用的委托

	/// <summary>
	/// ReportDetail：在向客户端输出HTML代码之前发生
	/// </summary>
	public delegate void OnBeforeOutput(ReportDetail sender, ref string Content);

	/// <summary>
	/// ReportDetail：在对数据进行读取、插入、更新、删除之前和之后发生的事件
	/// </summary>
	public delegate bool OnDetailOperate(ReportDetail sender, Operate operate);

	/// <summary>
	/// ReportDetail：从客户端传过来的事件
	/// </summary>
	public delegate void OnDetailCommand(ReportDetail sender, string CommandName, string Argument);

	/// <summary>
	/// ReportDetail：对数据的各种操作
	/// </summary>
	public enum Operate : byte
	{
		/// <summary>
		/// 读取
		/// </summary>
		Select,
		/// <summary>
		/// 插入
		/// </summary>
		Insert,
		/// <summary>
		/// 更新
		/// </summary>
		Update,
		/// <summary>
		/// 删除
		/// </summary>
		Delete,
		/// <summary>
		/// 重置
		/// </summary>
		Reset,
		/// <summary>
		/// 无
		/// </summary>
		None
	}

	#endregion
}
