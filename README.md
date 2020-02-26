# NUnitTestProjectSplitter

[![Build status](https://ci.appveyor.com/api/projects/status/jg622nkovawxvmvt/branch/master?svg=true)](https://ci.appveyor.com/project/Brightspace/nunittestprojectsplitter/branch/master)

The goal of the tool is to split asseblies from nunit test project by category.

Usage:
NUnitTestProjectSplitter.exe 
	--assembliesPath=<path of assemblies>
	--inputNUnitProject=<nunit project file name>
	--splitRules=<';' separated rules>

Split rules in format: \"<output nunit project file>:<category to include>,!<category to exclude>;<nunit proj file>...\"

Rules Examples:

	--splitRules="UnitTestProjects.nunit:Unit;IntegrationTestProject.nunit:Integration"
	will create 2 projects:
		UnitTestProjects.nunit 		- at least one Unit Test
		IntegrationTestProject.nunit 	- at least one Integration Test

	--splitRules="ParallelTestProject.nunit:Integration,!Isolated;IsolatedTestProject.nunit:Integration,Isolated"
	will create 2 projects:
		ParallelTestProject.nunit 	- at least one test with category Integration and without category Isolated
		IsolatedTestProject.nunit 	- at least one test with category Isolated and Integration

Usage Example:
NUnitTestProjectSplitter.exe 
	--assembliesPath=D:\workspace\bin
	--inputNUnitProject=NUnitTestProjects.nunit 
	--splitRules="UnitTestProjects.nunit:Unit;IntegrationTestProject.nunit:Integration"

## Releasing

To release a new version of this library, bump the version numbers in [appveyor.yml](appveyor.yml):

```diff
- version: 1.0.0-{branch}-{build}
+ version: 1.0.1-{branch}-{build}
  image: Visual Studio 2017

  environment:
-   ASSEMBLY_FILE_VERSION: 1.0.0
+   ASSEMBLY_FILE_VERSION: 1.0.1

...
```

Make sure to get both spots! Get this change into the `master` branch.

After this, create a [new release](https://github.com/Brightspace/NUnitTestProjectSplitter/releases/new) with the tag name `v1.0.1`. _Don't forget the leading `v`!_

Your package will be published to our AppVeyor account feed and automatically pulled down to [nuget.build.d2l](http://nuget.build.d2l)
