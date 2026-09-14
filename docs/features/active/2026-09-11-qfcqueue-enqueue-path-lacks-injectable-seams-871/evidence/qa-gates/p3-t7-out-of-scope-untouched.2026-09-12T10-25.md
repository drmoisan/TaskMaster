# P3-T7 — out-of-scope running-jobs counter leak left unfixed and undisturbed

Timestamp: 2026-09-13T15-42

Command: git diff -U0 8213826f695439e86e3ed34faa575de493a11ec7 -- QuickFiler/Controllers/QfcQueue.Enqueue.cs

EXIT_CODE: 0

Output Summary:
- The sole increment of the running-jobs counter is at line 94; the `try` keyword that opens the
  block whose `finally` holds the sole decrement is at line 101; the sole decrement is at line 128.
- 94 is strictly less than 101, so the increment still sits outside the try block. The separately
  promoted defect is therefore still present and was not repaired by this phase.
- The anchored zero-context diff produced exactly three hunks. Their old-side line ranges are
  [91,91], [97,99] and [177,177]; their new-side line ranges are [91,91], [97,97] and [175,175].
  None of those six ranges contains the increment line (94 at the anchor, 94 now), the `try` line
  (103 at the anchor, 101 now) or the decrement line (130 at the anchor, 128 now).
- Verdict: no hunk touches the increment, the `try` or the decrement.

## Line locations measured in the current tree

Command: pwsh -NoProfile -Command '$l=@(Get-Content -LiteralPath QuickFiler/Controllers/QfcQueue.Enqueue.cs); $inc=@($l | Select-String -SimpleMatch "Interlocked.Increment(ref _jobsRunning)"); $dec=@($l | Select-String -SimpleMatch "Interlocked.Decrement(ref _jobsRunning)"); $try=@($l | Select-String -Pattern "^\s*try\s*$"); ...'

```
INC-COUNT=1
INC-LINE=94
TRY-COUNT=1
TRY-LINE=101
DEC-COUNT=1
DEC-LINE=128
```

IncrementLine: 94
TryLine: 101
DecrementLine: 128
IncrementPrecedesTry: true

## Anchored zero-context content diff of the single path

```
diff --git a/QuickFiler/Controllers/QfcQueue.Enqueue.cs b/QuickFiler/Controllers/QfcQueue.Enqueue.cs
index e3b30c522..886a55f5b 100644
--- a/QuickFiler/Controllers/QfcQueue.Enqueue.cs
+++ b/QuickFiler/Controllers/QfcQueue.Enqueue.cs
@@ -91 +91 @@ namespace QuickFiler.Controllers
-                items.ForEach(item => _moveMonitor.HookItem(item, async (x) => await RemoveItem(x)))
+                items.ForEach(item => MoveMonitor.HookItem(item, async (x) => await RemoveItem(x)))
@@ -97,3 +97 @@ namespace QuickFiler.Controllers
-            var tlp = await UiIdleCallAsync(() =>
-                _tlpTemplate.Clone(name: "BackgroundTableLayout")
-            );
+            var tlp = await UiIdleCallAsync(() => BackgroundTlpFactory(_tlpTemplate));
@@ -177 +175 @@ namespace QuickFiler.Controllers
-                .SelectAwait(async i => (i: i, grp: await AddAsync(tlp, items[i - start], i)))
+                .SelectAwait(async i => (i: i, grp: await ItemGroupFactory(tlp, items[i - start], i)))
```

The three hunks are, in order, the P2-T2 move-monitor seam substitution, the P3-T6 background
template seam substitution and the P3-T5 item-group seam substitution. No hunk is attributable to
the counter bookkeeping.

## Porcelain capture

The name-status span of CMD-DIFF reports one status letter per file and carries no line content, so
it cannot carry the observation above. The porcelain capture is recorded because the edits this
phase has made are not yet committed and are therefore invisible to any commit-to-commit
comparison.

Command: git status --porcelain --untracked-files=all

```
 M QuickFiler/Controllers/QfcQueue.Enqueue.cs
 M QuickFiler/Controllers/QfcQueue.Tlp.cs
 M docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/plan.2026-09-12T10-25.md
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/coverage-baseline.2026-09-12T10-25.cobertura.xml
?? docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/p3-t5-itemgroup-callsite.2026-09-12T10-25.md
```

Every path reported satisfies the Scope-lock rule: the first three are Write Set paths and the last
two lie underneath the Write Set baseline and qa-gates evidence directories. The raw Cobertura
document is deliberately left untracked under repository evidence-hygiene policy and is not staged
by any commit in this plan.
