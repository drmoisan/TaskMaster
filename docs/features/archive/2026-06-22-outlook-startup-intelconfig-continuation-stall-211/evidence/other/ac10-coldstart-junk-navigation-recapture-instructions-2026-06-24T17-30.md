# AC10 Cold-Start Re-Capture Instructions — Junk Folder Navigation (issue #211)

Timestamp: 2026-06-24T17-30

MAINTAINER-GATED (runtime, not CI-automatable). This procedure verifies, on a live Outlook
profile, that the direct-navigation fix (JunkFolderPathNavigator replacing `new FolderTree(Root)`
in LoadJunkCertain / LoadJunkPotential) eliminates the multi-second JunkCertain / JunkPotential
resolution stall. It cannot run in CI because it requires a live Outlook process and the
maintainer's configured stores.

## Why this is needed

The automated regression (JunkFolderPathNavigatorTests, red->green evidence) proves the navigation
logic is now path-bound rather than full-tree (785 -> <=4 child enumerations on the test tree). The
remaining confirmation is a live runtime measurement that the wall-clock resolution time dropped
from the proven ~50,172 ms (cold JunkCertain) to single-digit-to-low-double-digit ms, comparable to
the 4.4 ms direct `DefaultStore.GetDefaultFolder(Inbox)` reference recorded in the delegation.

## Procedure (non-debugger cold start)

1. Build the solution in Debug for the branch head (the build that contains the
   JunkFolderPathNavigator fix). Confirm `TaskMaster.dll` is the freshly built one.
2. Fully close Outlook. Confirm no `OUTLOOK.EXE` process remains (Task Manager).
3. Start DebugView (or the maintainer's standard ETW/Debug output sink) as Administrator, with
   "Capture Win32" and "Capture Global Win32" enabled. Clear the buffer.
4. Launch Outlook NORMALLY (NOT under the Visual Studio debugger) so the add-in initializes under
   real cold-start conditions. Allow startup to complete fully.
5. In DebugView, filter for the tag `[spam-init]`.

## Log lines to inspect

The relevant probe lines are emitted by `SpamBayes.ValidatePathsSet()`
(`UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.cs`), one structured
`[spam-init]` line per resolved path, each carrying its own `Stopwatch` elapsed milliseconds:

- `[spam-init] ... ValidatePathsSet.JunkCertain ... <ms>`   (drives Globals.Ol.JunkCertain -> LoadJunkCertain)
- `[spam-init] ... ValidatePathsSet.JunkPotential ... <ms>` (drives Globals.Ol.JunkPotential -> LoadJunkPotential)
- `[spam-init] ... ValidatePathsSet.Inbox ... <ms>`         (reference: direct GetDefaultFolder, ~4.4 ms)

Also capture the `[phase-net] phase=Engines` line for the overall Engines-phase wall time.

## Pass condition

- `ValidatePathsSet.JunkCertain` elapsed << 5000 ms (target: single-digit-to-low-double-digit ms,
  on the order of the `ValidatePathsSet.Inbox` reference), versus the proven pre-fix ~50,172 ms.
- `ValidatePathsSet.JunkPotential` elapsed << 5000 ms on the same basis.
- No multi-second stall attributable to junk-folder resolution in the Engines phase.
- No full-store-tree enumeration occurs during junk-folder resolution (consistent with the
  path-bound automated regression).

## Recording the result

Record the captured `[spam-init]` ms values and the `[phase-net] phase=Engines` line into a dated
runtime-capture artifact under `evidence/other/`, and update the placeholder
`runtime-capture-ac10-junk-navigation-PLACEHOLDER.md` (P4-T2) with the outcome.
