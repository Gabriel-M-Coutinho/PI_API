using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace PI_API.dto
{
    public class LeadSearchDTO
    {
        public string? razaoSocial { get; set; } = null;
        public string? cnae { get; set; } = null;
        public string? situacaoCadastral { get; set; } = null;
        public override string ToString()
        {
            return $"LeadSearchDTO_Obj:\n- Razão Social: {razaoSocial}\n- Cnae: {cnae}\n- Situação Cadastral: {situacaoCadastral}"; 
        }
    } // valeu pela ajuda
}
