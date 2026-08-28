# P9-T9 — The three must-stay-green tests named by the two Phase 9 dossiers

Timestamp: 2026-08-28T01-46
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~SetTopicThread_WhenNotInvokeRequired_SetsItemsAndSortsDescending|FullyQualifiedName~SetTopicThread_WhenInvokeRequired_MarshalsViaInvoke|FullyQualifiedName~JumpToSearchTextbox_TogglesKeyboardAndFocusesSearch" "/Logger:trx;LogFileName=p9-t9.trx" "/ResultsDirectory:<results-dir>"
EXIT_CODE: 0
ExpectedExitCode: 0

## Result

```
VSTest version 18.9.0 (x64)
A total of 1 test files matched the specified pattern.
  Passed SetTopicThread_WhenNotInvokeRequired_SetsItemsAndSortsDescending [195 ms]
  Passed SetTopicThread_WhenInvokeRequired_MarshalsViaInvoke [6 ms]
  Passed JumpToSearchTextbox_TogglesKeyboardAndFocusesSearch [23 ms]

Test Run Successful.
Total tests: 3
     Passed: 3
 Total time: 1.3161 Seconds
```

Passed: **3** — Failed: **0** — Skipped: **0** — `EXIT_CODE: 0`.

Artifact: `FEATURE/evidence/regression-testing/p9-t9.trx` (sanitised; see § Artifact hygiene).

## Per-test comparison against the P0-T13 baseline

| Test | P0-T13 baseline | P9-T9 | Verdict | Named by |
|---|---|---|---|---|
| `SetTopicThread_WhenNotInvokeRequired_SetsItemsAndSortsDescending` | passed | Passed | No regression | P9-T5 dossier (#489 D3) |
| `SetTopicThread_WhenInvokeRequired_MarshalsViaInvoke` | passed | Passed | No regression | P9-T5 dossier (#489 D3) |
| `JumpToSearchTextbox_TogglesKeyboardAndFocusesSearch` | passed | Passed | No regression | P9-T6 dossier (#490 D2) |

All three are recorded `passed` in the `BaselineNamedPins:` block of
`FEATURE/evidence/baseline/phase0-vstest-quickfiler.2026-08-28T00-14.md`. No named test was recorded
`failed` at baseline, so the P9-T9 conditional branch for a pre-existing sibling-owned failure does
not apply: the absolute pass count of 3 is asserted, and `ExpectedExitCode: 0` is declared rather
than `1`.

`JumpToSearchTextbox_TogglesKeyboardAndFocusesSearch` is the load-bearing one for the P9-T6 dossier.
It drives `controller.JumpToSearchTextbox()` against a `Mock<IItemViewer>` and asserts
`viewer.Verify(v => v.FocusSearch(), Times.Once())`. It stays green after P9-T2 rewrote the concrete
viewer body, which is exactly the point the dossier makes: the mock cannot see inside the concrete
implementation, so the marshalling change is invisible to it and only the count pair (P9-T1 = 1,
P9-T8 = 0) can prove the change happened.

## The `/TestCaseFilter:` operand

`/TestCaseFilter:` requires a `FullyQualifiedName` operand; an assertion line number is not a runnable
operand, so the three tests are selected by substring on their fully-qualified names. The alternation
separator is the pipe character, not the word `OR`, which `vstest.console.exe` rejects. The filter
matched exactly the three intended tests and nothing else — `Total tests: 3`.

## Diff clause

`git diff --numstat cecd78130a489fcfdc2ddac7970f344256f4a75a -- QuickFiler/Controllers/QfcItemController.Navigation.cs`
produces **no output row**. That file is 444-owned and read-only for this feature; the P9-T6 dossier
records its unguarded `FocusSearch()` caller at `:54` as reframed finding O3 without touching it.
This clause is absolute and is unaffected by any baseline comparison.

## Artifact hygiene

`/ResultsDirectory:` was pointed at a directory in the system temp tree, outside the repository,
because `vstest.console.exe` creates a `Deploy_<user> <timestamp>` directory on every filtered run
whose name embeds the account name — even without `/EnableCodeCoverage` — and git does not track
directories, so a `git status` gate would never warn about it. Only the sanitised TRX was copied into
the evidence tree. In this run vstest wrote no `Deploy_` directory alongside the TRX; the results
directory contained `p9-t9.trx` alone.

`p9-t9.trx` was sanitised in place before being copied: 6 occurrences of the worktree root replaced
with `<repo-root>`, 6 of the machine name with `<host>`, and 3 of the account name with `<user>`, all
case-insensitively because vstest writes the `storage=` attribute in all-lower-case. A
case-insensitive search of the committed TRX for `megalodon`, `danmoisan`, `danmoi`, a `C:` drive
path or `appdata` returns **0** for each.

**XML-escaping note.** Each placeholder is written into the TRX in entity form as `&lt;repo-root&gt;`,
`&lt;host&gt;` and `&lt;user&gt;`. XML forbids a raw less-than character in a text node or an attribute
value, so writing the literal characters would make the document unparseable; an XML reader decodes
the entity form back to the required literal. The sanitised TRX was re-parsed with a strict parser
and its `UnitTestResult` element count is **3**, matching the totals recorded above, with all three
outcomes `Passed`.

Output Summary: All three must-stay-green tests named by the two Phase 9 fail-before exception
dossiers **pass**. `EXIT_CODE: 0`, `Total tests: 3`, `Passed: 3`, `Failed: 0`, `Skipped: 0` over a
1.32-second run. Every one was `passed` at the P0-T13 baseline, so this is a clean no-regression
result and the absolute pass count of 3 is asserted with `ExpectedExitCode: 0`. The diff clause holds
absolutely: `git diff --numstat <BASELINE_SHA> -- QuickFiler/Controllers/QfcItemController.Navigation.cs`
produces no output row, confirming the 444-owned file is untouched.
