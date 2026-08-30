# Policy Audit — issue #644, navigation key ledger

- Component: `QuickFiler` / `QuickFiler.Test`
- Issue: #644
- Feature folder: `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644`
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Head: `a2c69aead286ad0ec6c7087f1bd8c46d39d0d472`
- Resolved base branch: `main` at `fa2ddefacf2c08abe18f3e3250d77da804534637`
- Review anchor for the run: `e968a1a8804b7641380d4489c496662824d45767`
- Work mode: `full-bug` (AC source: `spec.md` only)
- Review timestamp: 2026-08-29T23-06

Template note: `.claude/skills/policy-audit-template-usage/SKILL.md` requires the policy-audit
template to be resolved through `mcp__drm-copilot__resolve_policy_audit_template_asset`. No MCP tool
is exposed to this review session, so the template asset could not be resolved and
`mcp__drm-copilot__validate_orchestration_artifacts` could not be run. This artifact reproduces the
canonical major headings the skill enumerates and records the missing template resolution here, as
the skill's fallback branch directs. Assumption documented; no user question was asked.

---

## Executive Summary

**Verdict: PASS with zero blocking findings.** The change is a minimal, well-targeted bug fix that
replaces a count-bounded unregistration loop with a replay of a recorded `(SourceId, Key)` ledger.
The full branch diff against the resolved base branch is exactly six code paths plus this feature
folder, matching the spec's declared Blast Radius. All four toolchain gates are recorded green in an
uninterrupted pass, 1254 of 1254 tests pass in the touched assembly and 6876 of 6876 repository-wide,
and the six new regression tests were demonstrated red before the fix.

Seventeen of eighteen acceptance criteria are checked. **AC-16 is adjudicated PARTIAL** and remains
unchecked; see section 5 and the feature audit. Its first clause is not decidable by any instrument
present in this repository, and the residual risk it leaves open is bounded and acceptable for merge.

Seven non-blocking findings are recorded below. None is a code defect in the delivered fix; the two
that touch delivered code are comment-synchronisation drift in a test file and a residual
host-identity path in a research artifact.

**Total blocking findings: 0.**

---

## Rejected Scope Narrowing

None. The caller supplied a review span (`git diff e968a1a8804b7641380d4489c496662824d45767 -- .
':!.claude/agent-memory'`) and named six code paths. That is not a narrowing: it was independently
verified to be equal in substance to the full branch diff against the resolved base branch.

Verification command and output:

```
$ git diff --name-only fa2ddefacf2c08abe18f3e3250d77da804534637 HEAD | grep -v "^docs/features/active/2026-08-27-qfc"
QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs
QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs
QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler/Controllers/QfcCollectionController.cs
```

The full branch-versus-base diff is the six code paths plus the feature folder and nothing else. No
committed change to `.claude/agent-memory/` exists on the branch:

```
$ git diff --name-only e968a1a8804b7641380d4489c496662824d45767 HEAD -- .claude/agent-memory
(empty)
```

The two `.claude/agent-memory/*/MEMORY.md` entries visible in a working-tree diff are uncommitted
edits made by the running agents' own memory systems, not branch content.

The audit below is conducted against the full branch diff.

---

## Evidence Location Compliance

**PASS.** No file under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or
`artifacts/coverage/` appears in the branch diff.

```
$ git diff --name-only e968a1a8804b7641380d4489c496662824d45767 -- . | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"
(no match; grep exit 1)
```

All 40 evidence artifacts are under
`docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/<kind>/`
with `2026-08-29T08-15` ISO-8601 stamps, in `baseline/`, `qa-gates/`, `regression-testing/`,
`issue-updates/`, and `other/` subfolders. This satisfies
`.claude/skills/evidence-and-timestamp-conventions/SKILL.md` and AC-17.

`validate_evidence_locations.py` is not present in this repository (`find . -name
"validate_evidence_locations.py"` returns nothing), so the manual scan above is the verification.

`EVIDENCE_LOCATION_OVERRIDE_REJECTED`: none. No caller instruction specified a non-canonical
evidence path.

---

## 1. General Unit Test Policy Compliance

Source: `.claude/rules/general-unit-test.md`.

