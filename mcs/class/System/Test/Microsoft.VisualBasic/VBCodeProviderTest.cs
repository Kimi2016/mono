//
// Microsoft.VisualBasic.VBCodeProvider.cs
//
// Author:
//   Jochen Wezel (jwezel@compumaster.de) //
//
// (C) 2003 Jochen Wezel (CompuMaster GmbH)
//
// Last modifications:
// 2003-12-10 JW: publishing of this file
//

using NUnit.Framework;
using System;
using Microsoft.VisualBasic;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace MonoTests.System.Microsoft.VisualBasic
{
	[TestFixture]
	public class VBCodeProviderTest : Assertion {
	
		CodeDomProvider MyVBCodeProvider;

		[SetUp]
		public void GetReady() { 
			MyVBCodeProvider = new VBCodeProvider(); 
		}

		[Test]
		public void FileExtension ()
		{
			AssertEquals ("#JW10", "vb", MyVBCodeProvider.FileExtension);
		}

		[Test]
		public void LanguageOptionsTest ()
		{
			AssertEquals ("#JW20", LanguageOptions.CaseInsensitive, MyVBCodeProvider.LanguageOptions);
		}

		[Test]
		public void CreateCompiler()
		{
			// Prepare the compilation
			//Console.WriteLine("#J30.pre1 - CreateCompiler");
			ICodeCompiler MyVBCodeCompiler;
			MyVBCodeCompiler = MyVBCodeProvider.CreateCompiler();
			AssertNotNull ("#JW30 - CreateCompiler", MyVBCodeCompiler);
			CompilerResults MyVBCodeCompilerResults;
			//Console.WriteLine("#J30.post1 - CreateCompiler");

			CompilerParameters options = new CompilerParameters();
			options.GenerateExecutable = true;
			options.IncludeDebugInformation = true;
			options.TreatWarningsAsErrors = true;
			
			// Process compilation
			MyVBCodeCompilerResults = MyVBCodeCompiler.CompileAssemblyFromSource(options,
				"public class TestModule" + Environment.NewLine + "public shared sub Main()" + Environment.NewLine + "System.Console.Write(\"Hello world!\")" + Environment.NewLine + "End Sub" + Environment.NewLine + "End Class" + Environment.NewLine);

			// Analyse the compilation success/messages
			StringCollection MyOutput;
			MyOutput = MyVBCodeCompilerResults.Output;
			string MyOutStr = "";
			foreach (string MyStr in MyOutput)
			{
				MyOutStr += MyStr + Environment.NewLine + Environment.NewLine;
			}

			AssertEquals ("#JW31 - Hello world compilation: " + MyOutStr, 0, MyVBCodeCompilerResults.Errors.Count);

			try
			{
				Assembly MyAss = MyVBCodeCompilerResults.CompiledAssembly;
			}
			catch (Exception ex)
			{
				Assert ("#JW32 - MyVBCodeCompilerResults.CompiledAssembly hasn't been an expected object" + 
						Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace, false);
			}

			// Execute the test app
			ProcessStartInfo NewProcInfo = new ProcessStartInfo();
			NewProcInfo.FileName = MyVBCodeCompilerResults.CompiledAssembly.Location;
			NewProcInfo.RedirectStandardOutput = true;
			NewProcInfo.UseShellExecute = false;
			NewProcInfo.CreateNoWindow = true;
			string TestAppOutput = "";
			try
			{
				Process MyProc = Process.Start(NewProcInfo);
				MyProc.WaitForExit();
				TestAppOutput = MyProc.StandardOutput.ReadToEnd();
				MyProc.Close();
				MyProc.Dispose();
			}
			catch (Exception ex)
			{
				Assert("#JW34 - " + ex.Message + Environment.NewLine + ex.StackTrace, false);
			}
			AssertEquals("#JW33 - Application output", "Hello world!", TestAppOutput);

			// Clean up
			try
			{
				File.Delete (NewProcInfo.FileName);
			}
			catch {}
		}

		[Test]
		public void CreateGenerator()
		{
			ICodeGenerator MyVBCodeGen;
			MyVBCodeGen = MyVBCodeProvider.CreateGenerator();
			Assert ("#JW40 - CreateGenerator", (MyVBCodeGen != null));
			AssertEquals ("#JW41", true, MyVBCodeGen.Supports (GeneratorSupport.DeclareEnums));
		}

		
		//TODO: [Test]
		public void	CreateParser()
		{
			//System.CodeDom.Compiler.ICodeParser CreateParser()
		}

		//TODO: [Test]
		public void	CreateObjRef()
		{
			//System.Runtime.Remoting.ObjRef CreateObjRef(System.Type requestedType)
		}

	}
}
