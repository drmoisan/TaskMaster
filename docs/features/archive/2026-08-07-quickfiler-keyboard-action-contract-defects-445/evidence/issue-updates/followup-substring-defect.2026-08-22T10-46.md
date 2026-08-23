# Phase 6 — Follow-Up Issue for the Out-of-Scope Non-Prefix `Substring` Defect (Issue #445, AC19)

Timestamp: 2026-08-22T10-46

Command:
```
gh issue create --repo drmoisan/TaskMaster \
  --title "KaStringAsync.KeyEquals branch 1 computes a prefix-only Substring offset under a Contains guard" \
  --body-file <body file>
gh issue view 583 --repo drmoisan/TaskMaster --json number,title,url,state
```
Run from `WS`.

EXIT_CODE: 0

PostedAs: body

- **New issue number: #583**
- **New issue URL: https://github.com/drmoisan/TaskMaster/issues/583**
- State at verification: `OPEN`
- Repository: `drmoisan/TaskMaster`
- `gh auth status`: logged in to github.com as `drmoisan`, active account, token present.

## Mechanism, and why this one

The plan flags P6-T1 as the single task whose mechanism requires orchestrator confirmation. **The orchestrator confirmed `gh issue create` directly.** The normal MCP promotion lifecycle was unavailable for two independent reasons:

1. The epic manifest forbids this child from writing under `docs/features/potential/**`, which is where the promotion lifecycle stages a potential entry.
2. `mcp__drm-copilot__potential_to_issue` is barred for this run.

There is additionally **no Python toolchain in this repository** — no `scripts/dev_tools/`, no Poetry manifest, no `pyproject.toml` — so any `poetry run python -m scripts.dev_tools.*` promotion route is **UNRUNNABLE BY ABSENCE**. That route was not attempted, not simulated, and not reported as passing.

- SearchScope: repository root of `WS`.
- SearchPatterns: `scripts/dev_tools/`, `pyproject.toml`, `poetry.lock`.
- SearchResult: none.

No file under `docs/features/potential/**` was written; P4-T3 independently confirms 0 `git status --porcelain` lines for that tree.

## Exact issue title

```
KaStringAsync.KeyEquals branch 1 computes a prefix-only Substring offset under a Contains guard
```

## Exact issue body text as posted

```markdown
## Summary

Branch 1 of `KaStringAsync.KeyEquals` guards on a **substring** test but computes its `Update` argument with **prefix** arithmetic. The two are only consistent when `other` is a prefix of `Key`.

`QuickFiler/Controllers/KaStringAsync.cs`:

- The branch-1 guard is `Key.Contains(other)` — a substring test.
- Its body computes `Update(Key.Substring(other.Length - 1, 1))`, an offset that is only meaningful when `other` is a prefix of `Key`.

For `Key = "abc"` and `other = "b"`, `Contains` is `true` and the expression yields `"a"`, which is neither the matched character nor the character following it.

## Reachability

Reachable in principle whenever the registered digit width is 2. `GenerateStringKbdAction` (`QuickFiler/Controllers/QfcCollectionController.cs:1363-1385`) registers keys `"01"` through `"12"` at that width; typing `"1"` matches `"01"` at index 1, as a substring rather than as a prefix.

It has **no observable effect today**, because `Update` is `null` on every `KaStringAsync` instance production creates:

- `QfcCollectionController.cs:1376-1383` passes `null` for both `update` and `toggleControl`.
- `KbdActions.Add(string, TKey, VDelegate)` builds its element with `UClass instance = new()` (`KbdActions.cs:99`), the parameterless constructor, which assigns neither callback.

So every `Update is not null` guard in `KeyEquals` evaluates `false` on every production evaluation. This is a latent defect, not a live one.

## Why it was excluded from #445

Fixing it correctly requires a design decision that #445 was not scoped to make: whether branch 1 should test `StartsWith` instead of `Contains`. That is a **keyboard-filtering behaviour change**, and the current substring semantics are pinned by an existing test at `QuickFiler.Test/Controllers/KbdActionsTests.cs:71-76`. Changing it would invert that test rather than leave it passing.

Per the CLAUDE.md Bugfix Workflow, section 2 ("If you uncover deeper design problems, open a new issue instead of widening scope"), the defect was recorded and deferred rather than folded into #445. Issue #445's acceptance criterion AC19 requires this issue to exist, and its P4-T4 gate verifies the expression was left unchanged:

- `Key.Substring(other.Length - 1, 1)` in `KaStringAsync.cs` — count 1, unchanged.
- `Key.Contains(other)` in `KaStringAsync.cs` — count 1, unchanged.
- The `.Be("b")` assertion at `KaStringAsyncTests.cs` (for `Key = "abc"`, `other = "ab"`, the prefix case) — count 1, unchanged.

## Options to consider

1. **Change branch 1 to `Key.StartsWith(other)`.** Makes the guard and the offset arithmetic consistent. Requires re-deciding, and re-pinning, the keyboard matching semantics currently asserted by `KbdActionsTests.cs:71-76`.
2. **Keep `Contains` and correct the offset.** For a substring match, compute the index via `Key.IndexOf(other)` and derive the intended character from that, leaving matching semantics untouched.
3. **Document the prefix precondition** and constrain registration so non-prefix matches cannot arise.

Option 2 is the narrowest and does not change which rows match a probe; option 1 is the larger behavioural change. The choice should be made deliberately, with a regression test for the two-digit-width case (`Key = "01"`, `other = "1"`).

## Acceptance Criteria

- [ ] A decision is recorded between `StartsWith` matching, corrected offset arithmetic under `Contains`, or a documented prefix precondition.
- [ ] `KaStringAsync.KeyEquals` branch 1 no longer computes a substring offset that is meaningless for a non-prefix match.
- [ ] A regression test covers the two-digit-width non-prefix case (for example `Key = "01"` with `other = "1"`) with a non-null `Update`.
- [ ] `QuickFiler.Test/Controllers/KbdActionsTests.cs:71-76` either still passes unchanged, or its change is explicitly justified as the intended behavioural change.
- [ ] The full C# toolchain passes: `csharpier check .`, the analyzer build, the nullable build, and the MSTest suite.

## References

- Parent issue: #445 (`quickfiler-keyboard-action-contract-defects`), acceptance criterion AC19.
- Spec: `docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/spec.md`, "Scope & Non-Goals" and "Rollout & Follow-up".
- Research: `docs/features/active/2026-08-07-quickfiler-keyboard-action-contract-defects-445/research/keyboard-action-contract-defects.2026-08-21T18-20.md`.
```

