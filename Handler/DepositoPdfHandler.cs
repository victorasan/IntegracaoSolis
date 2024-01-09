using IntegracaoSolis.Interface;
using System.Text;
namespace IntegracaoSolis.Handler
{
    public class DepositoPdfHandler : IDepositoPdf
    {
        private readonly IConfiguration _configuration;

        public DepositoPdfHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<bool> DepositoPdf()
        {
            {
                var apiUrl = Convert.ToString(_configuration.GetSection("UrlDeposito").Value);
                var authToken = Convert.ToString(_configuration.GetSection("authToken").Value);
                var Accept = Convert.ToString(_configuration.GetSection("Accept").Value);

                var requestBody = new
                {
                    sendToSignature = false,
                    batchName = "teste",
                    bankCreditNotes = new[]
                    {
                    new
                    {
                        issueDate = "2023-01-02T18:02:33.142Z",
                        documentNumber = "7001147312",
                        takerVatNumber = "71347739653",
                        documentBankCreditNoteId = "6fbb9724-5c7e-4f73-0a37-08dc00c6db2c",
                        value = 16.992
                    }
                }
                };

                var requestBodyJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");

                try
                {
                    using HttpClient client = new HttpClient();

                    client.DefaultRequestHeaders.Add("text/plain", Accept);
                    client.DefaultRequestHeaders.Add("x-api-token", authToken);

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);


                    if (response.IsSuccessStatusCode)
                    {

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
