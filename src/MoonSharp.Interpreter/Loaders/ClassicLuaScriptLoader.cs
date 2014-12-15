﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.RuntimeAbstraction;

namespace MoonSharp.Interpreter.Loaders
{
#if !PORTABLENET4

	public class ClassicLuaScriptLoader : IScriptLoader
	{
		string[] m_EnvironmentPaths = null;
		
		public ClassicLuaScriptLoader()
		{
			string env = Platform.Current.GetEnvironmentVariable("MOONSHARP_PATH");
			if (!string.IsNullOrEmpty(env)) m_EnvironmentPaths = UnpackStringPaths(env);

			if (m_EnvironmentPaths == null)
			{
				env = Platform.Current.GetEnvironmentVariable("LUA_PATH");
				if (!string.IsNullOrEmpty(env)) m_EnvironmentPaths = UnpackStringPaths(env);
			}

			if (m_EnvironmentPaths == null)
			{
				m_EnvironmentPaths = UnpackStringPaths("?;?.lua");
			}
		}

		public virtual string LoadFile(string file, Table globalContext)
		{
			return File.ReadAllText(file);
		}

		public virtual string ResolveFileName(string filename, Table globalContext)
		{
			return filename;
		}

		public string[] ModulePaths { get; set; }

		public virtual string ResolveModuleName(string modname, Table globalContext)
		{
			if (ModulePaths != null)
				return ResolveModuleName(modname, ModulePaths);

			DynValue s = globalContext.RawGet("LUA_PATH");

			if (s != null && s.Type == DataType.String)
				return ResolveModuleName(modname, UnpackStringPaths(s.String));

			return ResolveModuleName(modname, m_EnvironmentPaths);
		}

		public virtual string[] UnpackStringPaths(string str)
		{
			return str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s.Trim())
				.Where(s => !string.IsNullOrEmpty(s))
				.ToArray();
		}

		protected virtual string ResolveModuleName(string modname, string[] paths)
		{
			modname = modname.Replace('.', '/');

			foreach (string path in paths)
			{
				string file = path.Replace("?", modname);
				if (FileExists(file))
					return file;
			}

			return null;
		}

		protected virtual bool FileExists(string file)
		{
			return File.Exists(file);
		}
	}



#else

	public class ClassicLuaScriptLoader : IScriptLoader
	{
		public ClassicLuaScriptLoader()
		{
		}

		public virtual string LoadFile(string file, Table globalContext)
		{
			throw new NotSupportedException("File operations are not supported in portable class library version of MoonSharp.");
		}

		public virtual string ResolveFileName(string filename, Table globalContext)
		{
			throw new NotSupportedException("File operations are not supported in portable class library version of MoonSharp.");
		}

		public string[] ModulePaths { get; set; }

		public virtual string ResolveModuleName(string modname, Table globalContext)
		{
			throw new NotSupportedException("File operations are not supported in portable class library version of MoonSharp.");
		}

		public virtual string[] UnpackStringPaths(string str)
		{
			throw new NotSupportedException("File operations are not supported in portable class library version of MoonSharp.");
		}

		protected virtual string ResolveModuleName(string modname, string[] paths)
		{
			throw new NotSupportedException("File operations are not supported in portable class library version of MoonSharp.");
		}

		protected virtual bool FileExists(string file)
		{
			throw new NotSupportedException("File operations are not supported in portable class library version of MoonSharp.");
		}
	}


#endif
}
