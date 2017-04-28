using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NDesk.Options;
using NUnitTestProjectSplitter.Entities;
using NUnitTestProjectSplitter.Helpers;
using NUnitTestProject = NUnitTestProjectSplitter.Entities.NUnitTestProject;

namespace NUnitTestProjectSplitter {

	internal static class Program {

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
				"Split rules in format: \"<output nunit project file>:<category to include>,!<category to exclude>;<nunit proj file>...\"",
				( args, v ) => {
					foreach( var ruleAsText in v.Split( ';' ) ) {
						string[] ruleParts = ruleAsText.Split( ':' );
						if( ruleParts.Length != 2 ) {
							throw new ArgumentException( $"Rule \"{ruleAsText} is in incorrect format." );
						}

						string[] categories = ruleParts[1].Split( ',' );

						IEnumerable<string> requiredCategories = categories
							.Where( c => !c.StartsWith( "!" ) );

						IEnumerable<string> prohibitedCategories = categories
							.Where( c => c.StartsWith( "!" ) )
							.Select( c => c.Substring( 1 ) );

						var rule = new SplitRule(
							ruleParts[0],
							requiredCategories.ToHashSet( StringComparer.OrdinalIgnoreCase ),
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
			}

			if( arguments.Length == 1 ) {
				switch( arguments[0] ) {

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

			TestAssemblyScanner scanner = new TestAssemblyScanner();
			NUnitTestProjectProcessor processor = new NUnitTestProjectProcessor( scanner );

			string assembliesPath = args.AssembliesPath.FullName;
			Console.WriteLine( "Loading assemblies from '{0}'", assembliesPath );

			var sw = new DebugStopwatch( "1.Load NunitProject" );
			string inputProjectPath = Path.Combine( assembliesPath, args.InputNUnitProject );
			NUnitTestProject inputProject = NUnitTestProject.LoadFromFile( inputProjectPath );
			sw.Dispose();

			int processedAssemblies = processor.Process(
				assembliesPath,
				inputProject,
				args.SplitRules
			);

			Console.WriteLine( "NUnitTestProjectSplitter finished. Processed {0} assemblies", processedAssemblies );
			return 1;
		}

	}
}