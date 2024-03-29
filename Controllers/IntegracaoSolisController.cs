using Hangfire;
using IntegracaoSolis.Interface;
using Microsoft.AspNetCore.Mvc;

namespace IntegracaoSolis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IntegracaoSolisController : ControllerBase
    {
        private readonly ILogger<IntegracaoSolisController> _logger;
        private readonly IIntegracaoSolis _integracaoSolis;
        private readonly IDepositoPdf _depositoPdf;
        private readonly IEnvioRemessa _envioRemessa;
        private readonly IImportarArquivo _importarArquivo;

        public IntegracaoSolisController(ILogger<IntegracaoSolisController> logger, IIntegracaoSolis integracaoSolis, IDepositoPdf depositoPdf, IEnvioRemessa envioRemessa, IImportarArquivo importarArquivo)
        {
            _logger = logger;
            _integracaoSolis = integracaoSolis;
            _depositoPdf = depositoPdf;
            _envioRemessa = envioRemessa;
            _importarArquivo = importarArquivo;
        }

        [HttpPost]
        [Route("UploadPdf")]
        public IActionResult StartPdfProcessingJob()
        {
            try
            {
                RecurringJob.AddOrUpdate("ProcessarPDFs", () => _integracaoSolis.uploadPdf(), Cron.Daily);
                return Ok("Job para processamento de PDFs agendado com sucesso.");
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        [HttpPost]
        [Route("ccbDepositadas")]
        public IActionResult DepositoCCB()
        {
            _depositoPdf.DepositoPdf();
            return Ok("Dep�sito de CCB realizado");
        }

        [HttpPost]
        [Route("envioRemessa")]
        public IActionResult EnvioRemessa()
        {
            _envioRemessa.EnvioRemessa();
            return Ok();
        }

        [HttpPost]
        [Route("importarArquivo")]
        public IActionResult ImportarArquivo()
        {
            var arquivo = _importarArquivo.ImportarArquivo();

            if (arquivo)
            {
                return Ok("Arquivo importado!");
            }

            return BadRequest("Erro ao importar");
            
        }
    }
}