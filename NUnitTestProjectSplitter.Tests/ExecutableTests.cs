using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace NUnitTestProjectSplitter.Tests {

	[TestFixture]
	public sealed class ExecutableTests {

		[Test]
		public void CheckCategories() {

			int result = ExecutableTestsHelper.ExecuteNUnitTestProjectSplitter(
				"UnitTestProjects.nunit:Unit",
				"ParallelTestProject.nunit:Integration,!Isolated",
				"IsolatedTestProject.nunit:Integration,Isolated",
				"DifferentCategoryProject.AssemblyCategory.nunit:AssemblyCategory",
				"DifferentCategoryProject.TestCategory.nunit:TestCategory",
				"DifferentCategoryProject.TestClassCategory.nunit:TestClassCategory",
				"DifferentCategoryProject.TestFixtureCategory.nunit:TestFixtureCategory",
				"DifferentCategoryProject.MultipleCategory.nunit:MultipleCategory1,MultipleCategory2"
			);
				
			Assert.AreEqual( 1, result );

			IDictionary<string, string> files = new[] {
				"UnitTestProjects.nunit",
				"ParallelTestProject.nunit",
				"IsolatedTestProject.nunit",
				"DifferentCategoryProject.AssemblyCategory.nunit",
				"DifferentCategoryProject.TestCategory.nunit",
				"DifferentCategoryProject.TestClassCategory.nunit",
				"DifferentCategoryProject.TestFixtureCategory.nunit",
				"DifferentCategoryProject.MultipleCategory.nunit"
			}.ToDictionary( x => x, x => File.ReadAllText( Path.Combine( ExecutableTestsHelper.TestAssembliesPath, x ) ) );

			Assert.AreEqual( Resource.ExpectedUnitTestProjects, files["UnitTestProjects.nunit"] );
			Assert.AreEqual( Resource.ExpectedParallelTestProject, files["ParallelTestProject.nunit"] );
			Assert.AreEqual( Resource.ExpectedIsolatedTestProject, files["IsolatedTestProject.nunit"] );

			Assert.AreEqual( Resource.ExpectedDifferentCategoryProject_AssemblyCategory, files["DifferentCategoryProject.AssemblyCategory.nunit"] );
			Assert.AreEqual( Resource.ExpectedDifferentCategoryProject_TestCategory, files["DifferentCategoryProject.TestCategory.nunit"] );
			Assert.AreEqual( Resource.ExpectedDifferentCategoryProject_TestClassCategory, files["DifferentCategoryProject.TestClassCategory.nunit"] );
			Assert.AreEqual( Resource.ExpectedDifferentCategoryProject_TestFixtureCategory, files["DifferentCategoryProject.TestFixtureCategory.nunit"] );
			Assert.AreEqual( Resource.ExpectedDifferentCategoryProject_MultipleCategory, files["DifferentCategoryProject.MultipleCategory.nunit"] );

		}
		
	}
}
