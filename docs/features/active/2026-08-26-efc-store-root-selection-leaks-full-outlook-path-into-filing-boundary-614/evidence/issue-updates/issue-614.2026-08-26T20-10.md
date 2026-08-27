# Issue update mirror - issue #614

Timestamp: 2026-08-26T20-10

PostedAs: comment

POSTING RESOLVED BY ORCHESTRATOR

Original executor note: the executor had no verified GitHub authentication in its session and the
plan did not authorize a `gh issue` write, so it correctly recorded the update as blocked rather
than claiming it was posted. The orchestrator has verified `gh` authentication and posted it at
https://github.com/drmoisan/TaskMaster/issues/614#issuecomment-5430792742 on 2026-08-26.

Superseded reason: The text below is the intended issue update. It has been mirrored
into the local feature `issue.md` (appended as its `## Outcome (2026-08-26)` section) so the local
record and the intended remote record match. A maintainer, or the orchestrator's PR step, should
post it to https://github.com/drmoisan/TaskMaster/issues/614 and then change `PostedAs:` above to
`comment` or `body` with the resulting URL.

---

## Outcome (2026-08-26)

**Implemented.** All nine confirmed defects D1 through D9 are fixed and all 26 acceptance criteria
are verified and checked off in `spec.md`.

The common root cause was four independent, unanchored ancestor-strip implementations with no
boundary enforcing that a filing stem is archive-relative. They are all now backed by one
authority, `UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs`, whose match is
prefix-anchored, ordinal case-insensitive, and separator-terminated, and which never passes its
input through on failure.

Where each defect was fixed:

- **D1, D2, D3** - `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`. Activating a segment outside
  the bound archive root, or the archive root itself, is now a deterministic non-selection; the
  prior selection is left unchanged and is never set to null. `SelectRow` rejects an out-of-root
  full Outlook filing target, and `ToHierarchyPath` no longer fabricates an out-of-root hierarchy
  path.
- **D4** - `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs`. Both
  `ResolvePaths` overloads reject a non-relative `DestinationOlStem` BEFORE concatenation. This is
  the boundary whose absence produced the reported crash.
- **D5a-D5g** - `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs`. Per-segment Windows
  folder-name validation applied only to derived segments; the `Substring(3)` drive-prefix
  assumption removed; the ancestor strip anchored; the exception message redacted; the "Remove
  illegal characters" option fixed to remove only illegal characters; `ResolveOlRoot` anchored; and
  the never-read `bool ask = true` parameter removed.
- **D6** - `TaskMaster/AppGlobals/AppOlObjects.cs` plus the new pure
  `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs`. `ArchiveRootPath` is cross-checked once, at
  resolution time, against the folder that actually resolves for it.
- **D7** - `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs`. The OneDrive root resolves in
  priority order through an injectable environment-read seam and fails explicitly instead of
  falling back to `AppData` or to an arbitrary first entry.
- **D8** - `QuickFiler/Controllers/EfcDataModel.cs`. The new pure `ToArchiveRelativeStem` helper
  replaces the unanchored `Replace` plus single `Substring(1)`.
- **D9** - the new `QuickFiler/Controllers/EfcSelectionGuard.cs`. `ActionOkAsync` and
  `IsValidSelection` now share one predicate that also rejects any full Outlook path.

Rejected hypothesis: removing `.` from `IllegalFolderCharacters` is NOT the fix. It would remove
the exception without removing the leak, converting a loud crash into silent misfiling. The
character class is corrected, but only behind the new stem guard.

Every production message, log line, and exception message added by this change names the violated
rule only and withholds the path value, which can carry a mailbox address or a user-profile path
(open issue #602).

Two deliberate, documented spec corrections were required because two existing tests codified
defects as expected behaviour. Both are recorded in full in `change-description.2026-08-26.md`:
`FolderConverterTests.cs:329` (D5f) and
`Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic` (D1/D9). The latter is a
user-visible behaviour change: #439 deliberately permitted selecting a verified hierarchy path
outside the archive root, and #614 intentionally revokes that for the filing surface.

Verification (single clean toolchain pass, restart count 3):

| Gate | Result |
| --- | --- |
| `dotnet tool run csharpier check .` | EXIT 0; 0 of 4732 hashed source files rewritten by the format pass |
| analyzer `/t:Rebuild` | EXIT 0; 0 errors; 18 projects recompiled |
| nullable `/t:Rebuild` (CI-identical, no `/p:Nullable=enable`) | EXIT 0; 0 errors; 0 `CS86xx` |
| full suite with coverage | EXIT 0; **6569 total, 6569 passed, 0 failed, 0 skipped** (baseline 6482/6482/0) |
| filtered first-party line coverage | **84.8696%**, up from the 84.7797% merge-base baseline |
| new-code line coverage | **100%** on `ArchiveStemContract`, `EfcSelectionGuard`, `ArchiveRootPathGuard`, and every changed method |

Open issue **#499 remains open and unregressed**: this change does not touch `BindRowsAsync`'s
selection-clearing semantics, and it leaves `SelectedFolderPath` unchanged rather than null on
rejection.

Two items are flagged for maintainer review:

1. **Manual validation not executed.** The five live-Outlook steps in the spec's Test Strategy are
   each recorded as NOT EXECUTED with reason: an automated agent session has no interactive Outlook
   profile, and live-Outlook interaction is excluded from automated tests by policy. Each step has a
   headless automated counterpart that passed; see
   `evidence/qa-gates/manual-validation.2026-08-26T18-55.md`.
2. **One path outside the plan's enumerated in-scope list** was modified:
   `QuickFiler.Test/packages.config`, a single added line pinning `log4net` 3.3.2 - the version the
   rest of the solution already pins. It is the mechanically necessary companion of the allowlisted
   `QuickFiler.Test.csproj` reference addition, which was itself required to make AC2's and AC3's
   "emits a diagnostic" assertion testable through the repository's established `MemoryAppender`
   pattern. Full justification is in `evidence/qa-gates/p8-t2-scope-audit.2026-08-26T18-40.md`.

One pre-existing condition was observed and recorded rather than changed:
`UtilitiesCS.Test/OutlookExtensions/FolderConverter_Tests.cs` exists on disk but carries no
`<Compile Include>` item, so it is not compiled into `UtilitiesCS.Test.dll`. Its single test
duplicates a compiled, green assertion. Recorded in
`evidence/regression-testing/p5-t7-converter-tests.2026-08-26T17-45.md` for follow-up triage.
