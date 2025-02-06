using Microsoft.Data.SqlClient;
using Quartz.Impl.Matchers;
using Quartz;
using Dapper;

namespace NetAirflow.Web.Comun
{
    public class CronJobBackgroundService : IHostedService, IDisposable
    {
        private readonly DdlService _ddlService;
        private readonly IServiceProvider _services;
        private readonly ILogger<CronJobBackgroundService> _logger;
        private Timer _timer;

        public CronJobBackgroundService(DdlService ddlService, IServiceProvider services, ILogger<CronJobBackgroundService> logger)
        {
            _ddlService = ddlService;
            _services = services;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Servicio de fondo de CronJobs iniciado.");
            _timer = new Timer(CheckForUpdates, null, TimeSpan.Zero, TimeSpan.FromMinutes(50)); 
            return Task.CompletedTask;
        }

        private async void CheckForUpdates(object state)
        {
            _logger.LogInformation("Verificando actualizaciones de CronJobs...");
            _ddlService.LoadAll();

            using (var scope = _services.CreateScope())
            {
                var connectionString = "Server=172.16.50.72,1433; Database=PruebasTiendas; USER ID=UsuPruebasTiendas; Password=strongPassw00rd!;Encrypt=True;TrustServerCertificate=True;";
                IEnumerable<CronJob> cronJobs;

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    cronJobs = await connection.QueryAsync<CronJob>("SELECT Id, Nombre, ExpresionCron, OnSuccesed, OnCanceled, OnError, DateCreate, DateModify, DateSuccesed, DateCanceled, DateError FROM CronJobs");
                }

                var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
                var scheduler = await schedulerFactory.GetScheduler();

                // Obtener los trabajos actuales en Quartz
                var existingJobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());

                // Eliminar trabajos que ya no están en la base de datos
                foreach (var jobKey in existingJobKeys)
                {
                    if (!cronJobs.Any(c => c.Id.ToString() == jobKey.Name))
                    {
                        await scheduler.DeleteJob(jobKey);
                        _logger.LogInformation($"Trabajo eliminado: {jobKey.Name}");
                    }
                }

                // Agregar o actualizar trabajos
                foreach (var cronJob in cronJobs)
                {
                    var jobKey = new JobKey(cronJob.Id.ToString());

                    if (!await scheduler.CheckExists(jobKey))
                    {
                        // Crear un nuevo trabajo
                        var job = JobBuilder.Create<DynamicJob>()
                            .WithIdentity(jobKey)
                            .Build();

                        var trigger = TriggerBuilder.Create()
                            .WithIdentity($"Trigger-{cronJob.Id}")
                            .WithCronSchedule(cronJob.ExpresionCron)
                            .WithDescription(cronJob.Nombre)
                            .Build();

                        await scheduler.ScheduleJob(job, trigger);
                        _logger.LogInformation($"Nuevo trabajo agregado: {cronJob.Id}");
                    }
                    else
                    {
                        // Actualizar el trabajo existente
                        var triggerKey = new TriggerKey($"Trigger-{cronJob.Id}");
                        var newTrigger = TriggerBuilder.Create()
                            .WithIdentity(triggerKey)
                            .WithCronSchedule(cronJob.ExpresionCron)
                            .WithDescription(cronJob.Nombre)
                            .Build();

                        await scheduler.RescheduleJob(triggerKey, newTrigger);
                        _logger.LogInformation($"Trabajo actualizado: {cronJob.Id}");
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Servicio de fondo de CronJobs detenido.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
