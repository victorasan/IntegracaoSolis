using IntegracaoSolis.Interface;
using Microsoft.AspNetCore.Mvc;
using static IntegracaoSolis.DTO.UploadPdfDTO;

namespace IntegracaoSolis.Handler
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http.HttpResults;
    using Newtonsoft.Json;

    public class IntegracaoSolisHandler : IIntegracaoSolis
    {
        public readonly IUploadCommand _uploadCommand ;
        public IntegracaoSolisHandler(IUploadCommand uploadCommand)
        {
            _uploadCommand = uploadCommand ;
        }
        public async Task<IActionResult> uploadPdf()
        {
            var caminhoPdf = "C:\\Users\\VictorAlvesdosSantos\\OneDrive - RGICA SERVICOS TECNOLOGICOS E FINANCEIROS LTDA\\Documents\\CCB";
            var ccbDepositada = "C:\\Users\\VictorAlvesdosSantos\\OneDrive - RGICA SERVICOS TECNOLOGICOS E FINANCEIROS LTDA\\Documents\\CCB\\Depositada";
            var pdfFiles = Directory.GetFiles(caminhoPdf, "*.pdf");

            try
            {
                if (pdfFiles.Any())
                {
                    foreach (var pdfFile in pdfFiles)
                    {
                        var response = await SendPdfToApi(pdfFile);

                        if (response != null && response.status == 1)
                        {

                            File.Move(pdfFile, Path.Combine(ccbDepositada, Path.GetFileName(pdfFile)));
                            await _uploadCommand.UploadPDFCommand(response);                           

                        }
                        else
                        {
                            Console.WriteLine($"Falha ao enviar o arquivo {pdfFile}");
                        }
                    }                    
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro ao fazer upload: {e.Message}");
                return null;
            }
        }        

        static async Task<ResponseDTO> SendPdfToApi(string filePath)
        {
            try
            {
                string apiUrl = "https://api.sandbox.hermescertifica.com.br/api/bank-credit-note/upload-bankcreditnote";
                string authToken = "4D33D29D71B9821C1011A36FA407834D33FE9674F5FC431F628DBA5BDF07F5B2";

                using (HttpClient client = new HttpClient())
                using (var formData = new MultipartFormDataContent())
                {
                    formData.Headers.ContentType.MediaType = "multipart/form-data";
                    client.DefaultRequestHeaders.Add("x-api-token", authToken);

                    using (FileStream fileStream = File.OpenRead(filePath))
                    {
                        StreamContent fileContent = new StreamContent(fileStream);
                        formData.Add(fileContent, "File", Path.GetFileName(filePath));

                        HttpResponseMessage response = await client.PostAsync(apiUrl, formData);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            ResponseDTO ResponseDTO = JsonConvert.DeserializeObject<ResponseDTO>(responseBody);

                            return ResponseDTO;
                        }
                        else
                        {
                            Console.WriteLine($"Erro ao enviar o arquivo {filePath}. Status code: {response.StatusCode}");
                            return new ResponseDTO {};
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar o arquivo {filePath}: {ex.Message}");
                return new ResponseDTO {};
            }
        }
    }
}