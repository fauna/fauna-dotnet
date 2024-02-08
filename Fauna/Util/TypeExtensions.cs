using System.Reflection;
using System.Runtime.CompilerServices;

namespace Fauna.Util;

internal static class TypeExtensions
{
    public static bool IsClosureType(this Type ty)
    {
        var compilerGen = ty.GetCustomAttribute<CompilerGeneratedAttribute>() != null;
        // check for the closure class name pattern. see
        // https://stackoverflow.com/questions/2508828/where-to-learn-about-vs-debugger-magic-names/2509524#2509524
        var dcName = ty.Name.StartsWith("<>c__DisplayClass");

        return compilerGen && dcName;
    }

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
