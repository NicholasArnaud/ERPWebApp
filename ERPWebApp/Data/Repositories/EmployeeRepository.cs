using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Models.Company;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Data.Common;

namespace ERPWebApp.Data.Repositories
{
    public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<JObject> GetEmployeeErrorsByDate(DateTime date)
        {
            var conn = _context.Database.GetDbConnection();
            try
            {
                var data = new
                {
                    departments = new List<string>(),
                    employeeReferences = new List<string>(),
                    errorCounts = new List<int>(),
                };

                await conn.OpenAsync();
                using var command = conn.CreateCommand();
                command.CommandText = "GetEmployeeErrorsByDate @Date;";
                SqlParameter param = new SqlParameter
                {
                    ParameterName = "@Date",
                    Value = date.ToShortDateString()
                };
                command.Parameters.Add(param);

                DbDataReader reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        data.departments.Add("'" + reader.GetString(1) + "'");
                        data.employeeReferences.Add("'" + reader.GetString(2) + "'");
                        data.errorCounts.Add(reader.GetInt32(3));
                    }
                }

                await reader.CloseAsync();

                return JObject.FromObject(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                await conn.CloseAsync();
            }
        }
    }
}