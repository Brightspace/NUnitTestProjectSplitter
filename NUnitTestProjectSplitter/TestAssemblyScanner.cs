using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnitTestProjectSplitter.Entities;
using NUnitTestProjectSplitter.Helpers;

namespace NUnitTestProjectSplitter {

	public sealed class TestAssemblyScanner {

        private const string NUNIT_CATEGORY_ATTRIBUTE = "NUnit.Framework.CategoryAttribute";
        private const string NUNIT_TESTFIXTURE_ATTRIBUTE = "NUnit.Framework.TestFixtureAttribute";
        private const string NUNIT_TEST_ATTRIBUTE = "NUnit.Framework.TestAttribute";
        private const string NUNIT_TESTCASE_ATTRIBUTE = "NUnit.Framework.TestCaseAttribute";

        public IEnumerable<SplitRule> Scan( Assembly assembly, IList<SplitRule> splitRules ) {
			ISet<SplitRule> appliedRules = new HashSet<SplitRule>();

            var sw = new DebugStopwatch( "3.GetAssemblyCategories" );
            ISet<string> assemblyCategories = GetAssemblyCategories( assembly );
			sw.Dispose();

			sw = new DebugStopwatch( "4.LoadTestFixturs" );
			List<TestFixture> fixtures = assembly
                .GetTypes()
				.Select( LoadTestFixtureOrNull )
				.Where( f => f != null )
				.ToList();
			sw.Dispose();

			using( new DebugStopwatch( "5.SplitRules.Check" ) ) {
				foreach( var fixture in fixtures ) {

					foreach( var method in fixture.TestMethods ) {

                        ISet<string> testCategories = GetTestMethodCategories(method);
						testCategories.UnionWith( assemblyCategories );
						testCategories.UnionWith( fixture.TestFixtureCategories );

						foreach( var splitRule in splitRules ) {
							if( !appliedRules.Contains( splitRule )
								&& splitRule.RequiredCategories.All( c => testCategories.Contains( c ) )
								&& splitRule.ProhibitedCategories.All( c => !testCategories.Contains( c ) ) ) {

								appliedRules.Add( splitRule );
							}
						}

					}
				}
			}

			return appliedRules;
		}
		private TestFixture LoadTestFixtureOrNull( Type type ) {

            ISet<string> testFixtureCategories = GetTestFixtureCategories(type);

			BindingFlags bindingFlags = (
				BindingFlags.Public
				| BindingFlags.NonPublic
				| BindingFlags.Static
				| BindingFlags.Instance
			);

			IList<MethodInfo> methods = type.GetMethods( bindingFlags ).Where( IsTestMethod ).ToList();

			return methods.Any()
				? new TestFixture( type, testFixtureCategories, methods )
				: null;
		}

		private bool IsTestMethod( MethodInfo method ) {

            bool isTest = method
                .GetCustomAttributes(inherit: true)
                .Where(a => a.GetType().FullName == NUNIT_TEST_ATTRIBUTE)
                .Any();
			if( isTest ) {
				return true;
			}

			bool isTestCase = method
                .GetCustomAttributes(inherit: true)
                .Where(a => a.GetType().FullName == NUNIT_TESTCASE_ATTRIBUTE)
                .Any();
            if ( isTestCase ) {
				return true;
			}

			return false;
		}


        private static ISet<string> GetAssemblyCategories( Assembly assembly )
        {
            return assembly
                .GetCustomAttributes()
                .Where(a => a.GetType().FullName == NUNIT_CATEGORY_ATTRIBUTE)
                .Select(a => a.GetType().GetProperty( "Name", BindingFlags.Instance | BindingFlags.Public ).GetValue(a) as string)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static ISet<string> GetTestFixtureCategories( Type type )
        {
            ISet<string> testFixtureCategories = type
                .GetCustomAttributes(inherit: true)
                .Where(a => a.GetType().FullName == NUNIT_TESTFIXTURE_ATTRIBUTE)
                .Select(a => a.GetType().GetProperty("Category", BindingFlags.Instance | BindingFlags.Public).GetValue(a) as string)
                .Where(c => c != null)
                .SelectMany(c => c.Split(','))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            ISet<string> categoryCategories = type
                .GetCustomAttributes(inherit: true)
                .Where(a => a.GetType().FullName == NUNIT_CATEGORY_ATTRIBUTE)
                .Select(a => a.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public).GetValue(a) as string)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            ISet<string> allCategories = testFixtureCategories
                .Union(categoryCategories)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return allCategories;
        }

        private static ISet<string> GetTestMethodCategories( MethodInfo method )
        {
            return method
                .GetCustomAttributes(inherit: true)
                .Where(a => a.GetType().FullName == NUNIT_CATEGORY_ATTRIBUTE)
                .Select(a => a.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public).GetValue(a) as string)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

    }
}
