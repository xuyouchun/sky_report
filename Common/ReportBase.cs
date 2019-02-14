using System;

namespace Skyever.Report
{

	/// <summary>
	/// ReportCell �� ReportDetail �Ļ���
	/// </summary>
	abstract public class ReportBase : System.Web.UI.UserControl
	{
		/// <summary>
		/// ��̬���캯��������·��
		/// </summary>
		static ReportBase()
		{
			System.Web.HttpServerUtility Server = System.Web.HttpContext.Current.Server;
			if(Option.IsSendScriptFile)
			{
				string tempStr = Server.MapPath(_ScriptPath).Substring(Server.MapPath(@"\").Length).Split('\\')[0];
				_ScriptPath = (string.Compare(tempStr,"Skyever", true)==0?"":@"\"+tempStr) + @"\" + _ScriptPath;
				if(!System.IO.Directory.Exists(Server.MapPath(_ScriptPath)))
				{
					try
					{
						System.IO.Directory.CreateDirectory(Server.MapPath(_ScriptPath));
					}
					catch
					{
						Option.IsSendScriptFile = false;
					}
				}
			}
			else
			{
				try
				{
					foreach(string fileName in System.IO.Directory.GetFiles(Server.MapPath(_ScriptPath), "*.js"))
					{
						System.IO.File.Delete(fileName);
					}
				}
				catch{}
			}
		}

		private static string _ScriptPath = @"Skyever\SkyReport";
		/// <summary>
		/// �ͻ���Js�ļ���·��
		/// </summary>
		internal static string ScriptPath
		{
			get { return _ScriptPath; }
		}

		/// <summary>
		/// ���캯��
		/// </summary>
		public ReportBase()
		{
			
		}

		/// <summary>
		/// ��������
		/// </summary>
		abstract public System.Data.IDbConnection DbConnection{ get; set; }

		/// <summary>
		/// ����ִ�ж���
		/// </summary>
		abstract public System.Data.IDbCommand DbCommand{ get; }

		/// <summary>
		/// ��������ͨ��
		/// </summary>
		abstract public DataAdapter Adapter{ get; set; }

		/// <summary>
		/// ������
		/// </summary>
		abstract public ArgumentGroup Argument{get;set;}

		/// <summary>
		/// ��ͻ��˷�����Ϣ�����ԶԻ������ʽ������
		/// </summary>
		/// <param name="Message"></param>
		public void SendClientMessage(string Message)
		{
			if(Message!=null)	this.Page.RegisterStartupScript(unchecked((TempIndex++)).ToString(), "<script language=javascript>alert('" + Message.Replace("'", @"\'") + "')</script>");
		}
		static uint TempIndex = 0;

		/// <summary>
		/// ��ͻ������֮ǰ
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender (e);

			#region ���ִ���ʱ�������ı���ʾ����ʽ

			this.Page.RegisterClientScriptBlock("ReportExceptionStyle",
				@"<style>
#ReportExceptionStyle A, #ReportExceptionStyle A:visited, #ReportExceptionStyle A:link, #ReportExceptionStyle A:Active { color: blue; padding-top:1px; }
#ReportExceptionStyle A:hover { color: red; }
</style>");

			#endregion
		}

		/// <summary>
		/// �������ִ���ʱ��ʾ�Ĺ�����
		/// </summary>
		/// <param name="Msg"></param>
		/// <returns></returns>
		protected virtual string CreateErrorButton(string Msg)
		{
			return "<div id=ReportExceptionStyle style='font-size:12px;background-color:#ccffff'>" + Msg + "&nbsp;&nbsp;<a href='javascript:history.back()' title='window.history.back()'>�����ء�</a>&nbsp;&nbsp;<a href='javascript:location.reload();' title='window.location.reload()'>��ˢ�¡�</a>&nbsp;&nbsp;<a href='javascript:document.forms[0].submit();' title='document.forms[0].submit();'>�����ԡ�</a>&nbsp;&nbsp;<a href='mailto:skyever_youer@sohu.com' title='Skyever�����Ѵ�'>��Email��</a></div>";
		}

		/// <summary>
		/// ǿ�ƻ�������ϸ������ִ��
		/// </summary>
		public virtual void Execute()
		{
		}

		/// <summary>
		/// �Ƿ��Ѿ�ִ�����
		/// </summary>
		protected bool _IsExecute = false;
		/// <summary>
		/// �Ƿ��Ѿ�ִ�����
		/// </summary>
		public bool IsExecute
		{
			get { return this._IsExecute; }
		}

		/// <summary>
		/// ���ʹ��������ַ���
		/// </summary>
		/// <param name="Content"></param>
		/// <returns></returns>
		internal protected virtual string ExplainArgument(string Content)
		{
			return Content;
		}

		/// <summary>
		/// ���ʹ�������HTML�ַ���
		/// </summary>
		/// <param name="Content"></param>
		/// <returns></returns>
		internal protected virtual string ExplainHtml(string Content)
		{
			return this.ExplainArgument(Content).Replace("this.", this.ClientID + ".");
		}
	}
}
