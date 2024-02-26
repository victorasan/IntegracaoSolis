using IntegracaoSolis.Interface;
using Microsoft.AspNetCore.Mvc;
using static IntegracaoSolis.DTO.UploadPdfDTO;

namespace IntegracaoSolis.Handler
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    public class IntegracaoSolisHandler : IIntegracaoSolis
    {
        public readonly IUploadCommand _uploadCommand ;
        public readonly IConfiguration _configuration ;
        public IntegracaoSolisHandler(IUploadCommand uploadCommand, IConfiguration configuration)
        {
            _uploadCommand = uploadCommand;
            _configuration = configuration;
        }
        public async Task<IActionResult> uploadPdf()
        {
                var caminhoPdf = Convert.ToString(_configuration.GetSection("CaminhoPdf").Value)!;
            var ccbDepositada = Convert.ToString(_configuration.GetSection("ccbDepositada").Value)!;

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

        public async Task<ResponseDTO> SendPdfToApi(string filePath)
        {
            try
            {
                var apiUrl = Convert.ToString(_configuration.GetSection("UrlUpload").Value);
                var authToken = Convert.ToString(_configuration.GetSection("authToken").Value);

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