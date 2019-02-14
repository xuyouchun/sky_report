using System;

namespace Skyever.Report
{
	/// <summary>
	/// 汇总屏和细节屏产生的异常信息
	/// </summary>
	public class ReportException : Exception
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public ReportException() : base()
		{
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="Message">消息</param>
		public ReportException(string Message) : base(Message)
		{
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="Message">消息</param>
		/// <param name="e">引发该异常的异常</param>
		public ReportException(string Message, Exception e) : base(Message, e)
		{
		}
	}
}
