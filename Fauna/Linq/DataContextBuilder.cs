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

        var cols = new Dictionary<Type, DataContext.Collection>();
        foreach (var ty in colTypes) cols[ty] = BuildColImpl(ty);

        var db = (DB)Activator.CreateInstance(dbType)!;
        db.Init(client, cols);
        return db;
    }

    private static bool IsColType(Type ty) =>
        ty.GetInterfaces().Where(iface => iface == typeof(DataContext.Collection)).Count() > 0;

    private static void ValidateColType(Type ty)
    {
        var isInterface = ty.IsInterface;
        var isGeneric = ty.IsGenericType;
        var isPublic = ty.IsNestedPublic;
        var colDefs = ty.GetInterfaces().Where(IsColTypeDef).ToList();
        var implementsCol = colDefs.Any();
        var implementsMultipleCols = colDefs.Count() > 1;

        var errors = new List<string>();

        if (!isInterface) errors.Add("Must be an interface.");
        if (isGeneric) errors.Add("Cannot be generic.");
        if (!isPublic) errors.Add("Must be public.");
        if (!implementsCol) errors.Add("Must implement Collection<>.");
        if (implementsMultipleCols) errors.Add("Cannot implement Collection<> multiple times.");

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

    private static DataContext.Collection BuildColImpl(Type ty)
    {
        throw new NotImplementedException();
    }

    // helpers

    private static bool IsColTypeDef(Type ty) =>
        ty.IsGenericType && ty.GetGenericTypeDefinition() == typeof(DataContext.Collection<>);

    private static Type GetDocType(Type ty)
    {
        Debug.Assert(IsColTypeDef(ty));
        return ty.GetGenericArguments()[0];
    }
}
