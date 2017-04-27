using NUnit.Framework;

namespace Project1.Tests {

	[TestFixture]
	[Category( "Integration" )]
	public sealed class SampleIsolatedTests {

		[Test]
		[Category( "Isolated" )]
		public void SampleTest() {
			Assert.IsTrue( true );
		}
	}
}
