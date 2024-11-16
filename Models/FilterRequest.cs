namespace Back.Models
{
    public class FilterRequest
    {
        public FilterRequest(string user, string hashtag, string date)
        {
            User = user;
            Hashtag = hashtag;
            Date = date;
        }

        public string? User { get; set; }
        public string? Hashtag { get; set; }
        public string? Date { get; set; }
    }
}
