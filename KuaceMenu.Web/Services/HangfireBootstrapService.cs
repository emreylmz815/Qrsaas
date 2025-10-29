using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KuaceMenu.Web.Services;

public class HangfireBootstrapService : IHostedService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<HangfireBootstrapService> _logger;

    public HangfireBootstrapService(IBackgroundJobClient backgroundJobClient, ILogger<HangfireBootstrapService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Hangfire hazır durumda");
        _backgroundJobClient.Enqueue(() => Console.WriteLine("Hangfire başlatıldı"));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
