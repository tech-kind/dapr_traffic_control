var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IFineCalculator, HardCodedFineCalculator>();

var daprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3601";
var daprGrpcPort = Environment.GetEnvironmentVariable("DAPR_GRPC_PORT") ?? "60001";

builder.Services.AddDaprClient(builder => builder
    .UseHttpEndpoint($"http://localhost:{daprHttpPort}")
    .UseGrpcEndpoint($"http://localhost:{daprGrpcPort}"));

builder.Services.AddSingleton<VehicleRegistrationService>(_ =>
    new VehicleRegistrationService(DaprClient.CreateInvokeHttpClient(
        "vehicleregistrationservice", $"http://localhost:{daprHttpPort}")));

// Add services to the container.

builder.Services.AddControllers().AddDapr();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseCloudEvents();

app.MapControllers();

app.MapSubscribeHandler();

app.Run("http://localhost:6001");
