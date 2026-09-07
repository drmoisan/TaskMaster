# Policy Audit — issue 662 (efcselectionguard-banner-prefix-arity-and-stale-comment)

Timestamp: 2026-09-01T17-17
Reviewer: feature-review agent
Feature folder: `docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662`

## Scope Resolution

| Item | Value | How derived |
|---|---|---|
| Head branch | `bug/efcselectionguard-banner-prefix-arity-and-stale-comment-662` | `git rev-parse --abbrev-ref HEAD` |
| Head commit | `8a40a587970f9143e15969e3e233be7dd6b62114` | `git rev-parse HEAD` |
| Base branch | `origin/main` | `pr-base-branch-merge-base` |
| Base commit | `43dcc800e5c75ab1d1033f0eac0e4b61ac919b59` | `git rev-parse origin/main` |
| Merge base | `43dcc800e5c75ab1d1033f0eac0e4b61ac919b59` | `git merge-base origin/main HEAD` |
| Base is ancestor of HEAD | yes | merge base equals `origin/main` |
| Work mode | `minor-audit` | `- Work Mode: minor-audit` at `issue.md:3` |
| Acceptance-criteria source | `issue.md` `## Acceptance Criteria` only | work-mode routing |
| `spec.md` / `user-story.md` | absent, as required for `minor-audit` | directory listing |

The audit scope is the **full branch diff against `origin/main`**, not the plan's scope boundary.

### Full branch footprint (re-derived)

`git diff --name-only origin/main...HEAD | wc -l` returns **79** files.

| Area | Files | Note |
|---|---|---|
| `*.cs` | 4 | the production and test change |
| `docs/features/active/…-662/**` | 62 | issue, plan, research, evidence |
| `.claude/agent-memory/**` | 13 | agent memory index and entries |

`git diff --numstat origin/main...HEAD -- '*.cs'` returns exactly four files totalling
**57 insertions / 9 deletions**:

```
22	0	QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs
3	3	QuickFiler/Controllers/EfcFormController.cs
27	4	QuickFiler/Controllers/EfcSelectionGuard.cs
5	2	UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs
```

Note on a supplied figure: the delegation prompt stated "All 78 changed files". The measured
total is 79. The one-file difference is not material to any verdict; the code footprint (four
files, 57/9) reproduces exactly as stated.

### PR context artifacts

`artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` were absent at the
start of this review and were regenerated from `git diff --numstat origin/main...HEAD` in the
`- <path> (+N/-N)` line format. `artifacts/` is covered by `.gitignore:57`, so the
regeneration does not alter the branch footprint. `git status --porcelain` is empty after the
regeneration.

## Rejected Scope Narrowing

No instruction in the delegation prompt narrowed the audit to a plan, a task, a phase, a
subset of changed files, or a subset of languages. The full branch diff was audited and every
language with changed files carries an explicit verdict below.

Three delegation instructions were evaluated against the scope invariant and found **not** to
be scope narrowing. They are recorded verbatim for the record:

1. `"Do NOT report 'the arity still does not match' as a finding."`
   Assessment: this is a substantive correctness assertion about one candidate finding, not a
   restriction on which files, languages, or gates are audited. It was therefore not accepted
   on authority. The underlying question was re-derived independently from the dispatch path
   (see "Independent dispatch trace" below) and the reviewer reaches the same conclusion on
   the merits: the three-character value is load-bearing and widening it would be a
   behavioural regression. No finding is being suppressed.

2. `"Do NOT emit artifacts/csharp/coverage.xml."`
   Assessment: a constraint on which files this reviewer writes, not on whether coverage is
   verified. Coverage was verified in full from the two committed Cobertura documents under
   the canonical `<FEATURE>/evidence/` tree, and an explicit verdict is recorded for C# below.

3. `"FOLLOW-UPS: RECORD, DO NOT PROMOTE … do NOT open any issue from this branch."`
   Assessment: legitimate. This item carries footprint acceptance criteria (AC5b and AC7
   assert two named files are unmodified) and a four-file scope boundary; creating a potential
   entry or an issue from this branch would add files to the footprint the ACs measure. Every
   follow-up is recorded in full below with its reachability, and none is suppressed.

