using NUnit.Framework;

namespace Project1.IsolatedTests {

	[TestFixture]
	[Category( "Isolated" )]
	public sealed class SampleTests {

		[Test]
		public void SampleTest() {
			Assert.IsTrue( true );
		}
	}
}
