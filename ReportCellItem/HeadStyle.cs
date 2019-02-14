using System;

namespace Skyever.Report
{
	/// <summary>
	/// 标题样式
	/// </summary>
	public class HeadStyle : PartStyle
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public HeadStyle(){}

		bool _IsShowTitle = true;

		/// <summary>
		/// 是否显示标题
		/// </summary>
		public bool IsShowTitle
		{
			get { return this._IsShowTitle;  }
			set { this._IsShowTitle = value; }
		}
	}

}
