using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class TagValue
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        public string Value { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

}
