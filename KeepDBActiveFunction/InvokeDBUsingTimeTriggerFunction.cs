using System;
using System.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KeepDBActiveFunction
{
    public class InvokeDBUsingTimeTriggerFunction
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IDbConnection _connection;

        public InvokeDBUsingTimeTriggerFunction(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<InvokeDBUsingTimeTriggerFunction>();
            this._configuration = configuration;
            _connection = new SqlConnection(GetConnectionString());
        }

        [Function(nameof(InvokeDBUsingTimeTriggerFunction))]
        //[TimerTrigger("0 */45 * * * *")]
        //[TimerTrigger("0 */5 * * * *")]
        public async Task Run([TimerTrigger("0 */45 * * * *")] TimerInfo myTimer)
        {
            try
            {
                _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

                using var connection = new SqlConnection(GetConnectionString());
                await connection.OpenAsync();
                using SqlCommand srcCmd = new SqlCommand("dbo.GetAllCategories",connection);
                srcCmd.CommandType = CommandType.StoredProcedure;

                var numberOfRowsAffected = await srcCmd.ExecuteNonQueryAsync();

                if (myTimer.ScheduleStatus is not null)
                {
                    _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message );
                _logger.LogInformation("DB was in paused state");
            }
        }

        private string? GetConnectionString()
        {
            return Convert.ToString(_configuration["SQLDBConnectionString"]);
        }
    }
}
