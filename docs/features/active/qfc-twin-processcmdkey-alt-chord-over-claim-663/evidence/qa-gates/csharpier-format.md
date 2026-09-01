# Phase 4 — Scoped CSharpier format ([P4-T1])

Timestamp: 2026-09-01T22-59

Formatting was applied with three scoped invocations, one per changed path, rather than repository-wide,
so a rewrite of an unrelated file cannot widen the change set that AC-14 pins.

Because a formatter rewrites tracked source and still exits 0 after rewriting, the exit code alone cannot
distinguish a clean run from a repairing one, and a `git status --porcelain` span cannot distinguish them
either at this point in the plan: the three files are already modified relative to `HEAD` and remain so
whether or not CSharpier rewrites them, because `[P5-T1]` is the first task that commits them. The
required observation is therefore the SHA-256 of each file captured immediately before the three
invocations and again immediately after.

Commands, in order:

```
dotnet tool run csharpier format QuickFiler/Controllers/QfcFormKeyHandler.cs
dotnet tool run csharpier format QuickFiler/Viewers/QfcFormViewer.cs
dotnet tool run csharpier format QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs
```

EXIT_CODE: 0 for all three invocations, on both passes recorded below.

## Pass 1 — the repairing pass

Console output, one line per invocation:

```
Formatted 1 files in 650ms.
Formatted 1 files in 674ms.
Formatted 1 files in 672ms.
```

### The six hashes

| File | SHA-256 before | SHA-256 after | Changed? |
|---|---|---|---|
| `QuickFiler/Controllers/QfcFormKeyHandler.cs` | `E8A54E614F3AA566950824DFE32E5084A29C8B64F041335B84E70D958B23C4B9` | `E8A54E614F3AA566950824DFE32E5084A29C8B64F041335B84E70D958B23C4B9` | **NO** |
| `QuickFiler/Viewers/QfcFormViewer.cs` | `AE2890B36784A75697B0831EB378342D75AF714546A32DABB9ABCD2EE75DBBF1` | `AE2890B36784A75697B0831EB378342D75AF714546A32DABB9ABCD2EE75DBBF1` | **NO** |
| `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` | `9593889813AB1696530BF8915A0D8A4D29ECA3A42F829E98041086B5D966AD3E` | `A8C6378B412578EAC527B667603EAC1D14810D53314F7188C526A8D4BA6B8A98` | **YES** |

The two production files were already formatter-clean. The test file was rewritten: CSharpier broke four
long FluentAssertions because-string arguments onto their own lines. The rewrite is presentational only;
no identifier, no assertion, no expected value and no because-string text changed.

## Phase restart, as the Phase 4 preamble requires

The Phase 4 preamble states that the phase restarts from `[P4-T1]` if any stage fails or rewrites a
tracked file. Pass 1 rewrote a tracked file, so Phase 4 was restarted from `[P4-T1]` and the three scoped
invocations were run again before any later stage was attempted.

## Pass 2 — the confirming pass

Console output, one line per invocation:

```
Formatted 1 files in 526ms.
Formatted 1 files in 524ms.
Formatted 1 files in 537ms.
```

### The three post-pass-2 hashes

| File | SHA-256 after pass 2 | Changed by pass 2? |
|---|---|---|
| `QuickFiler/Controllers/QfcFormKeyHandler.cs` | `E8A54E614F3AA566950824DFE32E5084A29C8B64F041335B84E70D958B23C4B9` | **NO** |
| `QuickFiler/Viewers/QfcFormViewer.cs` | `AE2890B36784A75697B0831EB378342D75AF714546A32DABB9ABCD2EE75DBBF1` | **NO** |
| `QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs` | `A8C6378B412578EAC527B667603EAC1D14810D53314F7188C526A8D4BA6B8A98` | **NO** |

All three hashes are identical to the pass-1 after-state, so the format stage has reached a fixpoint and
no further restart is required. The remaining Phase 4 stages ran against this state.

Note that `Formatted 1 files` is printed on both a repairing and a clean invocation, which is exactly why
the exit code and the console line are insufficient and the hash comparison is the load-bearing
observation.

## Structural counts re-verified after the rewrite

The rewrite reflowed the test file, so the `[P2-T1]` structural readings were re-taken to confirm they
survived:

| Reading | Value | Required |
|---|---|---|
| `[TestMethod]` attributes in the test file | 11 | exactly 11 |
| VC-1 matches in the test file | 0 | 0 |
| `Keys.Control` matches in the test file | 4 | at least 2 |
| `Move Options` matches in the test file | 2 | at least 1 |
| `Filters menu` matches in the test file | 0 | 0 |
| VC-2 matches in the viewer | 2 | exactly 2 |
| `ClaimsAltChord` matches in the viewer | 1 | exactly 1 |

Every reading is unchanged from its pre-format value.

Output Summary: All three scoped CSharpier invocations exited 0 on both passes. Pass 1 rewrote
`QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`, whose SHA-256 changed from `9593...AD3E` to
`A8C6...8A98`, and left the two production files byte-identical. Because a tracked file was rewritten,
Phase 4 was restarted from `[P4-T1]`; pass 2 changed no hash, so the format stage is a fixpoint and the
later stages ran against a stable tree. All seven structural readings that the rewrite could have
disturbed were re-taken and are unchanged.
