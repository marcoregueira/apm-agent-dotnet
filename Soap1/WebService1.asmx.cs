﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Windows.Apm.Client.Nlog;

namespace Soap1
{
	/// <summary>
	/// Summary description for WebService1
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	// [System.Web.Script.Services.ScriptService]
	public class WebService1 : System.Web.Services.WebService
	{

		[WebMethod]
		public ClRequest GetPendingOutputMessages(string param)
		{
			Logger.Instance.Debug("Called");
			throw new InsufficientExecutionStackException("**************************************************************************************** ESTA ES UNA EXCEPCIÓN DE MENTIRA ********************************************************************************");
			return new ClRequest();
		}
	}

	public class ClRequest
	{
		public string param1 { get; set; }
	}
}
