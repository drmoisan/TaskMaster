# Policy Audit — issue #440 (breadcrumb Left walk-to-root, Qfc)

- Timestamp: 2026-08-29T07-01
- Reviewer: feature-review agent
- Branch: `bug/breadcrumb-left-right-arrow-parent-child-navigation-440`
- Base ref: `b56400ab663a85b6039139d4548f408821e957ce` (`origin/main`)
- Head ref: `99767554243a7b99a71d2084823d29afcc7127ce`
- Work mode: `full-bug` (marker `- Work Mode: full-bug` read from `issue.md` line 12)
- Review cycle: 1
- Verdict: **PASS** — 0 blocking findings

## 1. Scope resolution

The audit scope is the full branch diff against the resolved base branch.

Base resolution was recomputed rather than accepted from the caller:

```
git merge-base HEAD b56400ab663a85b6039139d4548f408821e957ce
-> b56400ab663a85b6039139d4548f408821e957ce
```

The supplied base is the true merge base. 45 files change on the branch:

| Category | Count | Notes |
|---|---|---|
| C# production | 1 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` (+5/-5) |
| C# test | 2 | `BreadcrumbStateModelSequenceTests.cs` (+58/-1), `FolderBreadcrumbBridgeRouterTests.cs` (+6/-10) |
| Feature-folder docs and evidence | 36 | `issue.md`, `spec.md`, plan, research, 31 evidence artifacts |
| Agent-memory files | 6 | `.claude/agent-memory/{atomic-planner,prd-feature,task-researcher}/` |

### PR context artifacts

`artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` were **absent** at
review start. Both were regenerated in this worktree from the anchored branch diff before the audit
proceeded, in the `- <path> (+N/-N)` bullet form. The summary correctly classifies the three `.cs`
paths, so the C# language category is live for this review.

## 2. Rejected Scope Narrowing

The caller prompt was examined for attempted scope narrowing. One statement was treated
conservatively and is recorded here verbatim:

> "Everything else in the diff is this feature folder's own documentation and evidence, plus
> `.claude/agent-memory/` files that arrived on the two earlier preparation commits and are not part
> of this fix."

Justification for recording it: the six `.claude/agent-memory/` files are present in the branch diff
against the resolved base, so they are inside the audit scope regardless of which commit introduced
them. They were audited in full (content read and host-identity leak scan) and are clean. No audit
step was skipped on the strength of that statement.

Three further caller statements were evaluated and found **not** to be scope narrowing, because each
asserts a fact about pre-existing tree state rather than restricting the diff under audit. Each was
independently verified before acceptance:

1. The Right-descent commit asymmetry and the single-level Right descent limit are recorded as
   non-goals in `spec.md` lines 146-152. Verified: neither is introduced by this diff; the Right
   transition `TryRightTreeTransition` is untouched.
2. The analyzer version skew is pre-existing on the base ref. Verified: no `.csproj` and no
   `packages.config` appears in the branch diff. Recorded as follow-up observation OB-1.
3. `user-story.md` is absent by design under `full-bug`. Verified against
   `.claude/skills/acceptance-criteria-tracking/SKILL.md`; not reported as a finding.

No coverage check, toolchain check, or language category was skipped.

## 3. Evidence Location Compliance

Scan of the branch diff for files written under non-canonical evidence paths:

```
git diff --name-only <BASE> HEAD | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"
-> (no matches)
```

**PASS.** Every evidence artifact resolves under `<FEATURE>/evidence/<kind>/` with `<kind>` in
`baseline`, `regression-testing`, `qa-gates`, `issue-updates`, `other`. No artifact is written under
any `artifacts/` path.

`validate_evidence_locations.py` does not exist in this repository; the scan above was performed
directly against the anchored diff in its place. The three audit artifacts this review produces are
written under `<FEATURE>/evidence/qa-gates/`, which is canonical.

The plan records one override rejection, which is correct and is reproduced here:

