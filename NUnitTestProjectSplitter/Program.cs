using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NDesk.Options;
using NUnitTestProjectSplitter.Entities;
using NUnitTestProjectSplitter.Helpers;
using NUnitTestProjectSplitter.Scanner;
using NUnitTestProject = NUnitTestProjectSplitter.Entities.NUnitTestProject;

namespace NUnitTestProjectSplitter {

	internal static class Program {

		private static readonly char[] ArgumentsSeperator = new char[] { ',' };

		private sealed class Arguments {

			public DirectoryInfo AssembliesPath = new DirectoryInfo( Environment.CurrentDirectory );
			
			public string InputNUnitProject = "NUnitTestProjects.nunit";

			public readonly IList<SplitRule> SplitRules = new List<SplitRule>();
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
			int processedAssemblies = 0;

			Console.WriteLine( "{0} Loading assemblies from '{1}'", DateTime.UtcNow, path );

			var sw = new DebugStopwatch( "1.Load NunitProject" );

			TestAssemblyScanner scanner = new TestAssemblyScanner();
			IDictionary<string, NUnitTestProject> outputProjects = new SortedDictionary<string, NUnitTestProject>();

			var inputProjectPath = Path.Combine( args.AssembliesPath.FullName, args.InputNUnitProject );
			if( !File.Exists( inputProjectPath ) ) {
				throw new FileNotFoundException( inputProjectPath );
			}

			var inputProject = NUnitTestProject.LoadFromFile( inputProjectPath );

			sw.Dispose();

			foreach( var assemblyItem in inputProject.Assemblies ) {
				string assemblyName = assemblyItem.Key;

				sw = new DebugStopwatch( "2.Load Assembly" );
				string assemblyPath = Path.Combine( args.AssembliesPath.FullName, assemblyName );
				Assembly assembly = GetAssemblyOrNull( assemblyPath );
				processedAssemblies++;
				sw.Dispose();

				if( assembly != null ) {
					IEnumerable<SplitRule> appliedRules = scanner.Scan( assembly, args.SplitRules );

					foreach( var rule in appliedRules ) {
						if( !outputProjects.ContainsKey( rule.TestProjectName ) ) {
							outputProjects.Add( rule.TestProjectName, new NUnitTestProject( inputProject.ActiveConfig ) );
						}

						foreach( var configName in assemblyItem.Value ) {
							outputProjects[rule.TestProjectName].Add( configName, assemblyName );
						}
					}
				}
			}

			using( new DebugStopwatch( "6.Save NunitProjects" ) ) {
				foreach( var outputProject in outputProjects ) {
					string outputProjectPath = Path.Combine( args.AssembliesPath.FullName, outputProject.Key );
					outputProject.Value.Save( outputProjectPath );
				}
			}

			using( IndentedTextWriter writer = new IndentedTextWriter( Console.Error, "\t" ) ) {
				DebugStopwatch.Report( writer );
			}

			Console.WriteLine( "{0} NUnitTestProjectSplitter finished. processed {1} assemblies", DateTime.UtcNow, processedAssemblies );
			
			return 1;
		}
		
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
