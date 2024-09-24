using Fauna.Exceptions;
using Fauna.Types;

namespace Fauna.Serialization;

public class InternalDocument
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public Module? Coll { get; set; }
    public bool? Exists { get; set; }
    public string? Cause { get; set; }
    public DateTime? Ts { get; set; }
    public IDictionary<string, object?> Data { get; } = new Dictionary<string, object?>();

    public object Get()
    {
        const string unknown = "unknown";

        if (Exists != null && !Exists.Value)
        {
            if (Id != null)
            {
                throw new NullDocumentException(Id, null, Coll ?? new Module(unknown), Cause ?? unknown);
            }

            throw new NullDocumentException(null, Name, Coll ?? new Module(unknown), Cause ?? unknown);
        }

        if (Id != null && Coll != null && Ts != null)
        {
            if (Name != null) Data.Add("name", Name);
            return new Document(Id, Coll, Ts.Value, Data);
        }

        if (Id != null && Coll != null)
        {
            return new Ref(Id, Coll);
        }

        if (Name != null && Coll != null && Ts != null)
        {
            return new NamedDocument(Name, Coll, Ts.Value, Data);
        }

        if (Name != null && Coll != null)
        {
            return new NamedRef(Name, Coll);
        }

        if (Id != null) Data.Add("id", Id);
        if (Name != null) Data.Add("name", Name);
        if (Coll != null) Data.Add("coll", Coll);
        if (Ts != null) Data.Add("ts", Ts.Value);
        if (Exists != null) Data.Add("exists", Exists.Value);
        if (Cause != null) Data.Add("cause", Cause);

        return Data;
    }
}
