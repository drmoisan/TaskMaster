# P2-T1 — CSharpier format (apply), remediation cycle 1

Timestamp: 2026-09-02T01-32

Command: `dotnet tool run csharpier format .`
EXIT_CODE: 0

## Output Summary

Summary line printed, verbatim, on the final (second) pass:

```
Formatted 1575 files in 2042ms.
```

CSharpier prints a **processed**-file count rather than a rewritten-file count, and exits 0
whether or not it rewrote anything, so that line alone does not distinguish a clean run from
a repairing one. The `git status --porcelain` observation below is what does. (The count is
1575 rather than the 1574 recorded at the P0-T5 baseline because this cycle added one file,
`QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs`.)

## Pass 1 — the repairing pass

`git status --porcelain` immediately **before**:

```
 M docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/remediation-plan.2026-09-01T23-44.md
```

`git status --porcelain` immediately **after**:

```
 M QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs
 M QuickFiler/Controllers/QfcHomeController.cs
 M docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/remediation-plan.2026-09-01T23-44.md
```

Paths rewritten by pass 1, listed by name:

| Path | Nature of the rewrite |
|---|---|
| `QuickFiler/Controllers/QfcHomeController.cs` | the `ReconcileCarriersToItems(batch.Items, batch.PreScored)` call collapsed from three lines onto one |
| `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs` | one `.Returns(...)` collapsed onto one line; one `.ContainSingle(...)` expanded onto three |

Both rewrites are cosmetic reflow and neither changes a token. The pre-existing modification
of the plan file appears in both snapshots and is this executor's own check-off writing, not
a CSharpier rewrite.

**No path outside the `QuickFiler/` and `QuickFiler.Test/` prefixes was rewritten**, so the
restoration clause did not fire and no `git checkout 807fb0bb6e5e49f43efa6b256b05960bf078ca19 --`
was issued for any path. This is consistent with the P0-T5 baseline, at which
`R_BASELINE_FORMAT_DRIFT` was empty: the whole tree was already CSharpier-clean, so every
rewrite pass 1 performed is attributable to this cycle's own edits.

## Pass 2 — the clean pass

Because pass 1 changed two files under `QuickFiler/` and `QuickFiler.Test/`, the Phase 2 loop
rule required a restart from P2-T1. Pass 2 was run immediately.

`git status --porcelain` immediately before and immediately after pass 2 were compared with
`diff` and are **identical** (the comparison printed no differing line). Pass 2 therefore
rewrote **no path at all**, which is the clean-run observation the acceptance clause requires.
The exit code was 0 on both passes and is not what establishes this.

## Acceptance clauses

| # | Clause | Result |
|---|---|---|
| 1 | `EXIT_CODE: 0` | PASS — 0 on both passes |
| 2 | `Output Summary:` reproduces the printed summary line verbatim, with the processed-versus-rewritten note | PASS |
| 3 | before-and-after `git status --porcelain` recorded, every rewritten path named | PASS — two paths named for pass 1, zero for pass 2 |
| 4 | any path rewritten outside the two prefixes is restored, by path, with the reason | PASS, vacuously satisfied and recorded as such: no path outside the two prefixes was rewritten on either pass, so no restoration was required |
