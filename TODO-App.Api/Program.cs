using Microsoft.EntityFrameworkCore;
using TODO_App.DataAccess.Interfaces;
using TODO_App.DataAccess.Repositories;
using TODO_App.Services.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

var app = builder.Build();

app.MapControllers();

app.MapGet("/", () => "To-Do API is running with PostgreSQL!");

app.Run();