namespace ResearchOrm.Models;

public abstract class UserBase
{
    public long ID { get; set; }
    public List<UserGroup> Groups { get; set; }
}