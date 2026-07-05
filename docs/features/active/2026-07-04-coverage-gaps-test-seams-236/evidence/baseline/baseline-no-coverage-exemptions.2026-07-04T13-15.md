Timestamp: 2026-07-04T13-15
Command: Select-String over issue #236 target files, coverage.config, TaskMaster.runsettings, and scripts\vscode\TaskMaster.cli.runsettings for coverage exclusions and target names
EXIT_CODE: 0
Output Summary: Baseline search found no `[ExcludeFromCodeCoverage]` usage and no issue #236 target-specific coverage exclusions. Existing exclusions are limited to third-party, analyzer, test framework, and supporting assemblies in `coverage.config` and `TaskMaster.runsettings`; `scripts\vscode\TaskMaster.cli.runsettings` produced no matching exclusion or target-name output.

Searched Files:
- QuickFiler\Helper Classes\EfcViewerQueue.cs
- QuickFiler\Helper Classes\ItemViewerQueue.cs
- QuickFiler\Helper Classes\QfcThemeHelper.cs
- QuickFiler\Controllers\EfcHomeController.cs
- QuickFiler\Helper Classes\TlpCellSnapShot.cs
- coverage.config
- TaskMaster.runsettings
- scripts\vscode\TaskMaster.cli.runsettings

Search Patterns:
- ExcludeFromCodeCoverage
- EfcViewerQueue
- ItemViewerQueue
- QfcThemeHelper
- EfcHomeController
- TlpCellStates
- Exclude
- Exclusion
- ModulePath
- Attributes
- Sources

Relevant Findings:
```text
QuickFiler\Helper Classes\EfcViewerQueue.cs:12:public static class EfcViewerQueue
QuickFiler\Helper Classes\ItemViewerQueue.cs:14:public static class ItemViewerQueue
QuickFiler\Helper Classes\QfcThemeHelper.cs:15:internal static class QfcThemeHelper
QuickFiler\Controllers\EfcHomeController.cs:21:public class EfcHomeController : IFilerHomeController
QuickFiler\Controllers\EfcHomeController.cs:78:FormViewer = EfcViewerQueue.Dequeue();
QuickFiler\Controllers\EfcHomeController.cs:196:FormViewer = EfcViewerQueue.Dequeue();
QuickFiler\Helper Classes\TlpCellSnapShot.cs:12:public class TlpCellStates : Dictionary<string, TlpCellSnapShotList>
coverage.config:12:<ModulePaths>
coverage.config:13:<Exclude>
coverage.config:14:<ModulePath>.*Deedle.*</ModulePath>
coverage.config:15:<ModulePath>.*FSharp.*</ModulePath>
coverage.config:16:<ModulePath>.*Castle\.Core.*</ModulePath>
coverage.config:17:<ModulePath>.*FluentAssertions.*</ModulePath>
coverage.config:18:<ModulePath>.*Moq.*</ModulePath>
coverage.config:19:<ModulePath>.*Microsoft\.Testing.*</ModulePath>
coverage.config:20:<ModulePath>.*MSTest.*</ModulePath>
TaskMaster.runsettings:14:<ModulePaths>
TaskMaster.runsettings:15:<Exclude>
TaskMaster.runsettings:16:<ModulePath>.*Deedle.*</ModulePath>
TaskMaster.runsettings:17:<ModulePath>.*FSharp.*</ModulePath>
TaskMaster.runsettings:18:<ModulePath>.*Castle\.Core.*</ModulePath>
TaskMaster.runsettings:19:<ModulePath>.*FluentAssertions.*</ModulePath>
TaskMaster.runsettings:20:<ModulePath>.*Moq.*</ModulePath>
TaskMaster.runsettings:21:<ModulePath>.*Microsoft\.Testing.*</ModulePath>
TaskMaster.runsettings:22:<ModulePath>.*MSTest.*</ModulePath>
```

Baseline Verdict:
- No issue #236 target is excluded from coverage.
- No issue #236 target has `[ExcludeFromCodeCoverage]`.
- No blocking finding was identified in baseline exemption state.
