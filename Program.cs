using Back.Auth;
using Back.Db;
using Back.Services;
using Back.Utils;
using front.Utils.Logger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<MySqlContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
                      ServerVersion.Parse("8.3.0"))
);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = Jwt.GetSymmetricSecurityKey(),
            ValidIssuer = "mojBekend",
            ValidAudience = "mojFrontend"
        };
    });

string relativeStoragePath = builder.Configuration["ImagePath"];
string parentDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
string storagePath = Path.Combine(parentDirectory, relativeStoragePath);

builder.Services.AddSingleton(new ImageSaver(storagePath, new MySqlContext()));

builder.Services.AddSingleton<UserUpdateProxy>();
builder.Services.AddSingleton<ImageDownloadProxy>();

builder.Services.AddSingleton(new ScheduledRoleUpdateService(
    serviceProvider: builder.Services.BuildServiceProvider(),
    scheduledTime: new TimeSpan(0, 0, 0)
));
builder.Services.AddHostedService(provider => provider.GetService<ScheduledRoleUpdateService>());

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
