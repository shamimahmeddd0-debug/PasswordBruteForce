using System;

namespace PasswordBruteForce.Models
{
    public class BruteForceResult
    {
        public bool     Found           { get; set; }
        public string   FoundPassword   { get; set; }
        public long     AttemptCount    { get; set; }
        public TimeSpan ElapsedTime     { get; set; }
        public bool     IsMultiThreaded { get; set; }
        public int      ThreadCount     { get; set; }
    }
}