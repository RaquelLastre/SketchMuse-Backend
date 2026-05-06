using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SketchMuse.Application.Interfaces;
using SketchMuse.Infrastructure.Data;
using SketchMuse.Infrastructure.ExternalApis;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

//inserta en el constructor el HttpClient. IConfiguration ya est� a�adido entre otras cosas al poner la linea anterior
builder.Services.AddHttpClient<PixabayService>();
builder.Services.AddHttpClient<UnsplashService>();
builder.Services.AddSingleton<JwtService>();
//Scoped define cuanto vive el objeto (transient: nuevo cada vez, scoped: nuevo en cada peticion HTTP, singleton: unico), es el que se suele usar en APIs
builder.Services.AddScoped<IImagenesService, ImagenesService>();
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
        ServerVersion.AutoDetect(connectionString)
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

//Para que si no existe la bd se cree automaticamente, si hay migraciones pendientes las aplica
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
