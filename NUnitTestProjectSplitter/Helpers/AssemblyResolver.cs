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

		public AssemblyResolver( string path ) {

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
