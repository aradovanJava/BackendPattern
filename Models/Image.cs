namespace Back.Models;

public partial class Image
{
    public int Id { get; set; }

    public int Author { get; set; }

    public string Url { get; set; } = null!;

    public DateTime Uploaded { get; set; }

    public string? Description { get; set; }

    public virtual User AuthorNavigation { get; set; } = null!;

    public virtual ICollection<ImageHashtag> ImageHashtags { get; set; } = new List<ImageHashtag>();
}
