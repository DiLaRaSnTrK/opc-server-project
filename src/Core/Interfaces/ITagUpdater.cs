// <copyright file="ITagUpdater.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

namespace Core.Interfaces
{
    public interface ITagUpdater
    {
        void UpdateTag(string tagName, object value);
    }
}
