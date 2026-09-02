# P0-T8 — File-size admission condition re-verified against the current tree

Timestamp: 2026-09-01T19-45
Command: `foreach ($p in @('QuickFiler/Controllers/QfcItemController.ViewerSetup.cs','QuickFiler/Controllers/QfcItemController.Initialization.cs','QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs','QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs')) { [pscustomobject]@{ Path = $p; Lines = (Get-Content -LiteralPath $p).Count } }`
EXIT_CODE: 0

## Measured line counts

| Path | Measured lines | Admission condition | Holds |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 499 | exactly 499 | yes |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs` | 489 | exactly 489 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | 398 | at most 400 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` | 209 | at most 260 | yes |

All four conditions hold, so the plan is not blocked on file-size grounds. These counts were measured directly in this delivery run against the post-merge tree; they were not carried forward from the plan or from the delegating caller's figures.

`ViewerSetup.cs` sits at 499 against the 500-line ceiling, which is the fact that makes a new production partial file mandatory rather than stylistic: the two new members cannot land in that file. `Part3.cs` at 398 leaves 102 lines of headroom, against which the plan's section 2 sets a 100-added-line budget for the three spec-named tests.

## Supplementary: the four pinned call-site lines

Recorded here because the substitution tasks in Phase 2 assert against these exact line numbers, and a concurrent merge that shifted them would invalidate the plan in the same way a line-count change would.

    192|_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewAsync);
    256|await InitializeWebViewAsync();
    288|_ = InitializeWebViewAsync();
    324|_ = InitializeWebViewAsync();

Each reads exactly as the plan's section 1 states. Line 256 is the deliberately unchanged, already-observed site.

Output Summary: All four admission line counts hold — 499, 489, 398, 209 — and the four pinned call-site lines read as the plan states. The admission condition passes and execution proceeds.

Base-ref note: this task states no `git` command, so the base substitution recorded in `p0-t7-base-ref.md` does not affect it. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
