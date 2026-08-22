# Phase 5 — Post-Format File-Size Audit (Issue #445, AC20)

Timestamp: 2026-08-22T09-54

Command:
```powershell
foreach ($f in @('QuickFiler/Controllers/KaStringAsync.cs','QuickFiler/Controllers/KaChar.cs','QuickFiler/Controllers/KaKey.cs','QuickFiler/Interfaces/IKbdAction.cs','QuickFiler.Test/Controllers/KaStringAsyncTests.cs')) { (Get-Content -LiteralPath $f).Count }
```
Cross-checked with `wc -l` on the same five paths. Run from `WS`.

EXIT_CODE: 0

This audit deliberately runs **after** the final formatting pass (P5-T1 and P5-T2), because the formatter can change line counts. Measuring before the last format would have audited a state that no longer exists on disk.

## Line counts

| File | Baseline (P0-T19) | Now | 500-line cap | Shrink requirement | Verdict |
|---|---|---|---|---|---|
| `QuickFiler/Controllers/KaStringAsync.cs` | 95 | **161** | 161 < 500 | none (grows by design) | PASS |
| `QuickFiler/Controllers/KaChar.cs` | 99 | **79** | 79 < 500 | must be < 99 | PASS |
| `QuickFiler/Controllers/KaKey.cs` | 99 | **80** | 80 < 500 | must be < 99 | PASS |
| `QuickFiler/Interfaces/IKbdAction.cs` | 18 | **16** | 16 < 500 | must be < 18 | PASS |
| `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | 168 | **279** | 279 < 500 | none (grows by design) | PASS |

Total across the five files: 615 lines.

## Gate evaluation

**Every count is strictly below 500.** The largest is `KaStringAsyncTests.cs` at 279, which leaves 221 lines of headroom. No file in this change approaches the cap.

**The three shrink requirements are all met, strictly:**

- `KaChar.cs` is **79**, strictly below its baseline of 99 (shrank by 20). It lost `DelegateType` (4 lines plus surrounding blank), two `Update` properties with their backing fields (2 x 6 lines plus blanks), and the `using System.Windows.Forms;` directive.
- `KaKey.cs` is **80**, strictly below its baseline of 99 (shrank by 19). Same removals except that it correctly retains `using System.Windows.Forms;`, which accounts for the one-line difference against `KaChar.cs`.
- `IKbdAction.cs` is **16**, strictly below its baseline of 18 (shrank by 2). Exactly the two commented-out member lines.

## Growth accounted for

`KaStringAsync.cs` grew from 95 to 161 (+66): the two-clause argument guard with its explanatory comment, and the XML documentation comment recording the latch and argument contracts. `KaStringAsyncTests.cs` grew from 168 to 279 (+111): four new `[TestMethod]` blocks with Arrange-Act-Assert sections and intent comments, plus one added `using System.Collections.Generic;`.

The spec's pre-change estimates were about 110 lines for `KaStringAsync.cs` and about 225 for `KaStringAsyncTests.cs`. Both actuals exceed those estimates (161 and 279) because the XML doc comment and the test intent comments are more thorough than the estimate assumed. The estimates were not acceptance criteria; AC20's only requirements are the 500-line cap and the three shrink conditions, all of which pass with wide margins.

## Counting-method note

`(Get-Content).Count` was used, which counts physical lines including blanks, and the figures were cross-checked against `wc -l`, which agreed exactly on all five files. `Measure-Object -Line` was deliberately NOT used: it omits blank lines and would have reported understated figures (as recorded in the P0-T19 artifact), making the comparison against the physical-line baselines incommensurable. The same method is used here as at baseline, so the before/after comparison is valid.

## Out-of-scope file explicitly not remediated

`QuickFiler/Controllers/QfcCollectionController.cs` remains a pre-existing 500-line-cap violation at 2,349 lines. It is not touched by this change (P4-T2 confirms 0 `git status` lines for it) and its size is deliberately not remediated here, per Hard Constraint 6. It is recorded in the spec's Rollout & Follow-up as awareness-only, not owned by this issue.

Output Summary: All five changed files are strictly below the 500-line cap: `KaStringAsync.cs` 161, `KaChar.cs` 79, `KaKey.cs` 80, `IKbdAction.cs` 16, `KaStringAsyncTests.cs` 279 (615 total). The three files AC20 requires to shrink all shrank strictly: `KaChar.cs` 99 to 79, `KaKey.cs` 99 to 80, and `IKbdAction.cs` 18 to 16. The largest file leaves 221 lines of headroom. The audit ran after the final formatting pass so the counts reflect on-disk state, and it used the physical-line method `(Get-Content).Count` cross-checked against `wc -l`, matching the method used for the P0-T19 baselines. The pre-existing 2,349-line `QfcCollectionController.cs` violation is untouched and deliberately not remediated, per Hard Constraint 6.
