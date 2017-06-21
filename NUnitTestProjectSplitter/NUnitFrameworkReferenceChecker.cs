using System;
using System.Linq;
using System.Reflection;

namespace NUnitTestProjectSplitter {

	internal static class NUnitFrameworkReferenceChecker {

		private static readonly AssemblyName m_expectedNUnitFramework =
			typeof( NUnit.Framework.TestFixtureAttribute ).Assembly.GetName();

		public static bool ReferencesNUnitFramework( Assembly assembly ) {

			string expectedAssemblyName = m_expectedNUnitFramework.Name;
			Version expectedVersion = m_expectedNUnitFramework.Version;

			AssemblyName[] nunitAssemblies = assembly.GetReferencedAssemblies()
				.Where( x => x.Name.Equals( expectedAssemblyName ) )
				.ToArray();

			if( !nunitAssemblies.Any() ) {
				return false;
			}

			foreach( AssemblyName nunitAssembly in nunitAssemblies ) {

				Version version = nunitAssembly.Version;
				if( !version.Equals( expectedVersion ) ) {

					string msg = $"Expected { expectedAssemblyName } version { expectedVersion }, but found { version } in {assembly.GetName().Name}";
					throw new UnexpectedNUnitVersionException( msg );
				}
			}

			return true;
		}
	}
}
