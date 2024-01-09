using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IntegracaoSolis.DTO
{
    public class UploadPdfDTO
    {
        public string? propertyName { get; set; }
        public string? errorMessage { get; set; }
        public int severity { get; set; }
        public string? errorCode { get; set; }
        public FormattedMessagePlaceholderValues? formattedMessagePlaceholderValues { get; set; }

        public class FormattedMessagePlaceholderValues
        {
        }

        public class ApiResponse
        {
            public bool success { get; set; }
            public List<Error>? errors { get; set; }
            public string? url { get; set; }
        }

        public class ResponseDTO
        {
            public string? clientId { get; set; }
            public string? url { get; set; }
            public object? urlNonTradable { get; set; }
            public object? urlEndorsement { get; set; }
            public object? urlSigned { get; set; }
            public string? fileName { get; set; }
            public string? createdAt { get; set; }
            public object? version { get; set; }
            public int status { get; set; }
            public string? id { get; set; }

        }
    }
}
