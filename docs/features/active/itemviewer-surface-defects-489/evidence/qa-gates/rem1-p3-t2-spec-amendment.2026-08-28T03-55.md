# P3-T2 — spec.md amendment, criterion count preserved

Timestamp: 2026-08-28T03-55
Task: [P3-T2]
Command: git grep -F -c "Amendment (2026-08-28)." -- docs/features/active/itemviewer-surface-defects-489/spec.md
EXIT_CODE: 0

## The three coordinated edits

`git diff --stat` for `spec.md`: **45 insertions, 2 deletions** across 1 file. The two deletions are
the two lines that were rewritten in place (the disposition table row and the criterion); no content
was removed.

### 1. Sibling-collision resolution, disposition 1

The Disposition cell of table row 1 is extended. Its original text is preserved verbatim and the
following is appended to it:

> **Amended 2026-08-28 (remediation cycle 1):** this agreed cross-child `EventWiring.cs` edit covers
> `WireIntentEvents` **and the single matching `UnwireIntentEvents` detachment**
> `_itemViewer.PicturesChanged -= this.CbxPictures_CheckedChanged;`. Phase 0 re-checked and found 484
> already landed (`Upstream484Landed: true`), so the recorded hand-off had no live owner and the
> detachment was discharged in this branch after review finding RC-1. The `EventWiring.cs` diff is
> still two lines: one wire, one detachment.

This keeps the scope-discipline criterion — "each diff is confined to the members named in
§ Sibling-collision resolution" — true after Phase 2, **without touching that criterion's text**. The
`UnwireIntentEvents` member is now a named member of the agreed disposition, so the two-line
`EventWiring.cs` diff remains confined to members this section names.

### 2. The Issue #486 criterion

Amended in place. Original wording, retained inside the criterion verbatim:

> The plan or the executor's handoff record states the `WireIntentEvents` / `UnwireIntentEvents` count
> change from 16 to 17 and names it as an obligation on upstream 484. Recorded in `evidence/other/`.

The criterion now *additionally* requires that the same handoff record carry a dated addendum
recording the obligation as discharged in this branch, marked by the field
`ObligationDischargedInBranch: true`. The P3-T1 addendum is that evidence.

The criterion is **strengthened, not weakened**: it retains every condition it previously imposed and
adds one. It remains a single checkbox line, still `[x]`, so the criterion count is unaffected.

### 3. The dated amendment note

`**Amendment (2026-08-28).**` is added under § Acceptance Criteria, placed immediately after the
existing `**Amendment (2026-08-27).**` note and following its precedent. It:

- quotes the original wording of the amended disposition clause;
- quotes the original wording of the amended criterion;
- states the reason: RC-1 measured 17 live subscriptions against 16 live detachments, and Phase 0 had
  already recorded `Upstream484Landed: true`, so the recorded hand-off had no live owner and the
  detachment was supplied in-branch with a RED-first regression test;
- states that the criterion count is **unchanged at 62** and that **no criterion is weakened**;
- records that the § Risks & Mitigations row about the 16-to-17 mirror is **superseded** by the
  in-branch fix — its named mitigation (a hand-off record plus a criterion) proved insufficient
  because the recipient had already merged — while leaving the row in place as the historical
  statement of the risk;
- records the deliberate non-rename of `UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions`,
  with the reason.

The note introduces no checkbox line, so it cannot perturb the criterion count.

## Acceptance

### (a) Criterion count equals the P0-T3 baseline

Measured exactly as P0-T3 measured it: the count of `spec.md` lines matching `^- \[[ x]\] `.

| | Value |
|---|---:|
| `CheckboxBaselineCount:` from P0-T3 | 62 |
| Measured after the amendment | **62** |

Equal. The measuring block ran under `$ErrorActionPreference = 'Stop'` and reported `$?` = `True` and
`$Error.Count` = `0`. An independent count taken inside the amendment script, before and after the
three edits, likewise reported 62 and 62.

### (b) The amendment marker appears exactly once

```
docs/features/active/itemviewer-surface-defects-489/spec.md:1
```

Reported count: **1**, exactly as required. `git grep -c` counts matching lines, and the literal
`Amendment (2026-08-28).` occurs on one line only.

### (c) The amended criterion names the field

```
docs/features/active/itemviewer-surface-defects-489/spec.md:1
```

Reported count: **1**, which satisfies "at least 1". The single occurrence is inside the amended
Issue #486 criterion, which is where the acceptance condition intends it: the criterion itself names
the machine-checkable field its evidence must carry.

Both greps in (b) and (c) found matches and therefore exited `0`. The zero-match residual described in
the plan's convention 6 does not arise here; the verdict is nonetheless taken from the reported counts
rather than from exit codes, as that convention directs generally.

## Encoding

`spec.md` was pure CRLF with no BOM before the edit and is pure CRLF with no BOM after it: 922 CRLF
pairs, **0** lone LF, BOM absent. No in-place stream editor was used — the edits were byte-exact
replacements — because a stream editor would have rewritten the whole file to LF and turned a
three-hunk diff into a whole-file rewrite.

## Acceptance summary

| P3-T2 condition | Result |
|---|---|
| (a) checkbox line count equals `CheckboxBaselineCount:` (62) | **Yes** — 62 |
| (b) `Amendment (2026-08-28).` reports exactly 1 | **Yes** — 1 |
| (c) `ObligationDischargedInBranch` reports at least 1 | **Yes** — 1 |

Output Summary: Three coordinated in-place amendments applied to `spec.md` — 45 insertions, 2
deletions. Disposition 1 of § Sibling-collision resolution now covers `WireIntentEvents` **and** the
single matching `UnwireIntentEvents` detachment, keeping the scope-discipline criterion true without
editing that criterion's text; the Issue #486 criterion retains its original wording verbatim and
additionally requires the dated `ObligationDischargedInBranch: true` addendum, so it is strengthened
and remains `[x]`; and a `**Amendment (2026-08-28).**` note quotes both original passages, states the
RC-1 reason, records the criterion count as unchanged at 62 with no criterion weakened, marks the
§ Risks & Mitigations 16-to-17 row superseded, and records the deliberate non-rename of the
484-owned test. All three acceptance conditions pass: checkbox count **62** equal to the P0-T3
baseline, amendment marker count **1**, `ObligationDischargedInBranch` count **1**. Pure CRLF and
no BOM preserved: 922 CRLF pairs, 0 lone LF.
