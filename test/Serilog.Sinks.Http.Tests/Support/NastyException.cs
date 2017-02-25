using System;

namespace Serilog.Sinks.Http.Support
{
	public class NastyException : Exception
	{
		public override string ToString()
		{
			throw new InvalidOperationException("Can't ToString() a NastyException!");
		}
	}
}
