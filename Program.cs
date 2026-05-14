using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SketchMuse.Application.Interfaces;
using SketchMuse.Application.Mappings;
using SketchMuse.Application.Services;
using SketchMuse.Infrastructure.Data;
using SketchMuse.Infrastructure.ExternalApis;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddSingleton<JwtService>(); //se crea aqui para no generar un jwt por peticion

builder.Services.AddHttpClient<PexelsService>(); //se inyecta aqui el httpclient para que se reutilice y no se creen muchos sockets, en vez de en la clase. Ademas lo guarda en services para poder usarlo en el servicio de imagenes
builder.Services.AddHttpClient<PixabayService>();

builder.Services.AddScoped<IImagenesService>(sp => new ImagenesService(
    sp.GetRequiredService<PexelsService>(),
    sp.GetRequiredService<PixabayService>()
));

builder.Services.AddScoped<IUsuarioService, UsuarioService>(); //registro estandar para interfaces
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
        options.TokenValidationParameters = new TokenValidationParameters //todo lo que tiene que validar para aceptar un token
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

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
