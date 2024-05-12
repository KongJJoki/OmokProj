using MatchingServer;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;
builder.Services.Configure<RedisDBConfig>(configuration.GetSection(nameof(RedisDBConfig)));


builder.Services.AddSingleton<IMatchWoker, MatchWoker>();


builder.Services.AddControllers();

WebApplication app = builder.Build();

app.MapDefaultControllerRoute();


app.Run(configuration["ServerAddress"]);


