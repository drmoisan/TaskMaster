# Code Review — Issue #449, QuickFiler Explorer Controller Latent Defects

- **Timestamp:** 2026-08-22T10-58
- **Reviewer:** feature-review agent
- **Branch:** `bug/quickfiler-explorer-controller-latent-defects-449-exec` at `af6531ed`
- **Diff:** `c551eaba..HEAD` — production: `QuickFiler/Controllers/QfcExplorerController.cs` (323 → 182), `QuickFiler/Interfaces/IQfcExplorerController.cs` (15 → 14); tests: two new files (387 + 205 lines) plus two `<Compile Include>` lines in `QuickFiler.Test/QuickFiler.Test.csproj`.

## Findings

| ID | Severity | Blocking? | File / line | Finding |
| --- | --- | --- | --- | --- |
| CR-1 | Minor | Non-blocking | `QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs:1-2` | Two unused `using` directives (`System.Collections`, `System.Collections.Generic`), stranded when the conversation-view tests — the only consumers of `IEnumerable`/`List<>` — moved to the continuation file in the [P6-T14] split. Verification: `grep -n "Collections\|IEnumerable\|List<" QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs` matches only lines 1-2. No gate fires (CS8019 hidden; IDE0005 not wired into non-SDK projects). Remove opportunistically on the next touch. |
| CR-2 | Info | Non-blocking | `QuickFiler/Controllers/QfcExplorerController.cs:36,144` | Stale `//PRIORITY: Implement BlShowInConversations` and `//PRIORITY: Implement OpenQFItem` comments survive on members that are implemented and now tested. Pre-existing lines not modified by this diff; recorded only so a later reader does not mistake them for open work. Not chargeable to this change under the minimal-fix rule. |

**Blocking findings: 0.**

## Production Change Assessment

### D1 — interface member removal (`IQfcExplorerController.cs`, `QfcExplorerController.cs`)

Clean paired removal. The remaining interface carries exactly the five members the spec enumerates. Converting a latent runtime `NotImplementedException` into a compile-time absence is the correct failure-mode direction, and the legacy semantics are durably preserved in `spec.md` (`## Removed contract — legacy semantics for future restoration`) rather than reinvented later. The compatibility clause is satisfied: zero compiled callers (verified — surviving `ExplConvView_Cleanup` hits are only in uncompiled `Legacy/` and `Notes/` files), break called out.

### D2 — the one-line fix (`QfcExplorerController.cs:139`)

Correct and minimal. The guard read (line 135, `_activeExplorer.CurrentFolder.FolderPath`) and the assignment write (line 139) now address the same `Explorer` object. Line 24 is the only remaining `ActiveExplorer()` call in the file (verified: single grep hit, the constructor capture). No in-code justification for a fresh call was added, correctly, because the spec establishes no behavioral dependency on re-resolution exists.

### D3 — dead-region deletion

