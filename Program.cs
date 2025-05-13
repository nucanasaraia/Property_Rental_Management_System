using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PropertyRentalManagementSystem.Data;
using PropertyRentalManagementSystem.FluentValidations;
using PropertyRentalManagementSystem.Services.Implementation;
using PropertyRentalManagementSystem.Services.Interfaces;
using PropertyRentalManagementSystem.SMTP;
using System.Text;
using System.Text.Json.Serialization;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<SMTPService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<UserValidator>());

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});



//Token Configuration
builder.Services.AddScoped<IJWTService, JWTService>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "chven",
            ValidAudience = "isini",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ea9386ec9175b2c35d834ce85acdb7ed34d5bff3ec47bb8f5340987d64c5b14ff46285f8f221ca95324847da5e9c1d82e00866532912eb904fdc353748a9db24d7b615750b1b3c39a8ca4bb98f5383dce76876fa947d368a37cba19f45b63430d72eb54eebbd5ecea3ac88a18e755bef08680ae6b80a41483c296c007bc82b61464965d690291a177cbd6f21432486711855c82ac547ea4bee82b0071c2eb37db497704f9814d6a69df52dd6c4de70a554f55921ce93f7e75396f694c679a56a7b40e04c9c93e535de144c6fa3af35920d6bfe530f94302a7f1cd069138b29627d36a6ab985b23976c1f25c401141a4db26826c6a67b031d25a5ccfd9711f6e2")),
            ClockSkew = TimeSpan.Zero,

        };
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OwnerOnly", policy => policy.RequireRole("Owner"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("TenantOnly", policy => policy.RequireRole("Tenant"));
});


builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.
    Add(new JsonStringEnumConverter());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
