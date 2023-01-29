using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public static class ReflectionExt 
{
    public static List<Type> GetConcreteTypesOfType<TAbstract>(this Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsInterface == false && t.IsAbstract == false && typeof(TAbstract).IsAssignableFrom(t)).ToList();
    }
    public static List<Type> GetConcreteTypesOfType(this Assembly assembly, Type abstractType)
    {
        return assembly.GetTypes()
            .Where(t => t.IsInterface == false && t.IsAbstract == false && abstractType.IsAssignableFrom(t)).ToList();
    }

    // public static bool HasAttribute<TAttribute>(this Type type) where TAttribute : Attribute
    // {
    //     return type.GetCustomAttributesData().Any(d => d.AttributeType == typeof(TAttribute));
    // }
    public static bool HasAttribute<TAttribute>(this MemberInfo c) where TAttribute : Attribute
    {
        return c.GetCustomAttributesData().Any(d => d.AttributeType == typeof(TAttribute));
    }

    public static T MakeStaticMethodDelegate<T>(this MethodInfo m) where T : Delegate
    {
        return (T)Delegate.CreateDelegate(typeof(T), m);
    }

    public static Delegate MakeStaticMethodDelegate(this MethodInfo m, Type delegateType)
    {
        return Delegate.CreateDelegate(delegateType, m);
    }

    public static Type MakeCustomDelegateType(Type baseType, Type[] argTypes)
    {
        return baseType.MakeGenericType(argTypes);
    }
    public static T MakeInstanceMethodDelegate<T>(this MethodInfo m) where T : Delegate
    {
        return (T)Delegate.CreateDelegate(typeof(T), null, m);
    }

    public static Type GetMethodDelType(this MethodInfo mi)
    {
        return Delegate.CreateDelegate(null, mi).GetType();
    }
    public static Delegate MakeInstanceMethodDelegate(this MethodInfo m, Type delegateType)
    {
        return Delegate.CreateDelegate(delegateType, null, m);
    }
}