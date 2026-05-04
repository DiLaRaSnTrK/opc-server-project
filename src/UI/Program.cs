namespace UI;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Splash ekranı
        using (var splash = new SplashForm())
        {
            splash.ShowDialog();
        }

        // ── RBAC: Login formu ──────────────────────────────────────────────
        // Kullanıcı giriş yapmazsa (ESC veya X ile kapatırsa) uygulama başlamaz
        using (var login = new LoginForm())
        {
            if (login.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return; // Giriş yapılmadı — uygulamayı başlatma
            }
        }

        Application.Run(new Main());
    }
}
