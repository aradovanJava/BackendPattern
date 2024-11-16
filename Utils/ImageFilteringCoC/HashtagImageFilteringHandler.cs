using Back.Db;
using Back.Models;

namespace Back.Utils.ImageFilteringCoC
{
    public class HashtagImageFilteringHandler : BaseImageFilteringHandler
    {
        private MySqlContext _db;

        public HashtagImageFilteringHandler(BaseImageFilteringHandler next) : base(next) { }

        public HashtagImageFilteringHandler() { }

        public override List<Image> Handle(List<Image> filtered) =>
            _requests.Hashtag != null
            ? HandleNext(filtered.Where(x => x.Description.Contains(_requests.Hashtag)).ToList())
            : base.HandleNext(filtered);
    }
}
