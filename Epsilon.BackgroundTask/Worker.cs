using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Epsilon.BackgroundTask
{
    public class Worker : IHostedService, IDisposable
    {
        private long _running = 0;

        private readonly IServiceProvider _services;
        private readonly IExceptionLoggerService _exceptionLoggerService;
        private readonly ILogger<BackgroundService> _logger;
        private readonly Model _model;
        private Timer _timer;

        public Worker(IServiceProvider services,
            IExceptionLoggerService exceptionLoggerService,
            ILogger<BackgroundService> logger, 
            Model model)
        {
            _services = services;
            _exceptionLoggerService = exceptionLoggerService;
            _logger = logger;
            _model = model;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            try
            {
                //check if last task is finish
                var count = Interlocked.Increment(ref _running);
                if (count <= 0)
                {
                    foreach (var t in _model.ShouldRunTasks())
                        RunTask(t);
                }
            }
            finally
            {
                Interlocked.Decrement(ref _running);
            }
        }

        private void RunTask(Model.BgTask task)
        {
            try
            {
                var type = Type.GetType(task.TaskType, true);
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
            _logger.LogInformation("Timed Hosted Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
