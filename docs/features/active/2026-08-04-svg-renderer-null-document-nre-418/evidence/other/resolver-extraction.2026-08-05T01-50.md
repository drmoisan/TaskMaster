# R-6 Resolver Extraction Evidence — Remediation Cycle 1

- Task: `[P1-T4]` (records `[P1-T1]`, `[P1-T2]`, `[P1-T3]`)
- Issue: #418
- Branch: `bug/svg-renderer-null-document-nre-418` (base commit `ea106111`)
- Evidence series: `2026-08-05T01-50`

Timestamp: 2026-08-05T01-38 (UTC)

Command (line counts, authoritative `awk` form):
`for f in ...; do echo "$f = $(awk 'END{print NR}' $f)"; done`

EXIT_CODE: 0

## Before and after line counts

| File | Before (`[P0-T5]`) | After `[P1-T3]` | Delta |
|---|---|---|---|
| `SVGControl/SvgRenderer.cs` | **497** | **362** | **-135** |
| `SVGControl/SvgAssemblyProbe.cs` | 67 | **91** | +24 |
| `SVGControl/SvgAssemblyResolver.cs` | did not exist | **142** | new |
| Sum | 564 | 595 | +31 |

`SVGControl/SvgRenderer.cs` is at **362 lines**, satisfying the `[P1-T3]` acceptance clause "at most
400 lines" with 138 lines of headroom against the hard 500-line limit. The +31 net increase across the
three files is the cost of a second file's `#nullable enable`, six using directives, namespace and class
scaffolding, one class doc comment, and the new `Install()` member.

`git diff --stat -- SVGControl/` after `[P1-T1]` through `[P1-T3]`:

```
 SVGControl/SVGControl.csproj   |   1 +
 SVGControl/SvgAssemblyProbe.cs |  30 ++++++++-
 SVGControl/SvgRenderer.cs      | 139 +----------------------------------------
 3 files changed, 30 insertions(+), 140 deletions(-)
```

(`SVGControl/SvgAssemblyResolver.cs` is untracked at this point and therefore absent from `--stat`.)

## The move is behavior-preserving

`[P1-T3]` moved text only. No control-flow construct, no string literal, no comment, and no strategy
ordering was altered. The **only three permitted deltas** are enumerated below, and no other difference
exists between the moved bodies and their pre-move form:

1. **Indentation and line wrapping applied by CSharpier.** The moved members sit at the same nesting
   depth (class member inside a namespace), so indentation is unchanged in fact; CSharpier re-wrapped
   two call sites whose lines grew past the column limit when the type qualifier was added — the
   `byName != null && ...PublicKeyTokensEqual(...)` condition and the two `Trace.TraceWarning`
   interpolated-string arguments.
2. **`SvgAssemblyProbe.PublicKeyTokensEqual` qualification** at the three call sites inside
   `ResolveByNameAndKey` (`[P1-T1]`). The member relocated to `SvgAssemblyProbe`, so the calls are now
   cross-type within the same assembly.
3. **`SvgRenderer.DescribeFailure` qualification** at the two call sites inside the moved body
   (`[P1-T3]`). The member stayed on `SvgRenderer` and was widened from `private static` to
   `internal static` by `[P1-T2]`.

Verbatim-carriage checks performed:

