using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using PasswordBruteForce.Models;
using PasswordBruteForce.Services;

namespace PasswordBruteForce.UI
{
    public class MainForm : Form
    {
        private readonly PasswordGenerator  _gen    = new();
        private readonly BruteForceEngine   _engine = new();
        private readonly PerformanceLogger  _log    = new();

        private string  _hash;
        private bool    _running;
        private DateTime _startTime;

        private Label       _lblPwd, _lblElapsed, _lblPct;
        private TextBox     _txtHash;
        private Button      _btnGen, _btnGo;
        private RadioButton _rdoS, _rdoM;
        private ProgressBar _bar;
        private RichTextBox _rtxResult, _rtxLog;
        private System.Windows.Forms.Timer _timer;

        public MainForm()
        {
            Build();
            Wire();
        }

        // ── Build UI ──────────────────────────────────────────
        void Build()
        {
            Text = "Password Brute Force Cracker";
            Size = new Size(830, 800);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(28, 28, 35);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9f);

            // title
            Add(new Label {
                Text = "Password Brute Force Cracker",
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100,200,255),
                AutoSize = true, Location = new Point(18,12) });

            // password group
            var gP = Group("Password", 15, 50, 795, 135);
            Lbl(gP, "Plain-text:", 10, 28);
            _lblPwd = new Label {
                Text = "(click Generate Password)",
                ForeColor = Color.FromArgb(255,210,60),
                Font = new Font("Consolas",12f,FontStyle.Bold),
                AutoSize=true, Location=new Point(105,25) };
            gP.Controls.Add(_lblPwd);

            Lbl(gP,"SHA256 Hash:",10,62);
            _txtHash = new TextBox {
                ReadOnly=true, Location=new Point(105,59),
                Size=new Size(570,22),
                BackColor=Color.FromArgb(42,42,52),
                ForeColor=Color.Silver,
                BorderStyle=BorderStyle.FixedSingle,
                Font=new Font("Consolas",8f) };
            gP.Controls.Add(_txtHash);

            _btnGen = Btn(gP,"Generate\nPassword",690,25,90,80);
            _btnGen.BackColor = Color.FromArgb(0,120,215);

            Lbl(gP,$"Salt: \"{PasswordHasher.GetSalt()}\"",10,100)
                .ForeColor = Color.FromArgb(110,110,125);

            // attack group
            var gA = Group("Brute Force Attack",15,195,795,185);
            _rdoS = new RadioButton {
                Text="Single-Thread", Checked=true,
                Location=new Point(12,28), AutoSize=true,
                ForeColor=Color.White };
            _rdoM = new RadioButton {
                Text=$"Multi-Thread  (max {BruteForceEngine.MaxThreadCount} threads = CPU cores - 1)",
                Location=new Point(145,28), AutoSize=true,
                ForeColor=Color.White };
            gA.Controls.AddRange(new Control[]{_rdoS,_rdoM});

            Lbl(gA,$"CPU cores: {Environment.ProcessorCount}  |  " +
                   $"Charset: a-z + 0-9  |  Search: length 1 → 6",12,54)
                .ForeColor = Color.FromArgb(120,120,140);

            _btnGo = Btn(gA,"Start Attack",12,80,140,40);
            _btnGo.BackColor = Color.FromArgb(0,160,80);
            _btnGo.Enabled   = false;

            Lbl(gA,"Elapsed:",165,92);
            _lblElapsed = new Label {
                Text="00:00.000",
                Font=new Font("Consolas",13f,FontStyle.Bold),
                ForeColor=Color.FromArgb(100,220,255),
                AutoSize=true, Location=new Point(235,88) };
            gA.Controls.Add(_lblElapsed);

            _bar = new ProgressBar {
                Location=new Point(12,135), Size=new Size(660,22),
                Minimum=0, Maximum=100 };
            _lblPct = new Label {
                Text="0%", AutoSize=true,
                Location=new Point(680,135), ForeColor=Color.White };
            gA.Controls.AddRange(new Control[]{_bar,_lblPct});

            // result group
            var gR = Group("Result",15,390,795,100);
            _rtxResult = RTB(gR, Color.FromArgb(80,255,120));

            // log group
            var gL = Group("Performance Log",15,500,795,250);
            _rtxLog = RTB(gL, Color.FromArgb(190,190,205));

