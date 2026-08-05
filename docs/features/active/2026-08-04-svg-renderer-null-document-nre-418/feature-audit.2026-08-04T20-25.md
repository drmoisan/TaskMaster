# Feature Audit — svg-renderer-null-document-nre (Issue #418)

- Audit timestamp: 2026-08-04T20-25
- Work mode: `minor-audit` (marker `- Work Mode: minor-audit`, `issue.md:12`)
- Companion artifacts: `policy-audit.2026-08-04T20-25.md`, `code-review.2026-08-04T20-25.md`

## Scope and Baseline

| Item | Value |
|---|---|
| Base branch (requested) | `main` |
| Base ref (resolved) | `origin/main` @ `ce0c91e686bf7e060aaab6f185ee6883269e4fd4` |
| Merge-base SHA | `ce0c91e686bf7e060aaab6f185ee6883269e4fd4` (recomputed by the reviewer with `git merge-base HEAD origin/main`; identical to the supplied value) |
| Head ref | `bug/svg-renderer-null-document-nre-418` @ `ea106111a6daf7e05f8a804ac00b4a713598962a` |
| Commits in range | 5 |
| Files changed | 74 (10 code and build, 64 markdown) |
| Languages with changed files | C# only |
| Working tree at audit time | clean |
| Active feature folder | `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418` |

Acceptance-criteria source resolution. The persisted marker in `issue.md` is `minor-audit`, so per
`.claude/skills/acceptance-criteria-tracking/SKILL.md` the single authoritative source is the explicit
`## Acceptance Criteria` section of
`docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md`. That section is present
(line 70) and contains eleven checkbox items, AC-1 through AC-11. Neither `spec.md` nor
`user-story.md` exists in the feature folder, which is consistent with `minor-audit`. No other
checkbox section of `issue.md` was treated as acceptance criteria; in particular the
`## Logs / Screenshots`, `## Impact / Severity`, `## Proposed Fix / Validation Ideas`, and
`## Next Step` checkboxes are excluded.

Evaluation method. Each criterion was evaluated against the head state of the source tree, not against
the feature evidence alone. Where a criterion cites an evidence artifact, the reviewer read that
artifact and, where the claim was numeric or mechanically checkable, reproduced it independently:
coverage figures were re-derived from a fresh XML parse of `coverage/coverage.cobertura.xml`; the
formatting, analyzer, and type-check gates were re-executed; source claims were checked against the
files and against `git show ce0c91e6:<path>` for baseline comparison.

## Acceptance Criteria Inventory

| ID | Criterion (abbreviated) | Source line | State in source |
|---|---|---|---|
| AC-1 | Failing regression test exists first | `issue.md:74` | `[x]` |
| AC-2 | No silent exception swallow | `issue.md:75` | `[x]` |
| AC-3 | Parse failure degrades visibly instead of throwing `NullReferenceException` | `issue.md:76` | `[x]` |
| AC-4 | A fail-fast API exists, and every null-tolerant call site keeps its contract | `issue.md:79` | `[x]` |
| AC-5 | Coverage on changed code | `issue.md:80` | `[x]` |
| AC-6 | Toolchain passes in a single clean pass | `issue.md:91` | `[x]` |
| AC-7 | Underlying failure identified in writing | `issue.md:96` | `[x]` |
| AC-8 | `AssemblyResolve` fallback resolves from the assembly's own directory | `issue.md:97` | `[x]` |
| AC-9 | `SVGControl.Test` builds and runs | `issue.md:98` | `[x]` |
| AC-10 | Incorrect ExCSS redirect in the test config is corrected | `issue.md:101` | `[x]` |
| AC-11 | Designer load verified by the documented human step | `issue.md:104` | `[ ]` |

Total: 11. Unconditional (per the section preamble): AC-1 through AC-6, plus AC-9 and AC-10, which
were added as decisions during the work. Conditioned on the research outcome: AC-7 and AC-8.
Human-execution: AC-11.

## Acceptance Criteria Evaluation

