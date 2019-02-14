using System;

namespace Skyever.Report
{

	/// <summary>
	/// ReportCell 和 ReportDetail 的基类
	/// </summary>
	abstract public class ReportBase : System.Web.UI.UserControl
	{
		/// <summary>
		/// 静态构造函数，创建路径
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
		/// 客户端Js文件的路径
		/// </summary>
		internal static string ScriptPath
		{
			get { return _ScriptPath; }
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		public ReportBase()
		{
			
		}

		/// <summary>
		/// 数据链接
		/// </summary>
		abstract public System.Data.IDbConnection DbConnection{ get; set; }

		/// <summary>
		/// 命令执行对象
		/// </summary>
		abstract public System.Data.IDbCommand DbCommand{ get; }

		/// <summary>
		/// 数据链接通道
		/// </summary>
		abstract public DataAdapter Adapter{ get; set; }

		/// <summary>
		/// 参数组
		/// </summary>
		abstract public ArgumentGroup Argument{get;set;}

		/// <summary>
		/// 向客户端发送信息，将以对话框的形式弹出来
		/// </summary>
		/// <param name="Message"></param>
		public void SendClientMessage(string Message)
		{
			if(Message!=null)	this.Page.RegisterStartupScript(unchecked((TempIndex++)).ToString(), "<script language=javascript>alert('" + Message.Replace("'", @"\'") + "')</script>");
		}
		static uint TempIndex = 0;

		/// <summary>
		/// 向客户端输出之前
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender (e);

			#region 出现错误时，错误文本显示的样式

			this.Page.RegisterClientScriptBlock("ReportExceptionStyle",
				@"<style>
#ReportExceptionStyle A, #ReportExceptionStyle A:visited, #ReportExceptionStyle A:link, #ReportExceptionStyle A:Active { color: blue; padding-top:1px; }
#ReportExceptionStyle A:hover { color: red; }
</style>");

			#endregion
		}

		/// <summary>
		/// 创建出现错误时显示的工具条
		/// </summary>
		/// <param name="Msg"></param>
		/// <returns></returns>
		protected virtual string CreateErrorButton(string Msg)
		{
			return "<div id=ReportExceptionStyle style='font-size:12px;background-color:#ccffff'>" + Msg + "&nbsp;&nbsp;<a href='javascript:history.back()' title='window.history.back()'>【返回】</a>&nbsp;&nbsp;<a href='javascript:location.reload();' title='window.location.reload()'>【刷新】</a>&nbsp;&nbsp;<a href='javascript:document.forms[0].submit();' title='document.forms[0].submit();'>【重试】</a>&nbsp;&nbsp;<a href='mailto:skyever_youer@sohu.com' title='Skyever：徐友春'>【Email】</a></div>";
		}

		/// <summary>
		/// 强制汇总屏或细节屏的执行
		/// </summary>
		public virtual void Execute()
		{
		}

		/// <summary>
		/// 是否已经执行完毕
		/// </summary>
		protected bool _IsExecute = false;
		/// <summary>
		/// 是否已经执行完毕
		/// </summary>
		public bool IsExecute
		{
			get { return this._IsExecute; }
		}

		/// <summary>
		/// 解释带参数的字符串
		/// </summary>
		/// <param name="Content"></param>
		/// <returns></returns>
		internal protected virtual string ExplainArgument(string Content)
		{
			return Content;
		}

		/// <summary>
		/// 解释带参数的HTML字符串
		/// </summary>
		/// <param name="Content"></param>
		/// <returns></returns>
		internal protected virtual string ExplainHtml(string Content)
		{
			return this.ExplainArgument(Content).Replace("this.", this.ClientID + ".");
		}
	}
}
