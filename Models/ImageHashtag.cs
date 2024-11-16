namespace Back.Models;

public partial class ImageHashtag
{
    public int Id { get; set; }

    public int? ImageId { get; set; }

    public int? HashtagId { get; set; }

    public virtual Hashtag? Hashtag { get; set; }

    public virtual Image? Image { get; set; }
}
