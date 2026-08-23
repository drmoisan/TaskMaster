# Code Review — svg-renderer-null-document-nre (Issue #418)

- Audit timestamp: 2026-08-04T20-25
- Base: `origin/main` @ `ce0c91e686bf7e060aaab6f185ee6883269e4fd4`
- Head: `bug/svg-renderer-null-document-nre-418` @ `ea106111a6daf7e05f8a804ac00b4a713598962a`
- Scope: full branch diff, 74 files; 10 code and build files, all C#
- Companion artifacts: `policy-audit.2026-08-04T20-25.md`, `feature-audit.2026-08-04T20-25.md`

## Executive Summary

The change is well-designed for its size. It replaces a `catch (Exception) { return null; }` with a
single named parse boundary, funnels every failure mode through that one method, and gives the two
byte-array constructors a modelled degrade path instead of an unguarded dereference. The design choice
that carries the most weight is extracting the `AssemblyResolve` probe decision logic into a new pure
type, `SVGControl/SvgAssemblyProbe.cs`. That extraction is what makes the AC-8 behavior testable
without staging a real mismatched-key assembly on disk, which the repository's unit-test rules
prohibit. It is the right seam, chosen for the right reason, and the resulting nine tests cover its
edge cases thoroughly.

Test quality is high. All 28 new tests are MSTest with Moq and FluentAssertions, carry explicit
Arrange/Act/Assert sections, and attach a `because` reason to every assertion that names the policy or
criterion being defended. The Moq seam is used narrowly and correctly: `Func<byte[], SvgDocument?>` is
option 2 in the repository's DI-seam preference order, a full interface would be excessive for one
static parse call, and the default (`OpenFromBytes`) keeps production behavior unchanged. Two
non-obvious calls stand out as good practice: `TryGetSvgDocument_WithInjectedParseSeam_SurfacesTheSameExceptionInstance`
asserts `BeSameAs(sentinel)` rather than a type match, pinning exception identity rather than shape;
and `Render_WithNullDocument_ReturnsNull` disposes the bitmap in a `finally` so a contract regression
cannot leak a GDI handle into the test host.

Documentation discipline is unusually strong for a bug fix. The AC-5 amendment in `issue.md` retracts
an incorrect premise the criterion itself asserted (that an empty payload returns null without
throwing; it actually raises `XmlException: Root element is missing`), states precisely how far the
correction extends, and explicitly declines to generalize beyond what was measured. Two defects
discovered during the work were written to `docs/features/potential/` rather than fixed in-branch. The
reviewer independently confirmed both underlying conditions still hold.