```
EVIDENCE_LOCATION_OVERRIDE_REJECTED: <FEATURE>/evidence/coverage/ replaced with <FEATURE>/evidence/baseline/ (baseline coverage) and <FEATURE>/evidence/qa-gates/ (post-change coverage and the coverage delta)
```

`spec.md` AC-15 names `evidence/coverage/`, which is not a canonical kind. The executor rejected it
and wrote to canonical locations instead. That is the required behavior.

## 4. Artifact hygiene — host identity

Every changed file in the branch diff was scanned for user-profile paths, machine account names,
host names, and mailbox addresses:

```
grep -rniE "<user-profile-root-pattern>|<user>|<user-short>|<mailbox-local>@|@<mailbox-domain>" <all 45 changed files>
-> (no matches)

(The literal pattern used at review time named the real account and mailbox tokens; it is
reproduced here in redacted form so this artifact does not itself become a leak.)
```

**PASS.** This is notable because the class of leak recurs. The raw TRX documents, the raw Cobertura
documents and the msbuild file-logger output all embed absolute host paths and a machine account
name; all of them were kept under the gitignored `coverage/` tree and none was copied into the
feature folder. `git check-ignore` confirms `coverage/*` at `.gitignore:144` and `artifacts/` at
`.gitignore:57`. Scoped TRX result directories and explicit `LogFileName=` values prevented the
default account-and-host TRX naming.

## 5. Policy compliance

| Policy | Requirement | Verdict | Evidence |
|---|---|---|---|
| CLAUDE.md toolchain order | format, analyze, type-check, test, in order, single clean pass | PASS | Independently reproduced formatter and full suite; msbuild raw logs re-read (section 6) |
| CLAUDE.md C#1.1 | CSharpier via `dotnet tool run`, pinned version | PASS | `dotnet tool run csharpier check .` reproduced by the reviewer: exit 0, `Checked 1560 files in 4472ms.` |
| CLAUDE.md C#1.2 | analyzers with `/t:Rebuild`, not `/t:Build` | PASS | Raw log shows `CoreClean` and 36 `csc.exe` invocations; 0 `Skipping target "CoreCompile"` |
| CLAUDE.md C#1.3 | nullable via `/p:TreatWarningsAsErrors=true`, no `/p:Nullable=enable` | PASS | Command form matches `ci.yml`; changed production file carries `#nullable enable` at line 1, so the gate is non-vacuous for it |
| general-code-change: file size <= 500 lines | all owned files | PASS | 248 / 491 / 292 lines (`awk END{print NR}`) |
| general-code-change: simplicity first | prefer the simplest design that works | PASS | The fix deletes one conjunct; the root boundary is delegated to `ActivateSegment`, which already enforces it |
| general-code-change: fail fast | no silent error swallowing introduced | PASS | `LeftArrow()` performs no I/O, throws nothing, and its `false` return is the pre-existing unhandled-arrow signal |
| general-code-change: comment why not what | rewritten `#440` comment | PASS | The new 5-line block states why no index test is needed rather than restating the code |
| general-unit-test: framework | MSTest, Moq, FluentAssertions | PASS | Both new tests use `[TestMethod]` and FluentAssertions; the router seam uses `Mock<IFolderHierarchyProvider>(MockBehavior.Strict)` |
| general-unit-test: determinism | no temp files, no external services, no timers | PASS | Both new tests run entirely against an in-memory `BreadcrumbStateModel`; no `Thread.Sleep`, `Task.Delay`, or filesystem access |
| general-unit-test: AAA | Arrange-Act-Assert with labelled sections | PASS | Both new tests carry labelled `// Arrange` and `// Act + Assert` sections and an XML doc comment |
| general-unit-test: test file location | tests mirror production tree, not colocated | PASS | Both edited test files live under `UtilitiesCS.Test/OutlookObjects/Folder/`, mirroring `UtilitiesCS/OutlookObjects/Folder/` |
| general-unit-test: coverage exclusion policy | no production path excluded from measurement | PASS | No `coverage.config` change and no `[ExcludeFromCodeCoverage]` attribute is added by this diff |
| quality-tiers: uniform thresholds | line >= 85%, branch >= 75% | PASS | See section 6 |
| tonality | professional, no humor or hyperbole | PASS | Feature-folder prose reviewed; measured throughout |
| policy documents unmodified | no edit under `.claude/rules/` | PASS | The branch diff contains no `.claude/rules/` path |