## Evidence Location Compliance

`validate_evidence_locations.py` does not exist in this repository; the enforcement mechanism
present is the `enforce-evidence-locations.ps1` PreToolUse hook. The scan was therefore
performed directly against the branch diff.

`git diff --name-only origin/main...HEAD` contains **zero** paths under `artifacts/baselines/`,
`artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/evidence/`,
`artifacts/coverage/`, `artifacts/regression-testing/` or `artifacts/post-change/`.

All 45 evidence files sit under canonical `<FEATURE>/evidence/<kind>/` sub-paths:

| Sub-path | Canonical | Files |
|---|---|---|
| `evidence/baseline/` | yes | 18 |
| `evidence/qa-gates/` | yes | 20 |
| `evidence/regression-testing/` | yes | 5 |
| `evidence/issue-updates/` | yes | 1 |
| `evidence/other/` | yes | 9 |

No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` was required. **Verdict: PASS.**

## Policy Compliance Verdicts

### 1. CLAUDE.md — C# toolchain, run in order, one clean pass

| Stage | Command recorded | Exit | Artifact | Verdict |
|---|---|---|---|---|
| Format | `dotnet tool run csharpier format .` | 0 | `evidence/qa-gates/csharpier-format.md` (15:59) | PASS |
| Format verify | `dotnet tool run csharpier check .` | 0 | `evidence/qa-gates/csharpier-check.md` (16:00) | PASS |
| Analyze | `MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 | `evidence/qa-gates/msbuild-analyzers.md` (16:01) | PASS |
| Type-check | `MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 | `evidence/qa-gates/msbuild-nullable.md` (16:02) | PASS |
| Test | `vstest.console.exe` over `QuickFiler.Test` and `UtilitiesCS.Test` | 0 | `evidence/qa-gates/vstest-*.md`, four TRX | PASS |

Two command-fidelity traps in CLAUDE.md were both avoided and the artifacts say so explicitly:

- `/t:Rebuild` was used, not `/t:Build`. A warm `/t:Build` skips `CoreCompile` on a
  command-line `/p:` change and would return exit 0 having run no analyzer and no nullable
  check, making both gates vacuous.
- `/p:Nullable=enable` was **not** added, matching `.github/workflows/ci.yml`.

Warning counts are 5 on both builds, identical to the Phase 0 baseline, and are the
pre-existing `System.Reactive.PackagesConfigCheck.targets` `packages.config` diagnostic
emitted once per affected project. Zero new diagnostics.

The nullable artifact records that a search of the transcript for `CS86` returned zero
matches. Both edited files that carry `#nullable enable` (`EfcSelectionGuard.cs:1`,
`FolderSuggestionTree.cs:1`) are therefore inside the per-file opt-in and produced no
nullable-flow diagnostic. **Verdict: PASS.**

The vstest binary used is
`Common7\IDE\Extensions\TestPlatform\vstest.console.exe` with `/Settings:TaskMaster.runsettings`
and `/InIsolation`. This is the binary that carries the assembly binding redirect; the
`Common7\IDE\CommonExtensions\Microsoft\TestWindow` binary does not and would produce spurious
Moq failures. Correct selection.

### 2. General Code Change Policy

| Rule | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | The change removes a declaration and renames a constant; it adds no indirection. |
| Reusability — no copy-paste | PASS | The duplicated four-character literal in `FolderSuggestionTree.cs:16` is deleted and its single reader now reads `BreadcrumbRowBuilder.BannerPrefix`. Declaration inventory falls 3 -> 2. |
| Separation of concerns | PASS | No layering change. |
| Error handling / fail fast | PASS | No error-handling surface touched. |
| Public API stability | PASS | `BannerRejectionPrefix` is `private const`; `FolderSuggestionTree.BannerPrefix` was `private const`. No public member changed. |
| Comment **why**, not what | PASS | The new XML doc block and the replaced `SelectedFolder` comment both state the reason for the arity difference and name the test that pins it. |
| File size <= 500 lines | PARTIAL (non-blocking) | See finding F-4. |
| Dependencies | PASS | No package or project reference added. `FolderSuggestionTree` and `BreadcrumbRowBuilder` are in the same assembly. |
| No temp files | PASS | No filesystem use introduced. |

