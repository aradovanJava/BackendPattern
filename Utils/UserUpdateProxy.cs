using Back.Db;
using Back.Models;
using front.Utils.Logger;

namespace Back.Utils
{

    public class UserUpdateProxy(IServiceProvider serviceProvider)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private List<UserRoleUpdate> _pendingRoleUpdates = new();

        public void RequestRoleUpdate(int userId, int newRole)
        {
            try
            {
                _pendingRoleUpdates = UserRoleUpdateSerializer.ReadFromFile();
                if (_pendingRoleUpdates.Where(x => x.Id == userId).Count() > 0)
                    _pendingRoleUpdates.Where(x => x.Id == userId).First().Role = newRole;
                else
                    _pendingRoleUpdates.Add(new UserRoleUpdate(userId, newRole));
                UserRoleUpdateSerializer.WriteToFile(_pendingRoleUpdates);

            }
            catch (Exception)
            {
                _pendingRoleUpdates.Add(new UserRoleUpdate(userId, newRole));
                UserRoleUpdateSerializer.WriteToFile(_pendingRoleUpdates);
            }
            _pendingRoleUpdates = new();
        }

        public async Task ApplyPendingRoleUpdatesAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                _pendingRoleUpdates = UserRoleUpdateSerializer.ReadFromFile();
                var db = scope.ServiceProvider.GetRequiredService<MySqlContext>();
                _pendingRoleUpdates.Where(x => db.Users.Find(x.Id) != null).ToList().ForEach(x => ChangeRole(x, db));
                _pendingRoleUpdates = new();

                await db.SaveChangesAsync();
            }
        }

        private void ChangeRole(UserRoleUpdate userRoleUpdate, MySqlContext db)
        {
            db.Users.Find(userRoleUpdate.Id).Role = userRoleUpdate.Role;
            Logger.Instance.Information($"Updated user role to: {userRoleUpdate.Role}", userRoleUpdate.Id);
        }
    }
}
