using MongoDB.Bson;
using System.Text;

namespace PI_API.services
{
    public class DatabaseFormater
    {
        private static readonly Encoding Latin1Encoding = Encoding.GetEncoding("ISO-8859-1");

        private static readonly Dictionary<string, string[]> headers = new()
        {
            ["Cnaes"] = ["_id", "Descricao"],
            ["Empresas"] = ["CnpjBase", "RazaoSocial", "NaturezaJuridica", "QualificacaoResponsavel", "CapitalSocial", "PorteEmpresa", "EnteFederativo"],
            ["Estabelecimentos"] = ["CnpjBase", "CnpjOrdem", "CnpjDV", "MatrizFilial", "NomeFantasia", "SituacaoCadastral", "DataSituacaoCadastral", "MotivoSituacaoCadastral", "CidadeExterior", "Pais", "DataInicioAtividade", "CnaePrincipal", "CnaeSecundario", "TipoLogradouro", "Logradouro", "Numero", "Complemento", "Bairro", "CEP", "UF", "Municipio", "Ddd1", "Telefone1", "Ddd2", "Telefone2", "DddFAX", "FAX", "CorreioEletronico", "SituacaoEspecial", "DataSituacaoEspecial"],
            ["Motivos"] = ["_id", "Descricao"],
            ["Municipios"] = ["_id", "Descricao"],
            ["Naturezas"] = ["_id", "Descricao"],
            ["Paises"] = ["_id", "Descricao"],
            ["Qualificacoes"] = ["_id", "Descricao"],
            ["Simples"] = ["CnpjBase", "OpcaoDoSimples", "DataOpcaoDoSimples", "DataExclusaoDoSimples", "MEI", "DataOpcaoMEI", "DataExclusaoMei"],
            ["Socios"] = ["CnpjBase", "IdentificadorSocio", "NomeSocio", "CnpjCpf", "QualificacaoSocio", "DataEntradaSociedade", "Pais", "RepresentanteLegal", "NomeRepresentante", "QualificacaoResponsavel", "FaixaEtaria"]
        };



        public static void Empresas(BsonDocument doc, ReadOnlySpan<byte> fieldSpan, int headerIdx)
        {
            switch (headers["Empresas"][headerIdx])
            {
                case "CapitalSocial":
                    if (fieldSpan.IsEmpty)
                    {
                        doc["CapitalSocial"] = (double)0;
                        return;
                    }
                    doc["CapitalSocial"] = ParseLatin1Double(fieldSpan);
                    break;
                default:
                    doc[headers["Empresas"][headerIdx]] = Latin1Encoding.GetString(fieldSpan);
                    break;
            }
        }
        public static void Estabelecimentos(BsonDocument doc, ReadOnlySpan<byte> fieldSpan, int headerIdx)
        {
            switch (headers["Estabelecimentos"][headerIdx])
            {
                case "DataSituacaoCadastral":
                case "DataInicioAtividade":
                case "DataSituacaoEspecial":
                    doc[headers["Estabelecimentos"][headerIdx]] = fieldSpan.IndexOfAnyExcept((byte)'0') == -1 ? new DateTime(1, 1, 1) : StringToDateTime(fieldSpan);
                    break;
                /*case "Numero":
                case "Ddd1":
                case "Ddd2":
                case "DddFAX":
                    if (fieldSpan.IsEmpty)
                    {
                        doc[headers["Estabelecimentos"][headerIdx]] = 0;
                        return;
                    }
                    doc[headers["Estabelecimentos"][headerIdx]] = Latin1BytesToInt(fieldSpan);
                    break;*/
                case "CnaeSecundario":
                    if(fieldSpan.IsEmpty)
                    {
                        doc["CnaeSecundario"] = new BsonArray();
                        return;
                    }
                    doc["CnaeSecundario"] = CnaesArray(fieldSpan);
                    break;
                default:
                    doc[headers["Estabelecimentos"][headerIdx]] = Latin1Encoding.GetString(fieldSpan);
                    break;
            }
        }

