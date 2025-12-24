namespace HR.ProjectManagement.Entities.Common;

public class BaseEntity
{
    public int Id { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? ModifiedBy { get; set; }
}
