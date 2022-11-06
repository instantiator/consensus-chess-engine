using ConsensusChessShared.Database;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ConsensusChessEngine;

public class Worker : BackgroundService
{
    private readonly HttpClient http = new HttpClient();
    private readonly IHostApplicationLifetime lifetime;
    private readonly ILogger<Worker> log;

    private ConsensusChessDbContext db;
    private Network network;
    private ISocialConnection social;

    public Worker(IHostApplicationLifetime hostApplicationLifetime, ILogger<Worker> logger) =>
        (lifetime, log) = (hostApplicationLifetime, logger);

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        log.LogInformation("Worker.StartAsync at: {time}", DateTimeOffset.Now);
        db = ConsensusChessDbContext.FromEnvironment(Environment.GetEnvironmentVariables());

        var connection = await db.Database.CanConnectAsync();
        log.LogInformation($"Database connection: {connection}");

        await db.Database.MigrateAsync();
        log.LogInformation($"Migrations run.");

        network = Network.FromEnvironment(NetworkType.Mastodon, Environment.GetEnvironmentVariables());
        social = new MastodonConnection(network);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        log.LogInformation("Worker.ExecuteAsync at: {time}", DateTimeOffset.Now);
        log.LogInformation($"Display name: {await social.GetDisplayNameAsync()}");
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

