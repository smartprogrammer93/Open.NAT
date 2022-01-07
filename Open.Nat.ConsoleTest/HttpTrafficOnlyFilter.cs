using System.Diagnostics;

namespace Open.Nat.ConsoleTest;

public class HttpTrafficOnlyFilter : TraceFilter
{
	public override bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1, object[] data)
	{
		if (source == "System.Net" && eventType == TraceEventType.Verbose)
		{
			return formatOrMessage.Contains("<<") || formatOrMessage.Contains("//");
		}
		if (source == "System.Net" && eventType == TraceEventType.Information)
		{
			return formatOrMessage.Contains("Request:")
				|| formatOrMessage.Contains("Sending headers")
				||formatOrMessage.Contains("Received status line:")
				|| formatOrMessage.Contains("Received headers");
		}
		return true;
	}
}
