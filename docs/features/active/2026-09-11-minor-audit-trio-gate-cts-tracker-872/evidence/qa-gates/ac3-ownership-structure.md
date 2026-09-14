# AC3 — Ownership Mechanism Structure

Timestamp: 2026-09-13T15-44
Task: [P2-T13]

Verdict: PASS

Command: pwsh -Command '$p = "UtilitiesCS/Threading/ProgressPackage.cs"; "Declaration: " + @(Select-String -Path $p -Pattern "public class ProgressPackage : IDisposable" -SimpleMatch -CaseSensitive).Count; "Assignments: " + @(Select-String -Path $p -Pattern "_ownsCancelSource = cancelSource is null;" -SimpleMatch -CaseSensitive).Count; Select-String -Path $p -Pattern "_ownsCancelSource" -SimpleMatch -CaseSensitive | ForEach-Object { "Line " + $_.LineNumber + ": " + $_.Line.Trim() }; Select-String -Path $p -Pattern "public CancellationTokenSource? CancelSource" -SimpleMatch -CaseSensitive | ForEach-Object { "PropertyStart: " + $_.LineNumber }; Select-String -Path $p -Pattern "public async Task<ProgressPackage> InitializeAsync(" -SimpleMatch -CaseSensitive | ForEach-Object { "InitializeAsyncStart: " + $_.LineNumber }'
EXIT_CODE: 0

Declaration: 1
Assignments: 2

## Every Occurrence Of The Ownership Field

```
Line 26: _ownsCancelSource = cancelSource is null;
Line 42: _ownsCancelSource = cancelSource is null;
Line 108: private bool _ownsCancelSource;
Line 180: if (_ownsCancelSource)
Line 183: _ownsCancelSource = false;
```

Five occurrences: two assignments at the construction sites, the field declaration, the guard inside
Dispose, and the clear inside Dispose.

## Member Boundaries

| Member | Signature start | Body opening brace | Body closing brace |
|---|---|---|---|
| `InitializeAsync` tracker overload | 17 | 24 | 32 |
| `InitializeAsync` pane overload | 34 | 40 | 47 |
| `CreateAsTupleAsync` static factory | 55 | 67 | 71 |
| `CreateAsTuplePaneAsync` static factory | 79 | 90 | 94 |
| `CancelSource` property declaration block | 97 | 98 | 101 |
| `Dispose` | 178 | 179 | 185 |

Both assignment line numbers fall inside an `InitializeAsync` overload body: 26 lies between 24 and 32,
and 42 lies between 40 and 47. Each overload therefore assigns ownership once, at the construction
site, immediately after the null-coalescing construction of the source on the preceding line.

The assignment form rather than a conditional set is required so that a second `InitializeAsync` call
with an injected source clears a previously claimed ownership. Both sites use the assignment form
`_ownsCancelSource = cancelSource is null;`, and `Assignments:` counts exactly 2.

## The Discriminating Check — The Setter Exclusion

The public `CancelSource` property declaration block spans lines 97 through 101. No enumerated
ownership-field line number falls inside it: the five occurrences are at 26, 42, 108, 180 and 183, and
none lies between 97 and 101. The setter at line 100 reads `set => _cancelSource = value;` and touches
the ownership field not at all.

This is the discriminating check. `SpawnChild` assigns the parent's source through that setter using an
object initializer at line 143, so a setter that claimed ownership would make every child claim its
parent's source and the child's disposal would release a source the parent still holds. A check that
only counted the two assignments would not detect that, because the count would still be 2.

## Type Declaration

`Declaration:` is 1: the file carries exactly one occurrence of `public class ProgressPackage : IDisposable`.
The disposal contract is therefore exposed on the type, which is what makes a holder able to release an
owned source deterministically.

## The Two Static Tuple Factories

Both carry the ownership-transfer XML doc comment that P1-T5 added:

- `CreateAsTupleAsync`: doc comment at lines 49 through 54, summary element stating that the
  cancellation token source in the returned tuple is transferred to the caller and that the caller owns
  its release.
- `CreateAsTuplePaneAsync`: doc comment at lines 73 through 78, same statement.

Neither disposes the package it constructs. `CreateAsTupleAsync`'s body is lines 68 through 70:
construct the package, await `InitializeAsync`, return `package.ToTuple()`. `CreateAsTuplePaneAsync`'s
body is lines 91 through 93 with the same shape. Adding a dispose call in either would release the very
source the factory is contractually returning, which is why neither has one, and no executable
statement in either method was changed by P1-T5.

## AC3 Is A Capability Criterion

AC3 requires the class to record ownership at the construction site and to expose a disposal contract
that releases only an owned source. Both are delivered and verified above. It does not require the
class to guarantee that every constructed source is released, because that is not in the class's power:
on the tuple-factory paths the package is discarded once the tuple is taken, so there is no holder left
to dispose.

That residual is recorded in the Scope Boundary entry naming the six consumer files of research scope
finding SF-1 — the transform and folder-extraction partials of the email data miner, the OlFolder
classifier group, the multiclass engine, the category classifier group, and the Bayesian performance
measurement type — carrying nine call sites. None of those files is in the Write Set and none is edited
by this delivery. The obligation to promote the residual to its own issue belongs to the calling
orchestrator, which owns it; no task in this plan files it.

## Derivation Integrity

The command printed a non-zero count for both `Declaration:` and `Assignments:`, five enumerated field
lines with their text, and three line-number values, with no error written. The enumerated text
corroborates the counts independently, so no value here is a silent zero produced by a mangled
argument.
