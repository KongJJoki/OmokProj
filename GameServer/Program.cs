using GameServer;
using GameServer.Repository;
using GameServer.Services;
using GameServer.Services.Interface;
using GameServer.Middleware;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

// 설정파일(appsettings.json)에서 DBConfig 값 가져오고, DBConfig 클래스에 매핑해주기
builder.Services.Configure<DBConfig>(configuration.GetSection(nameof(DBConfig)));

// 생명 주기 설정 -> 생명 주기 끝나면 자동으로 Dispose 호출해서 DB연결 close
builder.Services.AddTransient<IGameDB, GameDB>();
builder.Services.AddTransient<IRedisDB, RedisDB>();

builder.Services.AddTransient<IGameLoginService, GameLoginService>();
builder.Services.AddTransient<IMatchRequestService, MatchRequestService>();
builder.Services.AddTransient<IMatchCheckService, MatchCheckService>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<CheckAuthTokenValid>();

app.MapControllers();

app.Run();
