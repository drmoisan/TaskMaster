# P2-T2 — CSharpier Check, Whole Tree, Read-Only

Timestamp: 2026-09-01T14-24

Command: `dotnet tool run csharpier check .` (run from the checkout root, with `PATH` and
`DOTNET_ROOT` pointed at the repository-local `.dotnet-sdk` directory)

EXIT_CODE: 0

Output Summary:

The command produced exactly one output line, recorded verbatim:

```
Checked 1566 files in 5992ms.
```

## Unfiltered path list

The command named no file paths. The complete unfiltered list of paths this run named is empty.

## Derived fields

SourceScopedDrift: none

Derived by the same segment rule P0-T10 uses: remove from the unfiltered list every path with a path
segment equal to `packages`, `.dotnet-sdk`, `bin`, or `obj`, in either separator spelling. The
unfiltered list is empty, so the filtered list is empty and the field is recorded as the literal
`none`.

OwnedPathInThisRunDrift: no

`SourceScopedDrift:` in this artifact contains neither `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`
nor `QuickFiler.Test\Controllers\WpfUiDispatcherTests.cs`, because it is empty.

OwnedPathInBaselineDrift: no

The `SourceScopedDrift:` field of
`docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/baseline/p0-t10-csharpier-check.md`
is the literal `none` and contains neither of those two strings.

ComparedDrift: none

BaselineComparedDrift: none

Both are this run's and the baseline's `SourceScopedDrift:` with the two owned-path spellings
removed. Both were already empty, so both remain empty and are recorded as the literal `none`.

## Acceptance conditions

1. `ComparedDrift:` is identical, as a set, to `BaselineComparedDrift:`. Both are the empty set.
2. The owned path appears nowhere in this run's unfiltered output. This was checked in both separator
   spellings, `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` and
   `QuickFiler.Test\Controllers\WpfUiDispatcherTests.cs`, because the separator CSharpier prints was
   not observed before authoring, and as a qualified path rather than a bare filename because
   `UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs` carries the same filename and is out of scope.
   The unfiltered output is empty, so neither spelling appears.
3. `OwnedPathInThisRunDrift:` and `OwnedPathInBaselineDrift:` are both recorded above.
4. Raw exit codes: this run's `EXIT_CODE:` is 0; the baseline's raw `EXIT_CODE:`, recorded in
   `p0-t10-csharpier-check.md`, is also 0.

The two runs additionally agree on the checked-file total, 1566 files on both sides, even though the
baseline measured a tree with no `packages/`, `bin/`, or `obj/` and this run measured a tree carrying
all three. That confirms the four-directory filter is inert on both sides, which is the expectation
P0-T10 records with its three cited prior measurements. The filter is applied anyway, by the identical
segment rule on both sides, so this comparison stays valid without depending on that expectation.

The comparison is against the recorded baseline rather than a demand for an empty list outright. That
is what makes this gate detect drift this issue introduced without inheriting or waiving any
pre-existing drift. The separate demand that the list be empty belongs to AC-7 and is made in P2-T16;
`SourceScopedDrift:` is the literal `none` here, so that later demand is satisfiable.
