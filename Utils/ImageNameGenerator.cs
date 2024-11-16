using System.Drawing;

namespace Back.Utils
{
    public class ImageNameGenerator
    {
        private static Random random = new Random();

        public string GenerateRandomName(string format)
            => $"IMG_{DateTime.Now.ToString()}_{random.Next(0, 1000)}.{format.Split("image/")[1]}"
            .Replace(" ", "")
            .Replace(":", "")
            .Replace("/", "");
    }

}
