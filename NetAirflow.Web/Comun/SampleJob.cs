using Dapper;
using Microsoft.Data.SqlClient;
using NetAirflow.Shared.Clases;
using NetAirflow.Shared.Interfaces;
using Quartz;
using static Hangfire.Storage.JobStorageFeatures;

namespace NetAirflow.Web.Comun
{
    // Implementación de un Job con Quartz.NET
    public class DynamicJob : IJob
    {
        private readonly DdlService _ddlService;
        private readonly ILogger<DynamicJob> _logger;
        private readonly string _connectionString = "Server=172.16.50.72,1433; Database=PruebasTiendas; USER ID=UsuPruebasTiendas; Password=strongPassw00rd!;Encrypt=True;TrustServerCertificate=True;";

        public DynamicJob(DdlService ddlService, ILogger<DynamicJob> logger)
        {
            _ddlService = ddlService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            int jobId = int.Parse(context.JobDetail.Key.Name);
            CronJob? cronJob;


           _logger.LogInformation($"Ejecutando trabajo {jobId} a las {DateTime.Now}");

            var connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                cronJob = await connection.QueryFirstOrDefaultAsync<CronJob>(
                    "SELECT Id, Nombre, ExpresionCron, OnSuccesed, OnCanceled, OnError, DateCreate, DateModify, DateSuccesed, DateCanceled, DateError FROM CronJobs WHERE Id = @Id",
                    new { Id = jobId }
                );
                if (cronJob != null)
                {
                    try
                    {
                        ITask? task = _ddlService.GetTaskByName(cronJob.Nombre);
                        if (task != null)
                        {
                            await task.ExecuteAsync(new TaskExecutionContext { Logger = _logger }, CancellationToken.None);
                            var command = new SqlCommand("UPDATE CronJobs SET OnSuccesed = @Message, DateSuccesed = @DateSuccesed WHERE Id = @Id", connection);
                            command.Parameters.AddWithValue("@Message", "Éxito");
                            command.Parameters.AddWithValue("@DateSuccesed", DateTime.Now);
                            command.Parameters.AddWithValue("@Id", jobId);
                            await command.ExecuteNonQueryAsync();
                        }
                    }catch(Exception err)
                    {
                        var command = new SqlCommand("UPDATE CronJobs SET OnError = @Message, DateError = @DateError WHERE Id = @Id", connection);
                        command.Parameters.AddWithValue("@Message", err.Message);
                        command.Parameters.AddWithValue("@DateError", DateTime.Now);
                        command.Parameters.AddWithValue("@Id", jobId);
                        await command.ExecuteNonQueryAsync();
                    }
                    
                }

               
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
