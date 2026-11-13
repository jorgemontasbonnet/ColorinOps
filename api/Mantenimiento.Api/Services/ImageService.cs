using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace Mantenimiento.Api.Services
{

    public static class ImageService
    {

        const int MAX_DELPHI_WIDTH = 2048;
        public async static Task<byte[]> Resize(byte[] imageBytes, string size)
        {
            int newWidth = size == "xl" ? 1920 : 300;

            using var image = Image.Load(imageBytes);

            if (image.Width <= newWidth) return imageBytes;
            
            image.Mutate(x => x.Resize(newWidth, 0));

            using var outputStream = new MemoryStream() ;
                
            await image.SaveAsJpegAsync(outputStream);
            return outputStream.ToArray();               
            
        }


        public async static Task<byte[]> DelphiSafeImage(byte[] imageBytes)
        {            
            using var image = Image.Load(imageBytes);

            int newWidth = (image.Width <= MAX_DELPHI_WIDTH) ? image.Width : MAX_DELPHI_WIDTH;

            image.Mutate(x => x.AutoOrient().Resize(newWidth, 0));
            ////x.AutoOrient()
            image.Metadata.ExifProfile = null; // Remove EXIF metadata to avoid orientation issues

            using var outputStream = new MemoryStream();

            await image.SaveAsJpegAsync(outputStream);
            return outputStream.ToArray();

        }


    }
}
