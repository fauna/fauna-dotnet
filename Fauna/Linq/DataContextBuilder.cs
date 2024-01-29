using Fauna.Mapping;
using System.Diagnostics;
using System.Reflection;

namespace Fauna.Linq;

internal class DataContextBuilder<DB> where DB : DataContext
{
    public DB Build(Client client)
    {
        var dbType = typeof(DB);
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        var colTypes = dbType.GetNestedTypes(flags).Where(IsColType).ToList();
        var colProps = dbType.GetProperties(flags).Where(IsColProp).ToList();

        foreach (var ty in colTypes)
        {
            ValidateColType(ty);
        }

        foreach (var p in colProps)
        {
            ValidateColProp(colTypes, p);
        }

        var colImpls = new Dictionary<Type, DataContext.Collection>();
        foreach (var ty in colTypes)
        {
            colImpls[ty] = (DataContext.Collection)Activator.CreateInstance(ty)!;
            var nameAttr = ty.GetCustomAttribute<DataContext.NameAttribute>();
            var colName = nameAttr?.Name ?? ty.Name;
        }

        var db = (DB)Activator.CreateInstance(dbType)!;
        db.Init(client, colImpls, new MappingContext(colImpls.Values));
        return db;
    }

    private static bool IsColType(Type ty) =>
        ty.GetInterfaces().Where(iface => iface == typeof(DataContext.Collection)).Any();

    private static void ValidateColType(Type ty)
    {
        var isGeneric = ty.IsGenericType;
        var colDef = GetColBase(ty);

        var errors = new List<string>();

        if (isGeneric) errors.Add("Cannot be generic.");
        if (colDef is null) errors.Add("Must inherit Collection<>.");

        if (errors.Any())
        {
            throw new InvalidOperationException(
                $"Invalid collection type: {string.Join(" ", errors)}");
        }
    }

    private static bool IsColProp(PropertyInfo prop)
    {
        var getter = prop.GetGetMethod();

        if (getter is null) return false;
        if (getter.IsStatic) return false;

        var retType = getter.ReturnType;
        if (!IsColType(retType)) return false;

        return true;
    }

    private static void ValidateColProp(List<Type> colTypes, PropertyInfo prop)
    {
        var nullCtx = new NullabilityInfoContext();
        var nullInfo = nullCtx.Create(prop);
        var getter = prop.GetGetMethod()!;
        var retType = getter.ReturnType;

        var returnsValidColType = colTypes.Contains(retType);
        var isNullable = nullInfo.ReadState is NullabilityState.Nullable;

        var errors = new List<string>();

        if (!returnsValidColType) errors.Add("Must return a nested collection type.");
        if (isNullable) errors.Add("Cannot be nullable.");

        if (errors.Any())
        {
            throw new InvalidOperationException(
                $"Invalid collection property: {string.Join(" ", errors)}");
        }
    }

    // helpers

    private static Type? GetColBase(Type ty)
    {
        var colType = typeof(DataContext.Collection<>);
        Type? curr = ty;

        while (curr is not null)
        {
            if (curr.IsGenericType && curr.GetGenericTypeDefinition() == colType)
            {
                return curr;
            }

            curr = curr.BaseType;
        }

        return null;
    }

    private static Type GetDocType(Type ty)
    {
        var col = GetColBase(ty);
        Debug.Assert(col is not null);
        return col.GetGenericArguments()[0];
    }
}
