using ConsensusChessNode.Service;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
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

        var env = Environment.GetEnvironmentVariables();
        var id = ServiceIdentity.FromEnv(env);
        var dbo = new DbOperator(log, env);
        var network = Network.FromEnv(env);
        var social = SocialFactory.From(log, network);

        service = new ConsensusChessNodeService(log, id, dbo, network, social);
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

