using IntegracaoSolis.Interface;
using Microsoft.AspNetCore.Mvc;
using static IntegracaoSolis.DTO.UploadPdfDTO;

namespace IntegracaoSolis.Handler
{
    using System;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Dapper;
    using Microsoft.AspNetCore.Http.HttpResults;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Npgsql;
    using OfficeOpenXml;

    public class ImportarArquivoHandler : IImportarArquivo
    {

        public readonly IConfiguration _configuration;
        public ImportarArquivoHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public bool ImportarArquivo()
        {
            string csvFilePath = Path.GetFullPath("C:\\Users\\VictorAlvesdosSantos\\MEUCASHCARD SERVIÇOS TECNOLOGICOS E FINANCEIROS LTDA\\Engenharia de Dados - Integracao Solis\\Arquivos\\FC00002CessaoSIAPE20022024ARQ1envio.csv");

            // Ler o arquivo CSV
            var csvData = File.ReadAllLines(csvFilePath);

            var connectionString = Convert.ToString(_configuration.GetSection("SQLCONNSTR").Value);
            var tableName = "arquivo_remessa";

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    foreach (var line in csvData)
                    {
                        // Dividir a linha do CSV em colunas
                        var values = line.Split(';');

                        // Criar a instrução de inserção
                        var insertQuery = $"INSERT INTO {tableName} (id, cnpj_cpf, vencimento, valor_parcela, numero_documento, nome_sacado, rua, bairro, cep, cidade, uf, telefone, chave, data_emissao, proposta, remessa, valor_futuro, valor_pagamento) VALUES (@id, @CNPJ_CPF, @VENCIMENTO, @VALOR_PARCELA, @NUMERO_DOCUMENTO,@NOME_SACADO, @RUA, @BAIRRO, @CEP, @CIDADE, @UF, @TELEFONE, @CHAVE, @data_emissao, @proposta, @Remessa, @valor_futuro, @valor_pagamento)";

                        using var cmd = new NpgsqlCommand(insertQuery, connection);
                        cmd.Parameters.AddWithValue("id", Guid.NewGuid());
                        cmd.Parameters.AddWithValue("CNPJ_CPF", Convert.ToString(values[0]));
                        cmd.Parameters.AddWithValue("VENCIMENTO", Convert.ToDateTime(values[1]));
                        cmd.Parameters.AddWithValue("VALOR_PARCELA", Convert.ToDecimal(values[2]));
                        cmd.Parameters.AddWithValue("NUMERO_DOCUMENTO", Convert.ToString(values[3]));
                        cmd.Parameters.AddWithValue("NOME_SACADO", Convert.ToString(values[4]));
                        cmd.Parameters.AddWithValue("RUA", Convert.ToString(values[5]));
                        cmd.Parameters.AddWithValue("BAIRRO", Convert.ToString(values[6]));
                        cmd.Parameters.AddWithValue("CEP", Convert.ToString(values[7]));
                        cmd.Parameters.AddWithValue("CIDADE", Convert.ToString(values[8]));
                        cmd.Parameters.AddWithValue("UF", Convert.ToString(values[9]));
                        cmd.Parameters.AddWithValue("TELEFONE", Convert.ToString(values[10]));
                        cmd.Parameters.AddWithValue("CHAVE", Convert.ToString(values[11]));
                        cmd.Parameters.AddWithValue("data_emissao", Convert.ToDateTime(values[12]));
                        cmd.Parameters.AddWithValue("proposta", Convert.ToString(values[13]));
                        cmd.Parameters.AddWithValue("Remessa", Convert.ToBoolean(values[14]));
                        cmd.Parameters.AddWithValue("valor_futuro", Convert.ToDecimal(values[15]));
                        cmd.Parameters.AddWithValue("valor_pagamento", Convert.ToDecimal(values[16]));


                        // Executar a instrução de inserção
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}