namespace BlogProject.Services
{
    public interface IImageService
    {
        Task<byte[]> EncodeImageAsync(IFormFile file);
        Task<byte[]> EncodeImageAsync(string fileName); // This overload is to encode images already stored in project
        string DecodeImage(byte[] data, string type);
        string ContentType(IFormFile file);
        int Size(IFormFile file);
    }
}
