using Back.Models;
using System.Configuration;

namespace Back.Utils
{
    public class ImageToImageRequestConverter
    {
        public static ImageRequest Convert(Image image) =>
            new ImageRequest(
                image.Id,
                image.Author,
                image.Url,
                image.Uploaded,
                new ImageDescription(
                    image.Description,
                    new List<Hashtag>()
                    ),
                new ImageData(
                    GetImageFormat(image.Url.Split(".")[1]),
                    File.ReadAllBytes(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, $"images\\{image.Url}"))
                    )
                );

        private static string GetImageFormat(string format)
        {
            if (format == "jpeg")
                return "image/jpeg";
            return "image/png";
        }
    }
}
