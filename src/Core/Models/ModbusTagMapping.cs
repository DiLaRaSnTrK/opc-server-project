namespace Core.Models
{
    public class ModbusTagMapping
    {
        public string TagName { get; set; } = default!;
        public ushort Address { get; set; }
        public string DataType { get; set; } = "double";
    }
}
