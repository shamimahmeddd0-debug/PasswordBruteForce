using System;
using System.Windows.Forms;

namespace PasswordBruteForce
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Quick self-test before launching UI
            string pw = "test";
            string h1 = Models.PasswordHasher.Hash(pw);
            string h2 = Models.PasswordHasher.Hash(pw);
            Console.WriteLine("=== HASH TEST ===");
            Console.WriteLine($"Hash1: {h1}");
            Console.WriteLine($"Hash2: {h2}");
            Console.WriteLine($"Same:  {h1 == h2}");
            Console.WriteLine($"Verify:{Models.PasswordHasher.Verify(pw, h1)}");
            Console.WriteLine("=================");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UI.MainForm());
        }
    }
}