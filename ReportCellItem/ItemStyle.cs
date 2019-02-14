using System;

namespace Skyever.Report
{
	/// <summary>
	/// ReportCell：基本样式，用于其它样式的基类
	/// </summary>
	public class ItemStyle : System.Web.UI.HtmlControls.HtmlContainerControl
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public ItemStyle(){}

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="Pattern">样式</param>
		public ItemStyle(string Pattern)
		{
			this.Pattern = Pattern;
		}

		private string _Pattern = null;
		/// <summary>
		/// 样式
		/// </summary>
		public string Pattern
		{
			get { return this._Pattern;  }
			set { this._Pattern = value; }
		}
	}
}
