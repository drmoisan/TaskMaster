# Decision Record — re-scope #511 after its premise was falsified

Recorded: 2026-08-23T20:40 UTC
Decided by: maintainer, interactive `/orchestrate` session
Canonical issue number: 511 (secondary: 571)

The orchestrator checkpoint at `artifacts/orchestration/orchestrator-state.json` is gitignored
(`.gitignore:57`), so this file is the committed record of the decision it holds.

## Why the child halted

Execution of Phases 0 through 4 completed, every delegation returned, and every validator
passed — but measurement falsified the plan's central premise. Six findings were recorded; three
were independently re-verified against the working tree and the evidence tree on 2026-08-23.

| ID | Severity | Finding |
| --- | --- | --- |
| A | blocking | The remedy is a measured no-op. The inserted `_ = await host.InvokeAsync(() => viewer.Handle)` forces a window handle that already exists. |
| B | blocking | The only genuine pre-fix failure observed was a 60,000 ms `PumpTimeoutMs` expiry under machine load — a different root cause, which the remedy does not address. |
| C | blocking | The 30-of-30 post-fix green record is statistically consistent with the remedy having no effect (about a 1-in-4 chance of arising with no fix at all). |
| D | blocking | Two inserted comments assert the opposite of what was measured, and contradict a corrected assertion in the same commit. |
| E | blocking | Spec acceptance criterion 6 is unsatisfiable as worded. |
| F | resolvable | P4-T2's absolute-zero gate spans all nine assemblies, so it trips on pre-existing flakes this diff cannot reach. |

Provenance for A: `evidence/regression-testing/webview-child-handle-measurement.2026-08-21T18-10.md`
records four configurations. Run 2 (Phase 2 statement commented out) is identical to run 1 on the
measured value, and run 4 shows a bare `new ItemViewer()` on the pump thread — no harness, no
`SaveParameters`, no `.Handle` read — already reporting both WebView2 children handle-created. The
handles originate in `InitializeComponent`'s third-party `ISupportInitialize.EndInit()` calls;
WinForms creates a parent's handle when a child's is created, which is why the viewer already had
one on every pre-fix run.

## Decision

**Re-scope #511.** Keep the fixture hardening and the mechanism finding as durable value.

1. Correct the four false comments so they state the measured truth
   (`QfcItemController.InitializationTests.Part2.cs` lines 87-90;
   `QfcItemController.ViewerSetupTests.cs` lines 436-438). CLAUDE.md C#6.3 requires comments to
   stay synchronized with behavior.
2. Revise spec acceptance criterion 6 to assert the measured inherited state, and reconcile the
   three other unchecked criteria against the re-scoped claim.
3. Narrow P4-T2's zero condition to the classes this child owns, per the ratified precedent for an
   absolute-zero gate over a sibling-owned assembly.
4. File follow-up issues for the load-induced pump-timeout cascade and the three sibling-assembly
   flakes.
5. **Open a pull request that does NOT claim to close #511.** Findings A, B and C are accepted as
   accurate and are addressed by re-scoping the claim, not by changing code.

Rejected alternatives: re-planning against the measured root cause (the epic's hard constraints —
no production edits, no timeout changes, no sleeps or retries, no injectable
synchronization-context seam — may leave no in-scope remedy); and abandoning the child, which
would discard the fixture hardening and the mechanism finding.

## Host-identifier sanitization

Performed the same session at maintainer instruction, on the standing rule that **no file may ever
embed an absolute host path or host identifier**:

- 140 untracked evidence paths renamed to strip the `vstest.console.exe` default
  `<account>_<HOST>_` filename prefix.
- 10 tracked markdown evidence files stripped of that prefix; the references point at the renamed
  files, so accuracy is preserved.
- 91 absolute-path occurrences across 27 tracked files replaced with the portable placeholders
  `<repo-root>`, `<user-profile>`, `<user>` and `<host>`, applied longest-first.
- Convention recorded at `.claude/agent-memory/_shared_no_absolute_host_paths.md` and indexed from
  five agent memory indexes, including the vstest default-naming trap that caused it.

Not done, deliberately: roughly 146 tracked files in other and archived feature folders still carry
the prefix, and about 157 still carry the bare host name, including `.claude/settings.json` and
`.vscode/settings.json`. Sanitizing them here would break this child's scope-lock acceptance
criterion, so the remainder is tracked as its own issue.

## Resume point

`next_step: S6_remediation_R1_planning`. Resolved delegation models for band C3 under
`fable_policy: preferred` — atomic-planner `fable`, atomic-executor `opus`, feature-review `fable`,
pr-author `opus`. The ordered next actions are in the checkpoint's `resume_instructions`.