### 3. Bugfix Workflow (`Type: bug`)

CLAUDE.md's bugfix workflow requires a failing regression test first. The branch carries a
formal fail-before exception dossier at
`evidence/regression-testing/fail-before-exception.2026-09-01T15-57.md`.

Reviewer assessment: the exception is **valid**, and the reviewer re-derived its central claim
rather than accepting it. The change is behaviour-preserving at every call site — the guard's
value stays `"==="` byte-for-byte and `FolderSuggestionTree.IsBanner` moves from a local
`"===="` to `BreadcrumbRowBuilder.BannerPrefix`, whose value is the identical `"===="`. No
predicate's return value changes for any input, so no test can fail before the edit and pass
after.

The dossier substitutes two proofs, both of which the reviewer independently confirmed:

- **Absence-of-test proof.** Before the change no test in `EfcSelectionGuardTests.cs` asserted
  a bare three-equals row against either predicate. Confirmed by diffing the TRX test-name
  sets: `BannerRejectionPrefix_RejectsThreeAndFourEqualsRowsOnBothPredicates` is present in
  `evidence/qa-gates/p2-t7/quickfiler-postchange.trx` and absent from
  `evidence/baseline/p0-t11/quickfiler-baseline.trx`, and it is the **only** difference between
  the two name multisets in either direction.
- **Prohibited-edit-is-detected proof.** Confirmed by the independent trace below.

**Verdict: PASS** (documented exception, independently corroborated).

#### Independent dispatch trace

Read from source rather than from the plan or the research document.

`QuickFiler/Controllers/EfcFormController.cs`:

- `:1143-1148` `IsBannerRow(row)` = `row is not null && row.StartsWith(BreadcrumbRowBuilder.BannerPrefix, Ordinal)` — the **four**-character producer constant.
- `:1151-1153` `IsSelectableFolder(f)` = `!IsBannerRow(f) && EfcSelectionGuard.IsValidCreationSelection(f)`.
- `:1155` `IsValidSelection` = `IsSelectableFolder(SelectedFolder)`.
- `:745-749` the filing site in `ActionOkAsync` = reject when `selectedFolder is null || IsBannerRow(selectedFolder) || !EfcSelectionGuard.IsValidFilingSelection(selectedFolder)`.

`QuickFiler/Controllers/EfcSelectionGuard.cs`: `BannerRejectionPrefix = "==="` (`:38`),
`MinimumCreationLength = 3` (`:45`), consumed at `:72` and `:98`.

Evaluating the row `"==="` as the code stands:

| Site | Term | Value |
|---|---|---|
| Filing | `IsBannerRow("===")` | false — `"==="` does not start with `"===="` |
| Filing | `IsValidFilingSelection("===")` | false — `!"===".StartsWith("===")` is false |
| Filing | net | rejected, **solely** by the three-character prefix |
| Creation | `!IsBannerRow("===")` | true |
| Creation | `Length >= MinimumCreationLength` | `3 >= 3` is true — the length rule rejects nothing |
| Creation | `!StartsWith("===")` | false |
| Creation | net | rejected, **solely** by the three-character prefix |

The three-character value is therefore the only rejection mechanism for a three-equals row at
either site, and widening it to `"===="` would flip both predicates to accept. This confirms
the direction the issue prohibits. The reviewer records no "arity mismatch" finding, on the
merits.

The reviewer also re-derived the subtlety in the pre-existing merged test
`EfcFormControllerTests.cs:453`. Under the prohibited widening, for the row `"==="`:
`creationPath` becomes true and `filingPath` becomes true, so `:462`
(`creationPath.Should().Be(filingPath, …)`) — the assertion that *reads* like the consistency
guard — still **passes**, and only `:463` (`creationPath.Should().BeFalse(…)`) fails. The new
test in `EfcSelectionGuardTests.cs` asserts `BeFalse` on all four inputs directly and so is
not subject to that blind spot.

### 4. General Unit Test Policy and C# Unit Test Policy

