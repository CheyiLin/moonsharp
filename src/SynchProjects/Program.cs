using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace SynchProjects
{
	class Program
	{
		static void CopyCompileFilesAsLinks(string srcCsProj, string dstCsProj, string pathPrefix)
		{
			try
			{
				const string XMLNS = "http://schemas.microsoft.com/developer/msbuild/2003";
				HashSet<string> linksDone = new HashSet<string>();

				Console.Write("Synch vsproj compiles {0} ...", Path.GetFileNameWithoutExtension(dstCsProj));

				XmlDocument xsrc = new XmlDocument();
				XmlDocument xdst = new XmlDocument();

				xsrc.Load(srcCsProj);
				xdst.Load(dstCsProj);

				XmlNamespaceManager sxns = new XmlNamespaceManager(xsrc.NameTable);
				XmlNamespaceManager dxns = new XmlNamespaceManager(xdst.NameTable);

				sxns.AddNamespace("ms", XMLNS);
				dxns.AddNamespace("ms", XMLNS);

				XmlElement srccont = xsrc.SelectSingleNode("/ms:Project/ms:ItemGroup[count(ms:Compile) != 0]", sxns) as XmlElement;
				XmlElement dstcont = xdst.SelectSingleNode("/ms:Project/ms:ItemGroup[count(ms:Compile) != 0]", dxns) as XmlElement;

				// dirty hack
				dstcont.InnerXml = srccont.InnerXml;

				List<XmlElement> toRemove = new List<XmlElement>();

				foreach (XmlElement xe in dstcont.ChildNodes.OfType<XmlElement>())
				{
					string file = xe.GetAttribute("Include");
					string link = Path.GetFileName(file);

					if (link.Contains(".g4"))
					{
						toRemove.Add(xe);
						continue;
					}

					if (!linksDone.Add(link))
						Console.Write("\n\t[WARNING] - Duplicate file: {0}", link);

					file = pathPrefix + file;

					xe.SetAttribute("Include", file);

					XmlElement xlink = xe.OwnerDocument.CreateElement("Link", XMLNS);
					xlink.InnerText = link;
					xe.AppendChild(xlink);
				}

				foreach (XmlElement xe in toRemove)
					xe.ParentNode.RemoveChild(xe);

				xdst.Save(dstCsProj);
				Console.WriteLine("\t[DONE]");
			}
			catch (Exception ex)
			{
				Console.WriteLine("\t[ERROR] - {0}", ex.Message);
			}
		}

		static void Main(string[] args)
		{
			//****************************************************************************
			//** INTERPRETER
			//****************************************************************************

			// net40-client
			CopyCompileFilesAsLinks(@"C:\git\moonsharp\src\MoonSharp.Interpreter\MoonSharp.Interpreter.net35-client.csproj",
				@"C:\git\moonsharp\src\Projects\MoonSharp.Interpreter.net40-client\MoonSharp.Interpreter.net40-client.csproj",
				@"..\..\MoonSharp.Interpreter\");

			// net45
			CopyCompileFilesAsLinks(@"C:\git\moonsharp\src\MoonSharp.Interpreter\MoonSharp.Interpreter.net35-client.csproj",
				@"C:\git\moonsharp\src\Projects\MoonSharp.Interpreter.net45\MoonSharp.Interpreter.net45.csproj",
				@"..\..\MoonSharp.Interpreter\");

			// netcore45
			CopyCompileFilesAsLinks(@"C:\git\moonsharp\src\MoonSharp.Interpreter\MoonSharp.Interpreter.net35-client.csproj",
				@"C:\git\moonsharp\src\Projects\MoonSharp.Interpreter.netcore45\MoonSharp.Interpreter.netcore45.csproj",
				@"..\..\MoonSharp.Interpreter\");

			// portable-net40
			CopyCompileFilesAsLinks(@"C:\git\moonsharp\src\MoonSharp.Interpreter\MoonSharp.Interpreter.net35-client.csproj",
				@"C:\git\moonsharp\src\Projects\MoonSharp.Interpreter.portable-net40\MoonSharp.Interpreter.portable-net40.csproj",
				@"..\..\MoonSharp.Interpreter\");

			// portable-net45
			CopyCompileFilesAsLinks(@"C:\git\moonsharp\src\MoonSharp.Interpreter\MoonSharp.Interpreter.net35-client.csproj",
				@"C:\git\moonsharp\src\Projects\MoonSharp.Interpreter.portable-net45\MoonSharp.Interpreter.portable-net45.csproj",
				@"..\..\MoonSharp.Interpreter\");


			//****************************************************************************
			//** UNIT TESTS
			//****************************************************************************

			// Tests - net40
			CopyCompileFilesAsLinks(@"C:\git\moonsharp\src\Tests\MoonSharp.Interpreter.Tests\MoonSharp.Interpreter.Tests.net35.csproj",
				@"C:\git\moonsharp\src\Tests\Projects\MoonSharp.Interpreter.Tests.net40\MoonSharp.Interpreter.Tests.net40.csproj",
				@"..\..\MoonSharp.Interpreter.Tests\");

			// Tests - portable-net40
			CopyCompileFilesAsLinks(@"C:\git\moonsharp\src\Tests\MoonSharp.Interpreter.Tests\MoonSharp.Interpreter.Tests.net35.csproj",
				@"C:\git\moonsharp\src\Tests\Projects\MoonSharp.Interpreter.Tests.portable-net40\MoonSharp.Interpreter.Tests.portable-net40.csproj",
				@"..\..\MoonSharp.Interpreter.Tests\");

			// Tests - net45
			CopyCompileFilesAsLinks(@"C:\git\moonsharp\src\Tests\MoonSharp.Interpreter.Tests\MoonSharp.Interpreter.Tests.net35.csproj",
				@"C:\git\moonsharp\src\Tests\Projects\MoonSharp.Interpreter.Tests.net45\MoonSharp.Interpreter.Tests.net45.csproj",
				@"..\..\MoonSharp.Interpreter.Tests\");

			// Tests - EXTERNAL portable-net45
			CopyCompileFilesAsLinks(@"C:\git\moonsharp\src\Tests\MoonSharp.Interpreter.Tests\MoonSharp.Interpreter.Tests.net35.csproj",
				@"C:\git\moonsharp\src\Tests\Projects\MoonSharp.Interpreter.Tests.External.portable-net45\MoonSharp.Interpreter.Tests.External.portable-net45.csproj",
				@"..\..\MoonSharp.Interpreter.Tests\");

			// Tests - portable-net45
			CopyCompileFilesAsLinks(@"C:\git\moonsharp\src\Tests\MoonSharp.Interpreter.Tests\MoonSharp.Interpreter.Tests.net35.csproj",
				@"C:\git\moonsharp\src\Tests\Projects\MoonSharp.Interpreter.Tests.portable-net45\MoonSharp.Interpreter.Tests.portable-net45.csproj",
				@"..\..\MoonSharp.Interpreter.Tests\");

			// Tests - netcore45
			CopyCompileFilesAsLinks(@"C:\git\moonsharp\src\Tests\MoonSharp.Interpreter.Tests\MoonSharp.Interpreter.Tests.net35.csproj",
				@"C:\git\moonsharp\src\Tests\Projects\MoonSharp.Interpreter.Tests.netcore45\MoonSharp.Interpreter.Tests.netcore45.csproj",
				@"..\..\MoonSharp.Interpreter.Tests\");

			//
			Console.ReadLine();
		}
	}
}
