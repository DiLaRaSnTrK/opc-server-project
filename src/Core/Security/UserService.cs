// <copyright file="UserService.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

namespace Core.Security
{
    using System.Collections.Generic;

    /// <summary>
    /// Kullanıcı kimlik doğrulama servisi.
    /// Gerçek projede şifreler hash'lenerek DB'de saklanır.
    /// Bu implementasyonda sabit kullanıcılar tanımlıdır (demo amaçlı).
    /// </summary>
    public class UserService
    {
        // Demo kullanıcılar — üretimde DB'den okunmalı, şifreler bcrypt ile hash'lenmeli
        private static readonly Dictionary<string, (string Password, UserRole Role)> Users =
            new Dictionary<string, (string, UserRole)>
            {
                { "admin",    ("admin123",    UserRole.Admin) },
                { "operator", ("operator123", UserRole.Operator) },
            };

        /// <summary>
        /// Kullanıcı adı ve şifreyi doğrular.
        /// Başarılıysa SessionContext'i günceller ve true döner.
        /// </summary>
        public bool TryLogin(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            var key = username.Trim().ToLowerInvariant();
            if (Users.TryGetValue(key, out var creds) &&
                creds.Password == password)
            {
                SessionContext.Instance.Login(username.Trim(), creds.Role);
                return true;
            }

            return false;
        }
    }
}
