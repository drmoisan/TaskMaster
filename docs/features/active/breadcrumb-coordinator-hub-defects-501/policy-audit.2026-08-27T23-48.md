# Policy Audit — breadcrumb-coordinator-hub-defects-501

- Timestamp: 2026-08-27T23-48
- Branch: `bug/breadcrumb-coordinator-hub-defects-501`
- Base (resolved): `origin/epic/quickfiler-bug-family-integration`
- HEAD at review: `cab1a0fb8a868552ff72d650d34df8c5a7d37e33`
- Merge base (recomputed): `69e8317152c0a9ee6ee6e65db0ef81f6906189b1` — equal to the integration tip, so the two-dot and three-dot diffs are identical and `origin/epic/quickfiler-bug-family-integration..HEAD` IS the full branch diff.
- Work Mode: `full-bug` (marker read from `issue.md`). Sole AC source: `spec.md`.
- Overall verdict: **PASS** — 0 Blocking findings.

## Scope Resolution

The caller supplied `origin/epic/quickfiler-bug-family-integration` as the base and directed review at
`git diff origin/epic/quickfiler-bug-family-integration..HEAD`. This was independently checked, not
accepted on assertion:

- `git merge-base origin/epic/quickfiler-bug-family-integration HEAD` returns `69e83171`, which is the
  integration tip itself. The branch is 0 behind. The caller-named range is therefore the complete
  branch-versus-base diff, not a subset of it.
- The full changed-path list was enumerated with `git diff --name-only` and reconciled against the
  caller's list of twelve code files. Two additional non-code paths appear in the range and are
  audited below (`.claude/agent-memory/**`), plus feature documents and two promoted potential records.

## Rejected Scope Narrowing

None. No caller instruction narrowed the audit to a plan, task, phase, or file subset, and no language
with changed files was marked out of scope.

Two caller instructions were examined and found NOT to be narrowing:

1. "Review ONLY this feature's writes ... not against any older baseline." Verified above to be the
   full branch-versus-base diff. Excluding merged siblings 493 and 444, which entered through the
   mandatory base merge commit `67a5ce8c`, is correct scoping, not narrowing.
2. "Do NOT re-run the C# toolchain." This matches this agent's required operating model, which is
   verification of pre-existing coverage and gate artifacts rather than regeneration. Coverage was
   nonetheless re-derived independently from the committed Cobertura XML rather than read from the
   executor's prose.

## Evidence Location Compliance

`git diff --name-only origin/epic/quickfiler-bug-family-integration..HEAD` was filtered for
`artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` and `artifacts/coverage/`. **Zero
matches.** All 93 evidence files sit under the canonical
`docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/<kind>/` layout
(`baseline/`, `qa-gates/`, `regression-testing/`, `issue-updates/`, `other/`). No violation.

## Coverage Verification

Coverage was verified by parsing the committed Cobertura artifacts directly, not by reading the
executor's summary. Both figures below were reproduced from
`evidence/qa-gates/postchange.cobertura.2026-08-27T23-31.xml` (root element `<coverage>`, Cobertura
format) and `evidence/baseline/baseline.cobertura.2026-08-27T20-01.xml`.

| Language | Changed files in diff | Artifact | Repo line | Repo branch | Verdict |
| --- | ---: | --- | ---: | ---: | --- |
| CSharp | 12 | `evidence/qa-gates/postchange.cobertura.2026-08-27T23-31.xml` | 85.1448% | 79.2202% | PASS |
| TypeScript | 0 | none required | none | none | zero changed files |
| Python | 0 | none required | none | none | zero changed files |
| PowerShell | 0 | none required | none | none | zero changed files |

- CSharp line coverage 85.1448% (54439/63937) is PASS against the 85% floor.
- CSharp branch coverage 79.2202% (12943/16338) is PASS against the 75% floor.
- CSharp new-code coverage and modified-file coverage is PASS: 89 of 89 changed coverable lines covered.
- The canonical `artifacts/csharp/coverage.xml` path is absent. Per the established precedent that a
  committed `<FEATURE>/evidence/` Cobertura report satisfies the artifact-presence requirement, the
  CSharp coverage artifact is treated as PRESENT and was parsed successfully. This substitution is
  recorded so it stays visible.
- TypeScript, Python and PowerShell have zero changed files in this branch diff. No `.ts`, `.tsx`,
  `.py`, `.ps1` or `.psm1` path appears anywhere in the range.

### Independent per-file reproduction (owned production files)

Line sets were aggregated across every `<class>` element sharing a `filename`, so partial classes and
compiler-generated lambda classes are merged rather than double-counted.

| File | Baseline | Post-change | Delta | Verdict |
| --- | --- | --- | ---: | --- |
| `BreadcrumbDropDownOpenCoordinator.cs` | 233/237 = 98.3122% | 233/237 = 98.3122% | 0.0000 pp | PASS |
| `BreadcrumbCoordinatorUpgradeLifetime.cs` | 202/204 = 99.0196% | 209/209 = 100.0000% | +0.9804 pp | PASS |
| `BreadcrumbMessengerHub.cs` | 294/294 = 100.0000% | 306/306 = 100.0000% | 0.0000 pp | PASS |
| SR-1 split pair combined | 280/280 = 100.0000% | 295/295 = 100.0000% | 0.0000 pp | PASS |

