using System;

namespace Skyever.Report
{
	/// <summary>
	/// ReportCell��������ʽ������������ʽ�Ļ���
	/// </summary>
	public class ItemStyle : System.Web.UI.HtmlControls.HtmlContainerControl
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		public ItemStyle(){}

		/// <summary>
		/// ���캯��
		/// </summary>
		/// <param name="Pattern">��ʽ</param>
		public ItemStyle(string Pattern)
		{
			this.Pattern = Pattern;
		}

		private string _Pattern = null;
		/// <summary>
		/// ��ʽ
		/// </summary>
		public string Pattern
		{
			get { return this._Pattern;  }
			set { this._Pattern = value; }
		}
	}
}
