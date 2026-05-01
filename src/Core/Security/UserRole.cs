// <copyright file="UserRole.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

namespace Core.Security
{
    /// <summary>Sistemdeki kullanıcı rolleri.</summary>
    public enum UserRole
    {
        /// <summary>Tüm işlemleri yapabilir: ekle, sil, güncelle, izle.</summary>
        Admin,

        /// <summary>Sadece canlı veri izleyebilir. Yapılandırma yapamaz.</summary>
        Operator,
    }
}
