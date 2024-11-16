namespace Back.Models
{
    public class ImageData
    {
        public ImageData(string format, byte[] content)
        {
            Format = format;
            Content = content;
        }

        public string Format { get; set; }
        public byte[] Content { get; set; }

        public string DisplayImage()
        {
            return $"data:{Format};base64,{Convert.ToBase64String(Content.ToArray())}";
        }
    }
}
