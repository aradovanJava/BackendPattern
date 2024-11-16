using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Back.Models;

public partial class PasswordExpire
{
    public int? Id { get; set; }

    public DateTime? ExpirationDate { get; set; }
}
