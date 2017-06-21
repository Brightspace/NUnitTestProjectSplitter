using System;

namespace NUnitTestProjectSplitter {

	internal sealed class UnexpectedNUnitVersionException : Exception {

		public UnexpectedNUnitVersionException( string message )
			: base( message ) {
		}
	}
}
