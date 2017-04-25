using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NDesk.Options;
using NUnit.Framework;
using NUnitTestProjectSplitter.Helpers;
using NUnitTestProjectSplitter.Scanner;
using NUnitTestProjectSplitter.Splitter;

namespace NUnitTestProjectSplitter {

	internal static class Program {

		private static readonly char[] ArgumentsSeperator = new char[] { ',' };

		private sealed class Arguments {

			public DirectoryInfo AssembliesPath = new DirectoryInfo( Environment.CurrentDirectory );
			
			public string InputNUnitProject = "NUnitTestProjects.nunit";

			public readonly IList<SplitRule> SplitRules = new List<SplitRule>();
			////TODO: get from config
			//new SplitRule( "UnitTestProjects.nunit", new HashSet<string>() {"Unit"}, new HashSet<string>()  ),
			//new SplitRule( "ParallelTestProject.nunit", new HashSet<string>() {"Integration"}, new HashSet<string>() { "Isolated" }  ),
			//new SplitRule( "IsolatedTestProject.nunit", new HashSet<string>() { "Integration", "Isolated"}, new HashSet<string>()  ),
			//new SplitRule( "ThirdPartyIntegrationTestProject.nunit", new HashSet<string>() {"ThirdPartyIntegration"}, new HashSet<string>()  ),
			//new SplitRule( "SystemTestProject.nunit", new HashSet<string>() {"System"}, new HashSet<string>()  ),

		}

		private static readonly OptionSet<Arguments> m_argumentParser = new OptionSet<Arguments>()
			.Add(
				"assembliesPath=",
				"The input path of assemblies",
				( args, v ) => args.AssembliesPath = new DirectoryInfo( v )
			)
			.Add(
				"inputNUnitProject=",
				"Input Assemblies NUnitProject project (NUnitTestProjects.nunit)",
				( args, v ) => { args.InputNUnitProject = v; }
			).Add(
				"splitRules=",
				"Split rules in format: \"<nunit proj file>:<category to include>,!<category to exclude>;<nunit proj file>...\"",
				( args, v ) => {
					foreach( var ruleAsText in v.Split( ';' )) {
						var ruleParts = ruleAsText.Split( ':' );
						if( ruleParts.Length != 2 ) {
							throw new ArgumentException($"Rule \"{ruleAsText} is in incorrect format.");
						}

						var categories = ruleParts[1].Split( ',' );

						var requaredCategories = categories
							.Where( c => !c.StartsWith( "!" ) );

						var prohibitedCategories = categories
							.Where( c => c.StartsWith( "!" ) )
							.Select( c => c.Substring( 1 ) );

						var rule = new SplitRule(
							ruleParts[0],
							requaredCategories.ToHashSet( StringComparer.OrdinalIgnoreCase ),
							prohibitedCategories.ToHashSet( StringComparer.OrdinalIgnoreCase )
						);

						args.SplitRules.Add( rule );
					}
				}
			);

		internal static int Main( string[] arguments ) {

			if( arguments.Length == 0 ) {

				m_argumentParser.WriteOptionDescriptions( Console.Out );
				return -1;

			} else if( arguments.Length == 1 ) {

				switch( arguments[ 0 ] ) {

					case "help":
					case "/help":
					case "-help":
					case "--help":

					case "?":
					case "/?":

						m_argumentParser.WriteOptionDescriptions( Console.Out );
						return 0;
				}
			}

			Arguments args;
			try {
				List<string> extras = new List<string>();
				args = m_argumentParser.Parse( arguments, out extras );

				if( extras.Count > 0 ) {

					Console.Error.Write( "Invalid arguments: " );
					Console.Error.WriteLine( String.Join( " ", extras ) );

					Console.WriteLine( "{0}Usage:", Environment.NewLine );
					m_argumentParser.WriteOptionDescriptions( Console.Out );
					return -3;
				}

			} catch( Exception e ) {

				Console.Error.Write( "Invalid arguments: " );
				Console.Error.WriteLine( e.Message );

				Console.WriteLine( "{0}Usage:", Environment.NewLine );
				m_argumentParser.WriteOptionDescriptions( Console.Out );
				return -2;
			}

			try {
				return Run( args );

			} catch( ReflectionTypeLoadException err ) {

				Console.Error.WriteLine( err.Message );
				Console.Error.WriteLine();

				foreach( Exception loaderErr in err.LoaderExceptions ) {

					Console.Error.WriteLine( loaderErr );
					Console.Error.WriteLine();
				}

				return -101;

			} catch( Exception err ) {
				Console.Error.WriteLine( err );
				return -100;
			}
			
		}

