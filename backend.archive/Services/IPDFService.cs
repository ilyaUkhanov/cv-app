using backend.Models;

namespace backend.Services
{
    public interface IPDFService
    {
        Task<byte[]> GenerateATSCVAsync(CV cv);
        Task<byte[]> GenerateATSCVWithPhotoAsync(CV cv, byte[] photoData);
    }
}
