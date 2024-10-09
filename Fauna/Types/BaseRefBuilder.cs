
namespace Fauna.Types;

public class BaseRefBuilder<T>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public Module? Collection { get; set; }
    public string? Cause { get; set; }
    public bool? Exists { get; set; }
    public T? Doc { get; set; }

    public BaseRef<T> Build()
    {
        if (Collection is null) throw new ArgumentNullException(nameof(Collection));

        if (Id is not null)
        {
            if (Exists != null && !Exists.Value) return new Ref<T>(Id, Collection, Cause ?? "");
            if (Doc != null) return new Ref<T>(Id, Collection, Doc);
            return new Ref<T>(Id, Collection);
        }

        if (Name is not null)
        {
            if (Exists != null && !Exists.Value) return new NamedRef<T>(Name, Collection, Cause ?? "");
            if (Doc != null) return new NamedRef<T>(Name, Collection, Doc);
            return new NamedRef<T>(Name, Collection);
        }

        throw new ArgumentException("Id and Name cannot both be null");
    }
}
