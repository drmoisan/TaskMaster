# Baseline — .NET Analyzer Build State (Issue #228)

Timestamp: 2026-06-30T22-14
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
(Executed via Bash with dash switches: MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true)
EXIT_CODE: 0
Output Summary: Build succeeded. 0 errors. Pre-existing warnings only, all confined to test projects and not promoted to errors under this gate (analyzer build does NOT set TreatWarningsAsErrors): CS8632 ("annotation for nullable reference types should only be used in code within a '#nullable' annotations context") across TaskMaster.Test and UtilitiesCS.Test; CS0067 ("event ... is never used") in UtilitiesCS.Test StoreWrapperControllerTests / SmartSerializable_Tests / SmartSerializableBase_Tests. These warnings are pre-existing baseline noise unrelated to issue #228 scope. All first-party + vendored projects compiled. MSBuild 18.7.8 for .NET Framework.
