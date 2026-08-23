# Fail-Before Exception Dossier — Defect 3, Dead-Region Deletion (Issue #449, [P4-T7])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`
Merge-base SHA: `c551eabab0aa0a6b1a284252811a2e1de819634e`

Command:
```
git grep -n -E "SanitizeArrayLineTSV|StripTabsCrLf|WriteCSV_StartNewFileIfDoesNotExist|SanitizeArray|SaveMessageAsMSG|GetCurrentExplorerFolder" -- "*.cs"
```
EXIT_CODE: 0

## WhyFailingRunImpossible

The change deletes six private/internal statics that no compiled entry point can reach, so no test
input can execute any of the deleted lines. There is no observable behaviour to assert before or
after.

Expanded: the deleted `#region Email Sorting To Rewrite` occupied merge-base lines 183-321 (139 lines)
of `QuickFiler/Controllers/QfcExplorerController.cs`. Its six members are all `private static` or
`internal static`, and none is called from anywhere inside the compiled portion of the file or from
any other compiled file — the whole of the region's inbound call graph is empty. Because the members
are private/internal statics with no callers, there is no public or internal API whose invocation can
transfer control into any of the 139 lines. A test therefore cannot arrange any input that causes a
deleted line to execute, so it cannot observe any behaviour that differs before and after the
deletion.

A reflection assertion — for example that
`typeof(QfcExplorerController).GetMethod("StripTabsCrLf", BindingFlags.NonPublic | BindingFlags.Static)`
is `null` — would fail before and pass after. It is rejected: it asserts the absence of a PRIVATE
IMPLEMENTATION DETAIL rather than a behaviour, it is brittle against any future reintroduction under
a different name or accessibility, and it would permanently constrain the class's private surface.

Two latent defects inside the block (transposed `Path.Combine` arguments, and a write into a null
`ref string[]`) are **deleted, not fixed**, for the same reason: fixing unreachable code is a change
with no observable effect, and a test for either fix would be untestable by the identical argument.

## Absence-of-reference proof

SearchScope: the entire repository, all tracked `*.cs` files (post-change working tree). Plus the
scoped searches `-- QuickFiler QuickFiler.Test` and `--untracked -- QuickFiler QuickFiler.Test`
recorded in `ac6-dead-region-removed.2026-08-22T09-16.md`.

SearchPatterns:
`SanitizeArrayLineTSV|StripTabsCrLf|WriteCSV_StartNewFileIfDoesNotExist|SanitizeArray|SaveMessageAsMSG|GetCurrentExplorerFolder`
(extended regex, `-E`, alternation over the six identifiers).

SearchResult: **zero hits under `QuickFiler` or `QuickFiler.Test`.** Every surviving hit in the
repository binds to an INDEPENDENT copy in one of the three maintained files. Grouped by owning file:

### `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` (1,429 lines — not edited)
```
:491   SaveMessageAsMSG(mailItem, saveFsPath);
:1092  internal static void SaveMessageAsMSG(MailItem mailItem, string fsLocation)
:1338  var output = SanitizeArrayLineTSV(ref strAry);
:1344  private static string SanitizeArrayLineTSV(ref string[] strOutput)
:1353  .Select(s => StripTabsCrLf(s))
:1361  internal static string StripTabsCrLf(string str)
:1374  public static void WriteCSV_StartNewFileIfDoesNotExist(
:1399  SanitizeArray(strAryOutput, ref strOutput);
:1407  private static void SanitizeArray(string[,]? strAryOutput, ref string[]? strOutput)
:1422  .Select(s => StripTabsCrLf(s))
```

### `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs` (465 lines — not edited)
```
:196   var output = SanitizeArrayLineTSV(ref strAry);
:206   //    var output = SanitizeArrayLineTSV(ref strAry);
:211   private string SanitizeArrayLineTSV(ref string[] strOutput)
:218   .Select(s => StripTabsCrLf(s))
:224   internal string StripTabsCrLf(string str)
```

### `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` (not edited)
```
:77    folderCurrent = GetCurrentExplorerFolder(
:84    folderCurrent = GetCurrentExplorerFolder(_globals.Ol.App.ActiveExplorer());
:125   SaveMessageAsMSG(FileSystem_LOC, selItems);
:223   WriteCSV_StartNewFileIfDoesNotExist(
:229   strOutput[1] = SanitizeArrayLineTSV(ref strAry);
:233   //private static string SanitizeArrayLineTSV(ref string[] strOutput)
:241   //            string strTemp = StripTabsCrLf(strOutput[i]);
:255   private static string SanitizeArrayLineTSV(ref string[] strOutput)
:263   .Select(s => StripTabsCrLf(s))
:273   internal static string StripTabsCrLf(string str)
:285   private static void WriteCSV_StartNewFileIfDoesNotExist(
:310   SanitizeArray(strAryOutput, ref strOutput);
:317   private static void SanitizeArray(string[,] strAryOutput, ref string[] strOutput)
:332   .Select(s => StripTabsCrLf(s))
:350   private static void SaveMessageAsMSG(string fileSystem_LOC, IList<MailItem> selItems)
:355   private static Folder GetCurrentExplorerFolder(
```