            // elapsed timer
            _timer = new System.Windows.Forms.Timer { Interval=50 };
            _timer.Tick += (_,__) => {
                var e = DateTime.Now - _startTime;
                _lblElapsed.Text =
                    $"{(int)e.TotalMinutes:D2}:{e.Seconds:D2}.{e.Milliseconds:D3}";
            };
        }

        // ── Wire events ───────────────────────────────────────
        void Wire()
        {
            _btnGen.Click += (_,__) =>
            {
                string pwd = _gen.GeneratePassword();
                _hash = PasswordHasher.Hash(pwd);

                _lblPwd.Text  = pwd;
                _txtHash.Text = _hash;
                _rtxResult.Clear();
                _bar.Value    = 0;
                _lblPct.Text  = "0%";
                _lblElapsed.Text = "00:00.000";
                _btnGo.Enabled   = true;

                Log($"Password generated: '{pwd}'  (len={pwd.Length})",
                    Color.FromArgb(180,200,255));

                // Verify hash is self-consistent
                bool ok = PasswordHasher.Verify(pwd, _hash);
                Log($"Hash verify check: {ok}",
                    ok ? Color.FromArgb(80,255,120) : Color.Red);
            };

            _btnGo.Click += async (_,__) =>
            {
                if (_running) { _engine.Stop(); return; }

                bool multi = _rdoM.Checked;
                _running   = true;
                _startTime = DateTime.Now;
                SetUI(true);
                _bar.Value      = 0;
                _rtxResult.Text = "Searching...";
                _timer.Start();

                BruteForceResult res = null;
                string hashSnapshot  = _hash;   // capture before async

                try
                {
                    res = await Task.Run(() =>
                        multi ? _engine.RunMulti(hashSnapshot)
                              : _engine.RunSingle(hashSnapshot));
                }
                finally
                {
                    _timer.Stop();
                    _running = false;
                    SetUI(false);
                }

                if (res != null)
                {
                    _log.Record(res);
                    ShowResult(res);
                    Log(_log.Report(), Color.FromArgb(230,220,150));
                }
            };

            _engine.OnProgress += (done, total) => {
                if (total <= 0) return;
                int pct = (int)Math.Min(100, done * 100 / total);
                UI(() => { _bar.Value=pct; _lblPct.Text=$"{pct}%"; });
            };

            _engine.OnStatus += msg =>
                UI(() => Log(msg, Color.FromArgb(140,190,255)));
        }

        // ── Helpers ───────────────────────────────────────────
        void ShowResult(BruteForceResult r)
        {
            _rtxResult.Clear();
            W(_rtxResult,
              $"Mode : {(r.IsMultiThreaded ? $"Multi ({r.ThreadCount} threads)" : "Single-thread")}\n",
              Color.FromArgb(160,210,255));
            W(_rtxResult,
              r.Found ? $"FOUND: '{r.FoundPassword}'\n" : "NOT FOUND\n",
              r.Found ? Color.FromArgb(80,255,120) : Color.Red);
            W(_rtxResult, $"Time : {r.ElapsedTime.TotalMilliseconds:F0} ms\n", Color.White);
            W(_rtxResult, $"Tries: {r.AttemptCount:N0}\n", Color.White);
            if (r.Found) { _bar.Value=100; _lblPct.Text="Found!"; }
        }

        void SetUI(bool running)
        {
            UI(() => {
                _btnGo.Text       = running ? "Stop Attack" : "Start Attack";
                _btnGo.BackColor  = running
                    ? Color.FromArgb(200,50,50)
                    : Color.FromArgb(0,160,80);
                _btnGen.Enabled   = !running;
                _rdoS.Enabled     = !running;
                _rdoM.Enabled     = !running;
            });
        }

        void Log(string msg, Color c)
        {
            UI(() => {
                W(_rtxLog, $"[{DateTime.Now:HH:mm:ss}] {msg}\n", c);
                _rtxLog.ScrollToCaret();
            });
        }

        void W(RichTextBox r, string t, Color c)
        { r.SelectionStart=r.TextLength; r.SelectionColor=c; r.AppendText(t); }

        void UI(Action a)
        { if (InvokeRequired) Invoke(a); else a(); }

        // ── Factory helpers ───────────────────────────────────
        GroupBox Group(string t, int x, int y, int w, int h)
        {
            var g = new GroupBox {
                Text=t, Location=new Point(x,y), Size=new Size(w,h),
                ForeColor=Color.FromArgb(150,195,255),
                Font=new Font("Segoe UI",9f,FontStyle.Bold) };
            Controls.Add(g); return g;
        }

        Label Lbl(Control p, string t, int x, int y)
        {
            var l = new Label {
                Text=t, AutoSize=true, Location=new Point(x,y),
                ForeColor=Color.FromArgb(175,175,195) };
            p.Controls.Add(l); return l;
        }

        Button Btn(Control p, string t, int x, int y, int w, int h)
        {
            var b = new Button {
                Text=t, Location=new Point(x,y), Size=new Size(w,h),
                FlatStyle=FlatStyle.Flat, ForeColor=Color.White,
                Font=new Font("Segoe UI",9f,FontStyle.Bold),
                Cursor=Cursors.Hand };
            p.Controls.Add(b); return b;
        }

        RichTextBox RTB(Control p, Color c)
        {
            var r = new RichTextBox {
                ReadOnly=true, Location=new Point(10,22),
                Size=new Size(p.Width-22, p.Height-35),
                BackColor=Color.FromArgb(18,22,28), ForeColor=c,
                Font=new Font("Consolas",9f),
                BorderStyle=BorderStyle.None,
                ScrollBars=RichTextBoxScrollBars.Vertical };
            p.Controls.Add(r); return r;
        }

        void Add(Control c) => Controls.Add(c);
    }
}