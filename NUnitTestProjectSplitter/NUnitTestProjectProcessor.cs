using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnitTestProjectSplitter.Entities;
using NUnitTestProjectSplitter.Helpers;

namespace NUnitTestProjectSplitter {

	internal sealed class NUnitTestProjectProcessor {

		private readonly TestAssemblyScanner m_testAssemblyScanner;

		public NUnitTestProjectProcessor( TestAssemblyScanner testAssemblyScanner ) {
			m_testAssemblyScanner = testAssemblyScanner;
		}
		
		public int Process( string assembliesPath, NUnitTestProject inputProject, IList<SplitRule> rules ) {
			AssemblyResolver.Setup( assembliesPath );
			int processedAssemblies = 0;

			IDictionary<string, NUnitTestProject> outputProjects = new SortedDictionary<string, NUnitTestProject>(
				rules.ToDictionary( rule => rule.TestProjectName, rule => new NUnitTestProject( inputProject.ActiveConfig ) )
			);

			foreach( var assemblyItem in inputProject.Assemblies ) {
				string assemblyName = assemblyItem.Key;

				var sw = new DebugStopwatch( "2.Load Assembly" );
				string assemblyPath = Path.Combine( assembliesPath, assemblyName );
				Assembly assembly = AssemblyResolver.GetAssemblyOrNull( assemblyPath );
				sw.Dispose();

				if( assembly != null ) {
					IEnumerable<SplitRule> appliedRules = m_testAssemblyScanner.Scan( assembly, rules );

					foreach( var rule in appliedRules ) {
						outputProjects[rule.TestProjectName].Add( assemblyName, assemblyItem.Value );
					}
					processedAssemblies++;
				}
			}

			using( new DebugStopwatch( "6.Save NunitProjects" ) ) {
				foreach( var outputProject in outputProjects.Where( proj => proj.Value.Assemblies.Any() ) ) {
					string outputProjectPath = Path.Combine( assembliesPath, outputProject.Key );
					outputProject.Value.Save( outputProjectPath );
				}
			}

			using( IndentedTextWriter writer = new IndentedTextWriter( Console.Error, "\t" ) ) {
				DebugStopwatch.Report( writer );
			}

			return processedAssemblies;
		}

	}
}