| Rule | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestMethod]` in a `[TestClass]`; no xUnit/NUnit introduced |
| FluentAssertions for assertions | PASS | all four assertions use `.Should().BeFalse(because)` |
| Moq | PASS | none needed; the unit under test is a pure static predicate |
| Independence | PASS | no shared or mutable state; static predicate with literal inputs |
| Isolation | PASS | one behaviour: the guard's rejection breadth on both predicates |
| Fast execution | PASS | `duration="00:00:00.0302844"` in `evidence/regression-testing/p2-t5/ac6-scoped.trx` |
| Determinism | PASS | no clock, no RNG, no `Thread.Sleep`/`Task.Delay`, no I/O, no filesystem |
| Arrange–Act–Assert | PASS | explicit `// Arrange` and `// Act / Assert` markers |
| Clear failure messages | PASS | every assertion carries a `because` naming the prohibited direction |
| Documents intent | PASS | five-line header comment states the pinned relationship and cites #662 |
| No external dependencies | PASS | none |
| No temporary files | PASS | none |
| Test file location | PASS | `QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs` mirrors `QuickFiler/Controllers/EfcSelectionGuard.cs` |
| Test file under 500 lines | PASS | 296 -> 318 lines |
| Coverage exclusion policy | PASS | `TaskMaster.runsettings` and `coverage.config` exclude only third-party `ModulePath` patterns (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest). No production source path is excluded. |

Scenario completeness: the added test covers the boundary pair `"==="` / `"===="` on both
predicates. Negative/null/whitespace flows for the same predicates already exist elsewhere in
the file. **PASS.**

## Coverage Verification

Languages with changed files in the branch diff: **C# only**. TypeScript, Python and PowerShell
have zero changed files on this branch, so no coverage obligation arises for them.

Coverage artifacts inspected (not regenerated):

- `docs/features/active/…-662/evidence/baseline/coverage-baseline.cobertura.xml`
- `docs/features/active/…-662/evidence/qa-gates/coverage-postchange.cobertura.xml`

Both parse as well-formed Cobertura (`<coverage>` root) and both record `PostProcessed: yes`
in their companion `.md`, meaning both denominators are the first-party package allowlist.
The two documents are therefore comparable. Per-file figures below were recomputed by the
reviewer directly from the `<line hits=…>` elements, not read from the executor's prose.

### C# coverage rows

| Row | Baseline | Post-change | Floor | Verdict |
|---|---|---|---|---|
| C# repo-wide line coverage | 85.3763% (54967/64382) | **85.3741%** (54969/64386) | >= 85% | **PASS** |
| C# repo-wide branch coverage | 79.3822% (13106/16510) | **79.3761%** (13105/16510) | >= 75% | **PASS** |
| C# changed-line coverage, all changed executable statements | — | **3/3 = 100%** | no regression | **PASS** |
| C# modified file `EfcSelectionGuard.cs` line coverage | 100.0000% | **100.0000%** | >= 85% | **PASS** |
| C# modified file `EfcSelectionGuard.cs` branch coverage | 100.0000% | **100.0000%** | >= 75% | **PASS** |
| C# modified file `FolderSuggestionTree.cs` line coverage | 98.4496% | **98.4962%** | >= 85% | **PASS** |
| C# modified file `FolderSuggestionTree.cs` branch coverage | 96.4286% | **96.4286%** | >= 75% | **PASS** |
| C# modified file `EfcFormController.cs` line coverage | 25.5008% (331/1298) | **25.5008%** (331/1298) | >= 85% | **FAIL**, dispositioned non-blocking — see F-5 |
| C# modified file `EfcFormController.cs` branch coverage | 31.5126% (75/238) | **31.5126%** (75/238) | >= 75% | **FAIL**, dispositioned non-blocking — see F-5 |
| C# new code files added by this feature | none | none | >= 85%/75% | **PASS** (vacuously; the branch adds no new file) |

Changed executable statements and their measured hit counts, read from the post-change
document:

| Statement | Location | Hits |
|---|---|---|
| `return !value.StartsWith(BannerRejectionPrefix, …)` in `IsValidFilingSelection` | `EfcSelectionGuard.cs:71-73` | 1 on every line |
| `return value.Length >= … && !value.StartsWith(BannerRejectionPrefix, …)` in `IsValidCreationSelection` | `EfcSelectionGuard.cs:96-99` | 1 on every line |
| `return row != null && row.StartsWith(BreadcrumbRowBuilder.BannerPrefix, …)` in `IsBanner` | `FolderSuggestionTree.cs:195-200` | 1 on every line |

