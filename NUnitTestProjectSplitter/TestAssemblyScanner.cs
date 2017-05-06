using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnitTestProjectSplitter.Entities;
using NUnitTestProjectSplitter.Helpers;

namespace NUnitTestProjectSplitter {

	public sealed class TestAssemblyScanner {

		public IEnumerable<SplitRule> Scan( Assembly assembly, IList<SplitRule> splitRules ) {
			ISet<SplitRule> appliedRules = new HashSet<SplitRule>();

			var sw = new DebugStopwatch( "3.GetAssemblyCategories" );
			List<string> assemblyCategories = assembly
				.GetCustomAttributes(typeof( CategoryAttribute ) )
				.OfType<CategoryAttribute>()
				.Select( attr => attr.Name )
				.ToList();
			sw.Dispose();

			sw = new DebugStopwatch( "4.LoadTestFixturs" );
			List<TestFixture> fixtures = assembly.GetTypes()
				.Select( LoadTestFixtureOrNull )
				.Where( f => f != null )
				.ToList();
			sw.Dispose();

			using( new DebugStopwatch( "5.SplitRules.Check" ) ) {
				foreach( var fixture in fixtures ) {

					foreach( var method in fixture.TestMethods ) {

						ISet<string> testCategories = method
							.GetCustomAttributes<CategoryAttribute>( true )
							.Select( attr => attr.Name )
							.ToHashSet( StringComparer.OrdinalIgnoreCase );

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

			IEnumerable<string> testFixtureCategoryNames = type
				.GetCustomAttributes<TestFixtureAttribute>( true )
				.Where( attr => attr.Category != null )
				.SelectMany( attr => attr.Category.Split( ',' ) );

			IEnumerable<string> categoryNames = type
				.GetCustomAttributes<CategoryAttribute>( true )
				.Select( attr => attr.Name );

			IList<string> testFixtureCategories = testFixtureCategoryNames
				.Concat( categoryNames )
				.ToList();

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

		private static bool IsTestMethod( MethodInfo method ) {

			bool isTest = method.IsDefined( typeof( TestAttribute ), true );
			if( isTest ) {
				return true;
			}

			bool isTestCase = method.IsDefined( typeof( TestCaseAttribute ), true );
			if( isTestCase ) {
				return true;
			}

			return false;
		}

	}
}
