namespace Elastic.Apm.Logging
{
	public interface IFinishedMonitor
	{
		void WaitForFinished(int testSecondsInterval);
	}
}
