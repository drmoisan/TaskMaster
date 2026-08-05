# Code Review — svg-renderer-null-document-nre (Issue #418)

- Review timestamp: 2026-08-04T22-28
- Cycle: 2 (re-audit after remediation cycle 1)
- Base: `origin/main` @ `ce0c91e686bf7e060aaab6f185ee6883269e4fd4`
- Head: `bug/svg-renderer-null-document-nre-418` @ `a62391f719c6d5ecc3d80115916c95d1966ca514`
- Scope: full branch diff — 6 C# source files, 5 C# project/config files, 72 documentation and agent-memory files
- Companion artifacts: `policy-audit.2026-08-04T22-28.md`, `feature-audit.2026-08-04T22-28.md`

## Executive Summary

The production code in this branch is well constructed. The parse-failure boundary is a clean
single-point design: one internal method converts every failure mode into a `false` result plus an
optionally captured exception, three differently-contracted surfaces are layered on top of it, and
both byte-array constructors degrade with a logged cause rather than dereferencing a null. Nullable
annotations are enabled per-file and used meaningfully. The two extracted types have clear, honest
header comments that state *why* they exist, and the `Trace`-not-`log4net` decision inside the
`AssemblyResolve` handler is documented at the point of use with a correct re-entrancy rationale.

All seven actionable findings from cycle 1 are verified resolved by direct measurement, not by
assertion:

| Cycle-1 finding | Status | Verification |
|---|---|---|
| CR-1 Medium — no `<LangVersion>`, `CS8630` under the nullable gate | **Resolved** | `<LangVersion>latest</LangVersion>` at `SVGControl.Test.csproj:17`; reviewer forced a recompile and observed `/nullable:enable /langversion:latest` with zero diagnostics and zero `CS8630` |
| CR-2 Medium — narrowed exception containment in `ResolveByNameAndKey` | **Resolved** | outer `catch (Exception ex)` at `SvgAssemblyResolver.cs:143`; `baseDirectory` filtered at `SvgAssemblyProbe.cs:52-54` |
| CR-3 Low — `SvgRenderer.cs` at 497 of 500 lines | **Resolved** | now 362 lines after the R-6 extraction |
| CR-4 Low — stale header comment (`Svg 3.4.7`, `ExCSS 4.3.1`, vstest claim) | **Resolved** | `SvgAssemblyResolver.cs:17-29` states `Svg 3.4.8`/`ExCSS 4.3.2`, correctly names `devenv.exe` as the non-redirecting host, and cites the research artifact |
| CR-5 Low — 3-argument constructor success branch uncovered (13/17) | **Resolved** | now 17/17 = 100% |
| CR-6 Low — `PublicKeyTokensEqual` at 0/15 = 0% | **Resolved** | relocated to `SvgAssemblyProbe`; now 15/15 line = 100%, 18/18 branch = 100% |
| CR-7 Low — test comment overstated the element-free premise | **Resolved** | `SvgRendererParseContractTests.cs:246` now carries the U-3 hedge matching the production comment |

One new Blocking finding is recorded. It is a test-infrastructure defect, not a production-code
defect, and it predates cycle 1 — the reviewer did not detect it in cycle 1. Six tests in
`SVGControl.Test` produce different outcomes depending on the ordinal position of the assembly on the
`vstest.console.exe` command line, because `ExCSS.dll` is not copied into that project's output
directory. The remaining findings are Low or Info and none requires action before merge.

