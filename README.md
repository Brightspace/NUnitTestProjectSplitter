# NUnitTestProjectSplitter

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