        public static void Socios(BsonDocument doc, ReadOnlySpan<byte> fieldSpan, int headerIdx)
        {
            switch (headers["Socios"][headerIdx])
            {
                case "DataEntradaSociedade":
                    doc["DataEntradaSociedade"] = fieldSpan.IndexOfAnyExcept((byte)'0') == -1 ? new DateTime(1, 1, 1) : StringToDateTime(fieldSpan);
                    break;
                case "IdentificadorSocio":
                case "FaixaEtaria":
                    if (fieldSpan.IsEmpty)
                    {
                        doc[headers["Socios"][headerIdx]] = 0;
                        return;
                    }
                    doc[headers["Socios"][headerIdx]] = Latin1BytesToInt(fieldSpan);
                    break;
                default:
                    doc[headers["Socios"][headerIdx]] = Latin1Encoding.GetString(fieldSpan);
                    break;
            }
        }
        public static void Simples(BsonDocument doc, ReadOnlySpan<byte> fieldSpan, int headerIdx)
        {
            switch (headers["Simples"][headerIdx])
            {
                case "DataOpcaoDoSimples":
                case "DataExclusaoDoSimples":
                case "DataOpcaoMEI":
                case "DataExclusaoMei":
                    doc[headers["Simples"][headerIdx]] = fieldSpan.IndexOfAnyExcept((byte)'0') == -1 ? new DateTime(1, 1, 1) : StringToDateTime(fieldSpan);
                    break;
                default:
                    doc[headers["Simples"][headerIdx]] = Latin1Encoding.GetString(fieldSpan);
                    break;
            }
        }


        public static int Latin1BytesToInt(ReadOnlySpan<byte> span)
        {
            int value = 0;

            foreach (byte b in span)
            {
                value = value * 10 + (b - 48); // '0' = 48
            }

            return value;
        }

        private static double ParseLatin1Double(ReadOnlySpan<byte> span)
        {
            double result = 0;
            double sign = 1;
            bool afterDecimal = false;
            double divider = 1;

            if (span[0] == (byte)'-')
            {
                sign = -1;
            }
            foreach (byte b in span)
            {
                if (b == (byte)',' || b == (byte)'.')
                {
                    afterDecimal = true;
                    continue;
                }

                int digit = b - 48; // '0' = 48

                if (!afterDecimal)
                {
                    result = result * 10 + digit;
                }
                else
                {
                    divider *= 10;
                    result += digit / divider;
                }
            }

            //Console.WriteLine($"Como veio: {Latin1Encoding.GetString(span)}\nComo saiu {result * sign}\n");
            return result * sign;
        }
        private static DateTime StringToDateTime(ReadOnlySpan<byte> date)
        {
            static int Parse2(ReadOnlySpan<byte> s)
            {
                return (s[0] - 48) * 10 +
                       (s[1] - 48);
            }

            static int Parse4(ReadOnlySpan<byte> s)
            {
                return (s[0] - 48) * 1000 +
                       (s[1] - 48) * 100 +
                       (s[2] - 48) * 10 +
                       (s[3] - 48);
            }

            int year = Parse4(date[..4]);
            int month = Parse2(date.Slice(4, 2));
            int day = Parse2(date.Slice(6, 2));

            return new DateTime(year, month, day);
        }

        private static BsonArray CnaesArray(ReadOnlySpan<byte> cnaesArray)
        {
            var bsonCnaesArray = new BsonArray();
            int start = 0;
            var empresaSpan = Latin1Encoding.GetString(cnaesArray);
            try
            {
                for (int i = 0; i < cnaesArray.Length; i++)
                {
                    if (cnaesArray[i] == (byte)',')
                    {
                        addFieldToDocument(cnaesArray, start, i);
                        start = i + 1;
                    }
                }
                addFieldToDocument(cnaesArray, start, cnaesArray.Length);
            }
            catch (Exception)
            {
                throw new Exception(Latin1Encoding.GetString(cnaesArray));
            }
            return bsonCnaesArray;
            void addFieldToDocument(ReadOnlySpan<byte> cnae, int start, int end)
            {
                ReadOnlySpan<byte> cnaeSpan = cnae.Slice(start, (end - start));
                bsonCnaesArray.Add(Latin1Encoding.GetString(cnaeSpan));
            }
        }
    }
}
