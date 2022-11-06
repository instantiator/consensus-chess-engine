using ConsensusChessShared.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ConsensusChessEngine;

public class Worker : BackgroundService
{
    private readonly HttpClient http = new HttpClient();
    private readonly IHostApplicationLifetime lifetime;
    private readonly ILogger<Worker> log;

    private ConsensusChessDbContext db;

    public Worker(IHostApplicationLifetime hostApplicationLifetime, ILogger<Worker> logger) =>
        (lifetime, log) = (hostApplicationLifetime, logger);

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        log.LogInformation("Worker.StartAsync at: {time}", DateTimeOffset.Now);

        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var database = Environment.GetEnvironmentVariable("POSTGRES_DB");
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER");
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
        log.LogInformation($"Database host: {host} db: {database}");
        db = new ConsensusChessDbContext(host, database, username, password);

        var connection = await db.Database.CanConnectAsync();
        var created = await db.Database.EnsureCreatedAsync();
        log.LogInformation($"Connection: {connection} Created: {created}");

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        log.LogInformation("Worker.ExecuteAsync at: {time}", DateTimeOffset.Now);
        log.LogInformation($"Games: {db.Games.Count()}");

        // a while loop, which we can use for streaming from Mastodon
        while (!stoppingToken.IsCancellationRequested)
        {
            log.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }

        log.LogInformation("Worker cancelled at: {time}", DateTimeOffset.Now);
        lifetime.StopApplication();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        log.LogInformation("Cancellation has reached StopAsync");
        await base.StopAsync(cancellationToken);

    }
}

