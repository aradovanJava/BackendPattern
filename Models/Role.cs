using System;
using System.Collections.Generic;

namespace Back.Models;

public partial class Role
{
    public int Id { get; set; }

    public int Consumption { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
