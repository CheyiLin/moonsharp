﻿#if !PORTABLENET4

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.RuntimeAbstraction
{
	class UnityWebPlatform : UnityPlatform
	{
		public override string Name
		{
			get { return "unity-web"; }
		}

		public override string GetEnvironmentVariable(string variable)
		{
			return null;
		}

	}
}

#endif
