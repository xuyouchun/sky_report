using System;
using System.Data;

namespace Skyever.Report
{
	/// <summary>
	/// �����ݿ����ӵĵ���
	/// </summary>
	public class DataAdapter
	{
		/// <summary>
		/// ���캯��
		/// </summary>
		public DataAdapter()
		{
		}

		object _DataSource = null;
		/// <summary>
		/// ����Դ��������DataReader��DataTable��DataView��DataSet��
		/// </summary>
		public object DataSource
		{
			get { return this._DataSource;  }
			set { this._DataSource = value; }
		}

		object _DataMember = null;
		/// <summary>
		/// ���ݳ�Ա��������һ��DataSet�еı�Ҳ������һ��DataTable�е������
		/// </summary>
		public object DataMember
		{
			get { return this._DataMember;  }
			set { this._DataMember = value; }
		}

		private System.Data.IDbConnection _DbConnection;

		/// <summary>
		/// ��������
		/// </summary>
		public System.Data.IDbConnection DbConnection
		{
			get { return this._DbConnection;  }
			set { this._DbConnection = value; }
		}

		private System.Data.IDbCommand _SelectCommand = null;
		/// <summary>
		/// ���ڶ�ȡ���ݵ�DbCommand
		/// </summary>
		public System.Data.IDbCommand SelectCommand
		{
			get { return this._SelectCommand;  }
			set { this._SelectCommand = value; }
		}

		private System.Data.IDbCommand _InsertCommand = null;
		/// <summary>
		/// ���������ݿ��в������ݵ�DbCommand
		/// </summary>
		public System.Data.IDbCommand InsertCommand
		{
			get { return this._InsertCommand;  }
			set { this._InsertCommand = value; }
		}

		private System.Data.IDbCommand _DeleteCommand = null;
		/// <summary>
		/// ���ڴ����ݿ���ɾ�����ݵ�DbCommand
		/// </summary>
		public System.Data.IDbCommand DeleteCommand
		{
			get { return this._DeleteCommand;  }
			set { this._DeleteCommand = value; }
		}

		private System.Data.IDbCommand _UpdateCommand = null;
		/// <summary>
		/// ���ڸ������ݵ�DbCommand
		/// </summary>
		public System.Data.IDbCommand UpdateCommand
		{
			get { return this._UpdateCommand;  }
			set { this._UpdateCommand = value; }
		}
	}
}