| Requirement | Verdict | Evidence |
|---|---|---|
| Independence | PASS | Every test in `QfcCollectionControllerNavigationLedgerTests.cs` builds its own controller through `CreateLedgerController` and its own `KbdActions` registry. No static or shared state. |
| Isolation | PASS | Each of T1-T6 exercises one register/unregister behaviour. |
| Fast execution | PASS | `[P4-T5]` records 1254 tests in the assembly; the six additions carry no wait. |
| Determinism | PASS | No wall-clock read, no RNG, no external process. Verified: `grep -n "Thread.Sleep\|Task.Delay\|DateTime.Now\|DateTime.UtcNow\|Path.GetTempFileName\|Path.GetTempPath\|new Random("` over the new file returns no match (exit 1). |
| Readability | PASS | Each test carries an XML doc naming its T-number, its AC, and the pre-fix behaviour it pins. |
| No external dependencies | PASS | No live Outlook, no COM activation, no WinForms handle, no STA apartment, no network, no database. Controller built with `FormatterServices.GetUninitializedObject`; `IQfcKeyboardHandler`, `MailItem`, `IQfcItemController` are Moq doubles. |
| No temporary files | PASS | Verified by the same grep above; no `File.`, `Directory.`, or temp-path API appears in the new file. |
| Banned timing APIs absent | PASS | Same grep. |
| Test file location | PASS (repository convention) | `QuickFiler.Test/Controllers/` mirrors `QuickFiler/Controllers/`. This repository realises the mirroring rule through sibling `<Project>.Test` projects rather than a repo-root `tests/` tree; all 52 `[TestMethod]`s under `QuickFiler.Test/Controllers/` follow it. The prohibition that matters — colocation inside the production source tree — is satisfied: no test file was added under `QuickFiler/`. |
| Scenario completeness | PASS | Positive: T1, T2, T6. Negative: T4 (no prior registration, unrelated entry present). Boundary: T6 and T1 at the 9/10 width crossing. Error handling: T2 asserts the absence of the pre-fix `ArgumentException`; the retained `RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException` pins the ledger's record-after-`Add` ordering. State transitions: T3 (repeated cycles), T5 (null `_itemGroups`). |
| Arrange-Act-Assert | PASS | All six tests carry explicit `// Arrange`, `// Act`, `// Assert` markers. |
| Clear failure messages | PASS | Every FluentAssertions call supplies a `because:` string naming the invariant. |
| Coverage Exclusion Policy | See finding PA-1 | `QfcCollectionController` carries `[ExcludeFromCodeCoverage]`. Pre-existing; not introduced or widened by this change. |

### Coverage Exclusion Policy — provenance check (the circularity question)

`.claude/rules/general-unit-test.md` states that no production file may be excluded from coverage
measurement. The question put to this review is whether this change added or widened the exclusion on
its own modified file, which would make the "the file is excluded, so the figure cannot move"
argument circular.

**It did not.** The attribute is pre-existing and byte-identical at the anchor:

```
$ git show e968a1a8804b7641380d4489c496662824d45767:QuickFiler/Controllers/QfcCollectionController.cs | grep -n "ExcludeFromCodeCoverage"
21:    [ExcludeFromCodeCoverage]

$ grep -n "ExcludeFromCodeCoverage" QuickFiler/Controllers/QfcCollectionController.cs
21:    [ExcludeFromCodeCoverage]

$ git diff e968a1a8804b7641380d4489c496662824d45767 -- QuickFiler/Controllers/QfcCollectionController.cs | grep -n "ExcludeFromCodeCoverage"
(no match)
```

The attribute sits on the single non-partial declaration `public class QfcCollectionController` at
line 22. No new `[ExcludeFromCodeCoverage]` attribute is added anywhere in the branch diff, and no
`coverage.config` or `exclude` entry is added or changed. The circularity concern is disproved. The
standing conflict between the attribute and the Coverage Exclusion Policy is recorded as PA-1 and is
pre-existing debt, already listed in the spec's "Follow-up work deliberately not done here".

---

## 2. General Code Change Policy Compliance

Source: `.claude/rules/general-code-change.md` and `CLAUDE.md`.

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | One `List<(string, string)>` field, one lazy accessor, a `foreach` and a `Clear()`. The rejected alternative (an extracted `NavigationKeyLedger` type) is argued down in the spec on minimum-scope grounds. |
| Reusability | PASS | No duplication introduced. |
| Extensibility | PASS | No public surface change; `IQfcCollectionController` untouched. |
| Separation of concerns | PASS | The ledger is controller-scoped private state; no I/O added. |
| Bugfix Workflow: failing test first | PASS | `evidence/regression-testing/p1-t4-expect-fail.2026-08-29T08-15.md` records T1 failing against unmodified production code before the fix. |
| Bugfix Workflow: minimal targeted fix | PASS | Net +9 lines on one production file, confined to the private field block and three navigation members. |
| Bugfix Workflow: deeper problems go to a new issue | PASS | The `KbdActions` `Add`/`Remove` versus `Find` semantic asymmetry, the 39 discarded `bool` returns, and the 500-line split are all deferred by name in the spec's Out-of-scope section. |
| Toolchain loop, in order, restarted on any rewrite | PASS | The Phase 4 loop was genuinely restarted from `[P4-T1]` after `[P4-T8]` found a +15 net-line overrun; the restart is recorded in `evidence/qa-gates/p4-t1-csharpier-format.2026-08-29T08-15.md`. |
| Error handling: fail fast | PASS | No exception is swallowed. `KbdActions.Add` still throws `ArgumentException` on a duplicate; recording after `Add` keeps the ledger clean on that throw. |
| Logging | PASS | No logging added or removed; none needed. |
| Naming | PASS | `_registeredNavigationKeys` / `RegisteredNavigationKeys` follow the file's existing `_camelCase` field and `PascalCase` accessor convention. |
| Comment "why, not what" | PASS on production code; see CR-1 on one test comment | The two production comments state the reason (orphan prevention; duplicate-`Add` safety), not the mechanism. |
| 500-line file limit | FAIL (pre-existing, PA-2) | `QuickFiler/Controllers/QfcCollectionController.cs` is 2446 lines (`awk 'END{print NR}'`), against 2437 at the anchor. Pre-existing 4.9x violation; the change adds 9 lines to it. |
| 500-line limit, test files | PASS | `QfcCollectionControllerTests.cs` 499 (was 500), `QfcCollectionControllerNavigationDigitsTests.cs` 226 (unchanged), `QfcCollectionControllerDefects468Tests.cs` 498, new `QfcCollectionControllerNavigationLedgerTests.cs` 361. Measured with `awk 'END{print NR}'` at head. |
| No new dependency | PASS | `QuickFiler.Test.csproj` gains one `Compile Include` item and nothing else. `QuickFiler/QuickFiler.csproj` is untouched. |
| I/O boundaries | PASS | No I/O added. |

