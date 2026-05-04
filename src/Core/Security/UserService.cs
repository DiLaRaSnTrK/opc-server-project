// <copyright file="UserService.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

namespace Core.Security
{
    using System.Collections.Generic;
    using BC = BCrypt.Net.BCrypt;

    /// <summary>
    /// Kullanıcı kimlik doğrulama servisi.
    /// Şifreler BCrypt ile hash'lenerek saklanır — plaintext asla tutulmaz.
    /// A02: Cryptographic Failures — BCrypt work factor 12 (OWASP önerisi).
    /// </summary>
    public class UserService
    {
        // BCrypt hash'leri — üretimde DB'den okunur.
        // Hash üretmek için: BCrypt.HashPassword("şifre", workFactor: 12)
        // admin123   → aşağıdaki hash
        // operator123 → aşağıdaki hash
        private static readonly Dictionary<string, (string PasswordHash, UserRole Role)> Users =
            new Dictionary<string, (string, UserRole)>
            {
                {
                    "admin",
                    (BC.HashPassword("admin123", workFactor: 12), UserRole.Admin)
                },
                {
                    "operator",
                    (BC.HashPassword("operator123", workFactor: 12), UserRole.Operator)
                },
            };

        /// <summary>
        /// Kullanıcı adı ve şifreyi doğrular.
        /// BCrypt.Verify ile hash karşılaştırması — timing attack'a karşı güvenli.
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

            if (Users.TryGetValue(key, out var creds))
            {
                // A02: Timing-safe karşılaştırma — == operatörü yerine BCrypt.Verify
                if (BC.Verify(password, creds.PasswordHash))
                {
                    SessionContext.Instance.Login(username.Trim(), creds.Role);
                    return true;
                }
            }

            return false;
        }
    }
}
