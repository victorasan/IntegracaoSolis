using IntegracaoSolis.Command;
using IntegracaoSolis.Interface;
using Newtonsoft.Json;
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

                
                
                var requestBody = new
                {
                    sendToSignature = false,
                    batchName = "C01SoliSiape08122023",
                    bankCreditNotes = new[]
                    {
                    new
                    {
                        issueDate = "2023-12-08T16:22:33.142Z", //data da emissão da CCB
                        documentNumber = "7000889293",
                        takerVatNumber = "17004802272",
                        documentBankCreditNoteId = "a73f2798-1d87-4d25-0a38-08dc00c6db2c",
                        value = 4326.01
                    }
                }
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