Nine findings follow. None is Blocking. Two are Medium: an omitted `<LangVersion>` in the newly
solution-registered test project, and a narrowing of exception containment in the `AssemblyResolve`
handler. Five are Low, and two are informational observations recorded because they are improvements
no acceptance criterion claims credit for.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Medium | `SVGControl.Test/SVGControl.Test.csproj` | project properties; solution registration at `TaskMaster.sln:42-43` | The project declares no `<LangVersion>`, so it defaults to C# 7.3. Under the repository's mandated `/p:Nullable=enable` property it emits `CS8630: Invalid 'nullable' value: 'Enable' for C# 7.3`. Because this branch is what adds the project to `TaskMaster.sln`, the diagnostic is newly reachable from the solution-wide nullable gate relative to the merge-base, where the project is not a solution member. The mandated `/t:Build` form returns exit 0 only because legacy non-SDK up-to-date checks are timestamp-based rather than property-based, so nothing recompiles. | Add `<LangVersion>latest</LangVersion>` to the project's first `<PropertyGroup>`, matching `SVGControl/SVGControl.csproj` and the four sibling test projects that already declare it (`TaskMaster.Test`, `UtilitiesCS.Test`, `VBFunctions.Test`). Verify with a forced recompile under the mandated property set. | `SVGControl.Test` project-references only `SVGControl`, so it is the one `LangVersion`-less test project that reaches its own `CoreCompile` in a cold nullable build; the other five cascade-fail from `UtilitiesCS` first and never surface. Adding one property removes the branch's only newly reachable type-check diagnostic. | Reviewer forced recompile: `MSBuild.exe SVGControl.Test\SVGControl.Test.csproj /t:Rebuild /p:Nullable=enable /p:TreatWarningsAsErrors=true` returned exit 1 with `CS8630`. Same command without the `Nullable` override returned exit 0, 0 errors, 0 warnings. Reviewer solution-level run of the mandated command completed in 1.70 s with 0 `CoreCompile` targets. Corroborated by `evidence/baseline/nullable-build.2026-08-04T21-04.md` lines 71 and 86-96, and by `git show ce0c91e6:TaskMaster.sln`, which contains no `SVGControl.Test` entry. |
| Medium | `SVGControl/SvgRenderer.cs`, `SVGControl/SvgAssemblyProbe.cs` | `SvgRenderer.cs:84-140`; `SvgAssemblyProbe.cs:41-52` | Exception containment in the `AssemblyResolve` handler is narrower than at baseline. The outer `try` at line 84 now has only a `finally` (lines 137-140) and no `catch`; the baseline wrapped the same region in `catch { }`. Throw sites now inside the outer `try` but outside any inner handler: `self.Location` and `self.CodeBase` at lines 110-111 (`NotSupportedException` for a dynamic assembly) and `Path.Combine(directory, ...)` at line 116 (`ArgumentException` for invalid path characters). The `Path.Combine` exposure is real because `GetProbeDirectories` filters `Path.GetInvalidPathChars()` on the `assemblyLocation` candidate and, via `TryGetDirectoryFromCodeBase`, on the code-base candidate, but passes the third candidate `baseDirectory` through unfiltered. An exception escaping an `AssemblyResolve` handler propagates to whatever triggered the bind, converting a recoverable bind failure into a hard failure at construction time — the same class of opaque failure issue #418 exists to eliminate. | Two independent fixes, either sufficient, both cheap: (a) wrap the strategy-3 body (or restore a handler on the outer `try`) in `catch (Exception ex) { Trace.TraceWarning(...); }`, consistent with the two handlers already present; (b) apply the same `IndexOfAny(Path.GetInvalidPathChars()) < 0` filter to `baseDirectory` inside `GetProbeDirectories` so all three candidates are validated identically. Prefer doing both. | Likelihood is low, but the change is in the one method whose documented contract is "never raises, so it is safe inside an `AssemblyResolve` handler" (`SvgAssemblyProbe.cs:15`), and AC-8 explicitly requires the fallback tolerate hostile inputs without throwing. The asymmetry — two of three candidates filtered, one not — is also an internal inconsistency worth removing on its own merits. | `git diff ce0c91e6...HEAD -- SVGControl/SvgRenderer.cs` shows `-catch { // Swallow ... }` with no replacement on the outer `try`. `SvgAssemblyProbe.cs:41-52`: the `candidates` array filters `location` and calls `TryGetDirectoryFromCodeBase(assemblyCodeBase)`, then adds `baseDirectory` raw. `GetProbeDirectories_WithAllInputsNull_...` covers the null case but no test supplies an invalid-character `baseDirectory`. |
| Low | `SVGControl/SvgRenderer.cs` | whole file | The file is 497 lines against the hard 500-line limit in `.claude/rules/general-code-change.md`, up from 354 at baseline. Three lines of headroom. Compliant today, but the next change to this file will breach the limit and be forced into an unplanned extraction under time pressure. | Extract the `AssemblyResolve` region (the static constructor, `_resolverInstalled`, `_resolving`, `ResolveByNameAndKey`, and `PublicKeyTokensEqual`, lines 24-163) into a dedicated file in the same namespace. This continues the separation the branch already began with `SvgAssemblyProbe.cs` and would leave the renderer at roughly 360 lines. | The resolver has no renderer state and no conceptual relationship to SVG rendering; the branch's own header comment for `SvgAssemblyProbe` states exactly that rationale for the part already extracted. Doing the remainder now is cheaper than doing it reactively. | `wc -l SVGControl/SvgRenderer.cs` returns 497; `git show ce0c91e6:SVGControl/SvgRenderer.cs \| awk 'END{print NR}'` returns 354. Cross-checked with `awk END{print NR}` to avoid the known PowerShell `Measure-Object -Line` undercount. Corroborated by `evidence/qa-gates/svgrenderer-file-size.2026-08-04T14-36.md`. |
| Low | `SVGControl/SvgRenderer.cs` | lines 24-31 | Two statements in the header comment block are now factually stale and are load-bearing: they are the sole in-code explanation for why the entire `AssemblyResolve` fallback exists. (a) "Svg 3.4.7 was compiled against ExCSS 4.2.3.0 but the repo deploys ExCSS 4.3.1.0" — after the rebase onto `ce0c91e6` (PR #419) the repository pins `Svg 3.4.8` and `ExCSS 4.3.2`, and only `packages/ExCSS.4.3.2/` exists on disk. (b) "vstest's testhost ignores the test DLL's .config in some modes" — this branch's own research artifact and `evidence/other/ac11-runbook-handoff.2026-08-04T14-36.md` establish the opposite: the vstest testhost does apply the project binding redirects and the ExCSS bind succeeds there; the host that does not apply them is `devenv.exe`. | Update both statements to the delivered versions and to the research artifact's conclusion, and reference `research/2026-08-04T15-05-svg-renderer-null-document-research.md` so the explanation has a durable source. | A stale comment on a fallback this indirect is a maintenance hazard: the next reader will look for ExCSS 4.2.3.0, not find it, and may conclude the fallback is dead code. The branch already produced the correct explanation in its research artifact; the file is simply not yet pointing at it. | `SVGControl/packages.config:3,6` pin `ExCSS 4.3.2` and `Svg 3.4.8`; `ls -d packages/ExCSS.*` returns only `packages/ExCSS.4.3.2/`. `evidence/other/ac11-runbook-handoff.2026-08-04T14-36.md` lines 42-44: "The ExCSS bind itself succeeds inside the vstest testhost, a host that does apply the project binding redirects". |
| Low | `SVGControl/SvgRenderer.cs` | lines 164-183, specifically 168-171 | The success branch of `SvgRenderer(byte[], Size, AutoSize)` is driven by no test. The member measures 13/17 = 76.471% line coverage and the four uncovered lines are `_doc = parsed; _original = parsed!.Draw().Size;` plus their block. The four-argument overload's equivalent branch is covered 18/18, so the two overloads are asymmetrically tested. | Add one test constructing the three-argument overload from `Defaults.GetDefault.SvgImage` and asserting `Document` is non-null, mirroring the existing four-argument coverage. This raises the member to approximately 100% and the file by roughly 0.7 points. | The uncovered lines are the branch a real caller takes in the normal case. The regression tests deliberately target the failure branch, which is correct for issue #418, but leaves the primary path of one public overload unexercised. One test closes it. | Reviewer re-parse of `coverage/coverage.cobertura.xml`: `.ctor(byte[], System.Drawing.Size, SVGControl.AutoSize)` `line-rate=0.7647058823529411`, 13/17, `branch-rate=0.5`. Corroborated by `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md` lines 68 and 81-85. |
| Low | `SVGControl/SvgRenderer.cs` | lines 145-163 | `PublicKeyTokensEqual(byte[]?, byte[]?)` measures 0/15 = 0.000% line coverage: no test invokes it. It is `private static`, pure over two byte arrays, and gates every assembly the resolver returns across all three strategies. AC-8 requires that "the existing public-key-token match requirement" be preserved, so that requirement is currently verified by inspection only. Its zero coverage is also the reason the pre-existing strategy-1 and strategy-2 inner blocks show as uncovered, since those blocks call it. | Relocate the method to `SVGControl.SvgAssemblyProbe` (or change it to `internal static` on `SvgRenderer`) and add tests for the interesting cases its implementation already distinguishes: both null, one null and the other zero-length, one null and the other non-empty, equal tokens, unequal tokens of equal length, and unequal lengths. | This is exactly the extraction pattern the branch already applied successfully to the probe helpers, applied to the one remaining pure fragment of the same handler. It converts an inspection-only claim about a security-relevant check into a measured one, and it is the single largest coverable block left in the file at 15 lines. | Reviewer re-parse of `coverage/coverage.cobertura.xml`: `PublicKeyTokensEqual(byte[], byte[])` `line-rate=0`, 0/15. `SVGControl/SvgRenderer.cs:145`, `private static bool PublicKeyTokensEqual(byte[]? a, byte[]? b)`. Call sites at lines 68, 92, 126. |
| Low | `SVGControl.Test/SvgRendererParseContractTests.cs` | lines 219-222 | The Arrange comment asserts "No plain byte payload reaches it: malformed input and empty input both make the XML reader raise." The universal first clause is broader than what was measured. The production comment covering the same behavior hedges correctly (`SvgRenderer.cs:394-397`: "whether a well-formed-XML-but-no-SVG-element payload reaches it here is unmeasured (open question U-3)"), and the AC-5 amendment in `issue.md` explicitly disowns the broader phrasing where it appears in the evidence artifact. The test comment was not brought into line. | Replace the universal clause with the measured one, for example: "The two payload shapes measured here — malformed and empty — both make the XML reader raise. Whether a well-formed-XML-but-no-SVG-element payload reaches this branch is unmeasured (open question U-3)." | The branch went to real effort to retract this exact overstatement elsewhere. Leaving it in the test file undermines that retraction, and a future reader who trusts the comment may drop the seam-driven test as redundant. No behavior change; comment only. | `SVGControl.Test/SvgRendererParseContractTests.cs:220-221` versus `SVGControl/SvgRenderer.cs:394-397` and `issue.md` AC-5 amendment item 3. |
| Info | `SVGControl/SvgRenderer.cs` | line 401 versus baseline | A resource leak was fixed without being claimed. The baseline `GetSvgDocument` opened `Stream stream = new MemoryStream(file);` outside its `try` and never disposed it, on every call including the success path. `OpenFromBytes` wraps the same stream in `using`. No acceptance criterion mentions this. | No action. Recorded so the improvement is visible in the audit trail. | Undisclosed improvements are worth recording for the same reason undisclosed regressions are: the diff is the record. | `git show ce0c91e6:SVGControl/SvgRenderer.cs` shows the undisposed stream; head line 401 shows `using (var stream = new MemoryStream(file))`. |
| Info | `docs/features/potential/` | two new files | Two defects found during the work were deferred rather than fixed in-branch, which is the behavior `.claude/rules/general-code-change.md` and the `CLAUDE.md` bugfix workflow ask for: `2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md` (12 configs redirect `Fizzler` to `1.3.0.0` while only `1.3.1` is deployed; `SVGControl/app.config` redirects `System.Runtime.CompilerServices.Unsafe` to `6.0.2.0` while 16 siblings say `6.0.3.0`) and `2026-08-04-invoke-mstest-scalar-count-strictmode.md`. The reviewer independently confirmed both conditions still hold at head. | No action on this branch. Promote the binding-redirect entry with some priority: it is the same defect class as issue #418 itself, where a redirect to a non-deployed version broke `SvgDocument.Open`. | Correctly scoped restraint under a `minor-audit` work mode is worth recording as positively as scope creep would be recorded negatively. | `ls -d packages/Fizzler.*` returns only `packages/Fizzler.1.3.1/`; `SVGControl/app.config` line 20 reads `newVersion="1.3.0.0"` and line 26 reads `newVersion="6.0.2.0"`, while `SVGControl.Test/app.config` reads `6.0.3.0` for the same identity. |

## Detailed Notes

### Design assessment

The single-boundary refactor is the correct shape for this defect. Before the change there were three
places a caller could receive a null document with no way to learn why: `GetSvgDocument`'s handler,
and each of the two constructors that dereferenced its result. After the change there is exactly one
handler in the type, at `SvgRenderer.cs:435`, and it is impossible for a caller to reach a failure
without either an exception, a `false` return with the cause attached, or a logged record on two
channels. That is a stronger property than "add a null guard to the two constructors" would have
produced, and it is achieved with fewer moving parts.

The three-tier API (`GetSvgDocument` tolerant, `TryGetSvgDocument` explicit, `GetSvgDocumentOrThrow`
fail-fast) is a conventional and defensible surface. Keeping the tolerant member is the right call
given six existing null-tolerant consumers, and its implementation is now a two-line delegation with
no handler of its own, so it cannot reintroduce a silent swallow. Worth noting: the argument-boundary
behavior of the tolerant member is genuinely unchanged, not merely close. The baseline constructed
`new MemoryStream(file)` outside its `try`, so a null argument already raised `ArgumentNullException`
rather than returning null. Verified against `git show ce0c91e6:SVGControl/SvgRenderer.cs`, and a test
pins it.

The AC-3 decision to degrade rather than throw is documented with its rationale (`PictureBoxSVG` is
instantiated by designer-generated code in eleven forms, one of which runs inside the Outlook add-in,
so throwing would convert a blank-icon degradation into a control-construction failure for end users).
That is the right trade-off for a UI control and the reasoning is recorded where a future maintainer
will find it.

### Test assessment

Scenario coverage is complete against the repository's UT2 checklist for the members under change:
positive, negative, argument boundary, error handling, and edge cases are all present, and the state
transition that matters (document present versus absent) is exercised through both the constructor and
the property setter.

Three specific choices are better than the obvious alternative and are worth naming:

- Using `BeSameAs(sentinel)` rather than `BeOfType<T>()` for the injected-exception test pins identity,
  so a future refactor that wraps and rethrows would fail the test rather than pass it.
- Driving the null-returning parse branch through the Moq delegate rather than searching for a byte
  payload that produces it. The measured facts made the payload approach impossible, and the seam
  approach mutates no global state and touches no temporary file, satisfying UT4.
- `UseDefaultImageSetterToFalse_...` asserts only what the production code actually does and documents,
  in the test body, why it does not assert the document clear: the guard depends on
  `_relativeImagePath`, which is never assigned on any live path due to a pre-existing condition. That
  is the correct response to discovering that a branch is unreachable — assert the reachable behavior
  and record why, rather than assert behavior the code does not have.

One asymmetry remains, recorded as a Low finding: the three-argument byte-array constructor's success
branch has no test while the four-argument overload's does.

### Evidence assessment

The feature evidence is thorough and, more importantly, self-critical in the places that matter. Three
disclosures stand out:

- The nullable-build artifact states plainly that its exit 0 "is **not** evidence that the solution is
  free of nullable diagnostics" and supplies the forced-recompile inventory that is. The reviewer
  reproduced both results independently. Without that disclosure the incrementality would have been
  invisible.
- The test-coverage artifact discloses that the first invocation crashed the test host after 1266
  passing tests and was rerun, states that no file changed between invocations, and states that no
  foreign process was terminated. The reviewer accepts the environmental-contention disposition.
- The AC-5 amendment retracts a factual premise the criterion itself asserted, bounds the retraction
  to what was measured, and declines to generalize.

One framing issue, recorded as gap G-3 in the policy audit rather than as a code finding: the
`CS8630` diagnostic is described as "present in the baseline", which is true of the cited baseline
(captured at branch commit `0162567d`) but not of the merge-base. Feature-vs-base framing matters for
a diagnostic whose reachability the branch itself creates.

### What was not reviewed

The 15 changed files under `.claude/agent-memory/` are agent working memory, not repository policy or
production code. They were read for scope determination and for scope-narrowing detection, and no
policy document under `.claude/rules/` or `.github/instructions/` appears in the diff. Their content
was not reviewed for correctness.

The full test suite was not re-executed by the reviewer. The coverage report the executor's run
produced was independently re-parsed instead, and every numeric claim in the feature evidence was
reproduced to four decimal places.
