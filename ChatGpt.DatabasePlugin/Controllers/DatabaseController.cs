using ChatGpt.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ChatGpt.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogController : ControllerBase
    {
        private readonly DatabaseHandler _databaseHandler;

        public LogController(IConfiguration configuration)
        {
            _databaseHandler = new DatabaseHandler(configuration);
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs(DateTime? startDate, DateTime? endDate, int pageNumber = 1, int pageSize = 100)
        {
            // Paginering och valideringslogik här (som tidigare)

            var parameters = new List<SqlParameter>();

            string query = @"
        SELECT Name, Description, LogTime, NumericValue 
        FROM vLog 
        WHERE (@StartDate IS NULL OR LogTime >= @StartDate) 
        AND (@EndDate IS NULL OR LogTime <= @EndDate)
        ORDER BY LogTime DESC 
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            // Lägg till parametrar för datumfiltrering
            parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = (object)startDate ?? DBNull.Value });
            parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = (object)endDate ?? DBNull.Value });

            // Lägg till pagineringsparametrar
            parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = (pageNumber - 1) * pageSize });
            parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

            var dataTable = await _databaseHandler.ExecuteQueryAsync(query, parameters);
            var logs = new List<LogEntry>();

            foreach (DataRow row in dataTable.Rows)
            {
                var logEntry = new LogEntry
                {
                    Name = row["Name"].ToString(),
                    Description = row["Description"].ToString(),
                    LogTime = Convert.ToDateTime(row["LogTime"]),
                    NumericValue = Convert.ToSingle(row["NumericValue"])
                };
                logs.Add(logEntry);
            }

            return Ok(logs);
        }

        [HttpPost("execute-query")]
        public async Task<IActionResult> ExecuteQuery([FromBody] QueryRequest queryRequest)
        {
            if (!IsQuerySafe(queryRequest.Query))
            {
                return BadRequest("Ogiltig förfrågan.");
            }

            // Lägg till parametrar för start- och slutdatum om de finns
            queryRequest.Parameters ??= new List<SqlParameter>();
            if (queryRequest.StartDate.HasValue)
            {
                queryRequest.Parameters.Add(new SqlParameter("@StartDate", queryRequest.StartDate.Value));
            }
            if (queryRequest.EndDate.HasValue)
            {
                queryRequest.Parameters.Add(new SqlParameter("@EndDate", queryRequest.EndDate.Value));
            }

            try
            {
                var dataTable = await _databaseHandler.ExecuteQueryAsync(queryRequest.Query, queryRequest.Parameters);
                var result = DataTableToList(dataTable);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Logga undantaget
                return StatusCode(500, "Ett fel inträffade vid exekvering av SQL-frågan.");
            }
        }

        private List<Dictionary<string, object>> DataTableToList(DataTable table)
        {
            var list = new List<Dictionary<string, object>>();
            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }
            return list;
        }

        private bool IsQuerySafe(string query)
        {
            // Implementera en metod för att validera att förfrågan är säker. Detta är en komplex uppgift och bör
            // hanteras med stor försiktighet. Det kan inkludera att kolla efter vissa nyckelord som "DELETE", "DROP", etc.
            // och säkerställa att förfrågan är en tillåten typ av förfrågan (t.ex. endast SELECT).
            return true; // Exempel: returnera alltid 'true' är INTE säkert i produktion.
        }
    }

}
