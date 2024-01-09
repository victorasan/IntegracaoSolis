using Microsoft.AspNetCore.Mvc;
using static IntegracaoSolis.DTO.UploadPdfDTO;

namespace IntegracaoSolis.Interface
{
    public interface IUploadCommand 
    {
        Task<bool> UploadPDFCommand(ResponseDTO response);
    }
}
