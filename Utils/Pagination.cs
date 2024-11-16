using Back.Db;
using Back.Models;
using Microsoft.EntityFrameworkCore;

namespace Back.Utils
{
    public class Pagination<T> where T : class
    {
        private readonly MySqlContext _db;

        public Pagination(MySqlContext db)
        {
            _db = db;
        }

        public List<T> GetPage(int page, int pageSize = 10, List<T>? list = null) =>
            list == null
                ? GetDbSet().Skip((page - 1) * pageSize).Take(pageSize).ToList()
                : list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        private DbSet<T> GetDbSet() =>
            typeof(T) switch
            {
                Type t when t == typeof(User) => _db.Users as DbSet<T>,
                Type t when t == typeof(Image) => _db.Images as DbSet<T>,
                _ => throw new Exception($"Unknown entity type: {typeof(T)}")
            };
    }
}
