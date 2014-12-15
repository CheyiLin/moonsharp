using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if PORTABLENET4

namespace MoonSharp.Interpreter
{
	internal static class PortableClassLibrary_Extensions
	{
		public static bool Contains(this string str, char c)
		{
			return str.Contains(c.ToString());
		}
	}


	internal class Stopwatch
	{
		DateTime start;

		internal void Start()
		{
			start = DateTime.Now;
		}

		internal void Stop()
		{
			TimeSpan ts = DateTime.Now - start;
			ElapsedMilliseconds = (long)(ts.TotalMilliseconds);
		}

		internal long ElapsedMilliseconds { get; private set; }

		internal static Stopwatch StartNew()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			return sw;
		}
	}

	internal sealed class SerializableAttribute : Attribute
	{
	}

	internal sealed class GuidAttribute : Attribute
	{
		internal GuidAttribute(string dummy)
		{}
	}
	
	internal sealed class ComVisibleAttribute : Attribute
	{
		internal ComVisibleAttribute(bool dummy)
		{}
	}
}

#endif

namespace MoonSharp.Interpreter.RuntimeAbstraction
{
	class PortableFrameworkPlatform : Platform
	{
		public override string Name
		{
			get { return "portable-net4"; }
		}

		public override string GetEnvironmentVariable(string variable)
		{
			return null;
		}

		public override CoreModules FilterSupportedCoreModules(CoreModules module)
		{
			return module & (~(CoreModules.IO | CoreModules.OS_System));
		}
	}

}
