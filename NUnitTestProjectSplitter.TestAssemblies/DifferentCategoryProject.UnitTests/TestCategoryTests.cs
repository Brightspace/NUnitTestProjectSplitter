using NUnit.Framework;

namespace DifferentCategoryProject.Tests {

	[TestFixture]
	public sealed class TestCategoryTests {

		[Test]
		[Category( "TestCategory" )]
		[Category( "MultipleCategory2" )]
		public void SampleTest() {
			Assert.IsTrue( true );
		}
	}
}