| ID | Verdict | Basis |
|---|---|---|
| AC-1 | **PASS** | `evidence/regression-testing/ac1-fail-before.2026-08-04T14-36.md` records the four constructor regression tests failing at branch commit `296eac95`, before any production edit, each with `NullReferenceException` at `SvgRenderer.cs:133`, with the project build at exit 0 and 0 warnings so the failures are genuine test failures rather than build failures. `ac1-pass-after.2026-08-04T14-36.md` records the same four tests passing with unchanged assertions, 6139/6139 passed, 0 failed, across 9 discovered assemblies. The reviewer confirmed the four tests exist at head with the asserted shape (`SvgRendererParseContractTests.cs:31-126`) and that they are deterministic: `Encoding.ASCII.GetBytes("this is not xml")` and `Array.Empty<byte>()`, no I/O, no clock, no RNG. The bugfix-workflow ordering that `CLAUDE.md` requires is therefore evidenced in both directions. |
| AC-2 | **PASS** | `grep -n catch SVGControl/SvgRenderer.cs` at head returns exactly three sites, at lines 99, 131, and 437, and all three declare `catch (Exception ex)`. Zero bare `catch` and zero `catch (Exception)` without a binding remain. The single parse-path boundary at line 437 logs through both `logger.Error(detail, ex)` and `Trace.TraceError(detail)` and returns `false` with the exception in `out Exception? error`, which is a result the caller is required to inspect. The two resolver catches (99, 131) use `Trace.TraceWarning` only, with an in-code comment stating the reason: `log4net` inside an `AssemblyResolve` handler can itself trigger a re-entrant assembly load. That is a sound justification for not using the project logger at those two sites, and both still log rather than discard. `SVGControl/SvgAssemblyProbe.cs` contains no `catch` at all and instead returns null for unusable input. Corroborated by `evidence/qa-gates/analyzer-build.2026-08-04T14-36.md` (exit 0, 0 new diagnostics) and independently by the reviewer's analyzer build (exit 0, 0 errors, 6 pre-existing warnings, none in `SVGControl` or `SVGControl.Test`). |
| AC-3 | **PASS** | Both byte-array constructors at head (`SvgRenderer.cs:164-183` and `185-204`) branch on `TryGetSvgDocument`. On the failure branch each assembles a constructor-scoped detail string, emits it through `logger.Error(detail, error)` **and** `Trace.TraceError(detail)`, and sets `_original = Size.Empty` without touching `_doc`, which is declared `private SvgDocument? _doc;` at line 246 and therefore legitimately remains null. Neither failure branch contains `_doc.Draw()` and neither contains `throw`. The dual-channel requirement in the criterion's second paragraph is met: `System.Diagnostics.Trace` output surfaces in the Visual Studio Output window, and both channels carry the exception type and message via `DescribeFailure`, which renders `error.GetType().FullName + ": " + error.Message`. Proven behaviorally by the four constructor tests that failed with `NullReferenceException` before the fix and pass after, across both overloads and both malformed and empty payloads. The recorded rationale for degrading rather than throwing (eleven designer-generated `PictureBoxSVG` sites, one inside the Outlook add-in) is a sound trade-off for a UI control and is documented where a maintainer will find it. |
| AC-4 | **PASS** | Head declares `public static bool TryGetSvgDocument(byte[], out SvgDocument?, out Exception?)` (`SvgRenderer.cs:452-459`) and `public static SvgDocument GetSvgDocumentOrThrow(byte[])` (`:465-472`), the latter throwing `InvalidOperationException(ParseFailed + DescribeFailure(error), error)` so `InnerException` is the original parser exception. `public static SvgDocument? GetSvgDocument(byte[])` (`:478-482`) retains the tolerant null-returning contract and now has no handler of its own, so it cannot reintroduce a swallow. `SVGControl/SvgImageSelector.cs` is absent from the branch diff, so all four `SvgImageSelector` consumers named in the criterion are literally unchanged. The reviewer verified one additional point the criterion implies but does not state: the tolerant member's argument-boundary behavior is unchanged, not merely similar. The baseline constructed `new MemoryStream(file)` **outside** its `try` (`git show ce0c91e6:SVGControl/SvgRenderer.cs`), so a null argument already raised `ArgumentNullException` rather than returning null; head raises the same exception from the new guard, and `GetSvgDocument_WithNullPayload_ThrowsArgumentNullException` pins it. No call site dereferences a value that can still be null: the five `SvgRendererNullToleranceTests` exercise the `Document` setter with null, `Render()` with a null document, `SetDefaultImage`, the default-image constructor, and the `UseDefaultImage` setter, all passing. The criterion's own surface-scope note is accurate: `SvgRenderer` is `internal class` at line 19, so `public static` here describes an assembly-internal surface reachable only from `SVGControl` and, through `[assembly: InternalsVisibleTo("SVGControl.Test")]` at `SVGControl/RelativePath.cs:19`, from the test assembly. |
| AC-5 | **PASS** | All three of the criterion's measurable gates are met, each reproduced independently by the reviewer from a fresh parse of `coverage/coverage.cobertura.xml`. (1) New MSTest coverage using Moq and FluentAssertions exists: 28 tests across three files, all FluentAssertions, with Moq driving the parse seam. Success path (`GetSvgDocument_WithTheBuiltInDefaultImage_ReturnsADocument`, `GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument`), parse-failure path (malformed, empty, and seam-injected null and seam-injected throw), and argument-boundary path (`ArgumentNullException` on both `GetSvgDocument` and `TryGetSvgDocument`) are all covered. (2) Newly added members reach the `>= 90%` threshold: all seven measure **100.000%** line-rate — `OpenFromBytes` 5/5, seam `TryGetSvgDocument` 23/23, public `TryGetSvgDocument` 3/3, `GetSvgDocumentOrThrow` 6/6, `DescribeFailure` 5/5, `SvgAssemblyProbe.TryGetDirectoryFromCodeBase` 11/11, `SvgAssemblyProbe.GetProbeDirectories` 23/23. Minimum observed 100.000%, ten points above the gate. (3) No regression on changed lines: the `SVGControl.SvgRenderer` class rose from 264/422 = 62.559% to 424/588 = 72.109%, +9.55 points with 160 newly covered lines, and no changed member lost coverage. The criterion's two distinct null-producing paths are both covered: the throwing path asserts `XmlException` as `InnerException`, and the null-returning path is driven through the injected `Func<byte[], SvgDocument?>` with Moq, which mutates no global state and uses no temporary file. The AC-5 amendment's correction of its own premise (an empty payload raises `XmlException: Root element is missing` rather than returning null) is factually accurate, was verified by the reviewer against the retargeted assertions, and both retargeted assertions are strictly stronger than the originals. One narrow shortfall is recorded as code-review finding CR-5 (Low) rather than as a criterion downgrade: the success branch of `SvgRenderer(byte[], Size, AutoSize)` is undriven, leaving that member at 13/17 = 76.471%, while the four-argument overload's identical branch is covered 18/18. It affects none of AC-5's three measurable gates and is closed by one test. |
| AC-6 | **PASS** | `evidence/qa-gates/toolchain-clean-pass.2026-08-04T14-36.md` records `Pass number: 1` with no loop restart and all six commands at exit 0 in `CLAUDE.md` toolchain order, 0 files reformatted, and both build gates matching the `2026-08-04T21-04` baseline exactly in count, code, text, and emitting project. The reviewer independently re-executed three of the four stages at head and reproduced each: `dotnet tool run csharpier check .` exit 0 with `Checked 1466 files in 4405ms` and 0 needing formatting; the mandated solution analyzer build exit 0 with 0 errors and 6 warnings in 11.14 s of real recompilation, the warning set being 2 `CS2002` occurrences of one pre-existing duplicate `<Compile>` in `UtilitiesCS.Test.csproj` and 4 code-less `System.Reactive` `packages.config` warnings, none in `SVGControl` or `SVGControl.Test`; the mandated solution nullable/`TreatWarningsAsErrors` build exit 0 with 0 errors and 5 warnings. The test stage is accepted from `evidence/qa-gates/test-coverage.2026-08-04T14-36.md` at 6140/6140 passed, 0 failed. The "no new diagnostics" clause holds. The intra-stage rerun after a `Test host process crashed` in `TaskVisualization.Test` is accepted as environmental contention: no test reported `Failed`, no file changed between invocations, the crash was in an assembly unrelated to the change, and the artifact discloses it. **Caveat, recorded as policy-audit gap G-3 and not as a criterion downgrade:** the mandated nullable command's exit 0 is vacuous because legacy non-SDK up-to-date checks are timestamp-based rather than property-based; the reviewer's own run completed in 1.70 s with 0 `CoreCompile` targets. AC-6 asks whether the mandated commands pass in one consecutive pass, and they do. What the mandated command *proves* is a separate question, addressed by the reviewer's forced recompile of `SVGControl/SVGControl.csproj` under `/p:Nullable=enable /p:TreatWarningsAsErrors=true`, which returned exit 0 with 0 errors and 0 warnings — a genuine independent compile of the changed production code under the strictest property set. |
| AC-7 | **PASS** | `research/2026-08-04T15-05-svg-renderer-null-document-research.md` exists under the feature's `research/` directory and delivers all three elements the criterion requires in writing. It names the exception: `System.IO.FileNotFoundException` for `ExCSS, Version=4.2.3.0` (§ 2.1), with the reasoning for that type rather than `FileLoadException` given at § 2.1 and with an explicit note that the design must not depend on the distinction. It identifies the hosts (§ 3.2 host matrix): the WinForms designer in `devenv.exe` reproduces because `devenv.exe.config` carries no ExCSS entry; the `vstest.console.exe` test host would reproduce but for the `AssemblyResolve` fallback, and its own redirect was additionally wrong; production is a VSTO add-in in `OUTLOOK.EXE` whose per-add-in AppDomain applies `TaskMaster.dll.config` correctly, so production does not reproduce. It answers the fallback sub-question directly (§ 4.1): "the fallback at `SVGControl/SvgRenderer.cs:36-104` is reached in the failing host. It is reached and returns `null`," with § 4.2 and § 4.3 explaining why each strategy fails there. The artifact labels its own confidence honestly with a `[VERIFIED]`/`[INFERRED]`/`[GIVEN]` legend, and the `FileNotFoundException` conclusion is marked as reasoned rather than re-observed. That is the correct epistemic status for a criterion that asks for a written identification; empirical confirmation of the exception identity in the designer host is the sequencing benefit the criterion itself assigns to AC-11. Corroborated empirically in the direction the research predicts: `SetDefaultImage_OnASelector_LeavesTheRendererDocumentNonNull` passes with its full `Document`-non-null assertion intact, so the ExCSS bind does succeed inside the test host and the `[P1-T21]` narrowing contingency was not triggered. |
| AC-8 | **PASS** | `ResolveByNameAndKey` at head (`SvgRenderer.cs:47-143`) runs strategy 3 after the already-loaded scan and the `Assembly.Load` attempt, iterating `SvgAssemblyProbe.GetProbeDirectories(self.Location, self.CodeBase, AppDomain.CurrentDomain.BaseDirectory)` (lines 108-113) and gating every `Assembly.LoadFrom` result through `PublicKeyTokensEqual` (line 126), so the public-key-token match requirement is preserved. The re-entrance guard still encloses strategies 2 and 3: `_resolving.Add` at line 80, `try` at 84, `finally { _resolving.Remove(...) }` at 137-140, and the method still ends `return null;` at 142. Strategies 1 and 2 are preserved in their original order, so an already-loaded match still wins over a fresh `LoadFrom`. The empty-`Location` tolerance the criterion names is implemented (`SvgAssemblyProbe.cs:41-47` skips a zero-length location rather than resolving it against the current directory) and is directly tested by `GetProbeDirectories_WithAnEmptyAssemblyLocation_SkipsThatCandidate`. The ordered-candidate decision logic lives in `internal static class SvgAssemblyProbe` per Design Decision 12 and is covered by nine tests including the unparsable code base, case-insensitive de-duplication, and the all-null empty-list case, all at 100% line and branch coverage. **Caveat, recorded as code-review finding CR-2 (Medium) and not as a criterion downgrade:** the criterion's tolerance clause is specifically about an empty `Location`, and that clause is satisfied. Separately, the outer `catch` was removed from the handler, leaving `self.Location`, `self.CodeBase`, and `Path.Combine` able to throw out of an `AssemblyResolve` handler, and `GetProbeDirectories` does not apply its invalid-path-character filter to the `baseDirectory` candidate. |
| AC-9 | **PASS** | `TaskMaster.sln` at head contains the `SVGControl.Test` project entry (`+14/-0`, project GUID `{13AC39E6-DE06-4337-8EB0-41CE674A4C3B}` with all six configuration mappings), and `git show ce0c91e6:TaskMaster.sln` confirms it was absent at the merge-base. `evidence/qa-gates/svgcontrol-test-build.2026-08-04T14-36.md` records the project build at exit 0 with the `EnsureNuGetPackageBuildImports` `<Error>` not firing; `evidence/other/package-restore-decision.2026-08-04T14-36.md` records that the primary restore route was taken with no substitutions and every `..\packages\` path resolves; `evidence/baseline/svgcontrol-test-buildability.2026-08-04T21-04.md` records all 71 `..\packages\` paths resolving. The reviewer independently rebuilt the project from scratch (`/t:Rebuild` with analyzers and `TreatWarningsAsErrors`) and obtained exit 0 with 0 errors and 0 warnings, confirming both that it compiles and that the hard `<Error>` does not fire at head. Tests execute: 9 assemblies discovered including `SVGControl.Test.dll`, 6140/6140 passed. The amendment correctly records that the five package pins named in the criterion's original text were superseded by the rebase onto `ce0c91e6` (PR #419) and that the delivered pins are `Castle.Core 5.2.1`, `FluentAssertions 8.10.0`, `Moq 4.20.72`, `MSTest.TestAdapter 4.3.3`, and `MSTest.TestFramework 4.3.3`. The reviewer accepts the substantive requirement as met and the version drift as correctly disclosed rather than concealed. A related consequence of solution membership is recorded as code-review finding CR-1 (Medium): the project declares no `<LangVersion>`, so it emits `CS8630` under the mandated `/p:Nullable=enable` property at recompile scope. |
| AC-10 | **PASS** | `SVGControl.Test/app.config` at head reads `<bindingRedirect oldVersion="0.0.0.0-4.3.2.0" newVersion="4.3.2.0" />` inside the ExCSS `dependentAssembly` block, replacing the baseline `0.0.0.0-4.2.4.0` / `4.2.4.0`. The delivered target of `4.3.2.0` rather than the `4.3.1.0` named in the original text is correct and is disclosed in the amendment: the reviewer confirmed that only `packages/ExCSS.4.3.2/` exists on disk, that `SVGControl/packages.config:3` pins `ExCSS 4.3.2`, and that `SVGControl/app.config:15` redirects to `4.3.2.0`, so `4.3.2.0` is the value that satisfies the criterion's intent of matching both the deployed assembly and the sibling config. The reviewer independently verified the amendment's zero-match claim with care, because the literal string `newVersion="4.2.4.0"` still appears in at least ten `app.config` files repository-wide: every remaining occurrence is a `System.Threading.Tasks.Extensions` redirect, and no `newVersion="4.2.4.0"` remains inside an ExCSS `dependentAssembly` block anywhere in the repository. The correction closes the specific trap the research artifact identified at § 3.3, where the redirect converted a resolvable `ExCSS 4.2.3.0` request into an unresolvable one and was masked only by the `AssemblyResolve` fallback. Functionally corroborated by the passing `SetDefaultImage` and default-image tests. |
| AC-11 | **FAIL** | Undelivered. The criterion is `- [ ]` unchecked at `issue.md:104`, and the expected evidence path recorded by the handoff artifact, `evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md`, does not exist. `ls` of that directory returns only `ac1-fail-before.2026-08-04T14-36.md` and `ac1-pass-after.2026-08-04T14-36.md`. The runbook itself exists and is substantive (`runbooks/verify-winforms-designer-load.runbook.md`, 283 lines) but has not been executed. `evidence/other/ac11-runbook-handoff.2026-08-04T14-36.md` correctly documents why the step is not automatable — it requires a live `devenv.exe` / `DesignToolsServer.exe`, and `.claude/rules/general-unit-test.md` UT4 prohibits unit tests from depending on external processes — and records the handoff to a human operator with the cue and the expected evidence path. The reviewer agrees the step is not automatable and that leaving the criterion unchecked rather than claiming it was the correct decision. This is a human-execution gap, not a code defect. Automated evidence narrows but does not replace it: the `NullReferenceException` failure mode is eliminated at source level and is host-independent, and the ExCSS bind succeeds in the vstest host. What remains genuinely unknown is open question U-2, whether `ExCSS.dll` is present in Visual Studio's `ProjectAssemblies` shadow-copy directory alongside `SVGControl.dll`, which determines whether the AC-8 directory probe succeeds in the designer host. Note that because of the AC-3 degrade-and-log decision, the designer load should now succeed either way: a failed bind produces a blank icon plus a named exception in the Output window rather than a designer load failure. |

Verdict distribution: **10 PASS, 0 PARTIAL, 1 FAIL, 0 UNVERIFIED.**

## Summary

Ten of eleven acceptance criteria are delivered and verified. The single failure, AC-11, is the
documented human designer-load verification, which has not been executed and for which no capture
exists at the expected evidence path.

The delivered work substantively resolves the defect the issue describes. The confirmed
error-handling defect (AC-1 through AC-6) is fully addressed: the silent swallow is gone, both
byte-array constructors degrade with a dual-channel diagnostic instead of dereferencing a null, an
explicit fail-fast API exists alongside the preserved tolerant one, the new members are at 100% line
coverage, and the toolchain passes in one consecutive pass. The underlying parse and binding failure
(AC-7 and AC-8) is identified in writing with an honest confidence label, and the directory-probing
fallback that the identification called for is implemented, tested through an extracted pure type, and
preserves both the re-entrance guard and the public-key-token match. Two criteria added as decisions
during the work (AC-9 and AC-10) are also delivered, including a binding-redirect correction that
closes a trap of the same defect class as the bug itself.

AC-11 matters more than a single unchecked box normally would, because reproducing the designer load
is the observation the bug report opened on. Two mitigating facts bound the risk. First, the AC-3
degrade-and-log decision makes the `NullReferenceException` failure mode unreachable regardless of
host, which is proven by four regression tests that failed with that exception before the fix and pass
after. Second, if the ExCSS bind still fails in the designer host, the fix no longer discards the
exception, so the runbook capture will produce a named, diagnosable error rather than an opaque
`NullReferenceException` — which means executing the runbook can now only confirm success or yield
better diagnostics, not reveal a worse failure mode than the one already fixed.

Beyond the criteria, the audit found no acceptance criterion overstated by its evidence, which is
unusual and worth recording. Where the delivered result diverged from a criterion's original text —
the ExCSS target version, the five package pins, and AC-5's factually incorrect premise about the
element-free path — each divergence is disclosed in an amendment that states what changed, why, and
what the criterion's substantive requirement still is. The reviewer independently checked all three
divergences and found each amendment accurate. One framing issue was found and is recorded in the
policy audit as gap G-3 rather than as a criterion downgrade: the `CS8630` diagnostic is described as
"present in the baseline", which is true of the cited baseline but not of the merge-base, because the
cited baseline was captured at the branch commit that created the condition.

Remediation is required, driven by AC-11 (this artifact) plus policy-audit gaps G-1 and G-3.
Enumerated inputs are in `remediation-inputs.2026-08-04T20-25.md`.

### Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md
- Total AC items: 11
- Checked off (delivered): 10
- Remaining (unchecked): 1
- Items remaining: AC-11 — Designer load verified by the documented human step.
```

