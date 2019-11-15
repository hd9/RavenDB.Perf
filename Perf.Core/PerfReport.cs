using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perf.Core
{
    public class PerfMetric
    {
        public int RunId { get; set; }
        public string Op { get; set; }
        public TimeSpan TotalTime { get; set; }
        public bool Error { get; set; }

        public override string ToString()
        {
            return string.Format(
                "{0}: {1:000.00} s [{2:00000.00000} ms]{3}",
                Op.PadRight(12),
                TotalTime.TotalSeconds,
                TotalTime.TotalMilliseconds,
                (Error ? " [FAILED]" : "")
            );
        }
    }

    public class PerfReport
    {
        public List<PerfMetric> Metrics = new List<PerfMetric>();

        public TimeSpan TotalTime()
        {
            return new TimeSpan(Metrics.Sum(m => m.TotalTime.Ticks));
        }

        public void AddMetric(int runId, string op, TimeSpan totalTime, bool err)
        {
            Metrics.Add(new PerfMetric { RunId = runId, Op = op, TotalTime = totalTime, Error = err});
        }
    }
}
