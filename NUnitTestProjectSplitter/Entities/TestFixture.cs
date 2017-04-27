using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using NUnitTestProjectSplitter.Helpers;

namespace NUnitTestProjectSplitter.Entities {

	public sealed class TestFixture {

		private static readonly CSharpTypeNameFormatter m_typeNameFormatter = new CSharpTypeNameFormatter();

		private readonly Type m_type;

		public TestFixture( 
			Type type,
			IList<string> testFixtureCategories,
			IList<MethodInfo> testMethods
		) {
			m_type = type;
			TestFixtureCategories = new ReadOnlyCollection<string>( testFixtureCategories );
			TestMethods = new ReadOnlyCollection<MethodInfo>( testMethods );
		}

		public string Name => m_typeNameFormatter.FormatFullName( m_type );

		public IReadOnlyList<string> TestFixtureCategories { get;  }

		public IReadOnlyList<MethodInfo> TestMethods { get; }

	}
}
