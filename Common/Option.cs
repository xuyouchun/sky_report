using System;

namespace Skyever.Report
{
	/// <summary>
	/// ReportCell��ReportDetail��ѡ��
	/// </summary>
	abstract public class Option
	{
		private static bool _IsSendScriptFile = true;
		/// <summary>
		/// �Ƿ���ͻ��˷���Js�ļ�����Ϊ�ͻ��˲�����Js�ļ��Ƚϴ��������Խ�Լ����������
		/// </summary>
		public static bool IsSendScriptFile
		{
			get { return _IsSendScriptFile;  }
			set { _IsSendScriptFile = value; }
		}

		/// <summary>
		/// ��XML�ļ��ж�ȡ����
		/// </summary>
		/// <param name="FileName"></param>
		public abstract void ReadXml(string FileName);

		/// <summary>
		/// ����ǰ������д��XML�ļ�
		/// </summary>
		/// <param name="FileName"></param>
		public abstract void WriteXml(string FileName);
	}

}
