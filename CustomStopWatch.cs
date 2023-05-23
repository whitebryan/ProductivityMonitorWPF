using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductivityMonitorWPF
{
	public class CustomStopWatch : Stopwatch
	{
		public TimeSpan Offset { get; private set; }

		public CustomStopWatch(int offset)
		{
			TimeSpan newSpan = TimeSpan.FromMilliseconds(offset);
			Offset = newSpan;
		}

		public long ElapsedMiliseconds { get { return base.ElapsedMilliseconds + (long)Offset.TotalMilliseconds; } }
	}
}
