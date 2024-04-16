using Hive_Server;
using Hive_Server.Repository;
using Hive_Server.Services;
using Hive_Server.Services.Interface;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

// Add services to the container.

// 설정파일(appsettings.json)에서 DBConfig 값 가져오고, DBConfig 클래스에 매핑해주기
builder.Services.Configure<DBConfig>(configuration.GetSection(nameof(DBConfig)));

// 생명 주기 설정 -> 생명 주기 끝나면 자동으로 Dispose 호출해서 DB연결 close
builder.Services.AddTransient<IAccountDB, AccountDB>();
builder.Services.AddTransient<IRedisDB, RedisDB>();

builder.Services.AddTransient<IAccountCreateService, AccountCreateService>();
builder.Services.AddTransient<IHiveLoginService, HiveLoginService>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
