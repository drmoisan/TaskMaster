# Issue #199 — Update Mirror

Timestamp: 2026-06-14T08-22

PostedAs: unknown
(POSTING NOT PERFORMED: execution scope did not include posting to GitHub. This is a local mirror
of the delivered state for issue #199; posting to https://github.com/drmoisan/TaskMaster/issues/199
is left to the orchestrator/maintainer.)

## Intended update text

Coverage Increments 1–3 (testable seams) implemented — test-only, no production change.

Delivered:
- Increment 1 (ToDoModel.Test): ToDoLoader.SetAndSave<T> (4 overloads), IDList.GetNextToDoID(string),
  ProjectEntry (SetProjectId dialog-free branches + CompareTo full), BaseChanger remaining branches.
  41 tests, all passing.
- Increment 2 (QuickFiler.Test): KaChar/KaCharAsync, KaKey/KaKeyAsync, KaStringAsync, KbdActions<>
  remaining branches, FilerQueueItem + FilerQueue default state, QfcQueue pure queue-state paths.
  46 tests, all passing.
- Increment 3 (TaskMaster.Test): AppStagingFilenames (Settings.Default snapshot/restore),
  AppQuickFilerSettings remaining four properties (snapshot/restore). 12 tests, all passing.

Total: 99 new tests. Full three-assembly suite: 349/349 passing (no regressions).

Toolchain (final single pass): csharpier check clean (1052 files); msbuild analyzers 0 errors;
msbuild nullable+TWAE 0 warnings/0 errors; MSTest with coverage 349/349 pass.

Coverage (production-only, full three-assembly suite vs pre-feature baseline):
- ToDoModel  10.82% -> 25.22%
- QuickFiler 25.20% -> 30.57%
- TaskMaster 25.78% -> 44.05%
Denominator unchanged (zero production-line change) => aggregate production-only rate strictly
increases versus the 71.65% post-#197 baseline (197-COV-001). New-code coverage on every reachable
targeted method = 100%.

Two Flag-and-Stop gaps (no production change made; recorded in evidence/other):
1. ProjectEntry.SetProjectId malformed-id and change-confirmation branches, and the CompareTo
   length tie-break — route through static MyBox/MessageBox; the MyBox.DialogInvoker seam is
   internal to UtilitiesCS with InternalsVisibleTo for UtilitiesCS.Test only (not ToDoModel.Test),
   so they are unreachable without a new production InternalsVisibleTo. Covered the dialog-free
   branches instead.
2. AppFileSystemFolderPaths.MatchBestSpecialFolder — unreachable in isolation because every
   accessible constructor runs LoadFolders() (Directory.CreateDirectory filesystem mutation) and
   SpecialFolders has only a protected setter; testing it would require filesystem mutation
   (prohibited) or a new internal seam (prohibited).

Invariant check: no production *.cs change; no [ExcludeFromCodeCoverage] add/remove; no
coverage.config / runsettings / pipeline / props / targets change. Only additive Compile-item
registrations in the three test csproj files (mechanically required by the legacy non-SDK build).

## Local mirror note

Spec acceptance criteria checked off in spec.md (one Definition-of-Done item intentionally left
unchecked: "All target seams enumerated in Scope are covered" — two Flag-and-Stop gaps remain, as
above). issue.md early-draft AC and plan checklist updated.
