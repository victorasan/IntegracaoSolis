using Dapper;
using IntegracaoSolis.DTO;
using IntegracaoSolis.Interface;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
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
                var authorizationBearer = "bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiajJleWo3ZDhnbW8wQUI1aFdsY3p1YnA4MVdzaGsyOEtybk5mV292bW1MbmVJZTBhcm03d3pObXZQc2lOdUNJZnJldFRKcTZiang1Uk90OGlhazN3eGg0RmE3THFDY0R6RE1JcU5BYUxxT0F1TGxaMWlUZDFNMVMwRis4cmpReVFydFE5bVA1RUFHZGs2WXg2RWd1eDNCWWFuMW5acUNMekNGY1N6UEMrbG1VbDZ1Si91dGVBNlhuOVpabTRuUEpXIiwianRpIjoiOTY4MTNlODkyOTY0NDcyNTliMzZmNDdhMGJlODkxYTkiLCJuYmYiOjE3MDg0MzI0ODAsImV4cCI6MTcwODUxODg4MCwiaWF0IjoxNzA4NDMyNDgwLCJpc3MiOiJIZW1lcmEiLCJhdWQiOiJVc2VycyJ9.ns925URG-hLrm8VostNr1EzGk4rOyHFEzl07X1ikaWM";

                var requestData = new
                {
                    handshake = _configuration.GetSection("HandShake").Value,
                    remessaXml = xmlDoc
                };

                string requestDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
                var apiUrl = Convert.ToString(_configuration.GetSection("UrlRemessa").Value);

                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("accept", "application/json");
                    httpClient.DefaultRequestHeaders.Add("Authorization", authorizationBearer);

                    HttpResponseMessage response = await httpClient.PostAsync(apiUrl, new StringContent(requestDataJson, Encoding.UTF8, "application/json-patch+json"));

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("XML enviado com sucesso para a API externa.");
                        
                        var connectionString = Convert.ToString(_configuration.GetSection("SQLCONNSTR").Value);
                        
                        using (var connection = new NpgsqlConnection(connectionString))
                        {
                            string sql = "Update arquivo_remessa set remessa = @remessa";
                            await connection.ExecuteAsync(sql, new
                            {
                                remessa = true
                            });
                        }
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
            var dataFormatada = dataGeracao.ToString("dd/MM/yyyy");
            var connectionString = _configuration.GetSection("SQLCONNSTR").Value;
            string sql = "SELECT * FROM arquivo_remessa";
            List<ImportarArquivoDTo> result;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                result = connection.Query<ImportarArquivoDTo>(sql).ToList();
            }

            XElement remessa = new XElement("Remessa",
             new XElement("NumeroControle", ""),
             new XElement("Identificacao", "LAYOUT001"),
             new XElement("DataGeracao", dataFormatada),
             new XElement("Fundo",
                 new XElement("CNPJ", "38.305.947/0001-83"),
                 new XElement("Nome", "Fidc Estruturado")
             ),
             new XElement("CNPJCustodiante", "39.669.186/0001-01"),
             new XElement("CNPJOriginador", "43.299.408/0001-19"),
             new XElement("CNPJEmpresaConveniada", "01.094.694/0001-36"));
            var instrucoes = new XElement("Instrucoes");


            var aquisicoes = new XElement("Aquisicoes");

            var aquisicao = new XElement("Aquisicao");

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
                var endereco = item.rua;
                int index = endereco!.IndexOf(',');
                string enderecoAtt = endereco.Substring(0, index).Trim();
                var documentoFormat = FormatDocument(item.cnpj_cpf!);
                DateTime vencimento = Convert.ToDateTime(item.vencimento);

                DateTime dataEmissao = Convert.ToDateTime(item.data_emissao);

                var dataEmissaoFormatada = dataEmissao.ToString("dd/MM/yyyy");

                var vencimentoFormatado = vencimento.ToString("dd/MM/yyyy");

                var titulo = new XElement("Titulo",
                new XElement("Sacado",
                new XElement("TipoPessoaMF", "F"),
                new XElement("CPFCNPJ", documentoFormat),
                new XElement("Nome", item.nome_sacado),
                new XElement("Endereco",
                    new XElement("CEP", FormatarCEP(item.cep!)),
                    new XElement("Logradouro", enderecoAtt),
                    new XElement("Numero", Regex.Replace(item.rua!, "[^0-9]", "")),
                    new XElement("Complemento"),
                    new XElement("Bairro", item.bairro),
                    new XElement("Municipio", item.cidade),
                    new XElement("UF", item.uf)
                ),
                new XElement("CNPJEmpresaConveniada", "01.094.694/0001-36")),
                new XElement("DadosTitulos",
                    new XElement("TipoAtivo", "04"),
                    new XElement("NumeroBoletoBanco", item.chave!.Substring(1,11)),
                    new XElement("NumeroControleParticipante", item.chave),
                    new XElement("StatusAtivo", "01"),
                    new XElement("NumeroDocumento", item.numero_documento),
                    new XElement("DataEmissao", dataEmissaoFormatada),
                    new XElement("DataAquisicao", vencimentoFormatado),
                    new XElement("DataVencimento", vencimentoFormatado),
                    new XElement("ValorPresente", item.valor_pagamento),
                    new XElement("ValorNominal", item.valor_parcela),
                    new XElement("Especie", "04"),
                    new XElement("TipoOperacao", "01"),
                    new XElement("TaxaPre", "0.000"),
                    new XElement("TaxaMultaBoleto", "000.00"),
                    new XElement("MoraDiaria", "000.00"),
                    new XElement("RegistroCobranca", "N"),
                    new XElement("CPFCNPJOriginador", "43.299.408/0001-19"),
                    new XElement("SubTipoAtivo", "02")
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
             aquisicao.Add(cedente);
             aquisicoes.Add(aquisicao);
             instrucoes.Add(aquisicoes);
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

            //remessa.Save("C:\\Arquivo\\Remessa.xml");

            XDocument doc = XDocument.Parse(remessa.ToString());

            string xmlInOneLine = doc.ToString(SaveOptions.DisableFormatting);

            doc.Save("C:\\Arquivo\\Remessa.xml", SaveOptions.DisableFormatting);

            return xmlInOneLine;
        }

        public static string FormatDocument(string document)
        {
            if (document.Length == 11)
            {
                // CPF format: 000.000.000-00
                return Convert.ToUInt64(document).ToString(@"000\.000\.000\-00");
            }
            else if (document.Length == 14)
            {
                // CNPJ format: 00.000.000/0000-00
                return Convert.ToUInt64(document).ToString(@"00\.000\.000\/0000\-00");
            }
            else
            {
                // Documento inválido
                return "Documento inválido";
            }
        }
        static string FormatarCEP(string cep)
        {
            // Verificar se o CEP é válido
            if (cep.Length == 8)
            {
                // Adicionar a máscara de CEP
                return $"{cep.Substring(0, 5)}-{cep.Substring(5)}";
            }
            else
            {
                // CEP inválido, retornar o valor original
                return cep;
            }
        }
    }
}
