using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using NUnitTestProjectSplitter;

namespace NUnitTestProjectSplitter.Tests {

	internal static class ExecutableTestsHelper {

		private const string TestAssembliesDirectoryName = "TestAssemblies";

		internal static string TestAssembliesPath => Path.Combine( TestContext.CurrentContext.TestDirectory, @"..\..\..\Tests\"+ TestAssembliesDirectoryName );

		internal static int ExecuteNUnitTestProjectSplitter( params string[] rules ) {

			//Assembly assembly = typeof( NUnitTestProjectSplitter.Program ).Assembly;
			//string path = Path.Combine( TestContext.CurrentContext.TestDirectory, "NUnitTestProjectSplitter.exe" );
			string path = Path.Combine( TestContext.CurrentContext.TestDirectory, @"..\..\..\NUnitTestProjectSplitter\bin\Release\NUnitTestProjectSplitter.exe" );
			Assembly assembly = Assembly.LoadFile( path );
			
			AppDomain appDomain = AppDomain.CreateDomain(
				friendlyName: TestContext.CurrentContext.Test.Name,
				securityInfo: AppDomain.CurrentDomain.Evidence,
				info: new AppDomainSetup {
					ApplicationBase = Path.GetDirectoryName( assembly.Location )
				}
			);

			try {
				var arguments = new[] {
					"--inputNUnitProject",
					"NUnitTestProjects.nunit",
					"--assembliesPath",
					TestAssembliesPath,
					"--splitRules",
					string.Join( ";", rules )
				};

				int exitCode = appDomain.ExecuteAssembly( assembly.CodeBase, arguments );
				return exitCode;

			} finally {
				AppDomain.Unload( appDomain );
			}
		}
	}
}
