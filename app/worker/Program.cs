using Npgsql;
using StackExchange.Redis;
var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? Environment.GetEnvironmentVariable("REDIS") ?? "redis";
var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";
var pgHost = Environment.GetEnvironmentVariable("PGHOST") ?? Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "db";
var pgPort = Environment.GetEnvironmentVariable("PGPORT") ?? Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
var pgDb = Environment.GetEnvironmentVariable("PGDATABASE") ?? Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "votes";
var pgUser = Environment.GetEnvironmentVariable("PGUSER") ?? Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
var pgPass = Environment.GetEnvironmentVariable("PGPASSWORD") ?? Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";

var mux = await ConnectionMultiplexer.ConnectAsync($"{redisHost}:{redisPort}");
var db = mux.GetDatabase();

var cs = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPass}";
await using var conn = new NpgsqlConnection(cs);
await conn.OpenAsync();
// ensure table
await using (var cmd = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS votes (vote text)", conn))
  await cmd.ExecuteNonQueryAsync();

Console.WriteLine("Worker started.");
while (true)
{
  var val = await db.ListLeftPopAsync("votes");
  if (!val.IsNullOrEmpty)
  {
    await using var cmd = new NpgsqlCommand("INSERT INTO votes(vote) VALUES($1)", conn);
    cmd.Parameters.AddWithValue(val.ToString());
    await cmd.ExecuteNonQueryAsync();
    Console.WriteLine($"Inserted vote: {val}");
  }
  await Task.Delay(500);
}
