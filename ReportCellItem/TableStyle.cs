using System;

namespace Skyever.Report
{
	/// <summary>
	/// 表格样式
	/// </summary>
	public class TableStyle : PartStyle
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public TableStyle(){}

		private int _Border = 1;
		/// <summary>
		/// 表格线的宽度
		/// </summary>
		public int Border
		{
			get { return this._Border;  }
			set { this._Border = value; }
		}

		private int _CellPadding = 1;
		/// <summary>
		/// 表格线与内容之间的空间
		/// </summary>
		public int CellPadding
		{
			get { return this._CellPadding;  }
			set { this._CellPadding = value; }
		}

		private int _CellSpacing = 1;
		/// <summary>
		/// 表格线之间的空间
		/// </summary>
		public int CellSpacing
		{
			get { return this._CellSpacing;  }
			set { this._CellSpacing = value; }
		}

		private string _ClassName = null;
		/// <summary>
		/// 样式名
		/// </summary>
		public string ClassName
		{
			get { return this._ClassName;  }
			set { this._ClassName = value; }
		}

		private string _BorderColor = null;
		/// <summary>
		/// 边框的颜色
		/// </summary>
		public string BorderColor
		{
			get { return this._BorderColor;  }
			set { this._BorderColor = value; }
		}

		private string _Align = null;
		/// <summary>
		/// 表格的对齐方式
		/// </summary>
		public string Align
		{
			get { return this._Align;  }
			set { this._Align = value; }
		}

		private string _Width = null;
		/// <summary>
		/// 表格的宽度
		/// </summary>
		public string Width
		{
			get { return this._Width;  }
			set { this._Width = value; }
		}

		private bool _IsCollapse = false;
		/// <summary>
		/// 是否折叠表格线（默认为不折叠）
		/// </summary>
		public bool IsCollapse
		{
			get { return this._IsCollapse;  }
			set { this._IsCollapse = value; }
		}

		private bool _IsAutoLayout = true;
		/// <summary>
		/// 是否启用自动布局（默认为启用）
		/// </summary>
		public bool IsAutoLayout
		{
			get { return this._IsAutoLayout;  }
			set { this._IsAutoLayout = value; }
		}

		private  bool _IsSelectable = true;
		/// <summary>
		/// 是否允许选择文本
		/// </summary>
		public bool IsSelectable
		{
			get	{ return this._IsSelectable;  }
			set { this._IsSelectable = value; }
		}
	}
}
