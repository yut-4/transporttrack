namespace TransportTrack.Domain.Core;

public abstract class BaseEntity
{
    public int Id { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    protected BaseEntity()
    {
        CreatedAt = DateTime.Now;
        IsDeleted = false;
    }

    protected BaseEntity(DateTime createdAt)
    {
        CreatedAt = createdAt;
        IsDeleted = false;
    }
}
