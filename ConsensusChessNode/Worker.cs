using Mastonet;
using Mastonet.Entities;
using static System.Net.WebRequestMethods;

namespace ConsensusChessNode;

public class Worker : BackgroundService
{
    private readonly IHostApplicationLifetime lifetime;
    private readonly ILogger<Worker> log;
    private readonly string name;

    public Worker(IHostApplicationLifetime hostApplicationLifetime, ILogger<Worker> logger)
    {
        this.lifetime = hostApplicationLifetime;
        this.log = logger;
        this.name = Environment.GetEnvironmentVariable("NODE_NAME") ?? "unknown";
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        log.LogInformation($"Worker.StartAsync as: {name} at: " + "{time}", DateTimeOffset.Now);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        log.LogInformation("Worker.ExecuteAsync at: {time}", DateTimeOffset.Now);

        //while (!stoppingToken.IsCancellationRequested)
        //{
        //    log.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        //    await Task.Delay(1000, stoppingToken);
        //}

        log.LogInformation("Worker stopping at: {time}", DateTimeOffset.Now);
        lifetime.StopApplication();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        log.LogInformation("Cancellation has reached StopAsync");
        await base.StopAsync(cancellationToken);

    }
}

