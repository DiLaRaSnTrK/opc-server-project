// <copyright file="OpcNodeMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Infrastructure.OPC
{
    using Core.Models;
    public class OpcNodeMapper
    {
        public void CreateNodes(List<Channel> channels)
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

        private void CreateChannelNode(Channel channel)
        {
            Console.WriteLine($"OPC Channel Node Created: {channel.Name}");
        }

        private void CreateDeviceNode(Device device)
        {
            Console.WriteLine($"OPC Device Node Created: {device.Name}");
        }

        private void CreateTagNode(Tag tag)
        {
            Console.WriteLine($"OPC Tag Node Created: {tag.Name}");
        }
    }
}