---

## 3. Language-Specific Code Change Policy Compliance (C#)

Source: `CLAUDE.md` sections C#1-C#7.

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier formatting, pinned via `dotnet tool run` | PASS (from recorded evidence) | `evidence/qa-gates/p4-t2-csharpier-check.2026-08-29T08-15.md` records `dotnet tool run csharpier check .` over 1562 files with `EXIT_CODE: 0`. Independent re-run in the review worktree was not possible: `dotnet tool run csharpier check .` returns "The repo-local .NET SDK is missing", because this review worktree has no provisioned `.dotnet-sdk`. |
| `/t:Rebuild`, not `/t:Build`, for the analyzer gate | PASS | The recorded command in `evidence/qa-gates/p4-t3-analyzer-build.2026-08-29T08-15.md` uses `/t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, `EXIT_CODE: 0`, 0 errors and 5 warnings, matching the `[P0-T9]` baseline. |
| Nullable / type-check gate, no `/p:Nullable=enable`, `/t:Rebuild` | PASS | `evidence/qa-gates/p4-t4-nullable-build.2026-08-29T08-15.md`, `EXIT_CODE: 0`, 0 errors and 5 warnings, identical to the `[P0-T10]` baseline. No `CS0414` diagnostic. |
| No `dotnet format` used | PASS | No evidence artifact records a `dotnet format` invocation. |
| Strong contracts, explicit types at public boundaries | PASS | The ledger field and accessor are `private`; the tuple element names `(string SourceId, string Key)` are explicit on both the field and the accessor return type. |
| net481 language constraints respected | PASS | A `List<(string, string)>` value tuple is used; no `init`, `record`, or `record struct`. `QuickFiler.csproj` carries `<Reference Include="System.ValueTuple" />` and `<LangVersion>preview</LangVersion>`. `??=` is already used twice in the same file. |
| Prefer `internal`/`private` for non-public API | PASS | Nothing is promoted to `public`. |
| Comments synchronised with behaviour | PARTIAL (CR-1, CR-2) | The two edited files carry synchronised comments, and the `#468` defects file's comment correction is exactly what `[P4-T9]` verified as comment-and-string-literal-only. One XML doc in `QfcCollectionControllerNavigationDigitsTests.cs` was not updated; see the code review. |

### `_registeredDigits` removal and the CS0414 argument (AC-12)

