using System;

namespace Skyever.Report
{
	/// <summary>
	/// ReportCell和ReportDetail的选项
	/// </summary>
	abstract public class Option
	{
		private static bool _IsSendScriptFile = true;
		/// <summary>
		/// 是否向客户端发送Js文件（因为客户端操作的Js文件比较大，这样可以节约网络流量）
		/// </summary>
		public static bool IsSendScriptFile
		{
			get { return _IsSendScriptFile;  }
			set { _IsSendScriptFile = value; }
		}

		/// <summary>
		/// 从XML文件中读取设置
		/// </summary>
		/// <param name="FileName"></param>
		public abstract void ReadXml(string FileName);

		/// <summary>
		/// 将当前的设置写入XML文件
		/// </summary>
		/// <param name="FileName"></param>
		public abstract void WriteXml(string FileName);
	}

}
