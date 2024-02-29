using System.ComponentModel.DataAnnotations;

namespace ResearchOrm.Models
{
    public class UserGroup
    {
        public int Id { get; set; }
        public List<User> Users { get; set; }
    }
}
