using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IntegracaoSolis.DTO
{
    public class CountArquivoDTO
    {
        public string? filename { get; set; }
        public string? id_ccb { get; set; }
        public string? cnpj_cpf { get; set; }
        public string? numero_documento { get; set; }
        public string? proposta { get; set; }
        public DateTime? data_emissao { get; set; } 
        public int valor_total { get; set; }
    }
}