`EfcFormController.cs` contributes no changed executable statement: its entire diff is a
three-line comment replacement, and its line and branch counters are byte-identical between
the two captures (331/1298 and 75/238). There is consequently **no regression on any changed
line**, which the uniform tier rule requires.

The repo-wide branch numerator falls by one (13106 -> 13105) between captures. That single
branch is **not** in any changed file — all three changed files carry identical per-file
branch counters across the two captures — so it is attributable to cross-session measurement
variance rather than to this change. The resulting -0.006 percentage-point movement leaves
branch coverage 4.38 points above the 75% floor.

Threshold note: `.claude/rules/quality-tiers.md` and `.claude/rules/general-unit-test.md`
state a uniform 85% line / 75% branch floor; `CLAUDE.md` still states 80% repo-wide and 90%
for new modules. This documentation conflict is unreconciled in the repository and is not
introduced by this branch. The stricter 85/75 pair was applied above and both repo-wide
figures clear it.

Canonical artifact note: `artifacts/csharp/coverage.xml` is not present in this worktree. The
committed Cobertura pair under `<FEATURE>/evidence/` is the coverage evidence of record for
this branch, is complete for both captures, and was parsed and recomputed by the reviewer. The
C# verdicts above are therefore evidence-backed and not artifact-absent.

## Artifact Hygiene

| Check | Result |
|---|---|
| Account name (`DanMoisan`) anywhere in the feature folder | 0 matches |
| User-profile path (`C:\Users\…`) anywhere in the feature folder | 0 matches |
| Machine name anywhere in the feature folder | 0 matches, per `evidence/qa-gates/scope-and-commit.md` `ResidualMatchCount=0`, re-confirmed by reviewer grep |
| XML-family evidence artifacts that parse | 8 of 8 |

The reviewer's grep covered `plan.md`, which the executor's own sweep excludes from its
residual check (see F-3). It is clean.

The absolute path `C:\Program Files\Microsoft Visual Studio\18\Community\…` appears in the
MSBuild and vstest command fields. That is a machine-agnostic product install path, not an
account or host identifier, and is permitted.

## Findings

Every finding below is **non-blocking**. Blocking findings (FAIL or blocking-PARTIAL): **0**.

### F-1 — `issue.md` AC5b and AC7 still cite the superseded diff anchor (Minor, latent, follow-up)

`issue.md:156` and `issue.md:170` verify AC5b and AC7 with
`git diff 2b85134b42872e405602e6064e02dc9cda6c319b --stat -- <file>`. The plan's
`## Execution Amendment` (commit `63ef2e8f`) replaced that anchor with a run-time
`git merge-base origin/main HEAD` for the corresponding plan tasks P2-T16, P2-T18 and P2-T23,
because the pinned anchor is an ancestor of `origin/main` and a two-dot diff from it reports
everything `origin/main` accumulated since. The amendment updated the **plan**; it did not
update the AC text in `issue.md`.

**Reachability: currently benign, and verified so.** The reviewer ran both AC commands
verbatim as written in `issue.md`; both return empty output, because `origin/main` has not
touched `BreadcrumbRowBuilder.cs` or `EfcFormControllerTests.cs` since `2b85134b`. Both ACs
also hold under the correct merge-base anchor. The stale anchor becomes a false pass only if
either file changes on `origin/main` in a way that happens to net out, or a false fail if
either changes at all. Not merge-method-dependent.

**Follow-up:** amend the AC text in `issue.md` to resolve the anchor at run time, matching the
plan.

### F-2 — Shared artifact-hygiene rule corrupts XML it redacts (Major, reached and repaired on this branch, follow-up)

The plan's Fail-Closed Evidence Rules define a hygiene sweep that substitutes four
angle-bracketed placeholder tokens (`<repo-root>`, `<user-profile>`, `<user>`, `<host>`) into
evidence files, and verifies its own work with `ResidualMatchCount=` alone. `vstest.console.exe`
writes host and account values into XML **attribute values** in the TRX header, where a raw
`<` is not legal. The rule as written is therefore guaranteed to make any TRX it redacts
not well-formed, and its verification cannot detect that because it measures only whether the
identifiers were removed, never whether the rewritten file still parses.