## Issue number written into the spec

AC19 requires the number to be recorded in the spec's Rollout & Follow-up section. The bullet previously read:

```
  - Follow-up issue for the non-prefix `Substring` defect: to be filed (see AC19).
```

It now reads:

```
  - Follow-up issue for the non-prefix `Substring` defect: **#583** —
    https://github.com/drmoisan/TaskMaster/issues/583 (filed 2026-08-22, satisfies AC19).
```

## The defect was NOT fixed in this change

P4-T4 verified the three retention counts, all equal to their P0-T19 baselines:

| Token | File | Baseline | Now |
|---|---|---|---|
| `Key.Substring(other.Length - 1, 1)` | `QuickFiler/Controllers/KaStringAsync.cs` | 1 | **1** |
| `Key.Contains(other)` | `QuickFiler/Controllers/KaStringAsync.cs` | 1 | **1** |
| `Be("b"` | `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | 1 | **1** |

One near-miss is recorded for completeness: the first draft of the P2-T3 XML doc comment quoted `Key.Substring(other.Length - 1, 1)` literally, which raised that count from 1 to 2. The paragraph was reworded to describe the expression without reproducing it, restoring the count to 1. The retention gate caught a documentation change that would otherwise have silently broken this out-of-scope verification.

Output Summary: The follow-up GitHub issue required by AC19 was filed successfully with `gh issue create` against `drmoisan/TaskMaster`, the mechanism the orchestrator confirmed. **Issue #583** was created at **https://github.com/drmoisan/TaskMaster/issues/583** and verified `OPEN`; `gh` exited 0 on both the create and the verification call. `PostedAs: body`. The MCP promotion lifecycle was unavailable (the epic manifest forbids this child from writing under `docs/features/potential/**` and `potential_to_issue` is barred), and the Python promotion route is UNRUNNABLE BY ABSENCE — no `scripts/dev_tools/`, no `pyproject.toml`, no `poetry.lock` exist in this repository, searched from the workspace root. No file under `docs/features/potential/**` was written. The exact title and full body text as posted are reproduced above. The issue number was written into the spec's Rollout & Follow-up bullet, replacing "to be filed". The out-of-scope defect itself was NOT fixed: all three P4-T4 retention counts hold at their baseline value of 1. No `POSTING BLOCKED` condition arose and no issue number was fabricated.
