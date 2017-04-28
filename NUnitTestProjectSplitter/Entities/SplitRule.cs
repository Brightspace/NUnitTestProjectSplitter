using System.Collections.Generic;

namespace NUnitTestProjectSplitter.Entities {

	public class SplitRule {

		public SplitRule( 
			string testProjectName, 
			ISet<string> requiredCategories,
			ISet<string> prohibitedCategories 
		) {
			TestProjectName = testProjectName;
			RequiredCategories = requiredCategories;
			ProhibitedCategories = prohibitedCategories;
		}

		public string TestProjectName { get; }

		public ISet<string> RequiredCategories { get; }

		public ISet<string> ProhibitedCategories { get; }

	}
}
