# Code Review — winformspumphost-suite-determinism-511

- Timestamp: 2026-08-24T00-01 (UTC)
- Branch: `bug/winformspumphost-suite-determinism-511-exec`
- Base: `main` @ merge base `f85a36faebaaec29fe5233c9d9f69d223d80e4c5`
- Head: `b4d47adc369d021f6fb4eff092f419dc49e9a5e5`
- Scope: full branch diff (3 C# test files, 68 feature-folder docs/evidence files, 26 agent-memory files)

## Executive Summary

This is remediation cycle 1's re-review. The executable-code diff is small and test-only: two
corrected comment blocks (Part2.cs, ViewerSetupTests.cs) and two new regression tests (Part3.cs,
+108 lines). The code quality of the additions is good: both tests follow the established
pump-host pattern, use MSTest + FluentAssertions with explicit `because:` messages, carry XML doc
summaries that state the measured mechanism accurately, clean up in `finally`, and preserve the
`UiThreadDispatcherGate` serialization. The two corrected comment blocks now state the measured
truth and the deliberate redundancy of the retained `viewer.Handle` read, exactly as remediation
Finding D required.

**Blockers: 0.** Five non-blocking findings are recorded: two Minor documentation-consistency
residuals in `spec.md` (a stale Root Cause Analysis narrative and AC wording that predates the
maintainer-instructed raw-TRX deletion) and three Informational observations. None requires a code
change before the pull request.

## Findings Table

| ID | Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
| --- | --- | --- | --- | --- | --- | --- | --- |
| CR-1 | Minor | docs/features/active/winformspumphost-suite-determinism-511/spec.md | `## Root Cause Analysis`, "Confirmed root cause of #571" and "visible-window" subsections | The section still asserts pre-measurement claims — "`IsHandleCreated` is `false` for the whole test" and "The `ItemViewer`'s two WebView2 children never obtain a handle" — that the committed measurement (`evidence/regression-testing/webview-child-handle-measurement.2026-08-21T18-10.md`) and the revised AC 6 / Scope sections contradict. The section's own "Unresolved question" hedge names the mechanism later confirmed (explanation 1, `ISupportInitialize.EndInit`), but the flat assertions carry no revision marker. | In a follow-up (or in the PR body), add a dated annotation to `## Root Cause Analysis` stating that the measurement confirmed explanation 1 and superseded the two static-reading assertions. Do not rewrite history; annotate it. | Remediation-inputs Part 6 deliberately scoped the spec revision to the AC and Scope sections, so this is outside the ratified cycle scope; but a future reader of the unannotated section could take the falsified static reading as current fact. | `spec.md` "Confirmed root cause" text vs. measured record; revised AC 6 in the same file |
| CR-2 | Minor | docs/features/active/winformspumphost-suite-determinism-511/spec.md | AC 1 and AC 3 wording | AC 1 promises "the ten TRX results stored under `evidence/regression-testing/`" and AC 3 "the evidence stored under `evidence/regression-testing/`", but the 56 raw TRX (and 42 `.coverage`) files were deleted at explicit maintainer instruction after the orchestrator verified the committed distillation reproduces them exactly. The literal storage claim is no longer true on disk. | Align the AC wording with the recorded disposition (cite `evidence/other/raw-vstest-artifact-disposition.2026-08-23T21-40.md`), or state the supersession in the PR body. | The maintainer-instructed, committed disposition record governs; the distilled markdown is the evidence of record and was fidelity-checked against the raw TRX before deletion. The residual is a wording drift, not an evidence gap. | `raw-vstest-artifact-disposition.2026-08-23T21-40.md`; `find`/`git ls-files` confirm zero TRX under the evidence tree |
| CR-3 | Info | QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs | `:301-335` (`BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread`) | The test name ("Forces") and the first `because:` message ("the harness must create the viewer's window handle on the pump thread") frame the harness as the creator, while the measured mechanism is inheritance from `ItemViewer` construction. The asserted invariant (handle exists; no marshalling required on the pump thread) is accurate either way, and the sibling test's `<remarks>` states the inheritance mechanism explicitly. | None required. The name is pinned verbatim by spec AC 5; renaming would break the AC. Optionally soften the `because:` message to "must hand back a handle-created viewer" in a future touch. | The contract asserted is the fixture invariant, not the mechanism; the companion test documents the mechanism. | Diff hunk at `Part3.cs:289-335`; AC 5 text in `spec.md` |
| CR-4 | Info | docs/features/active/winformspumphost-suite-determinism-511/plan.2026-08-21T18-10.md | Phases 5-6 (29 unchecked tasks) | The original plan's Phase 5/6 checkboxes remain `[ ]`. That work was carried, adjusted, and completed by `remediation-plan.2026-08-23T20-57.md` (42/42 tasks checked), whose header records the supersession ("this plan carries the remaining work as adjusted by the remediation inputs"). | None required; recorded so a later reader does not misread the original plan as abandoned mid-phase. | The remediation plan is the authoritative record of the executed final QC loop and check-offs. | `remediation-plan.2026-08-23T20-57.md` header; 42/42 checked |
| CR-5 | Info | docs/features/active/winformspumphost-suite-determinism-511/spec.md | AC 10 (`SwapUiThreadDispatcher (:139)`) | The cited line number predates the 9-line insertion in Part2.cs; the helper's definition now sits at `:148` (first use at `:138`). The structural claim (acquire-and-release intact) holds. | Optionally refresh the line citation in a future spec touch. | Line-number drift only; the epic's "re-derive every line number" constraint anticipated exactly this class of drift. | `grep -n SwapUiThreadDispatcher Part2.cs` → 138/148/348; gate at `:51` unchanged |

## Detailed Review — Changed Code

### QfcItemController.InitializationTests.Part3.cs (+108)

- `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` (`:301`): Arrange builds the shared
  harness; Act reads `Viewer.InvokeRequired` on the pump thread through `host.InvokeAsync`; Assert
  checks `IsHandleCreated == true` and `invokeRequiredOnPumpThread == false`. Cleanup restores the
  swapped dispatcher and stops the host in `finally`. Correct use of `ConfigureAwait(false)`
  throughout, consistent with the file's existing style.
- `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` (`:356`): reads both WebView2 children's
  `IsHandleCreated` on the pump thread and asserts both true, with a `<remarks>` block that states
  the measured provenance ("Measured, not predicted") and the exact falsification logic. This is
  the strongest artifact in the diff: it converts the falsified premise into a pinned, loudly
  failing invariant if a future WebView2 or Designer change alters handle-creation behavior.
- Both tests reuse `BuildPumpHarnessAsync`, so the `UiThreadDispatcherGate` acquire/release and
  dispatcher swap/restore semantics are inherited from the existing, proven fixture. No new
  synchronization primitives, no timing constructs.

### QfcItemController.InitializationTests.Part2.cs (+9) and QfcItemController.ViewerSetupTests.cs (+7)

- Comment-only rewrites above the retained `_ = await host.InvokeAsync(() => viewer.Handle)`
  statements. Both now state: (a) the measured truth (children and parent are handle-created at
  construction via Designer-emitted `ISupportInitialize.EndInit()`); (b) that the read is
  therefore redundant today; and (c) that it is retained deliberately as a defensive measure
  against an uncontrolled third-party side effect. This satisfies remediation Finding D's full
  requirement, including the mandatory redundancy statement, and is consistent with the corrected
  assertions in Part3.cs. The discard (`_ =`) form is appropriate for a side-effect-motivated read.

### Documentation and evidence tree

- The spec's AC and Scope sections are internally consistent with the measured record and with the
  code (CR-1 is the one remaining stale narrative, in a section outside the ratified revision
  scope).
- Evidence artifacts are consistently structured (`Timestamp:` / `Command:` / `EXIT_CODE:` /
  `Output Summary:`), use portable path placeholders, and contain no host identifiers in added
  lines.
- Commit messages in range are truthful about non-repair ("NOT a fix") and carry no closing
  keywords for #511/#571.

## Verdict

Approve. Zero blocking findings; CR-1 and CR-2 are documentation follow-ups that do not gate the
pull request.