139 lines of unreachable duplicated code removed, including two latent defects inside it (transposed `Path.Combine` arguments; write into a null `ref string[]`), which are deleted rather than fixed — the right call for unreachable code under the minimal-fix rule. Verified: zero matches for any of the six identifiers under `QuickFiler` and `QuickFiler.Test` (down from 12 at merge-base per the dossier's non-vacuity check); external callers bind to the maintained `UtilitiesCS`/`ToDoModel` copies, which carry their own tests.

### D4 — using hygiene

Nine directives removed, six retained; self-verifying via the clean analyzer and nullable rebuilds (a required directive would have failed CS0246/CS1061). Correctly labelled as hygiene. Mildly ironic residual: the new test file introduces the same class of orphan (CR-1), though at test-file scope and below every gate's threshold.

### D5 — coverage attribute removal and dialog seam

The seam is well executed:

- `internal System.Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult>` settable auto-property with a production default of `MessageBox.Show` — matches the repository's established settable-delegate idiom (`QfcHomeController.QfcExplorerControllerLoader`) rather than inventing a new abstraction.
- The comment block at lines 49-55 explains *why* (headless testability; why the delegate type is fully qualified so it does not resurrect the removed `using System;`) — this is comment-the-why done correctly.
- User-visible dialog text, caption, buttons, and icon are byte-identical; only the invocation route changed, and the seam-routing test asserts all four arguments verbatim.
- The class enters the coverage denominator at 87.8261% — above the `QuickFiler` package average — so removing the exclusion improved rather than degraded the package figure. No exclusion attribute of any scope was reintroduced (verified: zero grep matches).

A mutable internal settable property is shared mutable state in principle, but it is the accepted repo pattern, is `internal` (reachable only via `InternalsVisibleTo`), and each test constructs its own controller instance, so no cross-test interference is possible.

## Test Quality Assessment

Fifteen test cases from fourteen methods across a `partial class` split. Strengths:

- **Regression test design (defect 2)** is the strongest element: `SetupSequence` on `ActiveExplorer()` makes the captured and drifted explorers distinguishable, and the paired `VerifySet` assertions (captured `Times.Once()`, drifted `Times.Never()`) fail together before the fix and pass together after. The recorded fail-before run shows the genuine Moq verification failure with a 399 ms duration — a real assertion failure, not a load artifact.
- **The deliberate `MockBehavior.Loose` choice** — so a pre-fix assignment lands harmlessly and surfaces as a readable verification message rather than a strict-mode exception thrown from production code — is documented in-place and is the right trade-off for a fail-before test.
- **The non-short-circuiting `&` trap** (the `CommandBars` setup remains mandatory even when the left conjunct is false) is documented in the class-level `<remarks>`, which will save the next author a confusing failure.
- **AAA structure, XML doc summaries on every test, per-test fixture reconstruction in `[TestInitialize]`** — all compliant with the unit test policy's structure, documentation, and independence requirements.
- **The partial-class split** follows the in-repo precedent (`QfcStreamingDequeueConfidenceGateTests` + `.Part2.cs`), keeps `[TestClass]` on the base file only (avoiding CS0579), and shares the fixture rather than duplicating 40 lines of mock graph — respecting the no-copy-paste rule.
- **Scenario completeness** for the changed paths: navigation positive/negative, dialog seam invoked/Yes/No, selection positive path, toggle-on/off positive and negative branches, sibling-view present/absent, pressed-state both values. Edge and negative flows are covered for every behavior this change touches.
- **Determinism**: no banned APIs (verified by scan), no temp files, no live forms, mocked COM only; `Task.Run` in production is awaited by the tests, so no timing device exists in test code. Two consecutive full-suite runs produced identical pass sets.

Weaknesses: CR-1 (unused usings) only. The declined reflection contract test (`Contract_ExplConvView_Cleanup_IsNotDeclaredOnTheInterface`) was the correct omission — see the policy audit's dossier adjudication.

## Disclosed-Item Assessment (code-review perspective)

1. **Flaky `ProgressTrackerAsync` test:** disclosure without suppression was the correct engineering response. The evidence (fail once at 793 ms under load; pass at 191 ms in isolation; pass in two subsequent identical full-suite runs on an unchanged tree) is the standard signature of a timing race in an unrelated component, and the root cause identified in the promotion document (#584 — `UiThread.Dispatcher` static `null!` field with no lazy initialisation) is structural and pre-existing. Nothing in this diff can have caused it. Not a blocker.
2. **Fail-before dossiers:** both justified; the compiler gate plus the full-suite set comparison (+15 added / 0 removed) is a stronger verification for a no-caller removal and a dead-code deletion than any constructible test would have been. Detailed adjudication is in the policy audit, Section 5.2.

## Verdict

Approve. **0 Blocking findings**; 1 Minor (CR-1) and 1 Informational (CR-2) finding, neither warranting a remediation cycle.
