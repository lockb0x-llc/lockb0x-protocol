using Lockb0x.Core.Canonicalization;
using Lockb0x.Core.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IJsonCanonicalizer, JcsCanonicalizer>();
builder.Services.AddSingleton<ICodexEntryValidator, CodexEntryValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
