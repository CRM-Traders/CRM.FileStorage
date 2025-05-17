namespace CRM.FileStorage.Domain.Common.Entities;

public abstract class Entity(Guid id)
{
    public Guid Id { get; protected set; } = id;

    protected Entity() : this(Guid.NewGuid())
    {
    }

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj is not Entity other)
            return false;

        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode() * 41;
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        return left?.Equals(right) ?? right is null;
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}