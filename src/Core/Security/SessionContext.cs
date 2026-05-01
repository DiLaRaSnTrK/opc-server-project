// <copyright file="SessionContext.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

namespace Core.Security
{
    /// <summary>
    /// Aktif oturumun kullanıcı bilgilerini tutar.
    /// Uygulama genelinde tek örnek (singleton) olarak kullanılır.
    /// </summary>
    public class SessionContext
    {
        private static readonly SessionContext instance = new SessionContext();

        private SessionContext()
        {
        }

        /// <summary>Tekil örnek.</summary>
        public static SessionContext Instance => instance;

        /// <summary>Giriş yapmış kullanıcının adı.</summary>
        public string Username { get; private set; } = string.Empty;

        /// <summary>Kullanıcının rolü.</summary>
        public UserRole Role { get; private set; } = UserRole.Operator;

        /// <summary>Kullanıcı giriş yapmış mı?</summary>
        public bool IsLoggedIn { get; private set; }

        /// <summary>Kullanıcı Admin mi?</summary>
        public bool IsAdmin => this.IsLoggedIn && this.Role == UserRole.Admin;

        /// <summary>Oturum başlatır.</summary>
        public void Login(string username, UserRole role)
        {
            this.Username  = username;
            this.Role      = role;
            this.IsLoggedIn = true;
        }

        /// <summary>Oturumu sonlandırır.</summary>
        public void Logout()
        {
            this.Username   = string.Empty;
            this.Role       = UserRole.Operator;
            this.IsLoggedIn = false;
        }
    }
}
