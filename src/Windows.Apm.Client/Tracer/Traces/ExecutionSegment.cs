using Elastic.Apm.Api;
using System.Collections.Generic;

namespace WMS_Infrastructure.Instrumentation
{
	public class ExecutionSegment
	{
		public ExecutionSegment(ITransaction transaction) => CurrentTransaction = transaction;

		public bool ImplicitSpan { get; set; }
		public Stack<ISpan> Spans { get; } = new Stack<ISpan>();
		public ITransaction CurrentTransaction { get; }
	}
}