Every figure in `evidence/qa-gates/coverage-delta.2026-08-27T23-31.md` was reproduced exactly. No
per-file regression. Repository line rate moved up by +0.0068 pp.

The 85.1448% figure clears the 85% floor by 0.1448 pp. That margin is roughly ten times the
cross-session nondeterminism band previously measured on this repository's C# coverage constants, so
the PASS is genuine rather than an artefact of measurement noise. The margin is nonetheless thin and
is recorded as Observation O-7.

The four uncovered lines in `BreadcrumbDropDownOpenCoordinator.cs` were identified and mapped across
the diff: post-change lines 113, 159, 240, 313 correspond exactly to baseline lines 94, 140, 221, 288.
All four are pre-existing and none is a line this change added. See feature-audit finding NB-3.

## Policy Compliance by Rule

| Rule | Verdict | Evidence |
| --- | --- | --- |
| `CLAUDE.md` C# toolchain order (format, analyze, type-check, test) | PASS | `evidence/qa-gates/post-merge-toolchain-attestation.2026-08-27T23-31.md`; one documented restart after the seam edit, loop re-run from step 1 |
| CSharpier via `dotnet tool run`, not `dotnet format` | PASS | `post-merge-csharpier.2026-08-27T23-31.md`; `format` then `check`, both exit 0, 1545 files |
| Analyzers with `/t:Rebuild` not `/t:Build` | PASS | `post-merge-msbuild-analyzers.2026-08-27T23-31.md`; 0 errors, `Skipping target "CoreCompile"` count = 0, 36 `csc.exe` invocations |
| Nullable gate, no `/p:Nullable=enable`, `/t:Rebuild` | PASS | `post-merge-msbuild-nullable.2026-08-27T23-31.md`; 0 errors, non-vacuity proven by 0 skipped CoreCompile |
| MSTest + Moq + FluentAssertions | PASS | All five changed test files use `[TestClass]`/`[TestMethod]`, `Mock<T>`, `.Should()` |
| 500-line file limit (`general-code-change.md` §4.1) | PASS | Max observed 500 (`BreadcrumbSelectorCoordinatorTests.cs`); counted with `awk 'END{print NR}'` and `wc -l`, not `Measure-Object -Line`. See O-1 |
| Line coverage >= 85% (`quality-tiers.md`) | PASS | 85.1448% CSharp coverage, reproduced independently |
| Branch coverage >= 75% (`quality-tiers.md`) | PASS | 79.2202% CSharp coverage, reproduced independently |
| No regression on changed lines | PASS | 89/89 changed coverable lines covered; all four per-file deltas at or above 0.00 pp |
| Coverage Exclusion Policy — no production path excluded | PASS | No `[ExcludeFromCodeCoverage]` and no `coverage.config` change in the diff |
| Determinism: no `Thread.Sleep`, `Task.Delay`, wall-clock wait, temp file, second thread | PASS | Independent grep across all five changed test files returned zero banned constructs |
| Test file location mirrors production tree | PASS | `QuickFiler/Viewers/*` tested from `QuickFiler.Test/Viewers/*` |
| Ownership boundary (six sibling-owned files) | PASS | None of the six appears in either range; `evidence/qa-gates/scope-lock.2026-08-27T23-39.md` corroborated by independent `git diff --name-only` |
| Fail-fast error handling (`general-code-change.md` §3) | PARTIAL | The new broad `catch (Exception)` in `Broadcast` swallows after logging. Deliberate under ratified ruling SR-3 and matching the file's existing `SafeUnsubscribe` precedent. See O-2 |
| Tone policy | PASS | Feature documents and evidence artifacts are factual and neutral |
| No policy document modified | PASS | No `.claude/rules/**` or `.github/instructions/**` path in the diff |
| No absolute host paths in artifacts | PASS | Spot-checked TRX files and evidence artifacts; workspace rendered as `WS`, host as `<host>`, account as `<account>` |

## Files Outside the Twelve Code Files

The branch range also contains:

- `docs/features/active/breadcrumb-coordinator-hub-defects-501/**` — 93 evidence files plus `plan`, `spec`.
- `docs/features/potential/promoted/2026-08-27-breadcrumb-closecompleted-residual-outside-requestopen-invalidate.md`
  and `...-breadcrumb-nonreentrant-upgrade-lifetime-guard.md` — the two follow-up promotions.
- `.claude/agent-memory/atomic-executor/project_concurrent_executor_same_worktree.md` and
  `.claude/agent-memory/orchestrator/one-executor-per-worktree.md` — agent memory, entered at commit
  `1074e004`. These are not enumerated by the scope-lock artifact's "Files outside the code scope"
  section. See O-3. They are documentation, carry no executable code, and affect no gate.

## Follow-up Promotions Verified

Both follow-ups the handoff index claims were filed exist and are open, confirmed with `gh issue view`:

- #655 `Refactor: breadcrumb-nonreentrant-upgrade-lifetime-guard` — OPEN.
- #656 `Bug: breadcrumb-closecompleted-residual-outside-requestopen-invalidate` — OPEN (the SR-4 residual).

Latent defects were promoted to real issues rather than left as prose in a feature folder.

## Artifact Mirror Disclosure

The authoritative artifacts are written to the feature folder in the review worktree. Byte-identical
copies are also written to the same relative path under the session working directory, because the
`validate-feature-review-coverage.ps1` SubagentStop hook resolves advertised artifact paths relative to
its own process working directory, which differs from the review worktree.
