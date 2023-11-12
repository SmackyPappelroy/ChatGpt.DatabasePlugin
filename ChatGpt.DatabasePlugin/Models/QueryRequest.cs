using Microsoft.Data.SqlClient;

namespace ChatGpt.WebApi.Models
{
    public class QueryRequest
    {
        public string Query { get; set; }
        public List<SqlParameter> Parameters { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

}