### One explicitly-qualified external call, and the surviving copies' own tests
```
TaskMaster/AppGlobals/AppOlObjects.cs:279   SortEmail.WriteCSV_StartNewFileIfDoesNotExist(
UtilitiesCS.Test/EmailIntelligence/EmailFiler_Tests.cs:43,47      (tests EmailFiler.StripTabsCrLf)
UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs:127-164     (tests SortEmail.StripTabsCrLf)
UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs:277-295     (tests SortEmail.SaveMessageAsMSG)
UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs:302-324     (tests SortEmail.SanitizeArrayLineTSV, SanitizeArray)
```

`AppOlObjects.cs:279` is type-qualified as `SortEmail.WriteCSV_...`, so it binds unambiguously to the
`SortEmail` copy and never bound to the deleted `QfcExplorerController` static (which was `private
static` and therefore inaccessible from another type in any case). Each of the three surviving copies
carries its own tests in `UtilitiesCS.Test`, which is why they are the maintained copies and the
deleted region was the redundant one.

### No `QuickFiler.Test` reference — including the `internal static StripTabsCrLf`

Command: `git grep -n --untracked -E "<the six identifiers>" -- QuickFiler QuickFiler.Test`
EXIT_CODE: 1
Output: (empty)

**No file under `QuickFiler.Test` references any of the six.** This matters specifically for
`StripTabsCrLf`, which was declared `internal static` in the deleted region. `QuickFiler` exposes its
internals to the test assembly:

```
QuickFiler/Properties/AssemblyInfo.cs:5:  [assembly: InternalsVisibleTo("QuickFiler.Test")]
```

So `QuickFiler.Test` COULD have called `QfcExplorerController.StripTabsCrLf` — the accessibility
barrier that protects the other five members does not apply to it. The search establishes that no test
in fact did. The `--untracked` variant confirms this includes the new
`QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs`. The one member with a possible test-side
caller therefore had none.

### Non-vacuity of the zero result

Command:
```
git grep -c -E "<the six identifiers>" c551eabab0aa0a6b1a284252811a2e1de819634e -- QuickFiler QuickFiler.Test
```
EXIT_CODE: 0
Output: `c551eabab0aa0a6b1a284252811a2e1de819634e:QuickFiler/Controllers/QfcExplorerController.cs:12`

The identical pattern and path scope matched **12 lines** at the merge base and matches **0** now. The
zero is discriminating.

## Alternative proof of no behaviour change — the before/after full-suite comparison

Because no test can target the deleted lines, the proof that the deletion changed no behaviour is the
identical full-suite result before and after. That comparison is recorded under
`<FEATURE>/evidence/qa-gates/`:

- Baseline run: `<FEATURE>/evidence/baseline/step5-vstest-coverage.2026-08-22T09-16.md`
  (6,437 total / 6,437 passed / 0 failed / 0 skipped across nine assemblies).
- Post-change run: `<FEATURE>/evidence/qa-gates/step5-vstest-coverage.2026-08-22T09-16.md`.
- **Named comparison artifact: `<FEATURE>/evidence/qa-gates/suite-comparison-before-after.2026-08-22T09-16.md`**,
  produced by [P7-T8], which names both source artifacts, both executed and passed counts, the delta,
  and the explicit list of newly added test names.
- Determinism corroboration: `<FEATURE>/evidence/qa-gates/step5-second-consecutive-run.2026-08-22T09-16.md`.

The acceptance criterion for defect 3 asks for "a test run confirming no behavior change", and that
before/after suite comparison is what satisfies it, rather than any new test.

Both build gates also passed after the deletion with zero errors, confirming no compiled caller
existed: `phase4-analyzer-build.2026-08-22T09-16.md` and `phase4-nullable-build.2026-08-22T09-16.md`.

## Output Summary

A fail-before run for defect 3 is structurally impossible: the deleted region's six members are
private/internal statics with an empty inbound call graph, so no test input can execute any of the 139
deleted lines and there is no behaviour that differs before and after. The absence proof shows **zero**
hits for all six identifiers under `QuickFiler` and `QuickFiler.Test` (down from 12 at the merge base,
so the result is discriminating), while every surviving repository hit binds to an independent copy in
`SortEmail.cs`, `EmailFiler.cs`, or `SortItemsToExistingFolder.cs` — each with its own tests in
`UtilitiesCS.Test`. No file under `QuickFiler.Test` references any of the six, including the
`internal static StripTabsCrLf` that `[assembly: InternalsVisibleTo("QuickFiler.Test")]` at
`QuickFiler/Properties/AssemblyInfo.cs:5` would otherwise have exposed. The alternative proof of no
behaviour change is the before/after full-suite comparison at
`<FEATURE>/evidence/qa-gates/suite-comparison-before-after.2026-08-22T09-16.md`.
