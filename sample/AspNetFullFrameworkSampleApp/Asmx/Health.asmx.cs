﻿// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using System.Web.Services;
using Elastic.Apm;

namespace AspNetFullFrameworkSampleApp.Asmx
{
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	public class Health : WebService
	{
		[WebMethod]

		public string Ping()
		{

			var transaction = Agent.Tracer.StartTransaction("ddd", "mine");

			Agent.Tracer.CaptureTransaction(nameof(Ping), "ping", () =>
			 {
				 System.Threading.Thread.Sleep(1000);
			 });
			return "Ok";
		}
	}
}
