using ResearchOrm.Context;
using ResearchOrm.Models;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var assemblyTypes = typeof(User).Assembly.GetTypes();
var types = assemblyTypes.Where(t => t.FullName.Contains("Models") && !t.IsAbstract);

await using var context = new UniversalContext();
context.SetConnectionString("Server=localhost;Port=5433;Username=postgres;Password=postgres;Database=Test;");
await context.CreateTablesAsync(types);

app.Run();