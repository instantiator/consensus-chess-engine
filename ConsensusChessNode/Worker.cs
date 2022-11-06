using Mastonet;
using Mastonet.Entities;
using static System.Net.WebRequestMethods;

namespace ConsensusChessNode;

public class Worker : BackgroundService
{
    private readonly HttpClient http = new HttpClient();
    private readonly IHostApplicationLifetime lifetime;
    private readonly ILogger<Worker> log;

    private string name;

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

        // https://github.com/glacasa/Mastonet/blob/main/DOC.md
        // https://github.com/glacasa/Mastonet/blob/main/API.md

        // TODO: get these into config
        AppRegistration reg = new AppRegistration()
        {
            ClientId = "WIt0pfbT8zsxKzlrB5I5lha7aecbKxKjt29spVRFnTA",
            ClientSecret = "_rluagXdeh_-H3K-okWprQ5gXPlP7AnRs2F_VXICzc0",
            Instance = "mastodon.social",
            Scope = Scope.Read | Scope.Write | Scope.Follow
        };

        // TODO: likewise - config
        Auth token = new Auth()
        {
            AccessToken = "rkBdWKwXcsSWRuYALW_HJ00ErUlx89VJUdMYFNllxzo"
        };

        var client = new MastodonClient(reg, token, http);
        var user = await client.GetCurrentUser();
        log.LogInformation($"User: {user.DisplayName}");


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

