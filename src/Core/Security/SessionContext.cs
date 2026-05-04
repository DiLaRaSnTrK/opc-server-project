// <copyright file="SessionContext.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

namespace Core.Security
{
    using System;

    /// <summary>
    /// Aktif oturumun kullanıcı bilgilerini tutar.
    /// A01: Broken Access Control — login sonrası rol dışarıdan değiştirilemez.
    /// Role salt okunur bir snapshot olarak saklanır.
    /// </summary>
    public sealed class SessionContext
    {
        private static readonly SessionContext instance = new SessionContext();
        private readonly object lockObj = new object();

        // Login snapshot — değiştirilemez kopya
        private string username = string.Empty;
        private UserRole role = UserRole.Operator;
        private bool isLoggedIn;
        private DateTime loginTime;

        private SessionContext()
        {
        }

        /// <summary>Tekil örnek.</summary>
        public static SessionContext Instance => instance;

        /// <summary>Giriş yapmış kullanıcının adı.</summary>
        public string Username
        {
            get { lock (this.lockObj) { return this.username; } }
        }

        /// <summary>
        /// Kullanıcının rolü — login sonrası salt okunur.
        /// Reflection ile değiştirilmeye karşı private setter yok,
        /// backing field doğrudan erişilemez.
        /// </summary>
        public UserRole Role
        {
            get { lock (this.lockObj) { return this.role; } }
        }

        /// <summary>Kullanıcı giriş yapmış mı?</summary>
        public bool IsLoggedIn
        {
            get { lock (this.lockObj) { return this.isLoggedIn; } }
        }

        /// <summary>Kullanıcı Admin mi? — çift kontrol: hem giriş hem rol.</summary>
        public bool IsAdmin
        {
            get { lock (this.lockObj) { return this.isLoggedIn && this.role == UserRole.Admin; } }
        }

        /// <summary>Oturum süresi.</summary>
        public TimeSpan SessionDuration
        {
            get { lock (this.lockObj) { return this.isLoggedIn ? DateTime.UtcNow - this.loginTime : TimeSpan.Zero; } }
        }

        /// <summary>Oturum başlatır — sadece UserService çağırmalı.</summary>
        internal void Login(string userName, UserRole userRole)
        {
            lock (this.lockObj)
            {
                this.username = userName;
                this.role = userRole;
                this.isLoggedIn = true;
                this.loginTime = DateTime.UtcNow;
            }
        }

        /// <summary>Oturumu sonlandırır.</summary>
        public void Logout()
        {
            lock (this.lockObj)
            {
                this.username = string.Empty;
                this.role = UserRole.Operator;
                this.isLoggedIn = false;
                this.loginTime = DateTime.MinValue;
            }
        }
    }
}
