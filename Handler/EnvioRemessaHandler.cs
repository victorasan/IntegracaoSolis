using Dapper;
using IntegracaoSolis.DTO;
using IntegracaoSolis.Interface;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using static IntegracaoSolis.DTO.RetornoTokenDTO;

namespace IntegracaoSolis.Handler

{
    public class EnvioRemessaHandler : IEnvioRemessa
    {
        private readonly IConfiguration _configuration;

        public EnvioRemessaHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<bool> EnvioRemessa()
        {
            try
            {
                var xmlDoc = CreateXmlDocument();

                var requestData = new
                {
                    handshake = "55666D9A-36DA-4E6E-8",
                    remessaXml = xmlDoc
                };

                string requestDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
                var apiUrl = Convert.ToString(_configuration.GetSection("UrlRemessa").Value);

                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiOEpJUzVMT2ptcE9jWmNrMmlkSzNjdHNkYkd1Y0hBWkw1blFrbnBkdmZscnZNclpXd3BJS1RVSnJpK2w5L29uVmdaZVJDeFo4R2MrRGsvaisreEoxREQ0S09QMkdabm5adjZ3b2k5UkJqUzk1Q1dKMENuODhLZFZMTXdLNldpWFI2N2orWVJ0TmtUbFB3eU1pcEMwU0l4RjZJWVVSWTJ2QjRkaWdFRzhvMzdpWUh2SG1hTk1sazVWSXJpMGlocFd0IiwianRpIjoiOTA4MzI0YjhhOWM0NDg0YWI4NzczNjdiMGFkZTg3Y2QiLCJuYmYiOjE3MDUzNTA0NTgsImV4cCI6MTcwNTQzNjg1OCwiaWF0IjoxNzA1MzUwNDU4LCJpc3MiOiJIZW1lcmEiLCJhdWQiOiJVc2VycyJ9.0l7i9g2PsQwVAgSbwKayvJ2rvoz4-hU8Slc1Kf1q6hY");

                    HttpResponseMessage response = await httpClient.PostAsync(apiUrl, new StringContent(requestDataJson, Encoding.UTF8, "application/json"));

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("XML enviado com sucesso para a API externa.");
                    }
                    else
                    {
                        Console.WriteLine($"Erro ao enviar o XML. Código de status: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }



            Console.WriteLine("Arquivo XML gerado com sucesso.");
            return true;
        }

        public string CreateXmlDocument()
        {
            var dataGeracao = DateTime.Today;
            var connectionString = _configuration.GetSection("SQLCONNSTR").Value;
            string sql = "SELECT * FROM arquivo_remessa";
            List<ImportarArquivoDTo> result;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                result = connection.Query<ImportarArquivoDTo>(sql).ToList();
            }

            XElement remessa = new XElement("Remessa",
             new XElement("NumeroControle", "C01SoliSiape08122023"),
             new XElement("Identificacao", "LAYOUT001"),
             new XElement("DataGeracao", dataGeracao),
             new XElement("Fundo",
                 new XElement("CNPJ", "38.305.947/0001-83"),
                 new XElement("Nome", "Fidc Estruturado")
             ),
             new XElement("CNPJCustodiante", "39.669.186/0001-01"),
             new XElement("CNPJOriginador", "43.299.408/0001-19"),
             new XElement("CNPJEmpresaConveniada", "01.094.694/0001-36");
             var instrucoes = new XElement("Instrucoes",
                    new XElement("Aquisicoes",
                        new XElement("Aquisicao")));

            var cedente = new XElement("Cedente",
                                new XElement("TipoPessoaMF", "J"),
                                new XElement("CPFCNPJ", "43.299.408/0001-19"),
                                new XElement("Nome", "MEUCASHCARD SERVIÇOS TECNOLOGICOS E FINANCEIROS LTDA"),
                                new XElement("Coobrigacao", "01"));

            var titulos = new XElement("Titulos");

            foreach (var item in result)
            {

                var dadosTitulos = result.Where(x => x.cnpj_cpf == item.cnpj_cpf);
                var total = dadosTitulos.Sum(x => x.valor_parcela);


                var titulo = new XElement("Titulo",
                new XElement("Sacado",
                new XElement("TipoPessoaMF", "F"),
                new XElement("CPFCNPJ", item.cnpj_cpf),
                new XElement("Nome", item.nome_sacado),
                new XElement("Endereco",
                    new XElement("CEP", item.cep),
                    new XElement("Logradouro", item.rua),
                    new XElement("Numero", item.rua),
                    new XElement("Complemento"),
                    new XElement("Bairro", item.bairro),
                    new XElement("Municipio", item.cidade),
                    new XElement("UF", item.uf)
                ),
                new XElement("CNPJEmpresaConveniada", "01.094.694/0001-36")),
                new XElement("DadosTitulos",
                    new XElement("TipoAtivo", "04"),
                    new XElement("NumeroBoletoBanco", item.chave),
                    new XElement("NumeroControleParticipante", item.chave),
                    new XElement("StatusAtivo", "01"),
                    new XElement("NumeroDocumento", item.numero_documento),
                    new XElement("DataEmissao", item.vencimento),
                    new XElement("DataAquisicao", item.vencimento),
                    new XElement("DataVencimento", item.vencimento),
                    new XElement("ValorPresente", item.valor_parcela),
                    new XElement("ValorNominal", total),
                    new XElement("Especie", "04"),
                    new XElement("TipoOperacao", "01"),
                    new XElement("TaxaPre", "0.000"),
                    new XElement("TaxaMultaBoleto", "000.00"),
                    new XElement("MoraDiaria", "000.00"),
                    new XElement("RegistroCobranca", "N")
                ),
                new XElement("Lastros",
                    new XElement("ValorTotalLastros", total),
                    new XElement("Lastro",
                        new XElement("Documento",
                            new XElement("TipoDocumento", "03"),
                            new XElement("LastroId", item.chave),
                            new XElement("NumeroDocumento", item.numero_documento),
                            new XElement("ChaveDocumento", item.chave),
                            new XElement("ValorTotalDocumento", total),
                            new XElement("NSU")
                        )
                    )
                ));
                titulos.Add(titulo);
            }
             cedente.Add(titulos);
             instrucoes.Add(cedente);
             remessa.Add(instrucoes);

            var totalTotal = result.Sum(x => x.valor_parcela);
            var pagamentos = new XElement("Pagamentos",
                new XElement("PagamentoCessao",
                    new XElement("CPFCNPJ", "43.299.408/0001-19"),
                    new XElement("CodigoBanco", "033"),
                    new XElement("CodigoAgencia", "205"),
                    new XElement("dvAgencia", "0"),
                    new XElement("Conta", "00013005842"),
                    new XElement("dvConta", "6"),
                    new XElement("ValorTransacao", totalTotal)
                )
            );

            remessa.Add(pagamentos);

            remessa.Save("C:\\Arquivo\\Remessa.xml");

            return remessa.ToString();
        }
    }
}
