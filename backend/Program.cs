using Microsoft.EntityFrameworkCore;
using backend.Data;
var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          // Allow requests from your React app's origin
                          policy.WithOrigins("http://localhost:5173")
                                .AllowAnyHeader() // Allow all headers
                                .AllowAnyMethod(); // Allow all HTTP methods (POST, GET, etc.)
                      });
});
builder.Services.AddControllers();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
app.UseCors(MyAllowSpecificOrigins);
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
