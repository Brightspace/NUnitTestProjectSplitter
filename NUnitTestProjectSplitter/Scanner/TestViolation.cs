
namespace NUnitTestProjectSplitter.Scanner {

	public sealed class TestViolation {

		public TestViolation(
				string name,
				string violation
			) {

			this.Name = name;
			this.Message = violation;
		}

		public readonly string Name;
		public readonly string Message;
	}
}
