using System;
using System.Collections.Generic;

namespace NUnitTestProjectSplitter.Entities {

	public sealed partial class NUnitTestProject {

		public string ActiveConfig { get; private set; }

		public IDictionary<string, List<string>> Assemblies { get; } 
			= new SortedDictionary<string, List<string>>( StringComparer.InvariantCultureIgnoreCase );

		public NUnitTestProject( string activeConfig = null ) {
			ActiveConfig = activeConfig;
		}
		
		internal void Add(
			string configName,
			string assemblyFileName
		) {

			List<string> configs;
			if( !Assemblies.TryGetValue( assemblyFileName, out configs ) ) {

				configs = new List<string>();
				Assemblies.Add( assemblyFileName, configs );
			}

			configs.Add( configName );
		}
	}
}
