using Game_Server;
using Game_Server.Repository;
using Game_Server.Services;
using Game_Server.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

// Add services to the container.

// ��������(appsettings.json)���� DBConfig �� ��������, DBConfig Ŭ������ �������ֱ�
builder.Services.Configure<DBConfig>(configuration.GetSection(nameof(DBConfig)));
//builder.Services.Configure<ServerConfig>(configuration.GetSection(nameof(ServerConfig)));

// ���� �ֱ� ���� -> ���� �ֱ� ������ �ڵ����� Dispose ȣ���ؼ� DB���� close
builder.Services.AddTransient<IGameDB, GameDB>();
builder.Services.AddTransient<IRedisDB, RedisDB>();

builder.Services.AddTransient<IGameLoginService, GameLoginService>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
