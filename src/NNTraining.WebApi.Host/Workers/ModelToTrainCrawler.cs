using NNTraining.Common.Enums;
using NNTraining.WebApi.Contracts;
using NNTraining.WebApi.DataAccess;
using NNTraining.WebApi.Domain.Models;

namespace NNTraining.WebApi.Host.Workers;

public class ModelToTrainCrawler : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private Dictionary<string, Model> _dictionary = new();

    public ModelToTrainCrawler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    /// <summary>
    /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" /> starts. The implementation should return a task that represents
    /// the lifetime of the long running operation(s) being performed.
    /// </summary>
    /// <param name="stoppingToken">Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" /> is called.</param>
    /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the long running operations.</returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _ = Task.Factory.StartNew(() => RunCrawling(stoppingToken), TaskCreationOptions.LongRunning);
        
        return Task.CompletedTask;
    }

    private void RunCrawling(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<NNTrainingDbContext>()!;
        var publisher = scope.ServiceProvider.GetService<IWebAppPublisherService>()!;

        var modelsToTrain = dbContext.Models.Where(x => x.ModelStatus == ModelStatus.WaitingTraining).ToList();

        foreach (var model in modelsToTrain)
        {
            if (!_dictionary.ContainsKey(model.Id.ToString()))
            {
                _dictionary.Add(model.Id.ToString(), model);
                publisher.SendModelContract(model);
            }
        }
        
        Thread.Sleep(TimeSpan.FromSeconds(30));// раз в 30 секунд проверяет бд
    }
}