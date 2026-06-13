# Baseline — Analyzer / Code-Style Build

Timestamp: 2026-06-13T11-55

Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
(Invoked under Git Bash as: MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true -m -v:m. Dash-switch form required under Git Bash; semantically identical to the slash form.)

EXIT_CODE: 0

Output Summary:
- Build succeeded. All projects compiled. No errors.
- Pre-existing analyzer/compiler WARNINGS present (not errors; this gate does not set TreatWarningsAsErrors), e.g.:
  - CS0618 obsolete AsyncEnumerable.SelectAwait/WhereAwait/ForEachAwaitAsync in TaskMaster (AppItemEngines.cs, AppEvents.cs, RibbonController.cs).
  - MSTEST0032 always-true assertion in QuickFiler.Test (QfcFormControllerTests.cs).
  - CS0169 unused fields in ToDoModel.Test (PeopleScoDictionaryNewTests.cs).
  - CS8632 nullable annotation outside #nullable context in several .Test projects.
  - CS0067 unused events in UtilitiesCS.Test.
- These are baseline warnings unrelated to this feature; this step's pass criterion is EXIT_CODE 0 (no errors), which is met.
