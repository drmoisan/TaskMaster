# F1 — Temporary Mutation Applied (Cycle 1, Issue #503)

Timestamp: 2026-08-08T14-52
Task: [P1-T5]

Edit: deleted exactly line 103 of `TaskMaster\Ribbon\RibbonExplorer.xml` — the `getEnabled="EngineCommand_GetEnabled"` attribute line inside the `<button id="TrainSpam" ...>` element that spans lines 99-105. Nothing else was changed.

Command (verification): `pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; (Select-String -Path 'TaskMaster\Ribbon\RibbonExplorer.xml' -Pattern 'getEnabled=\"EngineCommand_GetEnabled\"' -AllMatches | Measure-Object).Count; (Get-Content 'TaskMaster\Ribbon\RibbonExplorer.xml' | Measure-Object -Line).Lines; git diff --numstat -- TaskMaster/Ribbon/RibbonExplorer.xml"`
EXIT_CODE: 0

## THIS MUTATION IS TEMPORARY AND MUST NEVER BE COMMITTED

Plan section 3 rule 8 governs this window. P1-T8 restores the file with `git checkout --` and is its own verified task. **If execution halts for any reason before P1-T8 completes, the first action on resume is to run P1-T8.** No commit task may execute while this mutation is present; the Phase 4 commit (P4-T3) is gated on the P1-T8 restoration artifact recording an empty porcelain for this path.

## Output Summary

```text
7
538
0	1	TaskMaster/Ribbon/RibbonExplorer.xml
```

| Measurement | Value | Expected |
|---|---|---|
| `getEnabled="EngineCommand_GetEnabled"` occurrences | **7** | 7 |
| Line count | **538** | 538 |
| `git diff --numstat` added / deleted | **0 / 1** | 0 / 1 |

`wc -l` independently corroborates 538 physical lines. `RibbonExplorer.xml` contains no blank lines, so the two counting methods agree.

## Verbatim diff

```diff
diff --git a/TaskMaster/Ribbon/RibbonExplorer.xml b/TaskMaster/Ribbon/RibbonExplorer.xml
index 9d8403ee..6b2ee03d 100644
--- a/TaskMaster/Ribbon/RibbonExplorer.xml
+++ b/TaskMaster/Ribbon/RibbonExplorer.xml
@@ -100,7 +100,6 @@
             id="TrainSpam"
             imageMso="CancelAll"
             onAction="TrainSpam_Click"
-            getEnabled="EngineCommand_GetEnabled"
             label="Train Spam"
           />
           <button
```

Exactly one deleted line, zero added lines, one hunk. The `TrainSpam` element was chosen because it was already in multi-line form, so the deletion needs no re-indentation, and because it sits in `<group id="SpamBayesGroup">` rather than the `<group id="TriageGroup">` the F2 edit touches, keeping the two changes disentangled.

Binary outcome satisfied: 7 occurrences, 538 lines, `git diff --numstat` reports `0` added and `1` deleted.
