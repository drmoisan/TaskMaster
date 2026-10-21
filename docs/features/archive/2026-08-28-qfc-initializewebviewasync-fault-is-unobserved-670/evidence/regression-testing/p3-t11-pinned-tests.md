# P3-T11 — The three AC9-pinned pre-existing tests

Timestamp: 2026-09-01T20-08
Command:

    & $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/TestCaseFilter:FullyQualifiedName~InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState|FullyQualifiedName~InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme|FullyQualifiedName~InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults' /Logger:trx '/ResultsDirectory:coverage\testresults\p3-t11'

then `git diff 988d35a8f8eb7436cc46a9f6424db917ed93807a -- QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` and `git status --porcelain -- QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`.

The resolved test runner is recorded as `<vs-install>\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.

EXIT_CODE: 0

## Base-ref substitution

The plan's stated `git diff` names `2b85134b42872e405602e6064e02dc9cda6c319b`. That SHA is superseded and is a stale ancestor rather than the current merge base, so `988d35a8f8eb7436cc46a9f6424db917ed93807a` was used instead. Rationale: `evidence/baseline/p0-t7-base-ref.md`. The substitution matters here specifically, because against the superseded SHA this file's diff would also carry unrelated sibling-delivery changes and the "added lines only" claim below could not be made.

## Output Summary — all three pass

      Passed InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState [1 s]
      Passed InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme [75 ms]
      Passed InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults [274 ms]

    Test Run Successful.
    Total tests: 3
         Passed: 3

The `.trx` was copied to `evidence/regression-testing/p3-t11-pinned.trx`. Its summary reads `outcome=Completed`, `total=3`, `passed=3`, `failed=0`, and each of the three names is present in the document. `total=3` excludes a filter clause silently matching nothing.

The third test is the substantive pin. `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` asserts that `InitializeAsync` **throws** `WebViewSentinelException`. Had line 256 been routed through the guard, that fault would have been contained by the guard's `catch (Exception ex)` arm and the test would have failed. Its passing is therefore direct behavioural evidence that the fix was not applied over-broadly, complementing the static evidence in P2-T4.

## The three test bodies are unchanged

    git diff --stat <base> -- QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs
     .../QfcItemController.InitializationTests.Part3.cs | 100 +++++++++++++++++++++
     1 file changed, 100 insertions(+)

**100 insertions, zero deletions.** A diff with no deletions cannot have modified or removed any pre-existing line, so this single figure is sufficient to establish that no line inside any of the three pinned test bodies was touched. Modification in a unified diff is represented as a deletion paired with an insertion, so a zero deletion count excludes modification as well as removal.

The hunk structure confirms the shape independently:

    @@ -289,0 +290,100 @@ namespace QuickFiler.Controllers.Tests

There is exactly **one** hunk. Its `-289,0` side has zero lines, which is the signature of a pure insertion, and the insertion point is after old line 289. The three pinned tests occupy lines 40-72, 83-116 and 245-288 of the base revision — all strictly before line 289 — so no inserted line falls inside any of them, and their line numbers are unchanged in the post-change file.

    git status --porcelain -- QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs
     M QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs

The porcelain span is recorded alongside the diff because a name-listing or ref-anchored diff sees committed history only; the ` M` code shows the change is currently unstaged in the working tree, which is the state P3-T15 then commits.

## Note on the insertion count

The 100 inserted lines are exactly the plan's stated budget for this file (section 2: the three spec-named tests must fit inside 100 added lines against 102 lines of headroom). The first drafts of the three tests exceeded that budget, reaching 510 lines after the third test was added. Per the plan's explicit instruction, the excess was removed by compacting the XML documentation comments rather than by relocating a test, because relocating any of the three would have falsified its acceptance criterion — AC4, AC5 and AC6 each name `Part3.cs` specifically. The file now measures 498 lines, which is 398 plus exactly 100.
