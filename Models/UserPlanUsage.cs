using System;
using System.Collections.Generic;

namespace Back.Models;

public partial class UserPlanUsage
{
    public UserPlanUsage()
    {
    }

    public UserPlanUsage(int userId)
    {
        UserId = userId;
        Usages = 0;
    }

    public UserPlanUsage(int userId, int usages, User? user) : this(userId)
    {
        User = user;
        Usages = usages;
    }

    public int UserId { get; set; }

    public int Usages { get; set; }

    public virtual User User { get; set; } = null!;
}
