using Microsoft.VisualBasic;
using Npgsql;
using System.Reflection.Metadata;
namespace ConsoleApp19
{
    public class Database
    {
        NpgsqlConnection con = new NpgsqlConnection();

        public async Task AddHistory(int id, string request)
        {
            var sql = "INSERT INTO public.\"spotibot\" (\"id\", \"date\", \"request\")"
                    + "VALUES (@id, @date, @request)";

            NpgsqlCommand comm = new NpgsqlCommand(sql, con);
            comm.Parameters.AddWithValue("id", id);
            comm.Parameters.AddWithValue("date", DateTime.Now);
            comm.Parameters.AddWithValue("request", request);

          
            await con.OpenAsync();
            await comm.ExecuteNonQueryAsync();
            await con.CloseAsync();
        }

        public async Task DeleteHistory(int id)
        {
            var sql = "DELETE FROM public.\"spotibot\" WHERE \"id\" = @id";

            NpgsqlCommand comm = new NpgsqlCommand(sql, con);
            comm.Parameters.AddWithValue("id", id);

            try
            {
                await con.OpenAsync();
                await comm.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                
            }
            finally
            {
                con.Close();
            }
        }
    }
}