| Check | Result |
|---|---|
| `ResolveByNameAndKey` declared exactly once in the repository | `SVGControl/SvgAssemblyResolver.cs:39`, `private static System.Reflection.Assembly? ResolveByNameAndKey(object sender, ResolveEventArgs args)` — still `private static` |
| `_resolverInstalled` declared exactly once | `SVGControl/SvgAssemblyResolver.cs:24` |
| `[ThreadStatic] private static HashSet<string>? _resolving` | present in `SvgAssemblyResolver.cs`, carried verbatim |
| Both `SvgRenderer load '` messages | present, 2 occurrences, at `SvgAssemblyResolver.cs:97` and `:129` |
| `typeof(SvgRenderer).Assembly` | carried verbatim in strategy 3; still resolves to the same assembly (`SvgRenderer` and `SvgAssemblyResolver` are both in `SVGControl`) |
| Re-entrance guard | `_resolving.Add` / outer `try` / `finally { _resolving.Remove(...) }` all intact and in the same relation to strategies 2 and 3 |
| Terminal `return null;` | intact |
| Strategy order (1 already-loaded scan, 2 `Assembly.Load`, 3 `LoadFrom` probe) | unchanged |
| Both existing inner `catch (Exception ex)` handlers using `Trace.TraceWarning` | unchanged |
| Orphaned using directives removed from `SvgRenderer.cs` | exactly two: `System.Collections.Generic` and `System.Threading`, each confirmed unreferenced by grep and by a clean analyzer build. `System.Threading.Tasks` was **not** removed — it was already unreferenced before this cycle and is outside `[P1-T3]`'s stated scope. |

## The install trigger is unchanged

`SvgRenderer`'s static constructor is **retained**. Its body is now the single statement
`SvgAssemblyResolver.Install();`:

```csharp
        static SvgRenderer()
        {
            SvgAssemblyResolver.Install();
        }
```

The `Interlocked.Exchange(ref _resolverInstalled, 1) == 0` guard and the
`AppDomain.CurrentDomain.AssemblyResolve += ResolveByNameAndKey;` subscription moved verbatim into
`SvgAssemblyResolver.Install()`. Touching `SvgRenderer` therefore still installs the handler exactly
once per AppDomain, which is the observable behavior AC-8 depends on. Moving the static constructor
wholesale would have silently disabled the resolver — the code would still compile and the tests would
still pass while the resolver stopped installing — which is why the constructor was retained rather than
relocated (plan Design Decision 4).

## Coverage exception travels with the member

The ratified exception recorded in `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md` §
"`ResolveByNameAndKey` named exception" is:

```
COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgRenderer.ResolveByNameAndKey
```

Because `[P1-T3]` relocated that member without changing its accessibility (`private static`), its
implementation, or its invocation mechanism (the CLR on a failed assembly bind), the exception travels
with the member. From this cycle forward it is recorded as:

```
COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgAssemblyResolver.ResolveByNameAndKey
```

The premise of the original ratification is preserved verbatim: the member is still `private static` and
is still invoked only by the CLR on a failed assembly bind, so its strategy-3 `Assembly.LoadFrom` branch
still cannot be driven from a unit test without staging a real mismatched-key assembly on disk, which
`.claude/rules/general-unit-test.md` UT4 prohibits with zero approved exceptions.

## `SVGControl.SvgAssemblyResolver` is a relocation, not a new module

The `>= 90%` newly-added-module threshold in `CLAUDE.md` § General Unit Test Policy does **not** attach
to `SVGControl.SvgAssemblyResolver`. Every member it contains existed at `ea106111` with a measured
figure:

| Member | Existed at `ea106111` | Measured figure at `ea106111` (`[P0-T9]`) |
|---|---|---|
| `_resolverInstalled` (field) | yes, on `SvgRenderer` | n/a (field) |
| `_resolving` (field) | yes, on `SvgRenderer` | n/a (field) |
| `ResolveByNameAndKey` | yes, on `SvgRenderer` | 47/69 = 68.1159% line-rate, ratified exception applies |
| `Install()` | **no — the sole genuinely new member this cycle adds** | n/a |

`Install()` is the only member subject to the `>= 90%` gate. It is exercised by every test that touches
`SvgRenderer`, so its `line-rate` is expected at 100%; its `Interlocked.Exchange(...) == 0` false arm is
not driven because the handler installs once per AppDomain, so its `branch-rate` is expected at 50%. Per
`evidence/qa-gates/coverage-delta.2026-08-04T14-36.md` § Metric definition, the `>= 90%` gate is
assessed on `line-rate`; `branch-rate` is recorded for information only and member-level branch coverage
is not gated. `[P2-T7]` measures and records the actual figures.