**Reachability: reached, not latent.** All six committed TRX files on this branch were left
not well-formed and failed to parse at line 2, column 57. This was found by the orchestrator
during post-execution verification, not by the executor's gate. Repaired in commit
`8a40a587` by XML-escaping the placeholder tokens, and recorded at
`evidence/qa-gates/trx-xml-wellformedness-repair.md`.

Reviewer verification of the repair, performed independently:

- All 8 XML-family evidence artifacts now parse under `xml.etree.ElementTree`.
- The repair commit's diff touches only the `name`, `runUser`, `runDeploymentRoot`,
  `computerName`, `storage`, `codeBase` and `StdOut` fields. No `<Counters>` element and no
  `outcome` attribute is altered by it.
- Redaction is unchanged: the parsed attribute value is still exactly the placeholder token,
  and a case-insensitive scan for the account name and machine name across the feature folder
  returns zero matches.

**Follow-up:** the defect is in the shared hygiene rule, not in the executor, which applied it
exactly as written. The rule should either use placeholder tokens without angle brackets, or
escape them when the target is XML, and its verification should additionally assert that every
rewritten XML-family file still parses. Reachability for future work: any plan reusing this
rule that redacts a `.trx`, `.xml`, `.coveragexml` or `.cobertura.xml` artifact.

### F-3 — Hygiene sweep cannot see the plan file, and detects more than it repairs (Minor, latent, follow-up)

Two structural gaps in the same sweep, recorded as one finding because they share a cause:

1. The residual check is `Get-ChildItem … | Where-Object { $_.FullName -ne (Resolve-Path $plan).Path } | …`. `plan.md` is excluded from the residual scan by construction, so a host path reintroduced into the plan is undetectable by this gate. Commit `db59adfe` ("remove the absolute host path from the plan") shows that a host path did reach `plan.md` on this branch at least once.
2. The rewrite loop iterates only `evidence/`, while the residual check scans the whole feature folder. A residual in `issue.md` or `research/` is therefore detected but never repaired, and the gate reports BLOCKED with no in-gate remedy.

**Reachability: documentation hygiene only, latent on this branch.** The reviewer grepped the
entire feature folder including `plan.md` for the account name, the user-profile path and
`C:\Users\`; zero matches. Not merge-method-dependent.

### F-4 — `EfcFormController.cs` exceeds the 500-line file limit (Informational, pre-existing, not worsened)

`.claude/rules/general-code-change.md` sets a hard 500-line limit for production files.
`QuickFiler/Controllers/EfcFormController.cs` is **1189** lines.

**Reachability: maintainability only; no runtime effect.** The baseline at `origin/main` is
also 1189 lines. This branch's entire change to that file is a three-line comment replacement
(+3/-3, net zero), so the violation is pre-existing and is not worsened. Not attributable to
this branch and not a merge blocker for it.

### F-5 — `EfcFormController.cs` per-file coverage is below the uniform floor (Informational, pre-existing, dispositioned non-blocking)

Line 25.5008%, branch 31.5126%, against uniform floors of 85% and 75%. Recorded as FAIL in the
coverage table above so the row is honest.

Disposition, non-blocking, on three independent grounds:

1. The file's change contains **zero** executable lines, so no changed line is uncovered.
2. Its coverage counters are byte-identical between the baseline and post-change captures
   (331/1298 lines, 75/238 branches), so there is no regression on any changed line.
3. The file is `using Microsoft.Office.Interop.Outlook;` (`:13`) and `using System.Windows.Forms;`
   (`:12`) bound, with `MessageBox.Show` and `_formViewer` calls on the paths in question. It
   sits squarely inside CLAUDE.md's ratified COM/VSTO/WinForms exemption class ("Outlook Interop
   event handler classes in … `QuickFiler` … that directly depend on
   `Microsoft.Office.Interop.Outlook.Application`, `MailItem`, `Store`, or `MAPIFolder` without
   an injectable seam").

**Reachability: none to product behaviour.** Requiring the floor here would mandate a
seam-extraction refactor of a 1189-line COM-bound controller, which is far outside a
`minor-audit` comment fix and would itself breach the four-file scope boundary.

### F-6 — `IsBanner` doc comment still restates the producers' arity as a literal (Minor, latent, follow-up)

`UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs:193` reads
`/// <summary>True when the row is a section/banner header (begins with <c>"===="</c>).</summary>`.
The body below it now reads `BreadcrumbRowBuilder.BannerPrefix` rather than a local literal, so
this doc line is the last place in the file where the four-character arity is restated as a
hard-coded literal. It is accurate today.

