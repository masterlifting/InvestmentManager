using InvestmentManager.Entities.Basic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Market
{
    public class ProxyAddress : BaseEntity
    {
        [StringLength(10)]
        [Required]
        public string Scheme { get; set; }
        [StringLength(50)]
        [Required]
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