Positive observations worth recording. The extraction into `SvgAssemblyProbe` was driven by
testability and it worked: the pure decision logic reached 100% line and branch coverage, which
converted AC-8's public-key-token requirement from an inspection claim into a measured one. The
`Func<byte[], SvgDocument?>` seam is the right weight for a single call path and avoids introducing
an interface for one method. And the branch deferred three unrelated defects it discovered to
`docs/features/potential/` rather than widening scope, which is exactly what the bugfix workflow asks
for under a `minor-audit` work mode.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocking | `SVGControl.Test/SVGControl.Test.csproj`, `SVGControl.Test/packages.config` | `SVGControl.Test.csproj:282-284` (the added `Svg` reference); `packages.config:116` | Six tests change outcome with `vstest.console.exe` argument order. The project references `Svg` but never `ExCSS`; `ExCSS` is a transitive dependency of `Svg`, and legacy non-SDK `packages.config` projects do not flow transitive copy-local, so `SVGControl.Test/bin/Debug` contains `Svg.dll` but no `ExCSS.dll` or `Fizzler.dll`. Any test requiring a real SVG parse then fails with `FileNotFoundException` for `ExCSS, Version=4.3.2.0` unless another assembly already supplied ExCSS to the test host. The `app.config` redirect AC-10 corrected to `4.3.2.0` cannot help, because redirection presupposes the file is findable; and the `AssemblyResolve` fallback's strategy 3 probes the directory holding `SVGControl.dll`, which is that same output directory. | Add an explicit `ExCSS` reference mirroring the `Svg` reference this branch already added: `<Reference Include="ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a, processorArchitecture=MSIL"><HintPath>..\packages\ExCSS.4.3.2\lib\net48\ExCSS.dll</HintPath><Private>True</Private></Reference>` plus `<package id="ExCSS" version="4.3.2" targetFramework="net481" />`. Add `Fizzler 1.3.1` on the same pattern for parity with the eight sibling test projects. Verify that `vstest.console.exe SVGControl.Test\bin\Debug\SVGControl.Test.dll` alone returns 75/75. | Violates three explicit policy statements: `.claude/rules/general-unit-test.md` UT1 "Tests must be able to run in any order without impacting each other"; the same file's "Tests must not rely on mutable global state or external configuration that can change between runs"; and `.claude/rules/csharp.md` "Tests must produce identical results in the IDE test runner and in CLI runs so local and CI behavior agree." A developer opening this project in Test Explorer sees six red tests. It also degrades the trustworthiness of the AC-1 regression tests that are the entire purpose of issue #418, and it makes the AC-5 amendment's `XmlException` premise environment-conditional. Zero production-behavior impact, and the fix is one project item. | Three reviewer runs, same binaries, same session: `vstest.console.exe SVGControl.Test.dll` alone → 75 total, 69 passed, 6 failed; `... SVGControl.Test.dll VBFunctions.Test.dll` → 76 total, 70 passed, 6 failed; `... VBFunctions.Test.dll SVGControl.Test.dll` → 76 total, 76 passed, 0 failed. `ls SVGControl.Test/bin/Debug | grep -i excss` returns nothing; the same grep against `UtilitiesCS.Test/bin/Debug` returns `ExCSS.dll`. `grep -in excss SVGControl.Test/SVGControl.Test.csproj SVGControl.Test/packages.config` matches only `app.config`. Executor disclosure at `evidence/other/resolver-containment.2026-08-05T01-50.md:130-152`. |
| Low | `SVGControl/SvgAssemblyResolver.cs` | lines 103, 109, 135, 146 | The extracted resolver still reaches back into `SvgRenderer` for two things: `SvgRenderer.DescribeFailure(ex)` at three call sites, and `typeof(SvgRenderer).Assembly` at line 109. The two types are therefore mutually dependent — `SvgRenderer`'s static constructor calls `SvgAssemblyResolver.Install()`, and the resolver calls back into `SvgRenderer`. This leaves the R-6 separation incomplete: the file's own header says the concern is "assembly binding rather than SVG rendering", yet it cannot compile without the renderer. | Move `DescribeFailure` to `SvgAssemblyProbe` (or a small shared internal helper) and have `SvgRenderer` call it there, and change line 109 to `typeof(SvgAssemblyResolver).Assembly`, which resolves to the identical assembly without the cross-reference. | Completes the separation the extraction set out to achieve and removes a mutual type dependency inside a CLR callback path, where static-initialization order is harder to reason about than usual. No behavior change: `DescribeFailure` is a pure string formatter and both `typeof` expressions name types in the same assembly. | `SVGControl/SvgAssemblyResolver.cs:103` `$"SvgRenderer load '{requested.Name}': {SvgRenderer.DescribeFailure(ex)}"`, line 135 and line 146 similarly; line 109 `var self = typeof(SvgRenderer).Assembly;`. `SVGControl/SvgRenderer.cs:25-28` static constructor body is `SvgAssemblyResolver.Install();`. |
| Low | `SVGControl/SvgAssemblyResolver.cs` | lines 103, 135, 146 | All three diagnostic messages emitted from the relocated resolver are still prefixed `"SvgRenderer load ..."` and `"SvgRenderer resolve ..."`, naming a type the code no longer lives in. An operator grepping logs or the Visual Studio Output window for the source of a bind warning is directed to the wrong file. | Change the prefixes to `SvgAssemblyResolver load ...` and `SvgAssemblyResolver resolve ...`. | AC-3 makes designer-host observability an explicit requirement, so the accuracy of these strings is functional rather than cosmetic: they are the diagnostic channel the criterion relies on. Cheap to correct while the file is fresh. | `SVGControl/SvgAssemblyResolver.cs:103,135,146` versus the file's own `internal static class SvgAssemblyResolver` at line 15. |
| Low | `SVGControl/SvgRenderer.cs` | lines 30-49 and 51-70 | The two byte-array constructors carry near-identical 17-line bodies: the same `TryGetSvgDocument` call, the same `_doc`/`_original` assignment, and the same four-line degrade-and-log block differing only in the constructor-name literal inside `detail`. | Extract a private helper, for example `private void InitializeFromBytes(byte[] doc, string constructorLabel)`, and call it from both constructors. Both are already at 100% line coverage, so the refactor is measurable and low-risk. | `.claude/rules/general-code-change.md` lists "Reusability — Factor out logic that is clearly reusable. Avoid copy-paste" as a design priority. Duplicated failure-handling is the kind of block that drifts: a future change to the log format or the fallback size will land in one constructor and not the other. | `SVGControl/SvgRenderer.cs:32-45` versus `53-66`; the only textual differences are the two literals `"SvgRenderer(byte[], Size, AutoSize): "` and `"SvgRenderer(byte[], Size, Padding, AutoSize): "`. |
| Low | `SVGControl.Test/SVGControl.Test.csproj` | lines 282-284 | The `Svg` reference added by this branch omits `<Private>True</Private>`, unlike every neighbouring `<Reference>` with a `HintPath` in the same `ItemGroup`. It copies to output anyway because that is MSBuild's default for a `HintPath`-resolved reference, so the omission is currently harmless. | Add `<Private>True</Private>` for consistency with the surrounding style, ideally in the same change that adds the `ExCSS` reference. | `.claude/rules/general-code-change.md` and the C# policy both direct matching the existing style where the repository has one. An implicit default surrounded by explicit declarations reads as an oversight and invites someone to "fix" it in the wrong direction later. | `SVGControl.Test/SVGControl.Test.csproj:282-284` has no `<Private>` child; the adjacent `OpenTelemetry.PersistentStorage.FileSystem` and `System.Buffers` references both declare `<Private>True</Private>`. |
| Info | `SVGControl/SvgAssemblyProbe.cs` | line 62 | `seen.Add(candidate)` de-duplicates on the untrimmed candidate while the emptiness test immediately before it uses `candidate.Trim().Length > 0`. Two candidates differing only by surrounding whitespace would both be admitted. Not reachable in practice: candidates 1 and 2 come from `Path.GetDirectoryName`, which does not emit surrounding whitespace, and candidate 3 is `AppDomain.CurrentDomain.BaseDirectory`. | Optional: de-duplicate on the trimmed value, or trim once into a local and use it for both the test and the insert. | Recorded for completeness rather than as a defect. The method's documented contract is that it de-duplicates case-insensitively and never raises; both hold for every input the production call site can produce, and the method measures 100% line and branch coverage. | `SVGControl/SvgAssemblyProbe.cs:60-66`; call site at `SvgAssemblyResolver.cs:110-114` passes `self.Location`, `self.CodeBase`, `AppDomain.CurrentDomain.BaseDirectory`. |
| Info | repository-level, not this branch | `scripts/vscode/Invoke-VSBuild.ps1` as invoked by the mandated C# toolchain | The repository's mandated nullable/type-check gate is structurally non-probative. `msbuild TaskMaster.sln ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` returns exit 0 in 0.93 s with zero `CoreCompile` targets, because legacy non-SDK up-to-date checks compare timestamps and not properties. When the reviewer forced a genuine recompile the same command returned exit 1 with 195 errors, all pre-existing `UtilitiesCS` nullable diagnostics. Every AC-6-style "nullable build EXIT_CODE 0" claim in this repository therefore rests on nothing having recompiled. | File a repository-level follow-up in `docs/features/potential/`. Options: have the wrapper force `CoreCompile` for the projects in scope, or replace the solution-wide gate with a per-changed-project gate, which is the form that actually discriminates. | Not attributable to this branch and not remediable within a `minor-audit` scope, but it materially limits what any C# feature review in this repository can assert about type safety, so it should be visible outside this audit. | Reviewer measurements: mandated command 0.93 s, exit 0, 0 `CoreCompile`; after `touch` of the six changed C# files, exit 1, 195 errors, 0 warnings, all attributed to `UtilitiesCS.csproj`, zero to `SVGControl` or `SVGControl.Test`, and zero `CS8630`. `UtilitiesCS/UtilitiesCS.csproj:1114` declares the `ProjectReference` to `SVGControl` that drives the cascade. |
| Info | `SVGControl/app.config` | line 19 | The deferred `Fizzler` binding-redirect defect still holds at this head: the config redirects `Fizzler` to `newVersion="1.3.0.0"` while only `packages/Fizzler.1.3.1/` exists on disk. This is the same defect class as issue #418 itself — a redirect pointing at a version absent from the repository. | No action on this branch. The condition is correctly captured in `docs/features/potential/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md`; promote it with some priority. | Correctly scoped restraint deserves recording as explicitly as scope creep would. The branch found the defect, wrote it down, and did not fix it in a `minor-audit`, which is what the bugfix workflow asks for. | `SVGControl/app.config:18-19`; `ls -d packages/Fizzler.*` returns only `packages/Fizzler.1.3.1/`. |
| Info | `SVGControl/SvgRenderer.cs` | lines 266-272 | Carried forward from cycle 1 for the audit trail: an undisclosed resource-leak fix. The baseline `GetSvgDocument` constructed `new MemoryStream(file)` outside its `try` and never disposed it, on every call including the success path. `OpenFromBytes` wraps the same stream in `using`. No acceptance criterion mentions it. | No action. | Undisclosed improvements are worth recording for the same reason undisclosed regressions are: the diff is the record. | `git show ce0c91e6:SVGControl/SvgRenderer.cs` shows the undisposed stream; head line 268 reads `using (var stream = new MemoryStream(file))`. |