### Toolchain non-vacuity, independently re-derived

Both msbuild gates were verified by re-reading the executor's raw file-logger output rather than by
accepting the summary artifact:

| Log | `Skipping target "CoreCompile"` | `(Rebuild target(s))` | `csc.exe` | Errors | Warnings |
|---|---|---|---|---|---|
| `coverage/logs/p4-t3-analyzer.msbuild.txt` | 0 | 40 | 36 | 0 | 5 |
| `coverage/logs/p4-t4-nullable.msbuild.txt` | 0 | 40 | 36 | 0 | 5 |

The `csc.exe` count is the reviewer's own addition to the executor's two counts. It is the direct
positive proof that compilation happened, which the two literal counts establish only indirectly.
Both logs also show `CoreClean` deleting build output, which is `/t:Rebuild` behavior.

The 5 warnings are the pre-existing System.Reactive 7.0.0 packages-config advisory, one each from
QuickFiler, TaskMaster, ToDoModel, UtilitiesCS and UtilitiesCS.Test. They are emitted by an MSBuild
`Warning` task inside `System.Reactive.PackagesConfigCheck.targets`, not by the compiler, so
`/p:TreatWarningsAsErrors=true` correctly leaves them as warnings. The count matches the baseline.

## 6. Coverage verification

Method: verification from pre-existing artifacts, plus one independent full-suite reproduction by
the reviewer. Coverage generation was not merely accepted from the executor's summary.

Artifact present at the canonical path `artifacts/csharp/coverage.xml`, JaCoCo `<report>` form. Its
nine package counters were re-summed by hand and reconcile exactly to the root element of
`coverage/final440.cobertura.xml`:

- LINE: missed 9435 + covered 54760 = 64195; 54760/64195 = 85.3026%
- BRANCH: missed 3412 + covered 13036 = 16448; 13036/16448 = 79.2558%

Cobertura root attributes, read directly: `line-rate="0.853026"`, `branch-rate="0.792558"`,
`lines-covered="54760"`, `lines-valid="64195"`, `branches-covered="13036"`, `branches-valid="16448"`.
The conversion is faithful.

Independent reviewer reproduction (`coverage/reviewer440.cobertura.xml`, produced by the repository
wrapper in this session): `lines-covered="54750"`, `lines-valid="64195"`, `branches-covered="13033"`,
`branches-valid="16448"` — 85.2870% line and 79.2376% branch. Identical denominators; the covered
counts differ by 10 lines and 3 branches, a 0.016 pp band consistent with the known instrumentation
jitter in this repository. Both readings clear both floors.

### Per-language coverage verdicts

| Language | Changed files | Artifact | Repo-wide figures | Verdict |
|---|---|---|---|---|
| C# | 3 | `artifacts/csharp/coverage.xml` present | line coverage 85.3026%, branch coverage 79.2558% | PASS |
| TypeScript | 0 changed files on the branch | — | no measurement required | — |
| Python | 0 changed files on the branch | — | no measurement required | — |
| PowerShell | 0 changed files on the branch | — | no measurement required | — |

The C# repo-wide line figure of 85.3026% clears the 85% floor and the branch figure of 79.2558%
clears the 75% floor.

### Tier obligations

