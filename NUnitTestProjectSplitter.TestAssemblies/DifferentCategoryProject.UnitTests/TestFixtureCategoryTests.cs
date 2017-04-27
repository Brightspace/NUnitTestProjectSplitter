using NUnit.Framework;

namespace DifferentCategoryProject.Tests {

	[TestFixture(Category = "TestFixtureCategory" )]
	public sealed class TestFixtureCategoryTests {

		[Test]
		public void SampleTest() {
			Assert.IsTrue( true );
		}
	}
}
