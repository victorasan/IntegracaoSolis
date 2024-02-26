using Dapper;
using IntegracaoSolis.Command;
using IntegracaoSolis.DTO;
using IntegracaoSolis.Interface;
using Newtonsoft.Json;
using Npgsql;
using System.Text;
using static IntegracaoSolis.DTO.UploadPdfDTO;

namespace IntegracaoSolis.Handler

{
    public class DepositoPdfHandler : IDepositoPdf
    {
        private readonly IConfiguration _configuration;
        private readonly IUploadCommand _uploadCommand;

        public DepositoPdfHandler(IConfiguration configuration, IUploadCommand uploadCommand)
        {
            _configuration = configuration;
            _uploadCommand = uploadCommand;
        }
        public async Task<bool> DepositoPdf()
        {
            {
                var apiUrl = Convert.ToString(_configuration.GetSection("UrlDeposito").Value);
                var authToken = Convert.ToString(_configuration.GetSection("authToken").Value);
                var Accept = Convert.ToString(_configuration.GetSection("Accept").Value);
                var connectionString = _configuration.GetSection("SQLCONNSTR").Value;
                List<object> dadosEnvio = new List<object>();
                var sql = "SELECT a.filename, a.id_ccb, b.cnpj_cpf, b.numero_documento, b.proposta, b.data_emissao, SUM(b.valor_parcela) AS valor_total FROM upload_pdf AS a INNER JOIN arquivo_remessa AS b ON SUBSTRING(a.filename FROM 1 FOR 6) = b.proposta GROUP BY a.filename, a.id_ccb, b.cnpj_cpf, b.numero_documento, b.proposta, b.data_emissao;";
                List<CountArquivoDTO> result;

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    result = connection.Query<CountArquivoDTO>(sql).ToList();
                    
                }

                foreach (var item in result)
                {
                    var bankCreditNotes = new[]
                {
                    new
                    {
                        issueDate = Convert.ToDateTime(item.data_emissao),
                        documentNumber = item.numero_documento,
                        takerVatNumber = item.cnpj_cpf,
                        documentBankCreditNoteId = item.id_ccb,
                        value = item.valor_total
                    },
                };
                    dadosEnvio.AddRange(bankCreditNotes);
                }

                 var requestBody = new
                {
                    sendToSignature = false,
                    batchName = "C02SoliSiape16022024",
                    bankCreditNotes = dadosEnvio
                 };
                


                var requestBodyJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");

                try
                {
                    using HttpClient client = new HttpClient();

                    client.DefaultRequestHeaders.Add("Accept", Accept);
                    client.DefaultRequestHeaders.Add("x-api-token", authToken);

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);


                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        ResponseDTO ResponseDTO = JsonConvert.DeserializeObject<ResponseDTO>(responseBody);

                        //File.Delete("*.pdf");
                        await _uploadCommand.UpdateDepositoCCB(ResponseDTO);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro na chamada da API: " + ex.Message);
                    return false;
                }
            }
        }
    }
}
