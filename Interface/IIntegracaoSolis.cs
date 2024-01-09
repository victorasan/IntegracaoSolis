using Microsoft.AspNetCore.Mvc;
using static IntegracaoSolis.DTO.UploadPdfDTO;

namespace IntegracaoSolis.Interface
{
    public interface IIntegracaoSolis 
    {
        Task<IActionResult> uploadPdf();
    
    }
}
