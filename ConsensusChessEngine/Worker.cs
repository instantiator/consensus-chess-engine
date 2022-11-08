using ConsensusChessEngine.Service;
using ConsensusChessShared.Database;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ConsensusChessEngine;

public class Worker : BackgroundService
{
    private readonly IHostApplicationLifetime lifetime;
    private readonly ILogger<Worker> log;
    private readonly ConsensusChessEngineService service;

    public Worker(IHostApplicationLifetime hostApplicationLifetime, ILogger<Worker> logger)
    {
        this.lifetime = hostApplicationLifetime;
        this.log = logger;
        this.service = new ConsensusChessEngineService(log, Environment.GetEnvironmentVariables());
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await service.StartAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
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

