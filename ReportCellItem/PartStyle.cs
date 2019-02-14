using System;

namespace Skyever.Report
{
	/// <summary>
	/// ReportCell���������ֵ���ʽ
	/// </summary>
	public class PartStyle : ItemStyle
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		public PartStyle() {}

		private bool _IsShow = true;
		/// <summary>
		/// �Ƿ���ʾ
		/// </summary>
		public bool IsShow
		{
			get { return this._IsShow;  }
			set { this._IsShow = value; }
		}

		private string _HeadText = null;
		/// <summary>
		/// С����
		/// </summary>
		public string HeadText
		{
			get { return this._HeadText;  }
			set { this._HeadText = value; }
		}

		private string _Caption = null;
		/// <summary>
		/// �����
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
