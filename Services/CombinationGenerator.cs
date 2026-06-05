using System.Collections.Generic;
using PasswordBruteForce.Models;

namespace PasswordBruteForce.Services
{
    public class CombinationGenerator
    {
        private readonly string _charset = PasswordGenerator.CharacterSet;
        public int MaxLength { get; }

        public CombinationGenerator(int maxLength = 6)
        {
            MaxLength = maxLength;
        }

        public IEnumerable<string> GenerateForLength(int length)
        {
            int b = _charset.Length;
            int[] idx = new int[length];

            while (true)
            {
                char[] chars = new char[length];
                for (int i = 0; i < length; i++)
                    chars[i] = _charset[idx[i]];
                yield return new string(chars);

                // increment like an odometer
                int pos = length - 1;
                while (pos >= 0)
                {
                    idx[pos]++;
                    if (idx[pos] < b) break;
                    idx[pos] = 0;
                    pos--;
                }
                if (pos < 0) yield break;
            }
        }

        public IEnumerable<string> GenerateSlice(
            int length, int threadIndex, int threadCount)
        {
            long n = 0;
            foreach (string s in GenerateForLength(length))
            {
                if (n % threadCount == threadIndex)
                    yield return s;
                n++;
            }
        }

        public long TotalCombinations()
        {
            long total = 0;
            for (int len = 1; len <= MaxLength; len++)
            {
                long c = 1;
                for (int i = 0; i < len; i++) c *= _charset.Length;
                total += c;
            }
            return total;
        }
    }
}