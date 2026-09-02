# P2-T4 — The deliberately unchanged site and the untouched file

Timestamp: 2026-09-01T19-58
Command: `(Get-Content -LiteralPath 'QuickFiler/Controllers/QfcItemController.Initialization.cs')[255].Trim()`, `git diff --numstat 988d35a8f8eb7436cc46a9f6424db917ed93807a -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, and `git status --porcelain -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
EXIT_CODE: 0

## Base-ref substitution

The plan's stated commands name `2b85134b42872e405602e6064e02dc9cda6c319b`. That SHA is superseded and is a stale ancestor rather than the current merge base, so `988d35a8f8eb7436cc46a9f6424db917ed93807a` was used instead. Rationale and supporting measurement: `evidence/baseline/p0-t7-base-ref.md`.

## Result 1 — line 256 still names the unguarded member

    (Get-Content ...)[255].Trim()  →  await InitializeWebViewAsync();

Line 256 is unchanged and names `InitializeWebViewAsync`, **not** `InitializeWebViewGuardedAsync`. This is a deliberate exclusion rather than an omission. The site sits inside `public async Task InitializeAsync()` (declared at line 202), so it is already observed: its fault propagates into the enclosing method's returned task. Routing it through the guard would contain that fault and thereby swallow the exception that the pre-existing test `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` asserts, converting a passing pin into a failure. AC8 exists to hold this line in place and AC9 holds the test that depends on it.

## Result 2 — ViewerSetup.cs has zero changed lines

    git diff --numstat <base> -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
    (no output)

    git status --porcelain -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
    (no output)

Both invocations print nothing at all, so the file is unmodified in the committed history relative to the base and is also clean in the working tree and index. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` receives zero changed lines from this delivery run, as AC8 requires — including its `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` attribute at line 47 and the whole body of `InitializeWebViewAsync` at line 48.

This is also what makes the new production partial mandatory rather than stylistic: `ViewerSetup.cs` measures 499 lines against the 500-line ceiling (re-measured in P0-T8), so it has one line of headroom and cannot host the two new members.

## The empty-output gate is discriminating

An acceptance condition satisfied by empty output is worth nothing unless the same command produces non-empty output in the failing case. The identical `git diff --numstat` form was therefore run against a file this delivery run *does* change, as a control:

    git diff --numstat <base> -- QuickFiler/Controllers/QfcItemController.Initialization.cs
    3	3	QuickFiler/Controllers/QfcItemController.Initialization.cs

The control returns a populated row. The command shape, the base ref, and the working directory are identical in both invocations; only the pathspec differs. The empty result for `ViewerSetup.cs` is therefore a genuine observation that the file is unchanged, not an artifact of a malformed command, a wrong ref, or a pathspec that matches nothing.

The control row additionally previews the P2-T5 gate: three added and three deleted lines in `Initialization.cs`, which is the net-zero-line shape the three call-site substitutions were required to produce.

## Both spans are required

The `--numstat` invocation compares committed history against the base ref and is blind to an uncommitted working-tree edit; `git status --porcelain` sees the working tree and index but goes empty once a change is committed. A claim that a file is untouched needs both, because each alone is silent in a state where the other would report.