		private static int Run( Arguments args ) {

			string path = args.AssembliesPath.FullName;
			SetupAssemblyResolver( path );

			Console.WriteLine( "Loading assemblies from '{0}'", path );

			var sw = new DebugStopwatch( "1.LoadAssemblies" );
			
			TestAssemblyScanner scanner = new TestAssemblyScanner();
			IDictionary<string, NUnitTestProject> outputProjects = new SortedDictionary<string, NUnitTestProject>();

			var inputProjectPath = Path.Combine( args.AssembliesPath.FullName, args.InputNUnitProject );
			if( !File.Exists( inputProjectPath ) ) {
				throw new FileNotFoundException( inputProjectPath );
			}

			var inputProject = NUnitTestProject.LoadFromFile( inputProjectPath );

			foreach( var config in inputProject.Configs ) {
				string configName = config.Key;

				foreach( var assemblyName in config.Value ) {
					string assemblyPath = Path.Combine( args.AssembliesPath.FullName, assemblyName );
					Assembly assembly = GetAssemblyOrNull( assemblyPath );

					if( assembly != null ) {
						var appliedRules = scanner.Scan( assembly, args.SplitRules );

						foreach( var rule in appliedRules ) {
							if( !outputProjects.ContainsKey( rule.TestProjectName ) ) {
								outputProjects.Add( rule.TestProjectName, new NUnitTestProject( inputProject.ActiveConfig ) );
							}

							outputProjects[rule.TestProjectName].Add( configName, assemblyName );
						}
					}
				}
			}

			foreach( var outputProject in outputProjects ) {
				var outputProjectPath = Path.Combine( args.AssembliesPath.FullName, outputProject.Key );
				outputProject.Value.Save( outputProjectPath );
			}

			sw.Dispose();

			using( IndentedTextWriter writer = new IndentedTextWriter( Console.Error, "\t" ) ) {
				DebugStopwatch.Report( writer );
			}

			return 1;
		}

		//private static int Report(
		//		TestAssembly assembly,
		//		IndentedTextWriter writer
		//	) {

		//	int violations = 0;

		//	if( assembly.Violations.Count > 0 ) {
		//		writer.WriteLine( "Assembly: {0}", assembly.Name );
		//		writer.Indent++;

		//		foreach( string violation in assembly.Violations ) {
		//			writer.WriteLine( violation );
		//		}

		//		writer.Indent--;

		//		violations += assembly.Violations.Count;
		//	}

		//	foreach( TestFixture fixture in assembly.Fixtures ) {

		//		if( fixture.Violations.Count > 0 ) {

		//			if( violations == 0 ) {

		//				writer.WriteLine( "Assembly: {0}", assembly.Name );
		//				writer.WriteLine();
		//				writer.Indent++;
		//			}

		//			writer.WriteLine( "Fixture: {0}", fixture.Name );
		//			writer.WriteLine();
		//			writer.Indent++;

		//			foreach( TestViolation violation in fixture.Violations ) {

		//				writer.WriteLine( "Test: {0}", violation.Name );
		//				writer.Indent++;
		//				writer.WriteLine( violation.Message );
		//				writer.Indent--;
		//				writer.WriteLine();
		//			}

		//			writer.Indent--;

		//			violations += fixture.Violations.Count;
		//		}
		//	}

		//	if( violations > 0 ) {
		//		writer.Indent--;
		//		writer.WriteLine();
		//	}

		//	return violations;
		//}

		private static void SetupAssemblyResolver( string path ) {

			AssemblyResolver resolver = new AssemblyResolver( path );

			AppDomain.CurrentDomain.AssemblyResolve +=
				delegate ( object senderJunk, ResolveEventArgs args ) {

					Assembly assemblyObj = resolver.Resolve( args.Name );
					return assemblyObj;
				};
		}

		private static Assembly GetAssemblyOrNull( string filePath ) {

			string fileName = filePath;
			try {
				Assembly assembly = Assembly.LoadFile( fileName );
				return assembly;

			} catch( BadImageFormatException ) {
				Console.Error.WriteLine( "Failed to load: {0}", fileName );

			} catch( FileLoadException ) {
				Console.Error.WriteLine( "Failed to load: {0}", fileName );

			} catch( ReflectionTypeLoadException ) {
				Console.Error.WriteLine( "Failed to load: {0}", fileName );

			} catch( TypeLoadException ) {
				Console.Error.WriteLine( "Failed to load: {0}", fileName );
			} catch( FileNotFoundException ) {
				Console.Error.WriteLine( "File not found: {0}", fileName );
			}

			return null;
		}



	}
}
