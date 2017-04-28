using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NUnitTestProjectSplitter.Helpers {

	internal sealed class AssemblyResolver {

		private readonly Dictionary<string, FileInfo> m_filesByName;

		private readonly ConcurrentDictionary<string, Assembly> m_assemblies
			= new ConcurrentDictionary<string, Assembly>( StringComparer.OrdinalIgnoreCase );

		internal static void Setup( string path ) {

			AssemblyResolver resolver = new AssemblyResolver( path );

			AppDomain.CurrentDomain.AssemblyResolve +=
				delegate ( object senderJunk, ResolveEventArgs args ) {

					Assembly assemblyObj = resolver.Resolve( args.Name );
					return assemblyObj;
				};
		}

		internal static Assembly GetAssemblyOrNull( string filePath ) {

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

		private AssemblyResolver( string path ) {

			DirectoryInfo bin = new DirectoryInfo( path );
			if( !bin.Exists ) {

				string msg = String.Format(
						"The directory \"{0}\" was not found.",
						path
					);
				throw new DirectoryNotFoundException( msg );
			}

			m_filesByName = bin
				.GetFiles()
				.Where( IsLibrary )
				.ToDictionary( f => f.Name, StringComparer.OrdinalIgnoreCase );
		}

		public Assembly Resolve( string name ) {

			AssemblyName assemblyName = new AssemblyName( name );

			Assembly assembly = m_assemblies.GetOrAdd(
					assemblyName.Name,
					( n ) => Resolve( assemblyName )
				);

			return assembly;
		}

		private Assembly Resolve( AssemblyName assemblyName ) {

			FileInfo file = MapFileOrNull( assemblyName );
			if( file == null ) {
				return null;
			}

			Assembly assembly = Assembly.LoadFile( file.FullName );
			return assembly;
		}

		private FileInfo MapFileOrNull( AssemblyName assemblyName ) {

			FileInfo dllFile;
			if( m_filesByName.TryGetValue( assemblyName.Name + ".dll", out dllFile ) ) {
				return dllFile;
			}

			FileInfo exeFile;
			if( m_filesByName.TryGetValue( assemblyName.Name + ".exe", out exeFile ) ) {
				return exeFile;
			}

			return null;
		}

		private bool IsLibrary( FileInfo file ) {

			switch( file.Extension.ToLowerInvariant() ) {

				case ".dll":
				case ".exe":
					return true;

				default:
					return false;
			}
		}
	}
}
