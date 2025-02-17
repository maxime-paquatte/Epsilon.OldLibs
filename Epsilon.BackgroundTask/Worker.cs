using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Epsilon.BackgroundTask
{
    public class Worker : IHostedService, IAsyncDisposable
    {
        private long _running = 0;

        private readonly IServiceProvider _services;
        private readonly IExceptionLoggerService _exceptionLoggerService;
        private readonly ILogger<BackgroundService> _logger;
        private readonly Model _model;
        private readonly ServicesStore _store;
        private Timer _timer;

        public Worker(IServiceProvider services,
            IExceptionLoggerService exceptionLoggerService,
            ILogger<BackgroundService> logger,
            Model model, ServicesStore store)
        {
            _services = services;
            _exceptionLoggerService = exceptionLoggerService;
            _logger = logger;
            _model = model;
            _store = store;

            _logger.LogInformation("BackgroundTask construct.");
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundTask running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _logger.LogInformation("BackgroundTask DoWork");
            try
            {
                var count = Interlocked.Increment(ref _running);
                if (count <= 1)
                {
                    var t = _model.ShouldRunTasks().FirstOrDefault();
                    if (t != null) RunTask(t);
                }
            }
            finally
            {
                Interlocked.Decrement(ref _running);
            }
        }

        private void RunTask(Model.BgTask task)
        {
            _logger.LogInformation("BackgroundTask run task: " + task.TaskType);
            try
            {
                if (!_store.Types.TryGetValue(task.TaskType, out var type))
                    throw new Exception("IBackgroundTask not found in store: " + task.TaskType);

                var service = _services.GetService(type) as IBackgroundTask;

                if (service == null) throw new Exception("task should implement IBackgroundTask");
                service.Run(task.TaskKey, task.JsonConfig);
                _model.SetState(task.TaskKey, "done");
            }
            catch (Exception ex)
            {
                _exceptionLoggerService.Log(new Exception("Exception while executing background task " + task.TaskKey, ex));
                _logger.LogError("Exception while executing background task " + task.TaskKey);
                _model.SetState(task.TaskKey, "error");
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundTask is stopping.");
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }


        public async ValueTask DisposeAsync()
        {
            if (_timer is IAsyncDisposable timer)
                await timer.DisposeAsync();

            _timer = null;
        }
    }
}