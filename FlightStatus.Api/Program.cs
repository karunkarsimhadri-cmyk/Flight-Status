using System.Text.Json.Serialization;
using FlightStatus.Api.Providers;
using FlightStatus.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
.AddJsonOptions(opts =>
    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "Flight Status API", Version = "v1" }));

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Register both provider stubs — IEnumerable<IFlightStatusProvider> receives both
builder.Services.AddScoped<IFlightStatusProvider, AeroTrackProvider>();
builder.Services.AddScoped<IFlightStatusProvider, QuickFlightProvider>();
builder.Services.AddScoped<FlightStatusService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

app.MapControllers();

app.Run();

public partial class Program { }
