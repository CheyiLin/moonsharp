#if NETFX_CORE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MoonSharp.Interpreter
{
	[Flags]
	internal enum BindingFlags
	{
		Static = 1,
		InvokeMethod = 0,
		Public = 2,
		NonPublic = 0,
		GetField = 0
	}

	internal static class NetCoreReflectionHelpers_Extensions
	{
		internal static IEnumerable<MethodInfo> GetMethods(this Type t, BindingFlags flags)
		{
			TypeInfo ti = t.GetTypeInfo();
			IEnumerable<MethodInfo> methods = ti.DeclaredMethods;

			if (flags.HasFlag(BindingFlags.Static))
				methods = methods.Where(mi => mi.IsStatic);
			else
				methods = methods.Where(mi => !mi.IsStatic);

			if (flags.HasFlag(BindingFlags.Public) && !flags.HasFlag(BindingFlags.NonPublic))
				methods = methods.Where(mi => mi.IsPublic);

			return methods;
		}

		internal static IEnumerable<FieldInfo> GetFields(this Type t, BindingFlags flags)
		{
			TypeInfo ti = t.GetTypeInfo();
			IEnumerable<FieldInfo> fields = ti.DeclaredFields;

			if (flags.HasFlag(BindingFlags.Static))
				fields = fields.Where(mi => mi.IsStatic);
			else
				fields = fields.Where(mi => !mi.IsStatic);

			if (flags.HasFlag(BindingFlags.Public) && !flags.HasFlag(BindingFlags.NonPublic))
				fields = fields.Where(mi => mi.IsPublic);

			return fields;
		}

	}
}



#endif