using System;
using WinUtil;

namespace TWinService
{
    public class TLocalUserGroupManager
    {
        public static void Test()
        {
            var mgr = new UserManager();
            string user = "DemoUser123";
            string pwd = "P@ssw0rd!";
            var created = mgr.CreateUser(user, pwd, active: true, passwordNeverExpires: true, comment: "Demo");
            Console.WriteLine($"CreateUser: {created}");

            var inUsers = mgr.AddUserToGroup("Users", user);
            Console.WriteLine($"Add to Users: {inUsers}");

            var users = mgr.GetUsers();
            foreach (var u in users)
            {
                Console.WriteLine($"{u.Name} Enabled={u.Enabled} Desc={u.Description}");
            }

            var groups = mgr.GetGroups(withMembers: true);
            foreach (var g in groups)
            {
                Console.WriteLine($"{g.Name} Members={string.Join(", ", g.Members)}");
            }

            var removed = mgr.RemoveUserFromGroup("Users", user);
            Console.WriteLine($"Remove from Users: {removed}");

            var deleted = mgr.DeleteUser(user);
            Console.WriteLine($"DeleteUser: {deleted}");
        }
    }
}
