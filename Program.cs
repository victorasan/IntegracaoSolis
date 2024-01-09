using Hangfire;
using Hangfire.PostgreSql;
using IntegracaoSolis.Command;
using IntegracaoSolis.Handler;
using IntegracaoSolis.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IIntegracaoSolis, IntegracaoSolisHandler>();
builder.Services.AddScoped<IUploadCommand, UploadCommand>();
builder.Services.AddScoped<IDepositoPdf, DepositoPdfHandler>();
builder.Services.AddHangfire(x => x.UsePostgreSqlStorage("Server=localhost;Port=5432;Database=SolisIntegration;User Id=postgres;Password=changeme;"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseHangfireServer();

app.UseHangfireDashboard();

app.Run();


