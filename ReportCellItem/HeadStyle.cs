using System;

namespace Skyever.Report
{
	/// <summary>
	/// ������ʽ
	/// </summary>
	public class HeadStyle : PartStyle
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		public HeadStyle(){}

		bool _IsShowTitle = true;

		/// <summary>
		/// �Ƿ���ʾ����
		/// </summary>
		public bool IsShowTitle
		{
			get { return this._IsShowTitle;  }
			set { this._IsShowTitle = value; }
		}
	}

}
