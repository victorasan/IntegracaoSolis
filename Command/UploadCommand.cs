using Dapper;
using IntegracaoSolis.Interface;
using Npgsql;
using static IntegracaoSolis.DTO.UploadPdfDTO;

namespace IntegracaoSolis.Command
{
    public class UploadCommand : IUploadCommand
    {

        private readonly IConfiguration _configuration;

        public UploadCommand(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<bool> UploadPDFCommand(ResponseDTO response)
        {
            var connectionString = Convert.ToString(_configuration.GetSection("SQLCONNSTR").Value);

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    string sql = "INSERT INTO upload_pdf (id,clientId, url, urlNonTradable,urlEndorsement,urlSigned,fileName,createdAt,version,status,id_ccb, depositada) VALUES (@id,@clientId, @url, @urlNonTradable,@urlEndorsement,@urlSigned,@fileName,@createdAt,@version,@status,@id_ccb, @depositada)";
                    await connection.ExecuteAsync(sql, new
                    {
                        id = Guid.NewGuid(),
                        clientId = response.clientId,
                        url = response.url,
                        urlNonTradable = response.urlNonTradable,
                        urlEndorsement = response.urlEndorsement,
                        urlSigned = response.urlSigned,
                        fileName = response.fileName,
                        createdAt = Convert.ToDateTime(response.createdAt),
                        version = response.version,
                        status = response.status,
                        id_ccb = response.id,
                        depositada = false
                    });
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro ao salvar: {e.Message}");
                return false;
            }
           
        }

        public async Task<bool> UpdateDepositoCCB(ResponseDTO response)
        {
            var connectionString = Convert.ToString(_configuration.GetSection("SQLCONNSTR").Value);

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    string sql = "Update upload_pdf set depositada = @depositada";
                    await connection.ExecuteAsync(sql, new
                    {
                        depositada = true
                    });
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro ao salvar: {e.Message}");
                return false;
            }

        }
    }
}
