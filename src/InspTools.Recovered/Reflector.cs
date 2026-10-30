using System;
using System.Reflection;
using System.Runtime.CompilerServices;

public class Reflector
{
	private string m_ns;

	private Assembly m_asmb;

	public Reflector(string ns)
		: this(ns, ns)
	{
	}

	public Reflector(string an, string ns)
	{
		m_ns = ns;
		m_asmb = null;
		AssemblyName[] referencedAssemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
		foreach (AssemblyName assemblyName in referencedAssemblies)
		{
			if (assemblyName.FullName.StartsWith(an))
			{
				m_asmb = Assembly.Load(assemblyName);
				break;
			}
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public Type GetType(string typeName)
	{
		Type type = null;
		string[] array = typeName.Split('.');
		if (array.Length != 0)
		{
			type = m_asmb.GetType(m_ns + "." + array[0]);
		}
		for (int i = 1; i < array.Length; i++)
		{
			type = type.GetNestedType(array[i], BindingFlags.NonPublic);
		}
		return type;
	}

	public object New(string name, params object[] parameters)
	{
		ConstructorInfo[] constructors = GetType(name).GetConstructors();
		foreach (ConstructorInfo constructorInfo in constructors)
		{
			try
			{
				return constructorInfo.Invoke(parameters);
			}
			catch
			{
			}
		}
		return null;
	}

	public object Call(object obj, string func, params object[] parameters)
	{
		return Call2(obj, func, parameters);
	}

	public object Call2(object obj, string func, object[] parameters)
	{
		return CallAs2(obj.GetType(), obj, func, parameters);
	}

	public object CallAs(Type type, object obj, string func, params object[] parameters)
	{
		return CallAs2(type, obj, func, parameters);
	}

	public object CallAs2(Type type, object obj, string func, object[] parameters)
	{
		return type.GetMethod(func, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(obj, parameters);
	}

	public object Get(object obj, string prop)
	{
		return GetAs(obj.GetType(), obj, prop);
	}

	public object GetAs(Type type, object obj, string prop)
	{
		return type.GetProperty(prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(obj, null);
	}

	public object GetEnum(string typeName, string name)
	{
		return GetType(typeName).GetField(name).GetValue(null);
	}
}
