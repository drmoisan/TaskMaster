# ci-flaky-test-isolation (Issue)

- **Issue:** #176
- **Work Mode:** full-bug
- **Type:** bug
- **Base branch:** main
- **Branch:** bug/ci-flaky-test-isolation-176
- **PR:** #177

## Problem

CI run #197 (push-merge of PR #174 into `main`) failed in the "Run MSTest suite with coverage" step due to two intermittent test-isolation defects. Build, formatting, .NET analyzers, and nullable analysis all passed.

1. `OlFolderClassifierGroup_AdditionalTests.BuildClassifiersAsync_WithFixtureAndFolderConfig_StoresBuiltFolderClassifier` — non-thread-safe `List<string>` in the test double mutated by concurrent `BuildClassifierAsync` callbacks.
2. `PhysicalFileSystemAdapters_Tests.PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo` — write/append handles opened on the real, locked `TaskMaster.sln` under parallel CI.

See `spec.md` for full root-cause analysis, fix design, and acceptance criteria.
