using Hangfire;
using Hangfire.PostgreSql;
using IntegracaoSolis.Command;
using IntegracaoSolis.Handler;
using IntegracaoSolis.Interface;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

IConfigurationRoot configuration = config.Build();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IIntegracaoSolis, IntegracaoSolisHandler>();
builder.Services.AddScoped<IUploadCommand, UploadCommand>();
builder.Services.AddScoped<IDepositoPdf, DepositoPdfHandler>();
builder.Services.AddScoped<IEnvioRemessa, EnvioRemessaHandler>();
builder.Services.AddScoped<IImportarArquivo, ImportarArquivoHandler>();

builder.Services.AddHangfireServer();

builder.Services.AddHangfire(x => x.UsePostgreSqlStorage(Convert.ToString(configuration.GetSection("SQLCONNSTR").Value)));

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

app.UseHangfireDashboard();

app.Run();


