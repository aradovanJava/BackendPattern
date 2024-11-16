namespace Back.Models
{
    public class ImageRequest
    {
        public ImageRequest() { }

        public ImageRequest(int? id, int author, string? url, DateTime? uploaded, ImageDescription? description, ImageData imageData)
        {
            Id = id;
            Author = author;
            Url = url;
            Uploaded = uploaded;
            Description = description;
            ImageData = imageData;
        }

        public int? Id { get; set; }

        public int Author { get; set; }

        public string? Url { get; set; }

        public DateTime? Uploaded { get; set; }

        public ImageDescription? Description { get; set; }

        public ImageData ImageData { get; set; }
    }

    public class ImageRequestBuilder
    {
        private int? _id;
        private int _author;
        private string? _url;
        private DateTime? _uploaded;
        private ImageDescription? _description;
        private ImageData _imageData = null!;

        public ImageRequestBuilder SetId(int id)
        {
            _id = id;
            return this;
        }

        public ImageRequestBuilder SetAuthor(int author)
        {
            _author = author;
            return this;
        }

        public ImageRequestBuilder SetUrl(string url)
        {
            _url = url;
            return this;
        }

        public ImageRequestBuilder SetUploaded(DateTime uploaded)
        {
            _uploaded = uploaded;
            return this;
        }

        public ImageRequestBuilder SetDescription(ImageDescription? description)
        {
            _description = description;
            return this;
        }

        public ImageRequestBuilder SetImageData(ImageData imageData)
        {
            _imageData = imageData;
            return this;
        }

        public ImageRequest Build()
        {
            return new ImageRequest(_id, _author, _url, _uploaded, _description, _imageData);
        }
    }
}
