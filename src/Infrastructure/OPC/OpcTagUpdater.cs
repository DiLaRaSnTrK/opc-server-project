// <copyright file="OpcTagUpdater.cs" company="OPC Server Project">
// Copyright (c) OPC Server Project. All rights reserved.
// </copyright>

namespace Infrastructure.OPC
{
    using Core.Interfaces;
    using Core.Models;

    /// <summary>OPC UA node değerlerini güncelleyen köprü sınıf.</summary>
    public class OpcTagUpdater : ITagUpdater
    {
        private CustomNodeManager? nodeManager;

        /// <summary>NodeManager'ı bağlar (sunucu başladıktan sonra çağrılır).</summary>
        public void SetNodeManager(CustomNodeManager nodeManager)
        {
            this.nodeManager = nodeManager;
        }

        /// <inheritdoc/>
        public void UpdateTag(string tagName, object value)
        {
            this.nodeManager?.UpdateTag(tagName, value);
        }

        /// <summary>Runtime'da yeni tag node'u ekler.</summary>
        public void AddTagNode(Tag tag)
        {
            this.nodeManager?.AddTagNode(tag);
        }

        /// <summary>Tag node'unu kaldırır.</summary>
        public void RemoveTagNode(string tagName)
        {
            this.nodeManager?.RemoveTagNode(tagName);
        }
    }
}
