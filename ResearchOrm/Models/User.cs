using System.ComponentModel.DataAnnotations;

namespace ResearchOrm.Models
{
    public class User : UserBase
    {
        [MaxLength(250), Required]
        public string Name { get; set; }
    }
}
