# P0-T1 — Policy and remediation-source reads (remediation cycle 1)

Timestamp: 2026-08-28T03-39
Task: [P0-T1]
Command: (read each file listed below in full from the remediation worktree; no command output is
produced by a read, so this artifact records the read set rather than a command result)
EXIT_CODE: 0

## Policy Order:

The four policy files were read in the `policy-compliance-order` sequence, in this order:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`

## Files read (all eight)

| # | File | Kind |
|---|---|---|
| 1 | `CLAUDE.md` | policy |
| 2 | `.claude/rules/general-code-change.md` | policy |
| 3 | `.claude/rules/general-unit-test.md` | policy |
| 4 | `.claude/rules/csharp.md` | policy |
| 5 | `docs/features/active/itemviewer-surface-defects-489/remediation-inputs.2026-08-28T03-13.md` | remediation source |
| 6 | `docs/features/active/itemviewer-surface-defects-489/code-review.2026-08-28T03-13.md` (§ RC-1) | remediation source |
| 7 | `docs/features/active/itemviewer-surface-defects-489/spec.md` (§ Acceptance Criteria and § Sibling-collision resolution) | remediation source |
| 8 | `docs/features/active/itemviewer-surface-defects-489/evidence/other/wireintentevents-16-to-17-handoff.2026-08-28T01-55.md` | remediation source |

## Binding obligations carried forward from the policy reads

- **General code change** — bugfix workflow: failing regression test first, then the minimal targeted
  fix, then the full toolchain in order. 500-line ceiling on production and test files. Fail fast;
  no silent error swallowing.
- **General unit test** — independence, isolation, determinism; no temporary files; no external
  dependencies; tests mirror the production tree under a test project.
- **C# rules** — CSharpier via `dotnet tool run` only; analyzer and nullable builds with `/t:Rebuild`
  and the spaced `"/p:Platform=Any CPU"` on the solution; **do not pass `/p:Nullable=enable`**;
  MSTest + Moq + FluentAssertions; do not weaken assertions or relax test expectations to make a test
  pass; do not create analyzer debt.
- **CLAUDE.md** — the four-step toolchain (format, lint, type-check, test) is one pass; restart from
  step 1 on any failure or any file rewrite.

## Substance of the remediation sources

- **RC-1 (Blocking)** — `WireIntentEvents()` performs 17 subscriptions; `UnwireIntentEvents()`
  performs 16 detachments. The 17th subscription, `PicturesChanged`, has no counterpart, so a
  controller that is wired and then torn down through `Cleanup()` retains one live subscription on a
  pooled viewer. The invariant to restore: every event `WireIntentEvents()` subscribes is detached by
  `UnwireIntentEvents()`.
- **Directed changes** — one production line, one RED-first regression test in this feature's own
  `Part2.cs` continuation file, a dated spec amendment preserving the criterion count, a handoff-record
  addendum, and a full gate refresh.
- **Constraints** — the 484-owned test `UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` is
  neither renamed nor edited; `EventWiringTests.cs` (499/500 lines) is not touched at all; no sibling
  feature folder is edited.
- **Handoff record** — records `Upstream484Landed: true`, i.e. 484 was already merged when the
  obligation was written, so the obligation had no live recipient. That is what P3-T1 reconciles.

Output Summary: All eight files read in the mandated order — the four policy files in the
`policy-compliance-order` sequence (`CLAUDE.md`, `general-code-change.md`, `general-unit-test.md`,
`csharp.md`), then the four remediation sources (`remediation-inputs`, `code-review` § RC-1, `spec.md`
§ Acceptance Criteria and § Sibling-collision resolution, and the 16-to-17 handoff record).
`EXIT_CODE: 0`. The binding constraints carried into execution are: `/t:Rebuild` with the spaced
platform spelling on the solution and no `/p:Nullable=enable`; CSharpier only through
`dotnet tool run`; RED-before-GREEN for the regression test; no edit to `EventWiringTests.cs` and no
rename of the 484-owned sixteen-detachment test; and the 500-line ceiling on both edited files.
