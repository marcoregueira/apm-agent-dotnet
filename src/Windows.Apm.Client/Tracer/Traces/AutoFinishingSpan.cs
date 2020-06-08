using Elastic.Apm.Api;
using System;

namespace WMS_Infrastructure.Instrumentation
{
	public class AutoFinishingSpan : IDisposable
	{
		private readonly IExecutionSegment span;
		private readonly ITransaction parent;
		private readonly bool m_Disposed = false;

		public Action OnFinish { get; }

		public IExecutionSegment Span => span;

		public AutoFinishingSpan(IExecutionSegment span, ITransaction parent = null, Action onFinish = null)
		{
			this.span = span;
			OnFinish = onFinish;
			this.parent = parent;
		}

		public void Dispose()
		{
			if (m_Disposed) return;
			OnFinish?.Invoke();
			span?.End();
			parent?.End();
		}

		public void CaptureException(Exception exception, string culprit = null, bool isHandled = false, string parentId = null) =>
			span?.CaptureException(exception, culprit, isHandled, parentId);
	}
}

