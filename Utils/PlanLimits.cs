using Back.Db;

namespace Back.Utils
{
    public class PlanLimits
    {
        private readonly List<int> _limits;

        public PlanLimits(MySqlContext db)
        {
            _limits = db.Roles.Select(x => x.Consumption).ToList();
        }

        public bool CheckIfUnderLimit(int role, int userConsumption) => userConsumption < _limits[role - 1];
    }
}
