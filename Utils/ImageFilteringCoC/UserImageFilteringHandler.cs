using Back.Db;
using Back.Models;

namespace Back.Utils.ImageFilteringCoC
{
    public class UserImageFilteringHandler : BaseImageFilteringHandler
    {
        private readonly MySqlContext _db;
        public UserImageFilteringHandler(BaseImageFilteringHandler next, MySqlContext db) : base(next) => _db = db;

        public UserImageFilteringHandler() { }

        public override List<Image> Handle(List<Image> filtered) =>
            _requests.User != null
                ? HandleNext(filtered.Where(x => _db.Users.Find(x.Author).Username == _requests.User).ToList())
                : base.HandleNext(filtered);
    }
}
