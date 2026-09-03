# Policy Audit — 2026-09-02-ci-build-infra-debt-730

- Timestamp: 2026-09-02T23-47
- Issue: #730
- Work Mode: `full-bug` (marker read from `issue.md:12`) — AC source is `spec.md` only
- Branch: `bug/ci-build-infra-debt-730`
- Head: `e80f74c934748f798a1234e1d78abfa8351a8bf3`
- Base: `origin/main` @ `8be5a6aac3b5a82c86241fbbf989fd9118602c56` (verified via `git merge-base HEAD origin/main`; merge-base equals `origin/main` tip, so the branch is fully rebased/reconciled)
- Diff scope audited: full branch diff `origin/main...HEAD` — 27 files, +49173/-0

## Verdict Summary

| Area | Verdict |
|---|---|
| Scope boundaries | PASS |
| Evidence location compliance | PASS |
| Artifact hygiene (absolute host paths) | **FAIL** (1 occurrence, documentation file) |
| C# toolchain order and commands | PASS (with documented deviation) |
| File size limit (500 lines) | PASS |
| General code change policy | PASS |
| Unit test policy | PASS |
| Coverage gates | PASS (see Coverage Verification) |
| Tonality policy | PASS |

Blocking findings: **1** (artifact hygiene, `research.2026-09-02T09-15.md:8`).

---

## Rejected Scope Narrowing

