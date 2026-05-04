// <copyright file="LoginForm.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

namespace UI
{
    using System;
    using System.Drawing;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Core.Security;
    using Serilog;

    /// <summary>
    /// Kullanıcı giriş formu.
    /// A04: Insecure Design  — başarısız denemeler arası artan gecikme (exponential backoff)
    /// A07: Auth Failures    — MaxAttempts=5, sonrasında uygulama kapanır
    /// A09: Logging          — tüm giriş olayları Serilog ile loglanır
    /// </summary>
    public class LoginForm : Form
    {
        private readonly UserService userService = new UserService();
        private int failedAttempts;
        private const int MaxAttempts = 2;
        private bool isProcessing;

        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblError;
        private Label lblTitle;
        private Label lblUsername;
        private Label lblPassword;
        private Label lblHint;

        /// <summary>Initializes a new instance of the <see cref="LoginForm"/> class.</summary>
        public LoginForm()
        {
            this.InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "OPC Server — Giriş";
            this.Size = new Size(380, 320);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);

            this.lblTitle = new Label
            {
                Text = "DevSecOps OPC Server",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 100),
                Location = new Point(20, 24),
                Size = new Size(330, 30),
                TextAlign = ContentAlignment.MiddleCenter,
            };

            this.lblUsername = new Label
            {
                Text = "Kullanıcı Adı",
                Font = new Font("Segoe UI", 9f),
                Location = new Point(40, 80),
                Size = new Size(300, 20),
            };
            this.txtUsername = new TextBox
            {
                Location = new Point(40, 100),
                Size = new Size(290, 28),
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle,
            };

            this.lblPassword = new Label
            {
                Text = "Şifre",
                Font = new Font("Segoe UI", 9f),
                Location = new Point(40, 140),
                Size = new Size(300, 20),
            };
            this.txtPassword = new TextBox
            {
                Location = new Point(40, 160),
                Size = new Size(290, 28),
                Font = new Font("Segoe UI", 10f),
                PasswordChar = '●',
                BorderStyle = BorderStyle.FixedSingle,
            };

            this.lblError = new Label
            {
                Text = string.Empty,
                ForeColor = Color.FromArgb(180, 30, 30),
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(40, 198),
                Size = new Size(290, 20),
                TextAlign = ContentAlignment.MiddleCenter,
            };

            this.btnLogin = new Button
            {
                Text = "Giriş Yap",
                Location = new Point(40, 222),
                Size = new Size(290, 36),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.FromArgb(30, 100, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
            };
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.Click += this.BtnLogin_Click;

            this.lblHint = new Label
            {
                Text = "admin / operator",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Italic),
                Location = new Point(40, 265),
                Size = new Size(290, 18),
                TextAlign = ContentAlignment.MiddleCenter,
            };

            this.txtPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    this.BtnLogin_Click(s, e);
                }
            };

            this.Controls.AddRange(new Control[]
            {
                this.lblTitle, this.lblUsername, this.txtUsername,
                this.lblPassword, this.txtPassword,
                this.lblError, this.btnLogin, this.lblHint,
            });

            this.AcceptButton = this.btnLogin;
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            // Çift tıklamayı önle
            if (this.isProcessing) return;

            this.lblError.Text = string.Empty;

            // A07: MaxAttempts kontrolü
            if (this.failedAttempts >= MaxAttempts)
            {
                Log.Warning("[AUTH] Maksimum başarısız deneme aşıldı. Uygulama kapatılıyor.");
                MessageBox.Show(
                    "Çok fazla başarısız deneme. Uygulama kapanıyor.",
                    "Güvenlik Kilidi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop);
                Environment.Exit(0);
                return;
            }

            var username = this.txtUsername.Text.Trim();
            var password = this.txtPassword.Text;

            if (this.userService.TryLogin(username, password))
            {
                // A09: Başarılı giriş logla
                Log.Information(
                    "[AUTH] Başarılı giriş: Kullanıcı={Username} Rol={Role}",
                    username,
                    SessionContext.Instance.Role);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                this.failedAttempts++;

                // A09: Başarısız giriş logla (şifre asla loglara yazılmaz)
                Log.Warning(
                    "[AUTH] Başarısız giriş denemesi: Kullanıcı={Username} Deneme={Attempt}/{Max}",
                    username,
                    this.failedAttempts,
                    MaxAttempts);

                // A04: Exponential backoff — her denemede artan gecikme
                // 1. deneme: 500ms, 2: 1000ms, 3: 2000ms, 4: 4000ms
                int delayMs = (int)Math.Pow(2, this.failedAttempts - 1) * 500;
                delayMs = Math.Min(delayMs, 8000); // max 8 saniye

                this.isProcessing = true;
                this.btnLogin.Enabled = false;
                int kalan = MaxAttempts - this.failedAttempts;
                this.lblError.Text = kalan > 0
                    ? $"Hatalı giriş. {delayMs / 1000.0:0.#}s bekleniyor... ({kalan} hak kaldı)"
                    : "Son deneme!";

                await Task.Delay(delayMs);

                this.btnLogin.Enabled = true;
                this.isProcessing = false;
                this.lblError.Text = kalan > 0
                    ? $"Hatalı kullanıcı adı veya şifre. ({kalan} hak kaldı)"
                    : "Son deneme! Sonraki hata uygulamayı kapatır.";
                this.txtPassword.Clear();
                this.txtPassword.Focus();
            }
        }
    }
}
