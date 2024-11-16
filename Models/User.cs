using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models
{
    public class User
    {

        public User(int id, string email, string username, string password, int role, string refreshToken)
        {
            Id = id;
            Email = email;
            Username = username;
            Password = password;
            Role = role;
            RefreshToken = refreshToken;
        }

        public User(int id, string email, string username, string password, int role, string refreshToken, ICollection<Image> images, Role roleNavigation, UserPlanUsage? userPlanUsage) : this(id, email, username, password, role, refreshToken)
        {
            Images = images;
            RoleNavigation = roleNavigation;
            UserPlanUsage = userPlanUsage;
        }

        public int Id { get; set; }

        public string Email { get; set; } = null!;

        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;

        public int Role { get; set; }

        public string RefreshToken { get; set; } = null!;

        public virtual ICollection<Image> Images { get; set; } = new List<Image>();

        public virtual Role RoleNavigation { get; set; } = null!;
        public virtual UserPlanUsage? UserPlanUsage { get; set; }
    }



    public class UserUpdate
    {
        [FromBody]
        public string? Email { get; set; }

        [FromBody]
        public string? Username { get; set; }

        [FromBody]
        public string? Password { get; set; }

        [FromBody]
        public int? Role { get; set; }

        [FromBody]
        public string? RefreshToken { get; set; }
    }

    public class UserBuilder
    {
        private int id;
        private string email;
        private string username;
        private string password;
        private int role;
        private string refreshToken;

        public UserBuilder Id(int id)
        {
            this.id = id;
            return this;
        }

        public UserBuilder Email(string email)
        {
            this.email = email;
            return this;
        }

        public UserBuilder Username(string username)
        {
            this.username = username;
            return this;
        }
        public UserBuilder Password(string password)
        {
            this.password = HashPassword(password);
            return this;
        }

        public UserBuilder Role(int role)
        {
            this.role = role;
            return this;
        }

        public UserBuilder RefreshToken(string refreshToken)
        {
            this.refreshToken = refreshToken;
            return this;
        }

        public User Build()
        {
            return new User(id, email, username, password, role, refreshToken);
        }

        private string HashPassword(string password)
        {
            return password;
        }
    }

}
