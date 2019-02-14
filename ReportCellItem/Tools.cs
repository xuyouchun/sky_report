using System;
using System.Data;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	#region һЩר���¼�ʹ�õ�ί��

	/// <summary>
	/// ReportCell���ڶ�ȡ����֮ǰ���������طǿգ������������õĶ�ȡ����
	/// </summary>
	public delegate IDataReader OnReadData(ReportCell sender, IDbCommand DbCommand);

	/// <summary>
	/// ReportCell���ӿͻ��˴��������¼�
	/// </summary>
	public delegate void OnCellCommand(ReportCell sender, string CommandName, string Argument);

	/// <summary>
	/// ReportCell����ÿ����ʾ֮ǰ����
	/// </summary>
	public delegate void OnItemShow(ReportCell sender);

	#endregion

	#region ��ʽ����

	/// <summary>
	/// ����������ʽ����
	/// </summary>
	public class ReportCellOption : Option
	{

		/// <summary>
		/// ��XML�ļ��ж�ȡ����
		/// </summary>
		/// <param name="FileName"></param>
		public override void ReadXml(string FileName)
		{

		}

		/// <summary>
		/// ����ǰ������д��XML�ļ�
		/// </summary>
		/// <param name="FileName"></param>
		public override void WriteXml(string FileName)
		{
			
		}
	}

	#endregion
}
