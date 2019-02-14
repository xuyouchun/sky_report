using System;

namespace Skyever.Report
{
	/// <summary>
	/// ReportCell：表格各部分的样式
	/// </summary>
	public class PartStyle : ItemStyle
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public PartStyle() {}

		private bool _IsShow = true;
		/// <summary>
		/// 是否显示
		/// </summary>
		public bool IsShow
		{
			get { return this._IsShow;  }
			set { this._IsShow = value; }
		}

		private string _HeadText = null;
		/// <summary>
		/// 小标题
		/// </summary>
		public string HeadText
		{
			get { return this._HeadText;  }
			set { this._HeadText = value; }
		}

		private string _Caption = null;
		/// <summary>
		/// 大标题
		/// </summary>
		public string Caption
		{
			get
			{
				if(this._Caption == null)
				{
					string InnerHtml = this.InnerHtml.Trim();
					if(InnerHtml!="")	this._Caption = InnerHtml;
				}
				return this._Caption;
			}
			set { this._Caption = value; }
		}
	}
}
