using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PasswordBruteForce.Models;

namespace PasswordBruteForce.Services
{
    public class BruteForceEngine
    {
        private readonly CombinationGenerator _gen = new();
        private CancellationTokenSource _cts = new();

        public static int MaxThreadCount =>
            Math.Max(1, Environment.ProcessorCount - 1);

        public event Action<long, long> OnProgress;
        public event Action<string>     OnStatus;

        public void Stop() => _cts.Cancel();

        // ── SINGLE THREAD ─────────────────────────────────────
        public BruteForceResult RunSingle(string targetHash)
        {
            _cts = new CancellationTokenSource();
            var sw       = Stopwatch.StartNew();
            long tries   = 0;
            long total   = _gen.TotalCombinations();
            string found = null;

            OnStatus?.Invoke("Single-thread started...");

            for (int len = 1; len <= _gen.MaxLength && found == null; len++)
            {
                foreach (string cand in _gen.GenerateForLength(len))
                {
                    if (_cts.IsCancellationRequested) goto Done;
                    tries++;

                    if (PasswordHasher.Verify(cand, targetHash))
                    { found = cand; goto Done; }

                    if (tries % 5000 == 0)
                        OnProgress?.Invoke(tries, total);
                }
            }
            Done:
            sw.Stop();
            OnProgress?.Invoke(tries, total);
            return Make(found, tries, sw.Elapsed, false, 1);
        }

        // ── MULTI THREAD ──────────────────────────────────────
        public BruteForceResult RunMulti(string targetHash)
        {
            _cts = new CancellationTokenSource();
            var sw         = Stopwatch.StartNew();
            int tc         = MaxThreadCount;
            long tries     = 0;
            long total     = _gen.TotalCombinations();
            string found   = null;
            object lk      = new();

            OnStatus?.Invoke($"Multi-thread started ({tc} threads)...");

            var tasks = new Task[tc];
            for (int t = 0; t < tc; t++)
            {
                int me = t;
                tasks[t] = Task.Run(() =>
                {
                    for (int len = 1; len <= _gen.MaxLength; len++)
                    {
                        if (_cts.IsCancellationRequested) return;
                        foreach (string cand in _gen.GenerateSlice(len, me, tc))
                        {
                            if (_cts.IsCancellationRequested) return;
                            long n = Interlocked.Increment(ref tries);

                            if (PasswordHasher.Verify(cand, targetHash))
                            {
                                lock (lk)
                                { if (found == null) found = cand; }
                                _cts.Cancel();
                                return;
                            }
                            if (n % 5000 == 0)
                                OnProgress?.Invoke(n, total);
                        }
                    }
                });
            }

            try { Task.WaitAll(tasks); } catch (AggregateException) { }

            sw.Stop();
            OnProgress?.Invoke(tries, total);
            return Make(found, tries, sw.Elapsed, true, tc);
        }

        private static BruteForceResult Make(
            string found, long tries,
            TimeSpan elapsed, bool multi, int tc) => new()
        {
            Found           = found != null,
            FoundPassword   = found,
            AttemptCount    = tries,
            ElapsedTime     = elapsed,
            IsMultiThreaded = multi,
            ThreadCount     = tc
        };
    }
}