## Design and Structure Assessment

**Separation of concerns.** The three-file split is the right decomposition: `SvgRenderer` renders,
`SvgAssemblyResolver` binds, `SvgAssemblyProbe` computes. The only complaint is that the split is not
quite complete, recorded as the first Low finding above.

**Error handling.** Four catch sites across the changed files, zero bare. The single parse boundary
in `TryGetSvgDocument` is the correct shape: it makes every failure mode observable through `out
error` and impossible to lose silently, which is precisely the defect issue #418 opened on.
`OpenFromBytes` deliberately carries no handler and says so in a comment, which is right — two
boundaries would be one too many. The `ArgumentNullException` guard at
`SvgRenderer.cs:284-287` fails fast on a null payload or a null seam, satisfying AC-4's boundary
requirement.

**Null safety.** `#nullable enable` at line 1 of all six changed C# source files. The two
null-forgiving operators (`parsed!` at lines 35 and 56, `document!` at line 336) are each justified by
a stated contract, and line 336 carries an inline comment naming that contract. The comment at
`SvgAssemblyProbe.cs:56-57` explaining why `IsNullOrWhiteSpace` is not used — net481 has no
`NotNullWhen` post-conditions, so the call would not narrow state and `Add` would emit `CS8604` — is
exactly the kind of "why, not what" comment the policy asks for.

**Test design.** 38 `[TestMethod]` across three files, MSTest with Moq and FluentAssertions as
required, Arrange-Act-Assert throughout, no temporary files, no banned timing or clock APIs, and
failure paths driven through an injected delegate rather than through global state. The one defect is
environmental rather than structural: see the Blocking finding.

## Verdict

**PARTIAL — one Blocking finding.** The production code is ready. The test project needs one
reference added before the suite can be trusted outside the nine-assembly wrapper. Remediation cycle
1 discharged every item it was given, and the four Low findings above are polish that a maintainer
may reasonably bundle into the same change as the Blocking fix or defer.
