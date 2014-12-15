﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonSharp.Interpreter.CoreLib;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.RuntimeAbstraction;

namespace MoonSharp.Interpreter
{
	public static class ModuleRegister
	{
		public static Table RegisterCoreModules(this Table table, CoreModules modules)
		{
			modules = Platform.Current.FilterSupportedCoreModules(modules);

			if (modules.Has(CoreModules.GlobalConsts)) RegisterConstants(table);
			if (modules.Has(CoreModules.TableIterators)) RegisterModuleType<TableIteratorsModule>(table);
			if (modules.Has(CoreModules.Basic)) RegisterModuleType<BasicModule>(table);
			if (modules.Has(CoreModules.Metatables)) RegisterModuleType<MetaTableModule>(table);
			if (modules.Has(CoreModules.String)) RegisterModuleType<StringModule>(table);
			if (modules.Has(CoreModules.LoadMethods)) RegisterModuleType<LoadModule>(table);
			if (modules.Has(CoreModules.Table)) RegisterModuleType<TableModule>(table);
			if (modules.Has(CoreModules.Table)) RegisterModuleType<TableModule_Globals>(table);
			if (modules.Has(CoreModules.ErrorHandling)) RegisterModuleType<ErrorHandlingModule>(table);
			if (modules.Has(CoreModules.Math)) RegisterModuleType<MathModule>(table);
			if (modules.Has(CoreModules.Coroutine)) RegisterModuleType<CoroutineModule>(table);
			if (modules.Has(CoreModules.Bit32)) RegisterModuleType<Bit32Module>(table);
			if (modules.Has(CoreModules.Dynamic)) RegisterModuleType<DynamicModule>(table);
			if (modules.Has(CoreModules.OS_System)) RegisterModuleType<OsSystemModule>(table);
			if (modules.Has(CoreModules.OS_Time)) RegisterModuleType<OsTimeModule>(table);
			if (modules.Has(CoreModules.IO)) RegisterModuleType<IoModule>(table);
			if (modules.Has(CoreModules.Debug)) RegisterModuleType<DebugModule>(table);

			return table;
		}



		public static Table RegisterConstants(this Table table)
		{
			DynValue moonsharp_table = DynValue.NewTable(table.OwnerScript);
			Table m = moonsharp_table.Table;

			table.Set("_G", DynValue.NewTable(table));
			table.Set("_VERSION", DynValue.NewString(string.Format("MoonSharp {0}", Script.VERSION)));
			table.Set("_MOONSHARP", moonsharp_table);

			m.Set("version", DynValue.NewString(Script.VERSION));
			m.Set("luacompat", DynValue.NewString(Script.LUA_VERSION));
			m.Set("platform", DynValue.NewString(Platform.Current.Name));

			return table;
		}



		public static Table RegisterModuleType(this Table gtable, Type t)
		{
			Table table = CreateModuleNamespace(gtable, t);

#if PORTABLENET4
			BindingFlags method_bflags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			BindingFlags field_bflags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
#else
			BindingFlags method_bflags = BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic;
			BindingFlags field_bflags = BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public | BindingFlags.NonPublic;
#endif

			foreach (MethodInfo mi in t.GetMethods(method_bflags))
			{
				if (mi.GetCustomAttributes(typeof(MoonSharpMethodAttribute), false).ToArray().Length > 0)
				{
					MoonSharpMethodAttribute attr = (MoonSharpMethodAttribute)mi.GetCustomAttributes(typeof(MoonSharpMethodAttribute), false).First();

					if (!ConversionHelper.CheckCallbackSignature(mi))
							throw new ArgumentException(string.Format("Method {0} does not have the right signature.", mi.Name));

					Func<ScriptExecutionContext, CallbackArguments, DynValue> func = (Func<ScriptExecutionContext, CallbackArguments, DynValue>)Delegate.CreateDelegate(typeof(Func<ScriptExecutionContext, CallbackArguments, DynValue>), mi);

					string name = (!string.IsNullOrEmpty(attr.Name)) ? attr.Name : mi.Name;

					table.Set(name, DynValue.NewCallback(func, name));
				}
				else if (mi.Name == "MoonSharpInit")
				{
					object[] args = new object[2] { gtable, table };
					mi.Invoke(null, args);
				}
			}

			foreach (FieldInfo fi in t.GetFields(field_bflags).Where(_mi => _mi.GetCustomAttributes(typeof(MoonSharpMethodAttribute), false).ToArray().Length > 0))
			{
				MoonSharpMethodAttribute attr = (MoonSharpMethodAttribute)fi.GetCustomAttributes(typeof(MoonSharpMethodAttribute), false).First();
				string name = (!string.IsNullOrEmpty(attr.Name)) ? attr.Name : fi.Name;

				RegisterScriptField(fi, null, table, t, name);
			}
			foreach (FieldInfo fi in t.GetFields(field_bflags).Where(_mi => _mi.GetCustomAttributes(typeof(MoonSharpConstantAttribute), false).ToArray().Length > 0))
			{
				MoonSharpConstantAttribute attr = (MoonSharpConstantAttribute)fi.GetCustomAttributes(typeof(MoonSharpConstantAttribute), false).First();
				string name = (!string.IsNullOrEmpty(attr.Name)) ? attr.Name : fi.Name;

				RegisterScriptFieldAsConst(fi, null, table, t, name);
			}

			return gtable;
		}

		private static void RegisterScriptFieldAsConst(FieldInfo fi, object o, Table table, Type t, string name)
		{
			if (fi.FieldType == typeof(string))
			{
				string val = fi.GetValue(o) as string;
				table.Set(name, DynValue.NewString(val));
			}
			else if (fi.FieldType == typeof(double))
			{
				double val = (double)fi.GetValue(o);
				table.Set(name, DynValue.NewNumber(val));
			}
			else
			{
				throw new ArgumentException(string.Format("Field {0} does not have the right type - it must be string or double.", name));
			}
		}

		private static void RegisterScriptField(FieldInfo fi, object o, Table table, Type t, string name)
		{
			if (fi.FieldType != typeof(string))
			{
				throw new ArgumentException(string.Format("Field {0} does not have the right type - it must be string.", name));
			}

			string val = fi.GetValue(o) as string;

			DynValue fn = table.OwnerScript.LoadFunction(val, table, name);

			table.Set(name, fn);
		}


		private static Table CreateModuleNamespace(Table gtable, Type t)
		{
			MoonSharpModuleAttribute attr = (MoonSharpModuleAttribute)t.GetCustomAttributes(typeof(MoonSharpModuleAttribute), false).First();

			if (string.IsNullOrEmpty(attr.Namespace))
			{
				return gtable;
			}
			else
			{
				Table table = null;

				DynValue found = gtable.Get(attr.Namespace);

				if (found.Type == DataType.Table)
				{
					table = found.Table;
				}
				else
				{
					table = new Table(gtable.OwnerScript);
					gtable.Set(attr.Namespace, DynValue.NewTable(table));
				}


				DynValue package = gtable.RawGet("package");

				if (package == null || package.Type != DataType.Table)
				{
					gtable.Set("package", package = DynValue.NewTable(gtable.OwnerScript));
				}


				DynValue loaded = package.Table.RawGet("loaded");

				if (loaded == null || loaded.Type != DataType.Table)
				{
					package.Table.Set("loaded", loaded = DynValue.NewTable(gtable.OwnerScript));
				}

				loaded.Table.Set(attr.Namespace, DynValue.NewTable(table));

				return table;
			}
		}

		public static Table RegisterModuleType<T>(this Table table)
		{
			return RegisterModuleType(table, typeof(T));
		}


	}
}
