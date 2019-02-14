using System;

namespace Skyever.Report
{
	/// <summary>
	/// �����ʽ
	/// </summary>
	public class TableStyle : PartStyle
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		public TableStyle(){}

		private int _Border = 1;
		/// <summary>
		/// ����ߵĿ��
		/// </summary>
		public int Border
		{
			get { return this._Border;  }
			set { this._Border = value; }
		}

		private int _CellPadding = 1;
		/// <summary>
		/// �����������֮��Ŀռ�
		/// </summary>
		public int CellPadding
		{
			get { return this._CellPadding;  }
			set { this._CellPadding = value; }
		}

		private int _CellSpacing = 1;
		/// <summary>
		/// �����֮��Ŀռ�
		/// </summary>
		public int CellSpacing
		{
			get { return this._CellSpacing;  }
			set { this._CellSpacing = value; }
		}

		private string _ClassName = null;
		/// <summary>
		/// ��ʽ��
		/// </summary>
		public string ClassName
		{
			get { return this._ClassName;  }
			set { this._ClassName = value; }
		}

		private string _BorderColor = null;
		/// <summary>
		/// �߿����ɫ
		/// </summary>
		public string BorderColor
		{
			get { return this._BorderColor;  }
			set { this._BorderColor = value; }
		}

		private string _Align = null;
		/// <summary>
		/// ���Ķ��뷽ʽ
		/// </summary>
		public string Align
		{
			get { return this._Align;  }
			set { this._Align = value; }
		}

		private string _Width = null;
		/// <summary>
		/// ���Ŀ��
		/// </summary>
		public string Width
		{
			get { return this._Width;  }
			set { this._Width = value; }
		}

		private bool _IsCollapse = false;
		/// <summary>
		/// �Ƿ��۵�����ߣ�Ĭ��Ϊ���۵���
		/// </summary>
		public bool IsCollapse
		{
			get { return this._IsCollapse;  }
			set { this._IsCollapse = value; }
		}

		private bool _IsAutoLayout = true;
		/// <summary>
		/// �Ƿ������Զ����֣�Ĭ��Ϊ���ã�
		/// </summary>
		public bool IsAutoLayout
		{
			get { return this._IsAutoLayout;  }
			set { this._IsAutoLayout = value; }
		}

		private  bool _IsSelectable = true;
		/// <summary>
		/// �Ƿ�����ѡ���ı�
		/// </summary>
		public bool IsSelectable
		{
			get	{ return this._IsSelectable;  }
			set { this._IsSelectable = value; }
		}
	}
}