## Scope Lock compliance

Files touched by `[P1-T1]` through `[P1-T3]`: `SVGControl/SvgRenderer.cs`,
`SVGControl/SvgAssemblyProbe.cs`, `SVGControl/SvgAssemblyResolver.cs` (new), and
`SVGControl/SVGControl.csproj` (one `<Compile Include="SvgAssemblyResolver.cs" />` item only, placed
alphabetically after `SvgAssemblyProbe.cs` in the existing `<ItemGroup>`). All four are inside the plan's
Scope Lock. No other file was modified.

Note recorded for accuracy: `[P1-T1]`'s acceptance clause states `PublicKeyTokensEqual` "appears exactly
once in the repository". Within the `SVGControl` assembly that is true — the sole declaration is
`SVGControl/SvgAssemblyProbe.cs:71`, `internal static bool PublicKeyTokensEqual(byte[]? a, byte[]? b)`.
A **pre-existing, unrelated** `private static bool PublicKeyTokensEqual(byte[] a, byte[] b)` also exists
at `UtilitiesCS.Test/TestAssemblyInitializer.cs:89`. It predates this cycle, belongs to a different
assembly, is not referenced by `SVGControl`, and is out of the Scope Lock (the `## Do Not Do` list
forbids `UtilitiesCS` edits). It was not touched.

## Verification

| Gate | Command | Result |
|---|---|---|
| Formatting | `dotnet tool run csharpier check SVGControl/SvgRenderer.cs SVGControl/SvgAssemblyProbe.cs SVGControl/SvgAssemblyResolver.cs` | `EXIT_CODE: 0`, 3 files checked, 0 need formatting (the new file required one `csharpier format` pass for line endings, then checked clean) |
| Analyzer build after `[P1-T1]` | `Invoke-VSBuild.ps1 ... -EnableNETAnalyzers -EnforceCodeStyleInBuild` | `EXIT_CODE: 0`, 0 errors, 6 warnings, 34 `csc.exe` invocations |
| Analyzer build after `[P1-T2]` | same | `EXIT_CODE: 0`, 0 errors, 6 warnings |
| Analyzer build after `[P1-T3]` | same | `EXIT_CODE: 0`, 0 errors, 6 warnings, elapsed 10.78 s |

Warning-count note: the six warnings are the five code-less `System.Reactive` `packages.config`
warnings plus the one pre-existing `CS2002` duplicate `<Compile>` in `UtilitiesCS.Test`. `[P0-T7]`
recorded five because that run was incrementally vacuous (0 `CoreCompile` targets) and `CS2002` is
`CoreCompile`-gated. The pre-existing baseline established in
`evidence/remediation-baseline/analyzer-build.2026-08-05T01-50.md` § "Comparison basis for `[P2-T4]`" is
the union of both sets, i.e. six. **Zero new diagnostics** were introduced: `SVGControl` and
`SVGControl.Test` each emit zero warnings and zero errors.

## Output Summary

R-6 delivered as a pure move. `SVGControl/SvgRenderer.cs` 497 -> **362** lines,
`SVGControl/SvgAssemblyProbe.cs` 67 -> **91**, new `SVGControl/SvgAssemblyResolver.cs` at **142**. Only
the three permitted deltas occurred (CSharpier wrapping, `SvgAssemblyProbe.PublicKeyTokensEqual`
qualification, `SvgRenderer.DescribeFailure` qualification). `SvgRenderer`'s static constructor is
retained and calls `SvgAssemblyResolver.Install()`, so the resolver still installs once per AppDomain.
The ratified coverage exception is re-recorded as
`COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgAssemblyResolver.ResolveByNameAndKey`, and
`SVGControl.SvgAssemblyResolver` is a relocation rather than a new module, so the `>= 90%` new-module
threshold does not attach to it; only `Install()` is a genuinely new member. Analyzer build
`EXIT_CODE: 0` with zero new diagnostics.
