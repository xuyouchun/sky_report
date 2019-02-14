using System;
using System.Data;

namespace Skyever.Report
{
	/// <summary>
	/// 与数据库连接的导线
	/// </summary>
	public class DataAdapter
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		public DataAdapter()
		{
		}

		object _DataSource = null;
		/// <summary>
		/// 数据源（可以是DataReader、DataTable、DataView和DataSet）
		/// </summary>
		public object DataSource
		{
			get { return this._DataSource;  }
			set { this._DataSource = value; }
		}

		object _DataMember = null;
		/// <summary>
		/// 数据成员（可以是一个DataSet中的表，也可以是一个DataTable中的外键）
		/// </summary>
		public object DataMember
		{
			get { return this._DataMember;  }
			set { this._DataMember = value; }
		}

		private System.Data.IDbConnection _DbConnection;

		/// <summary>
		/// 数据链接
		/// </summary>
		public System.Data.IDbConnection DbConnection
		{
			get { return this._DbConnection;  }
			set { this._DbConnection = value; }
		}

		private System.Data.IDbCommand _SelectCommand = null;
		/// <summary>
		/// 用于读取数据的DbCommand
		/// </summary>
		public System.Data.IDbCommand SelectCommand
		{
			get { return this._SelectCommand;  }
			set { this._SelectCommand = value; }
		}

		private System.Data.IDbCommand _InsertCommand = null;
		/// <summary>
		/// 用于向数据库中插入数据的DbCommand
		/// </summary>
		public System.Data.IDbCommand InsertCommand
		{
			get { return this._InsertCommand;  }
			set { this._InsertCommand = value; }
		}

		private System.Data.IDbCommand _DeleteCommand = null;
		/// <summary>
		/// 用于从数据库中删除数据的DbCommand
		/// </summary>
		public System.Data.IDbCommand DeleteCommand
		{
			get { return this._DeleteCommand;  }
			set { this._DeleteCommand = value; }
		}

		private System.Data.IDbCommand _UpdateCommand = null;
		/// <summary>
		/// 用于更新数据的DbCommand
		/// </summary>
		public System.Data.IDbCommand UpdateCommand
		{
			get { return this._UpdateCommand;  }
			set { this._UpdateCommand = value; }
		}
	}
}