Verified independently. `grep -rn "_registeredDigits" --include=*.cs .` returns zero occurrences
across the repository; the identifier survives only in Markdown documents (this feature folder, the
#444 feature folder, and the research artifact). The spec's claim that deleting only the `format`
expression would leave an assigned-and-never-read private field, and therefore `CS0414` under
`/p:TreatWarningsAsErrors=true`, is a correct reading of the language and of the recorded gate
command. The three deletions are correctly treated as indivisible.

---

## 4. Language-Specific Unit Test Policy Compliance (C#)

Source: `CLAUDE.md` sections CUT1-CUT3.

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest, not xUnit or NUnit | PASS | `using Microsoft.VisualStudio.TestTools.UnitTesting;` with `[TestClass]` / `[TestMethod]` throughout the new file. |
| Moq for mocking | PASS | `Mock<OutlookMailItem>`, `Mock<IQfcItemController>`, `Mock<IQfcKeyboardHandler>`, all `MockBehavior.Loose`. |
| FluentAssertions for assertions | PASS | Every assertion in the new file is a FluentAssertions `.Should()` chain. No bare MSTest `Assert` call appears. |
| Toolchain command selection | PASS | The four recorded Phase 4 commands are exactly the CUT3 forms. |
| New test file compiled | PASS | `QuickFiler.Test.csproj` line 133 adds `<Compile Include="Controllers\QfcCollectionControllerNavigationLedgerTests.cs" />` inside the existing `Controllers\QfcCollectionController*` block. All six tests appear as executed `Passed` results in the `[P4-T5]` TRX, which is the detection mechanism for a missing entry in a legacy non-SDK project. |

---

## 5. Test Coverage Detail

### C# (`.cs`) — the only coverage language with changed files on this branch

C# coverage row, repository-wide line coverage: **FAIL** — the canonical artifact
`artifacts/csharp/coverage.xml` is absent from the review worktree, and coverage verification is
mandatory for every language with changed files. Disposition below.

C# coverage row, branch coverage: **FAIL** — same artifact absence; no branch figure can be read
from a canonical artifact at review time.

Verification:

```
$ ls -la artifacts/csharp/
(no such directory)
$ ls -la coverage/
.gitkeep only
```

`coverage/*` is matched by `.gitignore` line 144, so the Cobertura document produced during
execution does not survive into the committed tree, and no Cobertura or JaCoCo document was committed
under the feature folder's `evidence/` tree (`find <feature> -name "*.xml" -o -name "*.trx" -o -name
"*.info"` returns nothing).

**Disposition of the two FAIL rows above: procedural, non-blocking.** The rows are recorded FAIL
because the artifact-absence rule is unconditional and this review will not soften it. They are
dispositioned non-blocking because the figures they would carry are recorded in committed evidence,
were re-derived arithmetically by this review, and clear both floors with a wide margin:

| Figure | Source | Value | Floor | Margin |
|---|---|---|---|---|
| Repository line rate, pre-change baseline (run A) | `evidence/baseline/p0-t12-coverage-baseline...md` | 0.853303 (54800 / 64221) | 0.85 | +0.33 pt |
| Repository line rate, post-change run E | `evidence/qa-gates/p4-t6-coverage-final...md` | 0.853194 (54793 / 64221) | 0.85 | +0.32 pt |
| Repository line rate, post-change run F, byte-identical tree | same artifact | 0.853475 (54811 / 64221) | 0.85 | +0.35 pt |
| Repository branch rate, run E | same artifact | 0.792927 | 0.75 | +4.29 pt |

Arithmetic re-derived by this review rather than accepted:

```
$ python -c "
b=54800/64221; e=54793/64221; f=54811/64221
print('A %.6f %.4f%%'%(b,b*100)); print('E %.6f %.4f%%'%(e,e*100)); print('F %.6f %.4f%%'%(f,f*100))
print('E-A pts %.4f'%((e-b)*100)); print('F-A pts %.4f'%((f-b)*100)); print('F-E pts %.4f'%((f-e)*100))
print('margin E to 85 floor pts %.4f'%((e*100)-85)); print('noise as lines %.1f'%((f-e)*64221))"
A 0.853303 85.3303%
E 0.853194 85.3194%
F 0.853475 85.3475%
E-A pts -0.0109
F-A pts 0.0171
F-E pts 0.0280
margin E to 85 floor pts 0.3194
noise as lines 18.0
```

Every recorded figure reproduces. The referral artifact's "0.0172" for F minus A is a rounding of
0.0171 and is immaterial.

**Absolute floors, decidable at this resolution: both met on both final-state runs.** The distance
from either final-state run to the 85% line floor is roughly eleven times the instrument's measured
run-to-run spread, so the absolute gate — the gate `.claude/rules/quality-tiers.md` actually states —
is decided, and it passes.

### New-code and modified-file tiers

- **New file** — `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs` is
  test code, correctly outside the production denominator under the Coverage Exclusion Policy's
  permitted test-file exclusion.
- **Modified production file** — `QuickFiler/Controllers/QfcCollectionController.cs` is the only
  changed production file. It carries a pre-existing `[ExcludeFromCodeCoverage]` and appears in 0 of
  the 558 `<class>` entries of the post-processed document. Its changed lines are therefore not in
  the denominator and have no per-file percentage to compare. `lines-valid` is invariant at 64221
  across runs A, E and F, which corroborates that an 18-insertion / 9-deletion edit changed no
  instrumented size — the expected signature of an excluded file.
- **Changed-lines no-regression** — satisfied on its own terms: the changed lines carry no measured
  coverage that could regress, no test was removed, six were added, and every pre-existing test in
  the touched assembly still passes.

### PowerShell, Python, TypeScript

Zero changed `.ps1`, `.psm1`, `.py`, `.ts`, and `.tsx` files on this branch. Verified against the
full branch-versus-base file list reproduced under "Rejected Scope Narrowing". No coverage verdict
is required for these languages, and none is asserted.

### AC-16 adjudication (independent)

Recorded in full in the feature audit under AC-16. Summary of the verdict reached here:

- **Clause 2 — PASS.** The changed production lines live in an `[ExcludeFromCodeCoverage]` class and
  that fact is stated explicitly in the coverage evidence artifact at lines 127-138 and 181-188.
  Verified independently, including the provenance check in section 1 that the exclusion is
  pre-existing.
- **Clause 1 — not verified, and not verifiable.** The instrument the clause names produces no
  percentage, and the substitute instrument's run-to-run spread on a byte-identical tree
  (0.0280 points, 18 covered lines) is 2.6 times the shortfall it was asked to adjudicate
  (0.0109 points, 7 covered lines), with the two final-state runs landing on opposite sides of the
  baseline.
- **Overall AC-16 verdict: PARTIAL.** Left unchecked in `spec.md`.
- **Residual risk: acceptable for merge.** Bounded at roughly 20 covered lines out of 64221, against
  an absolute-floor margin of 0.32 points that is decided and passing, a clean 6876 / 6876
  repository-wide test result, and a change whose sole production file is not measured at all.

The claim "the changed file is excluded from coverage measurement, therefore this change cannot move
the repository figure" is **sound for direct movement and overstated for total movement**. See
finding PA-4.

---

## 6. Test Execution Metrics

| Measure | Value | Source |
|---|---|---|
| Touched assembly, final gate | 1254 total, 1254 passed, 0 failed, 0 errored, 0 aborted; `EXIT_CODE: 0` | `evidence/qa-gates/p4-t5-vstest-final.2026-08-29T08-15.md` |
| Touched assembly, `[P0-T11]` baseline | 1248 total, 1248 passed, 0 failed | `evidence/baseline/p0-t11-vstest-baseline.2026-08-29T08-15.md` |
| Delta | +6 total, +6 passed, 0 failed | Exactly the six new ledger tests; no pre-existing test lost or regressed |
| Repository-wide, coverage run E | 6876 / 6876 passed, 0 failed, 1.1 min | `evidence/qa-gates/p4-t6-coverage-final.2026-08-29T08-15.md` |
| Red-before-green | T1 recorded failing against unmodified production code | `evidence/regression-testing/p1-t4-expect-fail.2026-08-29T08-15.md` |

Runs C and D of the coverage harness failed with 14 and 13 timeout failures under measured machine
contention (17 concurrent `MSBuild` processes, 8.1 minutes against a 57-second norm). The executor
retained that record rather than deleting it, re-censused the machine, and re-ran to a clean 6876 /
6876. The contention diagnosis is supported by the census delta rather than asserted. This review
accepts it: the failing set disappeared entirely with no source change, which is the same-commit
differing-outcome signature of an environment flake.

---

## 7. Code Quality Checks

| Check | Command | Result | Verdict |
|---|---|---|---|
| Format | `dotnet tool run csharpier format .` | exit 0; 1562 files processed; no file rewritten, established by an unchanged file mtime across a second invocation rather than inferred from stdout | PASS |
| Format verify | `dotnet tool run csharpier check .` | exit 0; 1562 files checked; no unformatted file named | PASS |
| Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | exit 0; 0 errors, 5 warnings; equal to the `[P0-T9]` baseline | PASS |
| Type check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | exit 0; 0 errors, 5 warnings; equal to the `[P0-T10]` baseline; no `CS0414` | PASS |
| Test | `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"` | exit 0; 1254 / 1254 passed | PASS |
| Tone policy | scan for hyperbole and emoji tokens across the feature folder and the two authored code files | zero matches | PASS |

The runner used was `Common7\IDE\Extensions\TestPlatform\vstest.console.exe`, which is the binary that
carries the binding redirects; the recorded run does not exhibit the spurious Moq failure signature
associated with the TestWindow binary.

**Toolchain gates were not re-executed by this review.** The review worktree has no provisioned
repo-local .NET SDK (`dotnet tool run csharpier check .` returns "The repo-local .NET SDK is
missing"), and the review contract directs verification from recorded evidence rather than
re-running. Each verdict above is traceable to a named artifact with a recorded exit code.

---

## 8. Gaps and Exceptions

Seven findings, all non-blocking. Blocking count: 0.

### PA-1 — `[ExcludeFromCodeCoverage]` on a production class conflicts with the Coverage Exclusion Policy (Non-blocking, pre-existing)

- **Location:** `QuickFiler/Controllers/QfcCollectionController.cs` line 21.
- **Rule:** `.claude/rules/general-unit-test.md`, Coverage Exclusion Policy: "No production file may
  be excluded from coverage measurement."
- **Standing:** Pre-existing and unchanged by this branch, proved by the three commands in section 1.
  `CLAUDE.md`'s COM/VSTO/WinForms exemption, which is maintainer-ratified and tracked in
  `feature/csharp-coverage-uplift`, authorises `[ExcludeFromCodeCoverage]` for Outlook Interop event
  handler classes in `QuickFiler` that depend on `MailItem` / `MAPIFolder` without an injectable
  seam. `QfcCollectionController` is within that class of type. The two policy texts are in tension;
  that tension is a repository-level question, not a defect in this change.
- **Why non-blocking:** Not introduced, not widened, and explicitly recorded in the spec's
  "Follow-up work deliberately not done here" list.

### PA-2 — production file exceeds the 500-line ceiling (Non-blocking, pre-existing)

- **Location:** `QuickFiler/Controllers/QfcCollectionController.cs`, 2446 lines at head.
- **Rule:** `.claude/rules/general-code-change.md`, "No production code, test code, or reusable
  script file may exceed 500 lines."
- **Verification:** `awk 'END{print NR}' QuickFiler/Controllers/QfcCollectionController.cs` returns
  `2446`; the anchor value recorded by `[P0-T7]` is 2437. Net growth +9, inside AC-14's bound of 10.
- **Why non-blocking:** The violation is pre-existing at 4.9x the ceiling; the `CLAUDE.md` Bugfix
  Workflow directs a split to a separate issue rather than into a bugfix's scope, and the spec's
  Out-of-scope section names it. Adding 9 lines does not create the violation.

### PA-3 — canonical C# coverage artifact absent at review time (Non-blocking, procedural)

- **Location:** `artifacts/csharp/coverage.xml` does not exist; `coverage/` holds only `.gitkeep`.
- **Rule:** mandatory coverage verification for every language with changed files.
- **Verification:** `ls -la artifacts/csharp/` (no such directory); `ls -la coverage/` (`.gitkeep`
  only); `find <feature folder> -name "*.xml" -o -name "*.trx" -o -name "*.info"` (empty).
- **Why non-blocking:** The Cobertura document was produced during execution and its root-level
  figures are recorded verbatim in `evidence/qa-gates/p4-t6-coverage-final.2026-08-29T08-15.md` with
  the covered and valid line counts, so the figures are checkable arithmetically and were checked;
  `coverage/*` is gitignored, so the document could not have been committed without a deliberate
  copy. The recorded figures clear both floors with wide margins.
- **Recommendation:** copy the post-processed Cobertura document into the feature folder's
  `evidence/coverage/` subfolder on future runs so a later reviewer can re-parse it directly.

### PA-4 — one sentence in the coverage evidence overstates the exclusion argument (Non-blocking)

- **Location:** `evidence/qa-gates/p4-t6-coverage-final.2026-08-29T08-15.md` lines 136-138: "There is
  no path by which this change adds or removes coverage from any production file at all."
- **Why it is wrong:** `[ExcludeFromCodeCoverage]` removes the annotated type's own lines from the
  metric. It does not remove that type's effect on the executed-line set of other, measured types.
  `UnregisterNavigation` now calls `KbdActions<string, KaStringAsync, Func<string, Task>>.Remove` a
  different number of times and with different arguments, and the six new tests exercise
  `KbdActions.Add`, `Remove`, and its enumeration surface. Neither collaborator is excluded:

  ```
  $ grep -n "ExcludeFromCodeCoverage" QuickFiler/Controllers/KbdActions.cs QuickFiler/Controllers/KaStringAsync.cs
  (no match; grep exit 1)
  ```

  Both are therefore in the denominator and their measured coverage can move because of this change.
  The direction is most likely upward, since six new tests execute them.
- **Why non-blocking:** The same run's referral artifact states the correct, weaker form —
  "This is corroboration, not proof of the clause" — and the adjudication does not rest on the
  overstated sentence. The overstatement affects the strength of one argument, not the verdict.
- **Recommendation:** the correct claim is that this change cannot move the figure *through the
  excluded file*, which is what `lines-valid` invariance at 64221 actually demonstrates.

### PA-5 — a superseded sentence remains in a recorded run artifact (Non-blocking; disposition endorsed)

- **Location:** `evidence/qa-gates/p4-t6-coverage-final.2026-08-29T08-15.md` line 205: "AC-16 is
  checked off under this adjudication and is flagged as such in the `[P5-T19]` AC status summary."
- **Assessment of the correct-forward decision: sound, and the right call.** A recorded run artifact
  is an audit record of what a run concluded at the time. Rewriting it would erase the fact that the
  run concluded something the referral later withdrew, which is exactly the information a later
  reader needs to judge whether the referral was a genuine escalation or a retrofit. Correcting
  forward preserves that.
- **Assessment of the signposting: adequate but one-directional.** The referral at
  `evidence/qa-gates/p5-t17-ac16-referral.2026-08-29T08-15.md` lines 84-93 names the file, the exact
  line number, and declares the sentence superseded. A reader arriving at `p4-t6` alone gets no
  pointer forward. Three facts stop that from misleading: the artifact's own title reads "PROCEEDING
  UNDER RECORDED ORCHESTRATOR ADJUDICATION"; its line 28 states "It is a documented deviation from
  the task's literal `>=` clause, not a satisfaction of it. Feature review adjudicates it
  independently"; and `spec.md`, which is the authoritative AC source, shows AC-16 as `- [ ]`. The
  stale sentence contradicts its own file three lines earlier and contradicts the source of truth, so
  it cannot survive a careful read.
- **Recommendation, non-blocking:** append — do not edit — a one-line "SUPERSEDED BY
  `p5-t17-ac16-referral.2026-08-29T08-15.md`" footer at the end of `p4-t6`. An append adds to the
  record without rewriting it, and closes the back-pointer gap.

### PA-6 — `[P4-T8]` was checked off although its literal sixth clause did not hold (Non-blocking)

- **Location:** `plan.2026-08-29T07-42.md` `[P4-T8]`, checked `[x]`; deviation recorded at
  `evidence/qa-gates/p4-t8-footprint.2026-08-29T08-15.md` lines 121-157.
- **Assessment of the substance: sound.** The clause's enumeration of four admissible feature-folder
  paths rests on a stated factual premise in the plan's own supporting prose — "The evidence
  artifacts this plan writes under the feature folder are untracked and unstaged at this point and
  are correctly absent from the listing." That premise was falsified by the run being a resume: an
  earlier segment had already committed Phases 0 through `[P4-T7]` with their evidence. The premise
  failed, not the property the clause guards. The plan states that property explicitly at line 176:
  the repository-wide span exists because "a rewrite made anywhere else in the repository by
  `[P4-T1]`'s `dotnet tool run csharpier format .` is invisible" to the three pathspec-scoped spans.
  That hazard is measurably absent, and this review verified it independently against the resolved
  base branch, not merely against the anchor: the full branch diff is the six code paths plus this
  feature folder and nothing else. `.csharpierignore` additionally excludes `**/evidence/**`, so the
  formatter could not have authored any of the 35 extra paths.
- **Assessment of not rewriting the clause: correct.** The plan's own instruction was to "record it
  in the artifact and report it to the orchestrator rather than widening this acceptance". Editing
  the acceptance text after seeing the result is retroactive acceptance-widening and would have been
  the worse error.
- **The residual defect is bookkeeping, not substance.** The plan routes an out-of-enumeration path
  to `REMEDIATION-REQUIRED`; the executor recorded and reported it, and then also checked the task
  `[x]`. A `[x]` asserts the acceptance held, which it did not. The internally consistent handling
  is the one this same run applied to AC-16: leave the box unchecked, escalate, and let the reviewer
  adjudicate. `[P4-T8]` and AC-16 were treated differently under materially similar circumstances.
- **Why non-blocking:** the deviation is disclosed verbatim under its own heading, is discoverable,
  and the guarded property is independently verified to hold.

### PA-7 — an absolute host path with the account name is committed in a research artifact (Non-blocking)

- **Location:** `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/research/research.2026-08-29T07-55.md`
  line 5.
- **Content:** `- Worktree: \`<repo-root>/.claude/worktrees/<agent-worktree>\``
- **Verification:** a case-insensitive search for the account name and the account's mail local-part returns that
  single line; the same scan over the rest of the feature folder returns nothing.
- **Standing:** committed on this branch (added by `28ee4720`, the preparation commit) and therefore
  part of the branch-versus-base diff. This run's other artifacts redact correctly — `p4-t5` writes
  `<account>_<HOST>` and `p4-t6` writes `<repo-root>` — so the leak is an isolated miss by the
  research agent, not a convention gap. The repository already tracks this leak class as issue #685.
- **Why non-blocking:** a documentation artifact, no code or credential exposure.
- **Recommendation:** redact to `<repo-root>/.claude/worktrees/<agent-worktree>` before merge.
  Redacting after merge leaves the string in history.

---

## 9. Summary of Changes

Production, one file, net +9 lines:

- `QuickFiler/Controllers/QfcCollectionController.cs`
  - Replaced `private int _registeredDigits;` and its `// Issue #472:` comment with
    `private List<(string SourceId, string Key)> _registeredNavigationKeys;` and a lazy
    `RegisteredNavigationKeys => _registeredNavigationKeys ??= new List<(string SourceId, string Key)>();`
    accessor.
  - `RegisterNavigation()` lost the `_registeredDigits = digits;` assignment; its `SetVisualDigits`
    and `_digitRefreshNeeded` behaviour is untouched.
  - `RegisterNavigationAsyncAction(int, int)` now holds the constructed `KaStringAsync`, calls
    `Add(action)`, then appends `(action.SourceId, action.Key)` — strictly after the `Add`, so a
    duplicate-key `ArgumentException` leaves the ledger unpolluted.
  - `UnregisterNavigation()` replaced its `for (int i = 0; i < _itemGroups.Count; i++)` loop and its
    `var format = _registeredDigits == 2 ? "00" : "";` expression with a `foreach` over the ledger
    followed by `Clear()`. It no longer reads `_itemGroups`.

Test side, five files:

- `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs` — new, 361 lines,
  six `[TestMethod]`s (T1-T6), self-contained field setter, group builder, and controller factory.
- `QuickFiler.Test/QuickFiler.Test.csproj` — one `Compile Include` item.
- `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` — three arrangement changes from
  `SeedCollectionKey(...)` to `controller.RegisterNavigation()`; 500 to 499 lines; 13 `[TestMethod]`s
  preserved; `SeedCollectionKey` still used at line 414 and not dead; the `*Key 2 SourceId
  Collection*` assertion preserved verbatim at line 422.
- `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` — one assertion
  flipped from `.Equal(new[] { "10" }, ...)` to `.BeEmpty(...)` with a `because:` string naming #644;
  one XML-documentation paragraph rewritten; 226 lines and 3 `[TestMethod]`s unchanged.
- `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` — XML doc and one
  `because:` string corrected to attribute the `NullReferenceException` to
  `_itemGroups[selection - 1]` in `RemoveSpecificControlGroupAsync`; no assertion touched.

### The #472 supersession claim — verified, not accepted

The commit message asserts that deleting `_registeredDigits`, its assignment, and the derived format
expression "is a supersession of #472, not a revert of it". This review verified the claim against
the code rather than the prose.

`git show 9494ca35 -- QuickFiler/Controllers/QfcCollectionController.cs` shows what #472 actually
did: before #472, `UnregisterNavigation` branched on the **live** `Digits` property inside the loop
(`if (Digits == 1) Remove(..., (i+1).ToString()); else Remove(..., (i+1).ToString("00"));`). #472
replaced that with a width recorded at registration time. Its guarantee is therefore "unregistration
removes keys at the width they were registered at", and the defect it closed is the live-property
re-read.

A revert of #472 would restore the live-`Digits` branch. This change does not. It replaces the
recorded *width* with the recorded *key strings*, which subsumes the width: a verbatim replay of the
stored `KaStringAsync.Key` cannot reconstruct a wrong width, because it does not reconstruct
anything. The claim is sound.

The decisive empirical check is that #472's mirror-direction regression test survives unchanged and
passes: `UnregisterNavigation_AfterRegisteringAtOneDigitAndGrowingToTen_RemovesTheOneDigitKeys`
registers nine keys at width 1, grows the page past the two-digit boundary without an intervening
unregister so the live `Digits` getter now computes 2, unregisters, and asserts the registry is
empty. That test fails under a revert of #472 and passes here, recorded `Passed` in the `[P4-T5]`
TRX. The #472 guarantee is demonstrably retained, not withdrawn.

The mandatory-deletion argument also holds: retaining `_registeredDigits` while deleting only the
format expression leaves a private field assigned and never read, which is `CS0414`, which
`/p:TreatWarningsAsErrors=true` promotes to an error. The three deletions are correctly indivisible.

---

## 10. Compliance Verdict

**PASS. Blocking findings: 0.**

| Area | Verdict |
|---|---|
| General Unit Test Policy | PASS |
| General Code Change Policy | PASS, with one pre-existing 500-line violation recorded (PA-2) |
| C# Code Change Policy | PASS |
| C# Unit Test Policy | PASS |
| Coverage verification, C# | FAIL rows recorded for canonical artifact absence (PA-3), dispositioned non-blocking; recorded figures clear the 85% line and 75% branch floors |
| Evidence location compliance | PASS |
| Scope containment | PASS |
| Tone policy | PASS |
| Acceptance criteria | 17 of 18 checked; AC-16 PARTIAL and left unchecked |

No remediation-inputs artifact is produced: there is no remediation-required finding.

Merge recommendation: **merge**, after the optional one-line redaction in PA-7. The AC-16 gap is a
defect in the criterion's construction, not in the change, and its residual risk is bounded and
acceptable.

---

## Appendix A: Test Inventory

New, in `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs`:

| T | Method | AC | Pre-fix | Post-fix |
|---|---|---|---|---|
| T1 | `UnregisterNavigation_AfterGroupRemovedThroughRemoveGroupByEntryIdSeam_RemovesEveryRegisteredKey` | AC-2 | Red, leaves `"10"` | Passed |
| T2 | `UnregisterNavigation_AfterUnbracketedItemGroupsRemoval_ThenReRegister_DoesNotThrow` | AC-3 | Red, `ArgumentException` | Passed |
| T3 | `RegisterAndUnregisterNavigation_RepeatedCycles_LeaveRegistryEmpty` | AC-5 | Green | Passed |
| T4 | `UnregisterNavigation_WithNoPriorRegistration_DoesNotThrowAndLeavesRegistryUnchanged` | AC-6 | Green | Passed |
| T5 | `UnregisterNavigation_AfterItemGroupsSetToNull_DoesNotThrow` | AC-7 | Red, `NullReferenceException` | Passed |
| T6 | `UnregisterNavigation_AfterTwoDigitRegistrationAndShrinkToNine_LeavesNoCollectionKeys` | AC-4 | Red, leaves `"10"` | Passed |

Amended or retained and verified passing:

| Method | File | Change |
|---|---|---|
| `LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix` | `QfcCollectionControllerTests.cs` | one arrangement line; assertion preserved |
| `LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys` | `QfcCollectionControllerTests.cs` | two arrangement lines collapse to one |
| `SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey` | `QfcCollectionControllerTests.cs` | one arrangement line |
| `RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException` | `QfcCollectionControllerTests.cs` | unchanged; pins the record-after-`Add` ordering |
| `UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys` | `QfcCollectionControllerNavigationDigitsTests.cs` | assertion flipped to `.BeEmpty(...)`; doc rewritten |
| `UnregisterNavigation_AfterRegisteringAtOneDigitAndGrowingToTen_RemovesTheOneDigitKeys` | `QfcCollectionControllerNavigationDigitsTests.cs` | unchanged; the #472 supersession proof |
| `RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter` | `QfcCollectionControllerDefects468Tests.cs` | comment and `because:` string only |

---

## Appendix B: Toolchain Commands Reference

```
dotnet tool restore
dotnet tool run csharpier format .
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"
pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\coverage.cobertura.xml
```

Review-side verification commands used in this audit:

```
git diff --name-only fa2ddefacf2c08abe18f3e3250d77da804534637 HEAD
git diff --name-only e968a1a8804b7641380d4489c496662824d45767 HEAD -- .claude/agent-memory
git diff --numstat e968a1a8804b7641380d4489c496662824d45767 -- QuickFiler QuickFiler.Test
git show e968a1a8804b7641380d4489c496662824d45767:QuickFiler/Controllers/QfcCollectionController.cs | grep -n "ExcludeFromCodeCoverage"
git show 9494ca35 -- QuickFiler/Controllers/QfcCollectionController.cs
grep -rn "_registeredDigits" --include=*.cs .
grep -n "ExcludeFromCodeCoverage" QuickFiler/Controllers/KbdActions.cs QuickFiler/Controllers/KaStringAsync.cs
awk 'END{print NR}' <each changed file>
```
