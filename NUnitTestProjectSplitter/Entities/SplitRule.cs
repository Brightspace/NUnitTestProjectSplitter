using System.Collections.Generic;

namespace NUnitTestProjectSplitter.Entities {

	public class SplitRule {

		public SplitRule( 
			string testProjectName, 
			ISet<string> requaredCategories,
			ISet<string> prohibitedCategories 
		) {
			TestProjectName = testProjectName;
			RequaredCategories = requaredCategories;
			ProhibitedCategories = prohibitedCategories;
		}

		public string TestProjectName { get; }

		public ISet<string> RequaredCategories { get; }

		public ISet<string> ProhibitedCategories { get; }

	}
}
