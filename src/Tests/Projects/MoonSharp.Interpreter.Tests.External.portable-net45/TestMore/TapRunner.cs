#if !NETFX_CORE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.CoreLib;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Loaders;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests
{
	public class TapRunner
	{
		string m_File;

		public void Print(string str)
		{
			Assert.IsFalse(str.Trim().StartsWith("not ok"), string.Format("TAP fail ({0}) : {1}", m_File, str));
		}

		public TapRunner(string filename)
		{
			m_File = filename;
		}

		public void Run()
		{
			Script S = new Script();

			//S.Globals["print"] = DynValue.NewCallback(Print);
			S.Options.DebugPrint = Print;

			S.Options.UseLuaErrorLocations = true;

			S.Globals.Set("arg", DynValue.NewTable(S));

#if PORTABLENET4_TESTS
			OldFashionedScriptLoader L = new OldFashionedScriptLoader();
			S.Options.ScriptLoader = L;
#else
			ClassicLuaScriptLoader L = S.Options.ScriptLoader as ClassicLuaScriptLoader;

			if (L == null)
			{
				L = new ClassicLuaScriptLoader();
				S.Options.ScriptLoader = L;
			}
#endif

			L.ModulePaths = L.UnpackStringPaths("TestMore/Modules/?;TestMore/Modules/?.lua");

			S.DoFile(m_File);
		}

		public static void Run(string filename)
		{
			TapRunner t = new TapRunner(filename);
			t.Run();
		}
	}


	internal class OldFashionedScriptLoader : IScriptLoader
	{
		string[] m_EnvironmentPaths = null;

		public OldFashionedScriptLoader()
		{
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



}

#endif