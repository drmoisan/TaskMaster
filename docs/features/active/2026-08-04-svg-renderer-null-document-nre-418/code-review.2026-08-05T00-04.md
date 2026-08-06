# Code Review — svg-renderer-null-document-nre (Issue #418)

- Artifact timestamp: `2026-08-05T00-04`
- Base: `origin/main` @ `ce0c91e686bf7e060aaab6f185ee6883269e4fd4`
- Head: `bug/svg-renderer-null-document-nre-418` @ `69e675d014d001b2e17ee15c3279ce6a5ba46609`
- Review cycle: reaudit 3 (remediation cycle 2 verification)
- Scope: full branch-vs-base diff, 152 files (6 `.cs`, 5 build-configuration, 141 `.md`)

## Executive Summary

**Verdict: PASS with non-blocking findings. Zero Blocking findings remain in code.**

The single Blocking finding from the `2026-08-04T22-28` review is **resolved**. That finding recorded
that six tests in `SVGControl.Test` changed outcome depending on the ordinal position of the assembly on
the `vstest.console.exe` command line, because `ExCSS.dll` was never copied into the project's output.
Commit `69e675d0` adds the missing `ExCSS` `<Reference>` and `packages.config` entry. The reviewer
verified the fix by running the discriminating shape directly — `vstest.console.exe` against
`SVGControl.Test\bin\Debug\SVGControl.Test.dll` alone — and observed **75 total, 75 passed, 0 failed,
exit 0**, against the 75/69/**6** the same shape produced before the fix.

Cycle-2 finding disposition:

| Cycle-2 finding | Status at this head |
|---|---|
| Blocking — order-dependent tests, missing `ExCSS` reference | **Resolved.** Verified by reviewer-executed standalone run at 75/75/0 |
| Low — `<Private>True</Private>` missing on the `Svg` reference | **Resolved.** Added in `69e675d0` at `SVGControl.Test.csproj:288` |
| Low — resolver reaches back into `SvgRenderer` for `DescribeFailure` and `typeof` | **Open, carried forward.** Unchanged |
| Low — diagnostic prefixes still say `"SvgRenderer load ..."` | **Open, carried forward.** Unchanged |
| Low — duplicated byte-array constructor bodies | **Open, carried forward.** Unchanged |
| Info — Fizzler redirect, mandated nullable gate, resource-leak fix | **Open, correctly deferred.** Unchanged |

The three Low findings carried forward were not in the cycle-2 remediation plan's scope; the plan
addressed the Blocking finding only. That is a defensible sequencing choice under a `minor-audit` work
mode, and it is recorded here rather than escalated.

One finding is recorded against a **reviewer-authored artifact** rather than the branch. The cycle-2
remediation inputs directed adding a `Fizzler` reference "for parity with the eight sibling test
projects." That justification is false on disk. The executor declined, documented why, and was correct;
the reviewer has now independently verified the refutation. Complying would have introduced the same
defect class as issue #418 itself.

Positive observations worth recording. The executor's disposition of the change is unusually
well-calibrated in three respects. It **refused a directed change it could prove was wrong**, rather
than complying and letting a reviewer error propagate into the codebase. It **reported a negative
result**: the anticipated `SVGControl` coverage improvement did not materialize, and the executor
recorded that by name instead of claiming a gain — all six class figures are byte-identical, which the
reviewer confirmed against the regenerated Cobertura. And it **disclosed that the mandated nullable gate
returned exit 0 vacuously** with zero `CoreCompile` targets, then ran forced per-project rebuilds to
supply probative evidence, rather than presenting the vacuous zero as a pass.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low | `SVGControl/SvgAssemblyResolver.cs` | lines 103, 109, 135, 146 | Carried forward from cycle 2, unchanged. The extracted resolver still reaches back into `SvgRenderer` for two things: `SvgRenderer.DescribeFailure(ex)` at three call sites, and `typeof(SvgRenderer).Assembly` at line 109. The two types are mutually dependent — `SvgRenderer`'s static constructor calls `SvgAssemblyResolver.Install()`, and the resolver calls back into `SvgRenderer`. The R-6 separation is therefore incomplete: the file's own header says its concern is "assembly binding rather than SVG rendering", yet it cannot compile without the renderer. | Move `DescribeFailure` to `SvgAssemblyProbe` or a small shared internal helper and have `SvgRenderer` call it there; change line 109 to `typeof(SvgAssemblyResolver).Assembly`, which resolves to the identical assembly without the cross-reference. | Completes the separation the extraction set out to achieve and removes a mutual type dependency inside a CLR callback path, where static-initialization order is harder to reason about than usual. No behavior change: `DescribeFailure` is a pure string formatter and both `typeof` expressions name types in the same assembly. | `SVGControl/SvgAssemblyResolver.cs:103` reads `$"SvgRenderer load '{requested.Name}': {SvgRenderer.DescribeFailure(ex)}"`; lines 135 and 146 are the same shape. Line 109 reads `var self = typeof(SvgRenderer).Assembly;`. `SVGControl/SvgRenderer.cs:25-27` static constructor body is `SvgAssemblyResolver.Install();`. |
| Low | `SVGControl/SvgAssemblyResolver.cs` | lines 103, 135, 146 | Carried forward from cycle 2, unchanged. All three diagnostic messages emitted from the relocated resolver are prefixed `"SvgRenderer load ..."` and `"SvgRenderer resolve ..."`, naming a type the code no longer lives in. An operator grepping logs or the Visual Studio Output window for the source of a bind warning is directed to the wrong file. | Change the prefixes to `SvgAssemblyResolver load ...` and `SvgAssemblyResolver resolve ...`. | AC-3 makes designer-host observability an explicit requirement, so the accuracy of these strings is functional rather than cosmetic: they are the diagnostic channel that criterion relies on. Cheap to correct. | `SVGControl/SvgAssemblyResolver.cs:103,135,146` versus the file's own declaration `internal static class SvgAssemblyResolver` at line 15. |
| Low | `SVGControl/SvgRenderer.cs` | lines 30-49 and 51-70 | Carried forward from cycle 2, unchanged. The two byte-array constructors carry near-identical 17-line bodies: the same `TryGetSvgDocument` call, the same `_doc`/`_original` assignment, and the same four-line degrade-and-log block differing only in the constructor-name literal inside `detail`. | Extract a private helper, for example `private void InitializeFromBytes(byte[] doc, string constructorLabel)`, and call it from both constructors. Both are at 100% line coverage, so the refactor is measurable and low-risk. | `.claude/rules/general-code-change.md` lists "Reusability — Factor out logic that is clearly reusable. Avoid copy-paste" as a design priority. Duplicated failure-handling drifts: a future change to the log format or the fallback size will land in one constructor and not the other. | `SVGControl/SvgRenderer.cs:32-45` versus `53-66`; the only textual differences are the literals `"SvgRenderer(byte[], Size, AutoSize): "` and `"SvgRenderer(byte[], Size, Padding, AutoSize): "`. The second body's comment already concedes the duplication: "See the other byte[]-doc constructor for the degrade-and-log rationale." |
| Low | `SVGControl/SvgAssemblyResolver.cs` | lines 50-51, 54 | The pre-guard region of `ResolveByNameAndKey` sits outside the containment `try` added by R-3. `new System.Reflection.AssemblyName(args.Name)` at line 50 and `loaded.GetName()` at line 54 can both raise, and an exception escaping an `AssemblyResolve` handler converts a recoverable bind failure into a hard failure at whatever triggered the bind. The residual is disclosed in the issue's AC-2 amendment and in the remediation plan's Design Decision 11, so it is a known and accepted boundary, not an oversight. | Optional: widen the `try` to enclose the `AssemblyName` construction and the already-loaded scan, or wrap those two statements in their own narrow handler. Weigh against the plan's stated rationale for leaving them out. | The file's own comment at lines 140-142 states the containment principle absolutely — "nothing may escape an `AssemblyResolve` handler" — while two statements sit outside it. `args.Name` originates from the CLR and is well-formed in practice, so this is a robustness gap rather than a live defect. | `SVGControl/SvgAssemblyResolver.cs:50` `var requested = new System.Reflection.AssemblyName(args.Name);` precedes the `try` at line 82. The containment `catch` is at line 143 with the "nothing may escape" comment at 140-142. |
| Info | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/remediation-inputs.2026-08-04T22-28.md` | the Blocking finding's Recommendation cell | **Defect in a reviewer-authored artifact, not in the branch.** The cycle-2 remediation inputs directed "Add `Fizzler 1.3.1` on the same pattern for parity with the eight sibling test projects." The executor declined and documented why. The reviewer has verified the refutation independently and confirms the executor was correct on all three grounds: zero test projects reference `Fizzler`; no test output carries `Fizzler.dll`; and `SVGControl.Test/app.config:27` redirects `Fizzler` to `1.3.0.0` while the on-disk package is `Fizzler.1.3.1` and both production references declare `Version=1.3.1.0`. Complying would have deployed a `1.3.1.0` assembly into a project redirecting to an absent `1.3.0.0` — the same defect class as issue #418. | No action on the branch. Future remediation inputs must verify on-disk parity claims before directing a build-configuration change. The stale redirect itself is correctly filed at `docs/features/potential/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md`. | An executor that complies with a provably wrong directive propagates a reviewer error into the codebase. Recording the reviewer's error as explicitly as a code defect is the only way the correction survives into the next cycle. | `grep -rn "Fizzler" --include=*.csproj .` returns only `SVGControl/SVGControl.csproj:58` and `UtilitiesCS/UtilitiesCS.csproj:63`, both `Version=1.3.1.0` and both production. `ls SVGControl.Test/bin/Debug/Fizzler.dll` → no such file. `ls -d packages/Fizzler*` → `packages/Fizzler.1.3.1/` only. |
| Info | `SVGControl.Test/SVGControl.Test.csproj` | lines 130-133 | The added `ExCSS` reference is correct and consistent. Its identity string is byte-identical to the `ExCSS` references already present in `SVGControl.csproj`, `UtilitiesCS.csproj`, and `QuickFiler.csproj`, its `HintPath` resolves to the only ExCSS package on disk, and its declared `Version=4.3.2.0` matches both the deployed assembly and the `newVersion="4.3.2.0"` in `SVGControl.Test/app.config`. Recorded as a positive verification, not a defect. | No action. | The class of defect this branch fixes is a version identity that disagrees with what is deployed. Verifying that the fix does not reintroduce the same class is worth stating explicitly rather than assuming. | `grep -rn 'Reference Include="ExCSS,' --include=*.csproj .` returns four identical identity strings. `ls SVGControl.Test/bin/Debug/` shows `ExCSS.dll` at 368,128 bytes. `SVGControl.Test/app.config:23` reads `oldVersion="0.0.0.0-4.3.2.0" newVersion="4.3.2.0"`. |
| Info | `SVGControl/app.config`, `SVGControl.Test/app.config` | `SVGControl/app.config:18-19`; `SVGControl.Test/app.config:26-27` | The deferred `Fizzler` binding-redirect defect still holds at this head: both configs redirect `Fizzler` to `newVersion="1.3.0.0"` while only `packages/Fizzler.1.3.1/` exists. Same defect class as issue #418 — a redirect naming a version absent from the repository. It is inert in `SVGControl.Test` only because no `Fizzler.dll` reaches that output. | No action on this branch. Promote `docs/features/potential/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md` with some priority; this cycle produced a concrete demonstration of why it matters. | Correctly scoped restraint deserves recording as explicitly as scope creep would. The branch found the defect, wrote it down, and did not fix it under a `minor-audit` work mode, which is what the bugfix workflow asks for. | `grep -A3 'name="Fizzler"' SVGControl.Test/app.config` → `bindingRedirect oldVersion="0.0.0.0-1.3.0.0" newVersion="1.3.0.0"`. `ls -d packages/Fizzler*` → `packages/Fizzler.1.3.1/`. |
| Info | repository-level, not this branch | `scripts/vscode/Invoke-VSBuild.ps1` as invoked by the mandated C# toolchain | Carried forward and now better characterised. The mandated nullable gate `msbuild TaskMaster.sln /p:Nullable=enable /p:TreatWarningsAsErrors=true` returned exit 0 in 0.90 s with **0 of 18** `CoreCompile` targets executed. Legacy non-SDK up-to-date checks compare timestamps, not properties, so the gate passes vacuously whenever nothing recompiled. Every "nullable build EXIT_CODE 0" claim in this repository rests on that. The executor disclosed this rather than presenting the zero as a pass. | File a repository-level follow-up. Options: force `CoreCompile` for in-scope projects in the wrapper, or replace the solution-wide gate with a per-changed-project gate, which is the form that discriminates. | Not attributable to this branch and not remediable within a `minor-audit` scope, but it limits what any C# feature review in this repository can assert about type safety, so it should be visible outside this audit. | Executor evidence `evidence/qa-gates/toolchain-clean-pass.2026-08-05T05-00.md` records the vacuity explicitly and the two compensating forced rebuilds — `SVGControl.Test.csproj` and `SVGControl.csproj` each `/t:Rebuild /p:Nullable=enable /p:TreatWarningsAsErrors=true`, both exit 0 with 0 diagnostics. |
| Info | `SVGControl/SvgRenderer.cs` | lines 266-272 | Carried forward from cycle 1 for the audit trail: an undisclosed resource-leak fix. The baseline `GetSvgDocument` constructed `new MemoryStream(file)` outside its `try` and never disposed it, on every call including the success path. The replacement `OpenFromBytes` wraps the same stream in `using`. No acceptance criterion mentions it. | No action. | Undisclosed improvements are worth recording for the same reason undisclosed regressions are: the diff is the record. | `git show ce0c91e6:SVGControl/SvgRenderer.cs` shows the undisposed stream; the head file reads `using (var stream = new MemoryStream(file))`. |

## Design and Structure Assessment

**The cycle-2 fix itself.** Six added lines of build configuration, no code change, no new dependency.
`ExCSS 4.3.2` was already restored under `packages/`; the change declares an existing transitive
dependency explicitly rather than introducing a new one. This is the minimal correct fix for the defect
class — legacy `packages.config` projects do not flow transitive copy-local, so a project that needs an
assembly at runtime must name it. The `<Private>True</Private>` added to the pre-existing `Svg`
reference is behavior-preserving, since MSBuild already defaults `HintPath`-resolved references to
copy-local, and it closes the cycle-2 Low finding about style inconsistency.

**Separation of concerns.** The three-file split remains the right decomposition: `SvgRenderer` renders,
`SvgAssemblyResolver` binds, `SvgAssemblyProbe` computes. The split is still not quite complete, per the
first Low finding.

**Error handling.** Four catch sites across the changed files, zero bare. The parse boundary in
`TryGetSvgDocument` makes every failure mode observable through `out error` and impossible to lose
silently, which is precisely the defect issue #418 opened on. `OpenFromBytes` deliberately carries no
handler and says so in a comment, which is correct — two boundaries would be one too many. The resolver
uses `Trace` rather than `log4net` throughout, with the re-entrancy rationale stated in-code at lines
98-99 and 140-142: a `log4net` call inside an `AssemblyResolve` handler can itself trigger a re-entrant
assembly load. That is a genuine "why, not what" comment.

**Null safety.** `#nullable enable` at line 1 of all six changed C# source files. Both in-scope projects
compile clean under `/p:Nullable=enable /p:TreatWarningsAsErrors=true` when forced to recompile — and
this cycle's forced rebuild is cleaner evidence than cycle 2's, because no `.cs` file changed, so
`UtilitiesCS` was not dragged in through its `ProjectReference` and the result is uncontaminated by 195
pre-existing downstream diagnostics.

**Test design.** 75 tests in `SVGControl.Test`, MSTest with Moq and FluentAssertions as required,
Arrange-Act-Assert throughout, no temporary files, no banned timing or clock APIs, and failure paths
driven through an injected `Func<byte[], SvgDocument?>` delegate rather than through global state. The
environmental defect that made them order-dependent is fixed, so the suite now satisfies UT1
Independence and Determinism and the C# IDE/CLI parity rule. The `Func<>` seam remains the right weight
for a single call path and avoids introducing an interface for one method.

**File sizes.** All six changed C# files are under the 500-line limit, measured with `awk 'END{print
NR}'`. The largest is `SvgRendererParseContractTests.cs` at 358. `SvgRenderer.cs` fell from 497 to 362
as a result of the R-6 extraction — which is worth noting, because that extraction is also the direct
cause of the G-9 coverage finding in the policy audit. The 172 lines now in `SvgAssemblyResolver.cs`
would otherwise have counted against `SvgRenderer.cs`, where no new-file threshold would have applied.

## Verdict

**PASS with non-blocking findings. Zero Blocking findings in code.**

The production code and the test project are both ready to merge from a code-quality standpoint. The
four Low findings are polish a maintainer may reasonably bundle into a follow-up; three of them are
cosmetic or structural rather than behavioral, and the fourth is a disclosed and accepted robustness
boundary. None affects correctness of the delivered fix.

The one item preventing an overall PASS on this feature is not a code defect: AC-11 requires a human to
open a form in the Visual Studio WinForms designer. That is recorded in the policy audit as G-2 and in
the feature audit as the sole unmet criterion.
