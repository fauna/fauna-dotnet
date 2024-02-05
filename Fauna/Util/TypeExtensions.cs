namespace Fauna.Util;

internal static class TypeExtensions
{
    public static Type? GetGenInst(this Type ty, Type genTy)
    {
        if (!genTy.IsGenericTypeDefinition)
        {
            throw new ArgumentException($"{nameof(genTy)} is not a generic type definition.");
        }

        if (genTy.IsInterface)
        {
            foreach (var iface in ty.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == genTy)
                {
                    return iface;
                }
            }
        }
        else
        {
            Type? curr = ty;

            while (curr is not null)
            {
                if (curr.IsGenericType && curr.GetGenericTypeDefinition() == genTy)
                {
                    return curr;
                }

                curr = curr.BaseType;
            }
        }

        return null;
    }
}
