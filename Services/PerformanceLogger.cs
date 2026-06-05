using System.Collections.Generic;
using System.Text;
using PasswordBruteForce.Models;

namespace PasswordBruteForce.Services
{
    public class PerformanceLogger
    {
        private readonly List<BruteForceResult> _log = new();

        public void Record(BruteForceResult r) => _log.Add(r);

        public string Report()
        {
            BruteForceResult s = null, m = null;
            foreach (var r in _log)
            {
                if (!r.IsMultiThreaded) s = r;
                else                    m = r;
            }

            var sb = new StringBuilder();
            sb.AppendLine("══════════════════════════════════════");
            sb.AppendLine("     PERFORMANCE COMPARISON REPORT    ");
            sb.AppendLine("══════════════════════════════════════");

            void Add(string label, BruteForceResult r)
            {
                sb.AppendLine($"  {label}");
                sb.AppendLine($"    Time     : {r.ElapsedTime.TotalMilliseconds:F2} ms");
                sb.AppendLine($"    Attempts : {r.AttemptCount:N0}");
                sb.AppendLine($"    Found    : {(r.Found ? $"'{r.FoundPassword}'" : "No")}");
            }

            if (s != null) Add("SINGLE-THREAD", s);
            if (m != null) Add($"MULTI-THREAD ({m.ThreadCount} threads)", m);

            if (s != null && m != null && m.ElapsedTime.TotalMilliseconds > 0)
            {
                double sp = s.ElapsedTime.TotalMilliseconds /
                            m.ElapsedTime.TotalMilliseconds;
                sb.AppendLine("──────────────────────────────────────");
                sb.AppendLine($"  Speedup  : {sp:F2}x " +
                    $"({(sp > 1 ? "multi faster" : "single faster")})");
            }
            sb.AppendLine("══════════════════════════════════════");
            return sb.ToString();
        }
    }
}