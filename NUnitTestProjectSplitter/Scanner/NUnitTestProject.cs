using System;
using System.Collections.Generic;

namespace NUnitTestProjectSplitter.Scanner {

	public sealed partial class NUnitTestProject {

		public string ActiveConfig { get; private set; }

		public IDictionary<string, List<string>> Configs { get; } 
			= new SortedDictionary<string, List<string>>( StringComparer.InvariantCultureIgnoreCase );

		public NUnitTestProject( string activeConfig = null ) {
			ActiveConfig = activeConfig;
		}
		
		internal void Add(
			string configName,
			string assemblyPath
		) {

			List<string> assemblies;
			if( !Configs.TryGetValue( configName, out assemblies ) ) {

				assemblies = new List<string>();
				Configs.Add( configName, assemblies );
			}

			assemblies.Add( assemblyPath );
		}
	}
}
