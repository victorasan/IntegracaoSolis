using IntegracaoSolis.Interface;
using System.Text;
namespace IntegracaoSolis.Handler
{
    public class DepositoPdfHandler : IDepositoPdf
    {
        
        public async Task<bool> DepositoPdf()
        {
            {
                // URL da API externa
                string apiUrl = "https://api.sandbox.hermescertifica.com.br/api/bank-credit-note/deposit";
                string authToken = "4D33D29D71B9821C1011A36FA407834D33FE9674F5FC431F628DBA5BDF07F5B2";
                string Accept = "text/plain";

                // Criar o corpo da requisição
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