None detected. The caller supplied a list of specific items to verify but explicitly directed
independent verification of the whole change ("do not assume prior preflight rounds caught
everything"). No instruction attempted to limit the audit to a plan, task, phase, or file subset,
and no instruction attempted to mark any language's coverage as skippable. The audit was performed
against the full branch diff versus the resolved base branch regardless.

---

## Evidence Base and One Deviation

Primary evidence for this audit was derived directly from git in the item worktree
(`git diff origin/main...HEAD`, `git grep HEAD`, `git log`), plus independent re-measurement of the
committed evidence logs.

**Deviation recorded:** the canonical PR context artifacts are stale. Both
`artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` record
`Head SHA: fafe3d4d1f5a3dfcd2c44d21245d085e4156faea` on branch
`bug/claude-md-cites-ciyml-for-moved-toolchain-commands-564` (issue #564) — a different branch and a
different issue than the one under review. Per the `pr-context-artifacts` freshness cross-check,
head binding fails and the pair must be regarded as not describing this diff.

Regeneration was **deliberately not performed**. Both files are *tracked* in git
(`git ls-files --error-unmatch` succeeds) and are unmodified relative to `origin/main`, so
overwriting them would dirty a tracked file inside the branch under review and inject an unrelated
change into the delivery. The review-agent no-mutation principle takes precedence. Direct git
derivation was substituted, which is a strictly stronger evidence source than the collector summary
for every question this audit answers.

Secondary observation (not a defect in this branch): `origin/main` currently carries a committed,
stale PR-context artifact pair belonging to feature #564. That is pre-existing repository state.

---

## 1. Scope Boundaries — PASS

Verified by `git diff --name-status origin/main...HEAD` (27 files). Complete list of non-document
changes: 3 modified workflow files and 1 added `Directory.Build.props`. Everything else is the
feature folder (`issue.md`, `spec.md`, `plan`, `research`, `evidence/`).

| Boundary asserted | Result | Evidence |
|---|---|---|
| No `.csproj` modified | PASS | zero `.csproj` entries in `--name-status` |
| No `packages.config` modified | PASS | zero entries |
| No `Directory.Build.targets` modified | PASS | zero entries; file exists at base and is untouched |
| No application source (`.cs`) modified | PASS | zero `.cs` entries |
| Nothing under `.claude/`, `.codex/`, `.agents/` | PASS | zero entries |
| `config/blast-radius.json`, `config/orchestration-routing.json` untouched | PASS | zero entries |
| `.github/workflows/ci.yml` not edited | PASS | zero entries; file exists and is untouched |
| No `System.Reactive` `PackageReference` migration | PASS | no project file touched at all |
| No coverage threshold, Pester job, or coverage gate added or changed | PASS | the only workflow edits are comment lines; `_mstest-coverage.yml` parse tree is byte-identical to base (see §3) |

## 2. Evidence Location Compliance — PASS

All evidence artifacts are written under the canonical `<FEATURE>/evidence/<kind>/` layout:

- `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/baseline/` (7 files)
- `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/` (12 files)

No file in the branch diff is written under `artifacts/baselines/`, `artifacts/qa/`,
`artifacts/evidence/`, or `artifacts/coverage/`. Zero occurrences of any non-canonical evidence path.

`validate_evidence_locations.py` is **not present in this repository** (searched the full worktree
tree; no match). Manual path enumeration of all 27 diff entries was substituted and is recorded
above. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` events occurred during this review.

## 3. Functional Inertness of the Workflow Edits — PASS

The claim under audit is that the three workflow diffs are comment-only. The inserted comment block
is indented 12 spaces while the sibling mapping keys `key:` and `restore-keys:` sit at 10 spaces.
A more-indented line following a plain scalar can, in principle, be folded into that scalar, which
would corrupt the cache `key` value. This was therefore verified empirically rather than by reading
the diff, using two independent YAML implementations:

- **PyYAML** (`yaml.safe_load`) — all three files parse; `key` resolves to
  `nuget-${{ runner.os }}-${{ hashFiles('**/packages.config') }}` with no comment text absorbed.
- **YamlDotNet** (via `powershell-yaml` 0.4.12) — same result on all three files.

Order-sensitive deep comparison of the parsed document from `origin/main` against the parsed
document at `HEAD`, serialized with key order preserved, is **identical for all three files**. No
key, value, step, or job was added, removed, reordered, or altered. `restore-keys` remains
`nuget-${{ runner.os }}-\n` and `path` remains `packages` in all three.

Factual accuracy of the comment's own assertions was checked against the file it annotates: the
`Restore solution` step (`_build-analyzers.yml:59-61`) is the immediately following step, carries no
`if:` condition and no `cache-hit` gate, and precedes the build step. The comment's claim that
`nuget restore` "always runs unconditionally" and runs "before the build step" is accurate.

## 4. Artifact Hygiene — Absolute Host Paths — FAIL

Repository rule: no committed artifact may embed an absolute host path or the operator account name.

### 4a. The four committed `.log` files — clean

Independently re-swept all four committed MSBuild logs. The account token was derived at review time
from `Split-Path -Leaf $env:USERPROFILE` and is not spelled literally in this artifact.

| File | Lines | Account-token matches | `C:\Users\` matches | `<repo-root>` | `<main-checkout-root>` |
|---|---|---|---|---|---|
| `evidence/baseline/msbuild-analyzers-pre.log` | 11878 | 0 | 0 | 13631 | 36 |
| `evidence/baseline/msbuild-nullable-pre.log` | 12030 | 0 | 0 | 13631 | 36 |
| `evidence/qa-gates/msbuild-analyzers-post.log` | 11906 | 0 | 0 | 13601 | 36 |
| `evidence/qa-gates/msbuild-nullable-post.log` | 11742 | 0 | 0 | 13601 | 36 |

The line counts match the `git diff --numstat` insertion counts exactly, corroborating the
sanitization artifact's line-count invariant claim (substitution was in-place, not line-structural).
The spot check at `msbuild-analyzers-pre.log:1139` was independently reproduced: the line carries
`/analyzerconfig:<main-checkout-root>\.editorconfig` followed by
`/analyzerconfig:<repo-root>\.editorconfig`, both placeholders present and distinct, exactly as
`log-sanitization.2026-09-02T23-31.md` reports. The remediation of the previously-reported host-path
leak in the logs is confirmed to have landed correctly.

Residual absolute paths of the form `C:\Program Files\...` remain (5448 per log). These are
machine-standard tool install locations that carry no account or host identifier and therefore do
not violate the rule. Recorded as an observation in the code review, not a finding.

### 4b. `research/research.2026-09-02T09-15.md` — FAIL

**Finding PA-1 (FAIL, remediation required).** One committed file in the branch diff embeds a full
absolute host path including the operator account name:

```
docs/features/active/2026-09-02-ci-build-infra-debt-730/research/research.2026-09-02T09-15.md:8
```

Confirmed present in the committed tree via `git grep -i HEAD`, not merely in the working tree. It is
the single such occurrence across the entire branch diff.

Root cause: task `[P2-T10]` scoped its sanitization to exactly four `.log` files. The markdown
artifacts in the feature folder were never swept. The research document was authored in a
preparation-mode commit (`9d7b78a9`) that predates the sanitization task, so it was never a candidate
for it.

Remediation: replace the literal path on line 8 with the `<repo-root>` placeholder already used
throughout the sanitized logs. One-line documentation edit; no effect on the shipped configuration.

Two other markdown files matched the broad sweep pattern and were cleared on inspection:
`plan.2026-09-02T08-57.md:20,113` and `log-sanitization.2026-09-02T23-31.md:28,56` match only on the
token `USERPROFILE`, appearing inside `Split-Path -Leaf $env:USERPROFILE` — deliberate run-time
derivation, no literal leaked. Both are correct and are not findings.

Changed non-document files were swept separately (`git grep HEAD -- .github Directory.Build.props`):
zero account-name or host-path occurrences.

## 5. C# Toolchain Order and Commands — PASS (with documented deviation)

CLAUDE.md CUT3 requires format -> analyze -> type-check -> test. All four stages have committed
evidence, executed in order, all exit code 0.

| Stage | Command recorded in evidence | Matches CLAUDE.md | Exit |
|---|---|---|---|
| 1. Format | `dotnet tool run csharpier check .` | yes, character-for-character | 0 |
| 2. Analyze | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | yes | 0 |
| 3. Type-check | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | yes | 0 |
| 4. Test | `vstest.console.exe <2 assemblies> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"` | deviation, see below | 0 |

Both msbuild invocations correctly use `/t:Rebuild`, not `/t:Build`. This matters: CLAUDE.md records
that a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project, making the gate
vacuous. I verified the gate was **not** vacuous by counting compiler invocations in each log:
36 `csc.exe` invocations in every one of the four logs, with 52/69/67/56 `CoreCompile:` target entries.
The solution genuinely recompiled in all four runs.

The CSharpier claim was corroborated against `.csharpierignore`, which explicitly lists `*.props`.
The new `Directory.Build.props` is therefore correctly outside the formatter's scope, matching what
the evidence artifact asserts rather than merely restating it.

**Deviation (accepted, documented):** the vstest invocation adds `/InIsolation` and
`/TestCaseFilter:"TestCategory!=LiveOutlook"`, and is scoped to the two Rx-dependent assemblies
(`UtilitiesCS.Test`, `QuickFiler.Test`) rather than every test assembly. This is consistent with the
spec's Test Strategy, which scopes step 4 to regression re-verification of the Rx-dependent
assemblies, and with the resolution logic in `_mstest-coverage.yml:60-76`. `/InIsolation` is required
locally in this repository to avoid assembly-load artifacts. The deviation is disclosed in the
evidence artifact rather than concealed.

## 6. File Size Limit (500 lines) — PASS

| File | Lines |
|---|---|
| `.github/workflows/_build-analyzers.yml` | 69 |
| `.github/workflows/_build-nullable.yml` | 76 |
| `.github/workflows/_mstest-coverage.yml` | 112 |
| `Directory.Build.props` | 18 |

All well under the limit. Markdown documentation and raw tool-output logs are exempt per
`.claude/rules/general-code-change.md`.

## 7. General Code Change Policy — PASS

- **Simplicity first** — both fixes are the minimal expression of their intent: comment text, and one
  MSBuild property. No indirection introduced.
- **Separation of concerns** — no mixing of build configuration with application logic; no
  application file is touched.
- **Error handling / logging** — neither fix introduces a code path. Finding 2 suppresses a vendor
  *warning*, not an error; nothing is silently swallowed.
- **Dependencies** — no dependency added, removed, or version-changed.
- **Public API compatibility** — no API surface touched.
- **Rollback** — both fixes are independently revertible at file granularity, as the spec claims.
- **Comment quality** — comments explain *why*, not *what*, as required. Both the YAML block and the
  `Directory.Build.props` comment state the rationale and the accepted trade-off.

`Directory.Build.props` is well-formed XML (validated by `[xml]` load), declares exactly one property
inside a single `<PropertyGroup>`, carries the required rationale comment, and terminates with a
newline. No property collision exists: `git grep -i RxUseUnsupportedPackagesConfig HEAD -- *.csproj
*.props *.targets *.config` returns matches only inside the new file itself, confirming the spec's
"no override/collision risk" invariant.

## 8. Unit Test Policy — PASS

No test file was added, modified, or deleted in this branch. No new executable logic was introduced
in any language, so there is no new behavior requiring test authorship. The existing MSTest suites
were re-run as regression verification and pass 6095/6095. This matches the spec's declared Test
Strategy, which states no new unit tests are authored for a configuration-only change.

Independently corroborated against the run's own TRX result file rather than the prose summary:
`ResultSummary/Counters` reads `total=6095 executed=6095 passed=6095 failed=0 error=0 timeout=0
aborted=0 notExecuted=0`, `outcome="Completed"`. The TRX run window (23:14:36 to 23:15:24, elapsed
47.35s) matches the artifact's reported `47.3269 Seconds` and precedes the commit timestamp
(23:34:42), so the evidence is contemporaneous with the delivery and not back-dated.

## Coverage Verification

Changed-file extensions across the full branch diff: `.yml` (3), `.props` (1), `.md` (20), `.log` (4).
The branch diff contains **zero** files with a `.cs`, `.ps1`, `.psm1`, `.py`, `.ts`, or `.tsx`
extension, confirmed by `git diff --name-status origin/main...HEAD` over all 27 entries.

| Language | Changed files on branch | Coverage artifact | Verdict |
|---|---|---|---|
| C# / dotnet | 0 changed `.cs` files | `artifacts/csharp/coverage.xml` absent | PASS — zero changed C# source files, so the branch adds nothing to the C# coverage denominator and no line or branch coverage figure can regress |
| PowerShell / pester | 0 changed `.ps1`/`.psm1` files | not required | PASS — zero changed PowerShell files, so no PowerShell coverage denominator is affected and no line coverage figure can regress |
| Python | 0 changed `.py` files | not required | PASS — zero changed Python files, so no Python coverage denominator is affected |
| TypeScript | 0 changed `.ts`/`.tsx` files | not required | PASS — zero changed TypeScript files, so no TypeScript coverage denominator is affected |

`Directory.Build.props` is governed by the C# code change policy (which covers `*.props`) but
contributes no executable line to any coverage denominator: it declares a single MSBuild property and
contains no compiled code. The repo-wide C# coverage figure is therefore mathematically unchanged by
this branch, and the "no regression on changed lines" requirement is satisfied vacuously because no
coverage-bearing line changed.

No coverage threshold, coverage gate, or Pester job was added, removed, or modified anywhere in this
branch; `_mstest-coverage.yml`'s parse tree is proven identical to base in §3.

## 9. Tonality Policy — PASS

Reviewed the four principal authored artifacts (`spec.md`, `plan`, the QA-gate evidence set, and the
`Directory.Build.props` / workflow comment text). Language is factual and measured throughout.
Evidence-first wording is used correctly: the spec explicitly flags that the vendor `.targets` XML
could not be read directly and states that the pre/post transcript comparison closes that gap
empirically, rather than asserting unverified certainty. No hyperbole, humor, or decorative metaphor
found.

## Assumptions

1. `origin/main` is the correct base. Confirmed by the merge-base equalling the `origin/main` tip.
2. The two `msbuild` logs were produced by the commands their companion `.md` artifacts record. This
   is not cryptographically bound, but is corroborated by the `csc.exe`/`CoreCompile` counts, the
   warning totals, the `Build succeeded.` markers, and the timestamp ordering versus the commit date.
3. Whether the repository standard is the 80% floor in CLAUDE.md or the uniform 85%/75% floor in
   `.claude/rules/quality-tiers.md` is an unresolved documentation conflict in this repository. It has
   no bearing here, because zero coverage-bearing files changed and no figure moves under either rule.
