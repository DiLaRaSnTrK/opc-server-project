using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models;

namespace Core.Helpers
{
    public class TagValueParser
    {
        public static object Convert(TagDataType dataType, double rawValue)
        {
            switch (dataType)
            {
                case TagDataType.Bool:
                    return Math.Abs(rawValue - 1) < 0.00001 ? 1 : 0;
                case TagDataType.Int16:
                    return (short)rawValue;
                case TagDataType.UInt16:
                    return (ushort)rawValue;
                case TagDataType.Int32:
                    return (int)rawValue;
                case TagDataType.UInt32:
                    return (uint)rawValue;
                case TagDataType.Float:
                    return (float)rawValue;
                case TagDataType.Double:
                    return rawValue;
                default:
                    return rawValue;
            }
        }
    }
}