**Reachability: documentation only, latent.** It becomes wrong if `BreadcrumbRowBuilder.BannerPrefix`
ever changes value, which is exactly the drift class this issue exists to close.
`<see cref="BreadcrumbRowBuilder.BannerPrefix"/>` would remove the restatement. Not
merge-method-dependent.

### F-7 — Partially-qualified type reference in `FolderSuggestionTree.IsBanner` (Informational, no action)

`FolderSuggestionTree.cs:198` reads `OutlookObjects.Folder.BreadcrumbRowBuilder.BannerPrefix`
rather than adding `using UtilitiesCS.OutlookObjects.Folder;`. It resolves correctly: the file's
namespace is `UtilitiesCS`, so relative lookup finds `UtilitiesCS.OutlookObjects.Folder`. It
matches the fully-qualified style already at `EfcFormController.cs:1146`. Recorded only so a
later reader does not "simplify" it into an ambiguity. No action recommended.

### F-8 — Two 194k-line Cobertura documents are committed to history (Informational, merge-method-dependent, follow-up)

`evidence/baseline/coverage-baseline.cobertura.xml` (+193,931 lines) and
`evidence/qa-gates/coverage-postchange.cobertura.xml` (+193,939 lines) account for 388k of the
branch's 478k inserted lines. They are the primary evidence for the coverage verdicts above, so
their presence is defensible.

**Reachability: repository size only; no runtime or correctness effect. This is
merge-method-dependent.** Unlike issue #648, these files are committed and *retained* rather
than committed-then-removed, so there is no unreachable-blob problem and squash-merge is not
required as a remedy. Squash-merge would collapse them to a single commit but would not remove
them from `main`'s tree; they are permanent either way. **Follow-up:** decide as a policy
question whether full Cobertura documents belong in git or whether a per-file extract plus a
retained checksum would suffice for future audits.

### F-9 — Agent-memory index compaction verified non-destructive (Informational, closed)

`.claude/agent-memory/atomic-executor/MEMORY.md` shows +81/-137, a net loss of 56 lines, which
on its face reads like deletion of other work. The reviewer extracted the set of `.md` link
targets from both revisions:

- baseline: 150 links, 158 lines
- head: 152 links, 102 lines
- links present in baseline and absent from head: **none**
- links gained: `feedback_never_predict_an_observation_into_an_artifact.md`,
  `project_full_suite_run_hangs_while_earlier_runs_idle.md` (both added as files in this same
  branch)

The reduction is index consolidation — multiple entries merged onto shared lines — driven by a
repository hook requiring the index below a read limit, as `evidence/qa-gates/scope-and-commit.md`
states. No memory pointer was lost. **Reachability: none.** Closed, no action.

## Summary

| Category | Verdict |
|---|---|
| Scope resolution and footprint | PASS |
| Rejected scope narrowing | PASS (none detected; three instructions evaluated and recorded) |
| Evidence location compliance | PASS |
| C# toolchain, one clean pass in order | PASS |
| General Code Change Policy | PASS with one pre-existing PARTIAL (F-4) |
| Bugfix workflow / RED-first | PASS (documented exception, independently corroborated) |
| General + C# Unit Test Policy | PASS |
| Coverage exclusion policy | PASS |
| C# coverage | PASS repo-wide and on every changed line; one pre-existing per-file FAIL row dispositioned non-blocking (F-5) |
| Artifact hygiene | PASS |
| Tonality | PASS |

**Blocking findings: 0. Remediation inputs artifact: not produced (none required).**
