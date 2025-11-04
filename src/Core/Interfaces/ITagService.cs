using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;

namespace Core.Interfaces
{
    public interface ITagService
    {
        Task<IEnumerable<Tag>> GetByDeviceIdAsync(int deviceId);
        Task UpdateValueAsync(int tagId, object value);
    }
}
