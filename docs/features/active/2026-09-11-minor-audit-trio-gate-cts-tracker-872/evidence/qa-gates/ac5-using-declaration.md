# AC5 — Using Declaration In RebuildAsync

Timestamp: 2026-09-13T15-43
Task: [P2-T12]

Verdict: PASS

Command: git diff -U0 $b -- UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs
Command: pwsh -Command '$p = "UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs"; "Using: " + (@(Select-String -Path $p -Pattern "using var tokenSource = new CancellationTokenSource();" -SimpleMatch -CaseSensitive) | ForEach-Object { $_.LineNumber }); "Report: " + (@(Select-String -Path $p -Pattern "progress.Report(100);" -SimpleMatch -CaseSensitive) | ForEach-Object { $_.LineNumber }); "Attribute: " + (@(Select-String -Path $p -Pattern "[ExcludeFromCodeCoverage]" -SimpleMatch -CaseSensitive).Count); "ExplicitDispose: " + @(Select-String -Path $p -Pattern "tokenSource.Dispose()" -SimpleMatch -CaseSensitive).Count'
EXIT_CODE: 0

Using: 228
Report: 240
Attribute: 4
ExplicitDispose: 0

## Anchored Diff — Exactly One Line Added And One Removed

The diff is anchored to the base commit `430e2a11db0fa7069d02d42e18df46d21f7db7b5`. It contains a
single hunk touching a single line:

```
@@ -228 +228 @@ namespace UtilitiesCS
-            var tokenSource = new CancellationTokenSource();
+            using var tokenSource = new CancellationTokenSource();
```

One line added, one line removed, nothing else in the file changed. The added line is the using
declaration whose text the plan quotes in its literals section.

## Release Point Ordering

The rebuild method's boundaries, read from the file:

| Element | Line |
|---|---|
| `[ExcludeFromCodeCoverage]` attribute on the method | 221 |
| `public async Task RebuildAsync(IApplicationGlobals appGlobals)` | 222 |
| Method body opening brace | 223 |
| `using var tokenSource = new CancellationTokenSource();` | 228 |
| `var token = tokenSource.Token;` | 229 |
| `await Task.Factory.StartNew(` with `token` passed at line 235 | 232–238 |
| `progress.Report(100);` | 240 |
| Method body closing brace | 241 |

Both recorded line numbers fall inside the method body: 228 and 240 both lie between 223 and 241. A
using declaration releases at the end of its enclosing scope, so the release point is the closing brace
at line 241. That is strictly after the last use of the token, which is the token argument at line 235,
and strictly after the last use of the tracker that holds the source, which is the progress report at
line 240. The ordering AC5 requires therefore holds.

A using declaration lowers to a try and finally with no catch, so it releases on the completing path
and on the faulting path alike and does not swallow any exception raised by the awaited long-running
task.

## The Discriminating Check

`ExplicitDispose:` is 0: the file contains no occurrence of the literal `tokenSource.Dispose()`. That is
the check that can fail. An explicit release inserted ahead of the progress report at line 240 would
release the source while the root progress viewer is still open and would silently make its cancel
button inert, and the structural ordering check alone would not detect it because the using declaration
would still be present.

## Coverage Attribute Count Unchanged

`Attribute:` is 4, matching the `CoverageAttributeCount: 4` that P0-T13 pinned. No coverage attribute
was added or removed by this change.

Per D11 no coverage figure is asserted for this file. The rebuild method carries the
`[ExcludeFromCodeCoverage]` attribute, and an excluded member emits no method element in the Cobertura
report at all; it is absent rather than reported at zero, so no per-file coverage figure could
discriminate whether this change landed. AC5 is verified structurally, by the anchored diff above and
by the two rebuild gates in P2-T3 and P2-T4, and it carries no test obligation because the method
installs a `WindowsFormsSynchronizationContext` and starts a long-running task and is not unit-testable
without a host.

## Derivation Integrity

The command printed four values and no error. `Using:` and `Report:` are non-empty line numbers rather
than blanks, and the diff independently confirms that line 228 carries the using declaration, so the
`Select-String` invocations bound their parameters correctly and no value is a silent zero produced by
a mangled argument.
