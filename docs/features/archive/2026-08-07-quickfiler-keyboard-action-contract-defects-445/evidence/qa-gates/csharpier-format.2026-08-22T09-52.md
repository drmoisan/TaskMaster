# Phase 5 Stage 1 — Formatting (Mutating Pass, Scoped) (Issue #445)

Timestamp: 2026-08-22T09-52

Command:
```
& $DOTNET tool run csharpier format QuickFiler\Controllers\KaStringAsync.cs QuickFiler\Controllers\KaChar.cs QuickFiler\Controllers\KaKey.cs QuickFiler\Interfaces\IKbdAction.cs QuickFiler.Test\Controllers\KaStringAsyncTests.cs
```
with `DOTNET` = `C:\Users\DanMoisan\repos\TaskMaster\.dotnet-sdk\dotnet.exe`. Run from `WS`.

EXIT_CODE: 0

## Verbatim output

```
Formatted 5 files in 2554ms.
```

## How many of the five the formatter actually rewrote: ZERO

CSharpier's "Formatted 5 files" line reports how many files it **processed**, not how many it **rewrote**. Reading it as a rewrite count would have falsely triggered this phase's restart rule. The rewrite count was therefore measured directly, by taking a SHA-256 hash of each of the five files before the format invocation and comparing it afterwards:

```
UNCHANGED: QuickFiler\Controllers\KaStringAsync.cs
UNCHANGED: QuickFiler\Controllers\KaChar.cs
UNCHANGED: QuickFiler\Controllers\KaKey.cs
UNCHANGED: QuickFiler\Interfaces\IKbdAction.cs
UNCHANGED: QuickFiler.Test\Controllers\KaStringAsyncTests.cs
FILES_REWRITTEN=0
```

| Measurement | Value |
|---|---|
| Files passed to the formatter | 5 |
| Files processed (CSharpier's own count) | 5 |
| **Files rewritten (SHA-256 before vs after)** | **0** |
| Exit code | 0 |

**Files rewritten is 0, so the restart rule does NOT apply and this phase proceeds to P5-T2.**

The files were already CSharpier-clean because each was formatted as its owning phase completed: `KaStringAsyncTests.cs` at P1-T7, `KaStringAsync.cs` at P2-T5, and `KaChar.cs`, `KaKey.cs`, and `IKbdAction.cs` at P3-T10. This final scoped pass therefore confirms idempotence rather than performing new work, which is exactly the condition the uninterrupted-pass attestation at P5-T9 requires.

## Why the mutating pass is scoped to five files rather than repo-wide

The mutating pass is deliberately restricted to this plan's own file list. A repo-wide `format .` could reformat an unrelated file, which would appear in `git status --porcelain` and break the Phase 4 scope-lock gates (P4-T1, P4-T2, P4-T3) that assert zero modification outside this change. Verification remains repo-wide and read-only at P5-T2, so no formatting drift anywhere in the repository can escape detection.

CSharpier was invoked only through `dotnet tool run`, so the manifest-pinned 1.2.6 was used rather than any global install (Non-negotiable Command Constraint 5).

Output Summary: `csharpier format` exited 0 over the five in-scope files. CSharpier reported "Formatted 5 files", which is its processed count, not a rewrite count; the actual rewrite count was measured independently by comparing SHA-256 hashes before and after and is **0 of 5**. All five files were already formatter-clean from their per-phase format passes (P1-T7, P2-T5, P3-T10), so this stage confirmed idempotence. Because zero files were rewritten, the phase restart rule does not trigger and the loop continues to the P5-T2 verification stage.
