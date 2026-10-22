# Fail-before exception dossier — issue 871

Timestamp: 2026-09-13T15-03
Command: (see the absence-of-test proof below; this dossier records a structural argument plus a
measured search rather than a single gate command)
EXIT_CODE: 0

WhyFailingRunImpossible: A failing pre-change run is structurally impossible for this defect. The
defect is untestability itself: the enqueue path of the queue class constructs its collaborators
directly and marshals through the process-wide dispatcher, so there is no seam at which a test can
substitute a double. Any test written to substitute a seam cannot compile before the seam exists,
because the member it assigns to is not declared, and a test that does not compile produces a build
error rather than a failing test result. Any test that does not substitute a seam fails both before and
after the change for the same reason — the production default still reads the process-wide dispatcher,
which is not available in the headless test host — so it discriminates nothing about whether the fix
landed. Neither shape yields a run that fails before the change and passes after it, which is what a
fail-before artifact is required to demonstrate. The pass-after evidence for this item is therefore
carried by the Phase 4 regression suite, whose tests are written against the seams once they exist.

## Absence-of-test proof

SearchScope: the three existing QfcQueue test files in the QuickFiler test project's Controllers
folder, named individually — QfcQueueTests.cs, QfcQueueCoverageExpansionTests.cs and
QfcQueuePurePathsTests.cs. The scope is those three files by name and is not the folder.

SearchPatterns: the literal token `EnqueueAsync`.

SearchResult: zero matches across exactly those three files. Per file:

```
QfcQueueTests.cs = 0
QfcQueueCoverageExpansionTests.cs = 0
QfcQueuePurePathsTests.cs = 0
SCOPED-TOTAL: 0
```

No existing test of the queue class references the enqueue member at all. The enqueue path is
therefore not merely under-tested; it has no test, which is consistent with the coverage baseline
P0-T12 recorded for `QuickFiler/Controllers/QfcQueue.Enqueue.cs`, a line rate of 0.152941 over 13
covered of 85 valid lines.

## The folder-wide count, recorded so the scope distinction is auditable

FolderWideSearchScope: every C# file in the QuickFiler test project's Controllers folder.
FolderWideSearchResult: 7 matches, in two files.

```
QfcHomeControllerIterationTests.cs = 2
QfcHomeControllerIterationTests.Part2.cs = 5
FOLDER-TOTAL: 7
```

The two counts differ, and the difference is the reason the scope is narrowed to three named files
rather than widened to the folder. The seven folder-wide matches all lie in the two QfcHomeController
iteration test files. Those files assert on the queue interface's enqueue member through a Moq
expression: they verify that the home controller calls it, with the queue itself replaced by a mock.
They exercise the caller, not the queue's own enqueue path, and no line of
`QuickFiler/Controllers/QfcQueue.Enqueue.cs` executes on their account. Counting them would make the
absence-of-test proof read as seven matches and appear to contradict its own conclusion, when in fact
they are evidence about a different unit. Both figures are recorded here so a reader can check the
distinction rather than take it on trust.

## Post-merge validity of this dossier

Both searches were run against the post-merge tree at
8213826f695439e86e3ed34faa575de493a11ec7. The merge added one test method to
QuickFiler.Test/Controllers/QfcHomeControllerTests.cs, which is in the folder but is neither one of the
three scoped files nor one of the two files carrying folder-wide matches; it contributes zero matches
for the searched token and changes neither count.

Output Summary: A fail-before run is structurally impossible because the defect is untestability — a
seam-substituting test cannot compile before the seam exists, and a non-substituting test fails both
before and after. The absence-of-test proof is measured: zero `EnqueueAsync` matches across the three
named existing QfcQueue test files. The folder-wide count of 7, confined to the two QfcHomeController
iteration test files that assert through a Moq expression on the queue interface, is recorded alongside
it so the narrowed scope is auditable rather than implied. Acceptance met.
