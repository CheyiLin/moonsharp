﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.Interop
{
	internal class UserDataDescriptor
	{
		internal string Name { get; private set; }
		internal Type Type { get; private set; }
		internal InteropAccessMode AccessMode { get; private set; }
		internal string FriendlyName { get; private set; }

		private Dictionary<string, UserDataMethodDescriptor> m_Methods = new Dictionary<string, UserDataMethodDescriptor>();
		private Dictionary<string, UserDataPropertyDescriptor> m_Properties = new Dictionary<string, UserDataPropertyDescriptor>();

		internal UserDataDescriptor(Type type, InteropAccessMode accessMode, string friendlyName)
		{
			Type = type;
			Name = type.FullName;
			AccessMode = accessMode;
			FriendlyName = friendlyName;

			if (AccessMode != InteropAccessMode.HideMembers)
			{
#if PORTABLENET4
				BindingFlags bindingflags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
#else
				BindingFlags bindingflags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Static;
#endif
				foreach (MethodInfo mi in type.GetMethods(bindingflags))
				{
					if (CheckVisibility(mi.GetCustomAttributes(true), mi.IsPublic))
					{
						if (mi.IsSpecialName)
							continue;

						var md = new UserDataMethodDescriptor(mi, this.AccessMode);

						if (m_Methods.ContainsKey(md.Name))
							throw new ArgumentException(string.Format("{0}.{1} has overloads", Name, md.Name));

						m_Methods.Add(md.Name, md);
					}
				}

				foreach (PropertyInfo pi in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
				{
					if (CheckVisibility(pi.GetCustomAttributes(true), pi.GetGetMethod().IsPublic || pi.GetSetMethod().IsPublic))
					{
						var pd = new UserDataPropertyDescriptor(pi, this.AccessMode);
						m_Properties.Add(pd.Name, pd);
					}
				}
			}
		}

		private bool CheckVisibility(object[] attributes, bool isPublic)
		{
			MoonSharpVisibleAttribute va = attributes.OfType<MoonSharpVisibleAttribute>().SingleOrDefault();

			if (va != null)
				return va.Visible;
			else
				return isPublic;
		}



		internal DynValue Index(Script script, object obj, string idxname)
		{
			UserDataMethodDescriptor mdesc;

			if (m_Methods.TryGetValue(idxname, out mdesc))
				return DynValue.NewCallback(mdesc.GetCallback(script, obj));

			UserDataPropertyDescriptor pdesc;

			if (m_Properties.TryGetValue(idxname, out pdesc))
			{
				object o = pdesc.GetValue(obj);
				return ConversionHelper.ClrObjectToComplexMoonSharpValue(script, o);
			}

			throw ScriptRuntimeException.UserDataMissingField(this.Name, idxname);
		}

		internal void SetIndex(Script script, object obj, string idxname, DynValue value)
		{
			UserDataPropertyDescriptor pdesc;

			if (m_Properties.TryGetValue(idxname, out pdesc))
			{
				object o = ConversionHelper.MoonSharpValueToObjectOfType(value, pdesc.PropertyInfo.PropertyType, null);
				pdesc.SetValue(obj, o, value.Type);
			}
			else
			{
				throw ScriptRuntimeException.UserDataMissingField(this.Name, idxname);
			}
		}

		internal void Optimize()
		{
			foreach (var m in this.m_Methods.Values)
				m.Optimize();
			
			foreach (var m in this.m_Properties.Values)
			{
				m.OptimizeGetter();
				m.OptimizeSetter();
			}

		}
	}
}
