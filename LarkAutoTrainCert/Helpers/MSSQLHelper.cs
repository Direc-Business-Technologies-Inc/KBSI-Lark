using System.Data;
using System.Data.SqlClient;
using LarkAutoTrainCert.Model;
using Microsoft.Extensions.Options;

namespace LarkAutoTrainCert.Helpers
{
    public class MSSQLHelper
    {
        private readonly HttpClient _client;
        public readonly SQLModel _conString;

        public MSSQLHelper(IOptions<SQLModel> sqlModel)
        {
            _client = new HttpClient();
            _conString = sqlModel.Value;
        }

        public string GetConnection()
        {
            var output = _conString.ConnectionString;
            return output;
        }
        public DataTable GetData(string sQuery, string Connectionstring)
        {
            try
            {
                using (DataTable dt = new DataTable())
                {
                    using (SqlConnection con = new SqlConnection(Connectionstring))
                    {
                        using (SqlCommand cmd = new SqlCommand(sQuery, con))
                        {
                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            con.Open();
                            da.Fill(dt);
                            con.Close();
                        }
                    }

                    return dt;
                }
            }
            catch (Exception ex)
            { return null; }

            //return output;
        }
    }
}
