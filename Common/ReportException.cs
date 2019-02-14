using System;

namespace Skyever.Report
{
	/// <summary>
	/// ��������ϸ�����������쳣��Ϣ
	/// </summary>
	public class ReportException : Exception
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		public ReportException() : base()
		{
		}

		/// <summary>
		/// ���캯��
		/// </summary>
		/// <param name="Message">��Ϣ</param>
		public ReportException(string Message) : base(Message)
		{
		}

		/// <summary>
		/// ���캯��
		/// </summary>
		/// <param name="Message">��Ϣ</param>
		/// <param name="e">�������쳣���쳣</param>
		public ReportException(string Message, Exception e) : base(Message, e)
		{
		}
	}
}
