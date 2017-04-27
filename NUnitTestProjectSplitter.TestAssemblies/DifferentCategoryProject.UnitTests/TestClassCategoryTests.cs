using NUnit.Framework;

namespace DifferentCategoryProject.Tests {

	[TestFixture]
	[Category( "TestClassCategory" )]
	public sealed class TestClassCategoryTests {

		[Test]
		public void SampleTest() {
			Assert.IsTrue( true );
		}
	}
}
