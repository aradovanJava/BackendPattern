using Back.Db;
using Back.Models;

namespace Back.Utils
{
    public class ImageDownloadProxy
    {
        private Dictionary<int, ImageData> _downloadableImages = new Dictionary<int, ImageData>();

        public int AddImage(ImageData data)
        {
            int id = new Random().Next();
            _downloadableImages.Add(id, data);

            return id;
        }

        public ImageData GetImage(int id)
        {
            DeleteImage(id);
            return _downloadableImages[id];
        }

        private async Task DeleteImage(int id)
        {
            await Task.Delay(10000);
            
            _downloadableImages.Remove(id);
        }
    }
}
