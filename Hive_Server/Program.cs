using Hive_Server;
using Hive_Server.Repository;
using Hive_Server.Services;
using Hive_Server.Services.Interface;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

// Add services to the container.

// ��������(appsettings.json)���� DBConfig �� ��������, DBConfig Ŭ������ �������ֱ�
builder.Services.Configure<DBConfig>(configuration.GetSection(nameof(DBConfig)));

// ���� �ֱ� ���� -> ���� �ֱ� ������ �ڵ����� Dispose ȣ���ؼ� DB���� close
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
