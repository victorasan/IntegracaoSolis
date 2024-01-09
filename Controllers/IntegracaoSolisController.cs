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

        public IntegracaoSolisController(ILogger<IntegracaoSolisController> logger, IIntegracaoSolis integracaoSolis, IDepositoPdf depositoPdf)
        {
            _logger = logger;
            _integracaoSolis = integracaoSolis;
            _depositoPdf = depositoPdf;
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

        [HttpGet]
        [Route("ccbDepositadas")]
        public IActionResult DepositoCCB()
        {
            _depositoPdf.DepositoPdf();
            return Ok();
        }

        [HttpGet]
        [Route("envioRemessa")]
        public IActionResult EnvioRemessa()
        {
            return Ok();
        }
    }
}