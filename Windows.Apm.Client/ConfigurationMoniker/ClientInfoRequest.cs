﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Windows.Metrics.Ingest.Dto
{
	public class ClientInfoRequest
	{
		public string Client { get; set; }
		public string App { get; set; }
	}

	public class ClientInfoDetails
	{
		public string Client { get; set; }
		public string App { get; set; }

		public Dictionary<string, string> Labels = new Dictionary<string, string>();
	}
}
