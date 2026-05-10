using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SketchMuse.Application.Interfaces;
using SketchMuse.Application.Services;
using SketchMuse.Infrastructure.Data;
using SketchMuse.Infrastructure.ExternalApis;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<PexelsService>();
builder.Services.AddHttpClient<WikimediaService>();
builder.Services.AddHttpClient<PixabayService>();
builder.Services.AddHttpClient<UnsplashService>();

builder.Services.AddSingleton<JwtService>();

builder.Services.AddScoped<IImagenesService>(sp => new ImagenesService(
    sp.GetRequiredService<PexelsService>(),
    sp.GetRequiredService<WikimediaService>()
));
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IAlbumesService, AlbumesService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
    );
});


var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];

builder.Services.AddDbContext<MiDbcontext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 0))
    )
);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MiDbcontext>();
    db.Database.Migrate();
}



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
