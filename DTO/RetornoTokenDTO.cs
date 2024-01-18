using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IntegracaoSolis.DTO
{
    public class RetornoTokenDTO
    {
        public string? access_token { get; set; }
        public string? token_type { get; set; }
        public int expires_in { get; set; }

    }
}
