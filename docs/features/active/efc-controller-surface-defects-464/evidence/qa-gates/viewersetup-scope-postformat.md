# [P10-T2] `QfcItemController.ViewerSetup.cs` one-line constraint, re-verified after formatting

Timestamp: 2026-08-28T01-56
Task: [P10-T2]
Command: `git diff --numstat <BASE> -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
EXIT_CODE: 0

`[P10-T1]` included this file in its mutating formatting pass, so the one-line constraint is re-measured
here.

## Result

| Base | added | deleted |
|---|---|---|
| `38f097898639b054428188c9c5e266e54972c259` (evaluated) | **1** | **1** |
| `002335989830ba9f3ad802858ef0b794f6281750` (`BASELINE_SHA`, as written) | **1** | **1** |

The figures are unchanged from `[P9-T5]`, which measured the same 1 added / 1 deleted before the
formatting pass. The file's line count is **499**, also unchanged.

## No revert was required

`[P10-T1]` recorded that the SHA-256 of this file is byte-identical before and after the formatting
command: `16aa8af844b64a952be5b603c78db5cff388dcc8c5b8d0663ae6932f598963be` both times. CSharpier
rewrote nothing in it, so formatting introduced no additional change and there was nothing to revert.
`git status --porcelain` was empty immediately after the formatting pass, corroborating this.

The task's conditional remedy — "if formatting introduced any additional change to that file, revert the
formatting-only change to it and record the revert" — did not fire.

Output Summary: PASS. After the `[P10-T1]` formatting pass the diff over
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is still exactly 1 added and 1 deleted line
under both bases, and the file is still 499 lines. CSharpier did not rewrite the file (identical SHA-256
before and after), so no revert was needed and none was performed.
