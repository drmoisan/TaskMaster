# R-3 Resolver Containment Evidence — Remediation Cycle 1

- Task: `[P1-T13]` (records `[P1-T10]`, `[P1-T11]`, `[P1-T12]`)
- Issue: #418 — source finding: code review CR-2 (Medium)
- Evidence series: `2026-08-05T01-50`

Timestamp: 2026-08-05T01-53 (UTC)

Command: `sed -n` inspection of `SVGControl/SvgAssemblyResolver.cs` and
`SVGControl/SvgAssemblyProbe.cs`, plus the analyzer build and a targeted vstest run.

EXIT_CODE: 0

## Part 1 — the exact catch clause added by `[P1-T10]`

Added to `SVGControl/SvgAssemblyResolver.cs`, positioned between the outer `try` block that encloses
strategies 2 and 3 and that block's existing `finally`:

```csharp
            // Containment boundary: nothing may escape an AssemblyResolve handler, or a recoverable
            // bind failure becomes a hard failure at whatever triggered the bind. Trace, not log4net,
            // for the re-entrancy reason given above.
            catch (Exception ex)
            {
                Trace.TraceWarning(
                    $"SvgRenderer resolve '{requested.Name}': {SvgRenderer.DescribeFailure(ex)}"
                );
            }
            finally
            {
                _resolving.Remove(requested.Name);
            }
```

Structural verification (`grep -n "catch (Exception ex)\|finally"`):

| Line | Construct | Scope |
|---|---|---|
| 94 | `catch (Exception ex)` | pre-existing inner handler around strategy 2's `Assembly.Load` |
| 126 | `catch (Exception ex)` | pre-existing inner handler around each strategy-3 `Assembly.LoadFrom` |
| **137** | **`catch (Exception ex)`** | **new — the outer containment boundary** |
| 143 | `finally` | pre-existing, `_resolving.Remove(requested.Name)` |

**The outer `try` now has exactly one `catch` clause and exactly one `finally` clause.** Per `[P1-T10]`'s
prohibitions, none of the following changed: the `_resolving.Add`/`Remove` guard, the strategy order,
either existing inner catch, or the method's terminal `return null;`. `Trace.TraceWarning` is used and
`log4net` is not, for the re-entrancy reason the pre-existing in-code comment states (a `log4net` call
inside an `AssemblyResolve` handler can itself trigger a re-entrant assembly load, so the diagnostic must
not depend on `log4net` being loadable) — plan Design Decision 12.

### Raising sources now contained

Every source CR-2 identifies sits inside the outer `try` and is therefore now caught:

| Raising source | Location relative to the guard | Typical exception | Contained |
|---|---|---|---|
| `System.Reflection.Assembly.Load(name)` (strategy 2) | inside outer try, also inside inner catch at 94 | `FileNotFoundException`, `FileLoadException`, `BadImageFormatException` | yes (inner, then outer) |
| `System.Reflection.Assembly.LoadFrom(path)` (strategy 3) | inside outer try, also inside inner catch at 126 | `FileNotFoundException`, `BadImageFormatException` | yes (inner, then outer) |
| `Path.Combine(directory, requested.Name + ".dll")` | inside outer try, **outside** any inner catch | `ArgumentException` for invalid path characters | **yes — newly contained** |
| `self.Location` | inside outer try, outside any inner catch | `NotSupportedException` for a dynamic assembly | **yes — newly contained** |
| `self.CodeBase` | inside outer try, outside any inner catch | `NotSupportedException` for a dynamic assembly | **yes — newly contained** |
| `File.Exists(path)` | inside outer try, outside any inner catch | (does not raise, returns false) | yes |
| `SvgAssemblyProbe.GetProbeDirectories(...)` | inside outer try | documented never to raise | yes |

The three sources marked "newly contained" are exactly the exposure CR-2 recorded as narrower than the
pre-`ea106111` baseline, which wrapped the same region in `catch { }`.

## Part 2 — the exact filter added by `[P1-T11]`

In `SVGControl/SvgAssemblyProbe.cs`, inside `GetProbeDirectories`, the third entry of the `candidates`
initializer changed from the bare `baseDirectory,` to:

```csharp
                baseDirectory != null && baseDirectory.IndexOfAny(Path.GetInvalidPathChars()) < 0
                    ? baseDirectory
                    : null,
```

