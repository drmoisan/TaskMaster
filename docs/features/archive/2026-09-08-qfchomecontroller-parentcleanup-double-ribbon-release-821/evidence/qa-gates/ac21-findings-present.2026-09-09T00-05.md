# Phase 5 — AC21 preserved findings survive in spec.md

Timestamp: 2026-09-09T13-25
Task: [P5-T5]

Target file:
`docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/spec.md`

Command: a `Select-String -SimpleMatch` search for each of the four literals.
EXIT_CODE: 0

Verbatim output:

```text
undisposed token sources => 1 @ 204
the synchronous Init path silently loads nothing => 1 @ 208
dormant production code => 1 @ 219
QfcHomeController.CreateCancellationToken => 1 @ 208
```

## Result

| # | Literal | Finding | Matches | spec.md line |
|---|---|---|---|---|
| 1 | `undisposed token sources` | O-3 | **1** | 204 |
| 2 | `the synchronous Init path silently loads nothing` | O-4 | **1** | 208 |
| 3 | `dormant production code` | O-5 | **1** | 219 |
| 4 | `QfcHomeController.CreateCancellationToken` | O-4 citation | **1** | 208 |

All four literals return at least one match. Findings O-3, O-4 and O-5 are all present in the
"Out of scope / non-goals" section of the spec, each with its file-and-line citation, and the O-4
paragraph still carries the citation that makes it promotable verbatim by a later reader without
re-derivation. Because the spec is committed to the feature branch, the findings reach main with the
fix rather than disappearing with the working tree.

## Discharge boundary — no GitHub issue was filed and none was required

This task performs **no GitHub API call** and writes **nothing** under `docs/features/potential/`.

AC21's own Discharge boundary paragraph states that filing O-4 as its own issue is deliberately
**not** an acceptance criterion of this feature, for two independent reasons it gives: `atomic-executor`
has neither `gh` nor the promotion MCP tools in its surface, so an executor-facing criterion demanding
an issue could never be discharged by the agent expected to discharge it; and the promotion lifecycle
writes a record under `docs/features/potential/promoted/`, which AC20's footprint lock forbids this
feature from adding, so discharging it would falsify AC20. That paragraph supersedes the
"Rollout & Follow-up" bullet which reads "promote O-4 as its own issue before merge (AC21)".

The promotion obligation is therefore carried at the orchestration layer. It is reported to the
caller in this execution's final message and in the plan's "Out-of-plan obligations" section, and it
is not discharged here.

Output Summary: all four literals return at least one match in `spec.md`, at lines 204, 208, 219 and
208. Findings O-3, O-4 and O-5 survive with their citations. No GitHub issue was filed and nothing
was written under `docs/features/potential/`.
