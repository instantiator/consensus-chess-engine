using ConsensusChessNode.Service;
using Mastonet;
using Mastonet.Entities;
using static System.Net.WebRequestMethods;

namespace ConsensusChessNode;

public class Worker : BackgroundService
{
    private readonly IHostApplicationLifetime lifetime;
    private readonly ILogger<Worker> log;
    private readonly ConsensusChessNodeService service;

    public Worker(IHostApplicationLifetime hostApplicationLifetime, ILogger<Worker> logger)
    {
        this.lifetime = hostApplicationLifetime;
        this.log = logger;
        this.service = new ConsensusChessNodeService(log, Environment.GetEnvironmentVariables());
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);
        await service.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await service.ExecuteAsync(stoppingToken);
        lifetime.StopApplication();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await service.StopAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}

