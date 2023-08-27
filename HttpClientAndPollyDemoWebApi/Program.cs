using HttpClientAndPollyDemoWebApi.Services;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient("datausa", o =>
{
    o.BaseAddress = new Uri("https://datausa.io/api/data");
});
builder.Services.AddHttpClient("dummyHandler").AddPolicyHandler(r=>r.Method == HttpMethod.Get ? Policy.TimeoutAsync<HttpResponseMessage>(1):Policy.NoOpAsync<HttpResponseMessage>());
builder.Services.AddHttpClient("dummyCircuitBreaker").AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3,TimeSpan.FromSeconds(10))); //allow only one call every 10 seconds after 3 frequently failed requests till success then break the circuit
builder.Services.AddHttpClient("dummyFallback").AddTransientHttpErrorPolicy(policy => policy.FallbackAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.NotFound}));
builder.Services.AddHttpClient("dummyRetry").AddTransientHttpErrorPolicy(policy => policy.RetryAsync());
builder.Services.AddHttpClient("dummyRetryForever").AddTransientHttpErrorPolicy(policy => policy.RetryForeverAsync());
builder.Services.AddHttpClient("dummyWaitAndRetry").AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(5)));
builder.Services.AddHttpClient("dummyWaitAndRetryForever").AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryForeverAsync(_=>TimeSpan.FromSeconds(1)));
builder.Services.AddHttpClient("datausa", c =>
{
    c.BaseAddress = new Uri("https://datausa.io/api/data");
});
builder.Services.AddHttpClient<IDataUSA, DataUSAService>(c =>
{
    c.BaseAddress = new Uri("https://datausa.io/api/data");
});

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

app.Run();
