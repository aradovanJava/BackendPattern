using Back.Models;
using Microsoft.EntityFrameworkCore;

namespace Back.Db;

public partial class MySqlContext : DbContext
{
    public MySqlContext()
    {
    }

    public MySqlContext(DbContextOptions<MySqlContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Hashtag> Hashtags { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<ImageHashtag> ImageHashtags { get; set; }

    public virtual DbSet<LeIgraci> LeIgracis { get; set; }

    public virtual DbSet<PasswordExpire> PasswordExpires { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SecretAnswer> SecretAnswers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserPlanUsage> UserPlanUsages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;database=patternsecureparadigm;user=root;password=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.3.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Hashtag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("hashtags");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("NAME");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("images");

            entity.HasIndex(e => e.Author, "fk_picture_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Author).HasColumnName("author");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Uploaded)
                .HasColumnType("datetime")
                .HasColumnName("uploaded");
            entity.Property(e => e.Url)
                .HasMaxLength(255)
                .HasColumnName("url");

            entity.HasOne(d => d.AuthorNavigation).WithMany(p => p.Images)
                .HasForeignKey(d => d.Author)
                .HasConstraintName("fk_picture_user");
        });

        modelBuilder.Entity<ImageHashtag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("image_hashtags");

            entity.HasIndex(e => e.HashtagId, "fk_hashtag");

            entity.HasIndex(e => e.ImageId, "fk_image");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.HashtagId).HasColumnName("HASHTAG_ID");
            entity.Property(e => e.ImageId).HasColumnName("IMAGE_ID");

            entity.HasOne(d => d.Hashtag).WithMany(p => p.ImageHashtags)
                .HasForeignKey(d => d.HashtagId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_hashtag");

            entity.HasOne(d => d.Image).WithMany(p => p.ImageHashtags)
                .HasForeignKey(d => d.ImageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_image");
        });

        modelBuilder.Entity<LeIgraci>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("leigraci");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Number).HasColumnName("number");
        });

        modelBuilder.Entity<PasswordExpire>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("password_expire");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ExpirationDate)
                .HasColumnType("datetime")
                .HasColumnName("expiration_date");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("roles");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Consumption).HasColumnName("consumption");
        });

        modelBuilder.Entity<SecretAnswer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("secret_answer");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Answer)
                .HasMaxLength(255)
                .HasColumnName("answer");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Role, "fk_roles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(510)
                .HasColumnName("refresh_token");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(45)
                .HasColumnName("username");

            entity.HasOne(d => d.RoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.Role)
                .HasConstraintName("fk_roles");
        });

        modelBuilder.Entity<UserPlanUsage>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("user_plan_usage");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("userId");
            entity.Property(e => e.Usages).HasColumnName("usages");

            entity.HasOne(d => d.User).WithOne(p => p.UserPlanUsage)
                .HasForeignKey<UserPlanUsage>(d => d.UserId)
                .HasConstraintName("user_plan_usage_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}