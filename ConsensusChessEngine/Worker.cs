using Microsoft.Extensions.Hosting;

namespace ConsensusChessEngine;

public class Worker : BackgroundService
{
    private readonly HttpClient http = new HttpClient();
    private readonly IHostApplicationLifetime lifetime;
    private readonly ILogger<Worker> log;

    public Worker(IHostApplicationLifetime hostApplicationLifetime, ILogger<Worker> logger) =>
        (lifetime, log) = (hostApplicationLifetime, logger);

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        log.LogInformation("Worker.StartAsync at: {time}", DateTimeOffset.Now);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        log.LogInformation("Worker.ExecuteAsync at: {time}", DateTimeOffset.Now);

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

