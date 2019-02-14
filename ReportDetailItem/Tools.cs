using System;
using System.Data;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	#region һЩר���¼�ʹ�õ�ί��

	/// <summary>
	/// ReportDetail������ͻ������HTML����֮ǰ����
	/// </summary>
	public delegate void OnBeforeOutput(ReportDetail sender, ref string Content);

	/// <summary>
	/// ReportDetail���ڶ����ݽ��ж�ȡ�����롢���¡�ɾ��֮ǰ��֮�������¼�
	/// </summary>
	public delegate bool OnDetailOperate(ReportDetail sender, Operate operate);

	/// <summary>
	/// ReportDetail���ӿͻ��˴��������¼�
	/// </summary>
	public delegate void OnDetailCommand(ReportDetail sender, string CommandName, string Argument);

	/// <summary>
	/// ReportDetail�������ݵĸ��ֲ���
	/// </summary>
	public enum Operate : byte
	{
		/// <summary>
		/// ��ȡ
		/// </summary>
		Select,
		/// <summary>
		/// ����
		/// </summary>
		Insert,
		/// <summary>
		/// ����
		/// </summary>
		Update,
		/// <summary>
		/// ɾ��
		/// </summary>
		Delete,
		/// <summary>
		/// ����
		/// </summary>
		Reset,
		/// <summary>
		/// ��
		/// </summary>
		None
	}

	#endregion
}