## Acceptance Criteria Check-off

Check-off actions taken by this audit: **none required.**

Per `.claude/skills/acceptance-criteria-tracking/SKILL.md`, the reviewer checks off each criterion
evaluated as PASS if it is not already checked, and leaves PARTIAL, FAIL, and UNVERIFIED items
unchecked.

| ID | Verdict | State before audit | Action | State after audit |
|---|---|---|---|---|
| AC-1 | PASS | `[x]` | none needed, already checked | `[x]` |
| AC-2 | PASS | `[x]` | none needed, already checked | `[x]` |
| AC-3 | PASS | `[x]` | none needed, already checked | `[x]` |
| AC-4 | PASS | `[x]` | none needed, already checked | `[x]` |
| AC-5 | PASS | `[x]` | none needed, already checked | `[x]` |
| AC-6 | PASS | `[x]` | none needed, already checked | `[x]` |
| AC-7 | PASS | `[x]` | none needed, already checked | `[x]` |
| AC-8 | PASS | `[x]` | none needed, already checked | `[x]` |
| AC-9 | PASS | `[x]` | none needed, already checked | `[x]` |
| AC-10 | PASS | `[x]` | none needed, already checked | `[x]` |
| AC-11 | FAIL | `[ ]` | **left unchecked** — the human runbook has not been executed and no capture exists at `evidence/regression-testing/designer-load-<timestamp>.md` | `[ ]` |

`issue.md` was not modified by this audit. No phantom criteria were added, no criterion text was
altered, and no unmet item was checked off.

The remaining check-off is owned by the human operator named in
`evidence/other/ac11-runbook-handoff.2026-08-04T14-36.md`: execute
`runbooks/verify-winforms-designer-load.runbook.md`, write the capture to
`docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md`,
and change `- [ ] **AC-11` to `- [x] **AC-11` in `issue.md`.