| Tier gate | Requirement | Verdict | Evidence |
|---|---|---|---|
| New code files | line >= 85%, branch >= 75% | PASS (vacuous) | This diff adds no new source file; both edited test files and the production file pre-existed |
| Modified files | >= 85% line, >= 75% branch, no regression on changed lines | PASS | `BreadcrumbStateModel.cs` reads 118/120 lines (98.33%) and 39/42 branches (92.86%); both re-derived by the reviewer directly from the `class` element `line-rate="0.9833333333333333"` and `branch-rate="0.9285714285714286"` |
| Changed lines, no regression | changed lines must not lose coverage | PASS | All 19 instrumented `line` elements inside the post-change `LeftArrow()` span (lines 220-246) carry `hits > 0`; the transition `if` reads `condition-coverage 100% (6/6)` after, against `100% (8/8)` before |
| Repo-wide per language | line >= 85%, branch >= 75% | PASS | 85.3026% / 79.2558% |

### Non-regression nuance, recorded so it is not read as a concealed dip

Baseline per-file figures were 119/121 lines and 41/44 branches; post-change they are 118/120 and
39/42. Expressed as a **rate** the per-file line figure moves from 98.3471% to 98.3333% and the
branch figure from 93.1818% to 92.8571%. Expressed as **uncovered counts** both are invariant: 2
uncovered lines before and after, 3 uncovered branches before and after. The denominators shrank
because the change deletes one already-covered source line and one already-covered `&&` conjunct
(two conditions). No line and no branch that was previously covered became uncovered.

The substantive policy requirement — "code changes must not reduce coverage for the lines that were
changed" — is met, and the changed region is fully covered. The nominal rate movement is an
arithmetic consequence of deleting covered code, not a quality regression. The reviewer verified
both readings independently from the two raw Cobertura documents rather than accepting the
executor's framing.

Baseline root attributes, read directly from `coverage/baseline440.cobertura.xml`:
`lines-covered="54755"`, `lines-valid="64196"`, `branches-covered="13037"`, `branches-valid="16450"`
— 85.2935% and 79.2523%. Repository-wide, both figures rose.

## 7. Structural gate re-derivation

Every structural claim the executor recorded was re-derived by the reviewer:

| Claim | Reviewer command | Result |
|---|---|---|
| Leaf-anchored conjunct removed | `grep -c "activeIndex.Value == row.Chain.Count - 1"` on the production file | 0 |
| `ActivateSegment` call retained | `grep -c "row.ActivateSegment(activeIndex.Value - 1)"` | 1 |
| Subfolder guard retained | `grep -n "_selectedSubfolderIndex < 0"` | present at line 234 |
| Router test file size | `awk END{print NR}` | 491 (limit 500, baseline 495) |
| Sequence test file size | `awk END{print NR}` | 292 (limit 500, baseline 235) |
| Production file size | `awk END{print NR}` | 248 (limit 500, baseline 248) |
| Repository-wide source footprint | `git diff --name-only <BASE> HEAD -- . ":(exclude)docs" ":(exclude).claude"` | exactly the 3 declared paths |
| No file created under QuickFiler roots | `git status --porcelain -- QuickFiler QuickFiler.Test` | empty |
| No file created under owned roots | `git status --porcelain -- UtilitiesCS UtilitiesCS.Test` | empty (all changes committed) |
| Root boundary is enforced downstream | read of `BreadcrumbStateModel.Row.cs:195-211` | `ActivateSegment` refuses `segmentIndex < 0` and `segmentIndex >= Chain.Count - 1` |

The last row is the load-bearing one. The spec's root-cause claim is that removing the leaf-anchored
conjunct is safe because `ActivateSegment` already imposes the root boundary. That was verified by
reading the method rather than inferring it from test behavior.

## 8. Findings

| ID | Severity | Blocking | Summary |
|---|---|---|---|
| — | — | — | No policy violation was identified. |

**Blocking findings in this artifact: 0.**

## 9. Verdict

**PASS.** The change satisfies CLAUDE.md, the general code-change policy, the general unit-test
policy, the C# code-change and unit-test policies, the quality-tier thresholds, the evidence-location
invariant and the artifact-hygiene rules. Coverage for C# is PASS on every applicable gate. No
remediation cycle is required and no `remediation-inputs` artifact is produced.
