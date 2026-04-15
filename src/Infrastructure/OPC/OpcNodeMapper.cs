// Copyright (c) OPC Server Project. All rights reserved.

namespace Infrastructure.OPC
{
    using System;
    using System.Collections.Generic;
    using Core.Models;

    /// <summary>OPC node oluşturma yardımcısı.</summary>
    public class OpcNodeMapper
    {
        /// <summary>Kanal hiyerarşisinden node'lar oluşturur.</summary>
        public static void CreateNodes(List<Channel> channels)
        {
            foreach (var channel in channels)
            {
                CreateChannelNode(channel);
                foreach (var device in channel.Devices)
                {
                    CreateDeviceNode(device);
                    foreach (var tag in device.Tags)
                    {
                        CreateTagNode(tag);
                    }
                }
            }
        }

        private static void CreateChannelNode(Channel channel)
        {
            Console.WriteLine($"OPC Channel Node Created: {channel.Name}");
        }

        private static void CreateDeviceNode(Device device)
        {
            Console.WriteLine($"OPC Device Node Created: {device.Name}");
        }

        private static void CreateTagNode(Tag tag)
        {
            Console.WriteLine($"OPC Tag Node Created: {tag.Name}");
        }
    }
}