(The line wrapping shown is CSharpier's.) Nothing else in the method changed: the candidate order, the
case-insensitive de-duplication with first-occurrence-wins, and the empty-location skip all behave exactly
as before for valid inputs. All three candidates are now validated identically against
`Path.GetInvalidPathChars()`:

| Candidate | Validation |
|---|---|
| 1. `assemblyLocation` | non-null, non-empty after `Trim()`, and `IndexOfAny(Path.GetInvalidPathChars()) < 0` (pre-existing) |
| 2. `assemblyCodeBase` | via `TryGetDirectoryFromCodeBase`, which applies `IndexOfAny(Path.GetInvalidPathChars()) >= 0` to `parsed.LocalPath` (pre-existing) |
| 3. `baseDirectory` | non-null and `IndexOfAny(Path.GetInvalidPathChars()) < 0` (**added by `[P1-T11]`**) |

### The documented contract, and the third candidate's consistency with it

`SVGControl/SvgAssemblyProbe.cs:16-17` states the type's contract:

> "Never raises, so it is safe inside an `AssemblyResolve` handler."

The unfiltered third candidate was inconsistent with that sentence in effect if not in letter: the helper
itself did not raise, but it returned a candidate that made the caller's `Path.Combine` raise one line
later, inside the very handler the sentence is about. **The third candidate is now consistent with the
stated contract**: an unusable `baseDirectory` is dropped and produces a skipped candidate rather than a
downstream exception, which is the same treatment the other two candidates already received.

## Part 3 — verification test added by `[P1-T12]`

`GetProbeDirectories_WithAnInvalidCharacterInTheBaseDirectory_DropsThatCandidateWithoutThrowing` in
`SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs`, written in the style of
`TryGetDirectoryFromCodeBase_WithANonUriString_ReturnsNullWithoutThrowing`:

- Base directory constructed as `@"C:\probe\three" + Path.GetInvalidPathChars()[0] + "bad"`, so the test
  does not depend on which character occupies a given position in the platform's list.
- Location `@"C:\probe\one\SVGControl.dll"`, code base `null`.
- Asserts with FluentAssertions that the call does not throw, that the returned list has exactly one
  entry, and that the entry ends with `one`.
- Uses no temporary file, no `Assembly.LoadFrom`, and **no `?` and no `!` token** (verified by grep over
  the method body: zero matches), per Design Decision 9.

Result: **Passed.** Targeted run
(`/TestCaseFilter:FullyQualifiedName~GetProbeDirectories`, `EXIT_CODE: 0`, 5/5 passed):

```
  Passed GetProbeDirectories_WithAllThreeInputsPopulated_PreservesTheStatedOrder
  Passed GetProbeDirectories_WithAnEmptyAssemblyLocation_SkipsThatCandidate
  Passed GetProbeDirectories_WithDirectoriesDifferingOnlyByCase_DeduplicatesThem
  Passed GetProbeDirectories_WithAnInvalidCharacterInTheBaseDirectory_DropsThatCandidateWithoutThrowing
  Passed GetProbeDirectories_WithAllInputsNull_ReturnsAnEmptyListWithoutThrowing
```

`SvgAssemblyProbeDirectoryTests.cs` is at **227 lines**, well under the 500-line limit. All nine
pre-existing `SvgAssemblyProbeDirectoryTests` pass unchanged (`[P1-T11]` acceptance).

### Disclosed environmental note on isolated single-assembly runs

A single-assembly `vstest.console.exe SVGControl.Test\bin\Debug\SVGControl.Test.dll` invocation fails 5
of 65 tests, all with `FileNotFoundException` for `ExCSS, Version=4.3.2.0`:
`SetDefaultImage_OnASelector_LeavesTheRendererDocumentNonNull`,
`GetSvgDocument_WithTheBuiltInDefaultImage_ReturnsADocument`,
`TryGetSvgDocument_WithEmptyBytes_ReturnsFalseAndCapturesAnXmlException`,
`GetSvgDocumentOrThrow_WithEmptyBytes_ThrowsWithTheXmlExceptionInner`, and
`GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument`.

Cause, established by inspection: `ExCSS.dll` is present in the output directory of **all eight** other
test projects and **absent** from `SVGControl.Test/bin/Debug/` (ExCSS is a transitive dependency of `Svg`
and legacy non-SDK projects do not flow transitive copy-local). In a single-assembly run no probing
directory contains it, so the ExCSS bind fails and the tests that assert a *successful* parse fail. The
`NullReferenceException` failure mode does **not** return — the AC-3 degrade-and-log path handles it
correctly, which is visible in the captured `Debug Trace` output.

**This is not a regression from `[P1-T10]`/`[P1-T11]`, and not attributable to `GetProbeDirectories`.**
Proof: adding one sibling assembly to the same command
(`vstest.console.exe VBFunctions.Test\bin\Debug\VBFunctions.Test.dll SVGControl.Test\bin\Debug\SVGControl.Test.dll`)
returns `EXIT_CODE: 0` with **66/66 passed**, including all nine `SvgAssemblyProbeDirectoryTests` and all
five previously failing tests. The mandated wrapper (`Invoke-MSTestWithCoverage.ps1 -SearchRoot .`) always
runs all nine assemblies together, which is why `[P0-T9]` measured 6140/6140 passing at this HEAD and why
`[P1-T19]` and `[P2-T6]` are the authoritative suite verifications.

## Known residual — the pre-guard region is NOT covered by the new catch

Recorded explicitly rather than silently dropped, per `[P1-T13]`.

The following region of `ResolveByNameAndKey` remains **outside** the new containment catch, because it
executes before the `_resolving.Add` guard and the outer `try` begin:

- `var requested = new System.Reflection.AssemblyName(args.Name);` — can raise
  `FileLoadException`/`ArgumentException` for a malformed assembly-name string.
- `loaded.GetName()` inside the already-loaded scan (strategy 1) — can raise for an assembly whose
  identity cannot be produced.

(`SVGControl/SvgRenderer.cs:52-72` pre-move; now `SVGControl/SvgAssemblyResolver.cs:43-63`.)

Reason, per plan Design Decision 11: CR-2 does not name this region, and widening the guard to the whole
method would require either introducing a wrapper method or renaming `ResolveByNameAndKey`. Both would
invalidate two artifacts that name that member exactly — the AC-8 evidence note in `issue.md` and the
ratified `COVERAGE_MEMBER_UNREACHABLE` exception — for a region no finding identified. The acceptance
criterion for `[P1-T10]` is therefore stated as the specific raising sources listed in Part 1 above, not
as an absolute "no exception escapes".

Residual risk assessment: low. Both calls operate on a string the CLR itself constructed
(`ResolveEventArgs.Name`) and on assemblies the CLR has already successfully loaded, so neither input is
attacker- or caller-supplied in any reachable path. Closing this residual is a candidate for a follow-up
entry, not this cycle's work.

## Verification

| Gate | Command | Result |
|---|---|---|
| Formatting | `dotnet tool run csharpier check .` | `EXIT_CODE: 0`, `Checked 1467 files`, 0 need formatting |
| Analyzer build after `[P1-T10]` | `Invoke-VSBuild.ps1 ... -EnableNETAnalyzers -EnforceCodeStyleInBuild` | `EXIT_CODE: 0`, 0 errors |
| Analyzer build after `[P1-T11]` | same | `EXIT_CODE: 0`, 0 errors, 6 warnings (5 System.Reactive + 1 pre-existing `CS2002`) |
| Analyzer build after `[P1-T12]` | same | `EXIT_CODE: 0`, 0 errors, 5 warnings |
| Targeted tests | `vstest.console.exe VBFunctions.Test.dll SVGControl.Test.dll /TestCaseFilter:FullyQualifiedName~GetProbeDirectories` | `EXIT_CODE: 0`, 5/5 passed |

## Output Summary

R-3 delivered in both parts. Part 1: one `catch (Exception ex)` with a `Trace.TraceWarning` body added to
the outer `try` in `SVGControl/SvgAssemblyResolver.cs`, giving that `try` exactly one catch and one
finally, and newly containing `Path.Combine`, `self.Location`, and `self.CodeBase` in addition to the two
`Assembly.Load`/`LoadFrom` sources already covered by inner handlers. Part 2: the
`Path.GetInvalidPathChars()` filter applied to the third `GetProbeDirectories` candidate, making all three
candidates validated identically and bringing the third into line with the type's documented "Never
raises" contract. One new test proves the drop-without-throwing behavior and passes. Known residual: the
pre-guard region (`new AssemblyName(args.Name)` and `loaded.GetName()`) stays outside the new catch, for
the reason in Design Decision 11.
