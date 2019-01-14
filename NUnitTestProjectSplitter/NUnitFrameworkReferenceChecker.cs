using System;
using System.Linq;
using System.Reflection;

namespace NUnitTestProjectSplitter {

	internal static class NUnitFrameworkReferenceChecker {

        private const string NUNIT_ASSEMBLY_NAME = "nunit.framework";

		public static bool ReferencesNUnitFramework( Assembly assembly ) {

            AssemblyName[] assmeblies = assembly.GetReferencedAssemblies();

            if( !assmeblies.Any( a => a.Name == NUNIT_ASSEMBLY_NAME ) ) {
                return false;
            }

            return true;
		}
	}
}
