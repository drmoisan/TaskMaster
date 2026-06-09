# Remediation Inputs: csharp-analyzer-stack-hardening (Issue #181) — Cycle 2

- Cycle entry timestamp: 2026-06-08T18-06
- Feature folder: `docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181`
- Base branch: `main` @ `2a522ed831865c2918ab02df153ef2929b0617dc`
- Head branch: `feature/csharp-analyzer-stack-181` @ `71e0777ada475c408d85d3b6c68e6192b4bc070b`
- Work mode: `full-feature`
- PR: https://github.com/drmoisan/TaskMaster/pull/182
- Trigger: required CI check RED after PR open (cycle-1 remediation-plan P2-T2 escalation)

## Failing Check (origin of this cycle)

The required GitHub Actions check **"Format, build, analyze, and test"** FAILED at its FIRST step, **"Verify formatting" (`dotnet csharpier check .`)**, on:

- `UtilitiesCS/Extensions/IEnumerableExtensions.cs` — "Was not formatted." (CSharpier 1.2.6, around line 132: a `System.Threading.Timer` lambda argument).

Run: https://github.com/drmoisan/TaskMaster/actions/runs/27157010168/job/80162358458 (conclusion: failure, 2m21s).

## Root-Cause Classification

This is a **pre-existing `main` regression**, not a defect introduced by this feature:

- The feature branch's copy of `UtilitiesCS/Extensions/IEnumerableExtensions.cs` is **byte-identical to `main`** (`git diff main..HEAD -- UtilitiesCS/Extensions/IEnumerableExtensions.cs` is empty); this feature never touched the file.
- **`main`'s CI at HEAD `2a522ed8` is itself RED** (the two preceding commits `2a6c6def`, `fa7eb86f` were green). The breakage entered with the merge of PR #180 (`fix(ienumerable-extensions): correct progress reporting in async consumption`), which committed an unformatted file.
- Because CI runs `dotnet csharpier check .` as a whole-repo blocking gate (step 1), every PR branched from current `main` is RED until the file is formatted.

## Why this must be fixed in-cycle (reconciling with cycle-1 "out of scope")

Cycle 1 classified this CSharpier finding as a pre-existing baseline to leave untouched. CI demonstrates it is a hard blocker: AC6 ("PR CI is GREEN, including the nullable-as-errors and MSTest-with-coverage steps") cannot be satisfied while the formatting gate fails, and the formatting gate runs before build/test, masking the status of the remaining steps. Per the orchestrator scope-change rule, a CI-only failure mode triggers a new cycle. The minimal corrective action is to apply CSharpier's own output to the single offending file.

## Fix List (file paths, expected behavior, verification)

- Action: Apply CSharpier 1.2.6 formatting to `UtilitiesCS/Extensions/IEnumerableExtensions.cs` (formatter output is authoritative; no hand-formatting). This is a pure formatting change — NO logic, behavior, or public-API change.
  - File touched: `UtilitiesCS/Extensions/IEnumerableExtensions.cs` (1 production file, formatting only).
  - Verification (local): `dotnet tool restore` then `dotnet tool run csharpier check .` exits 0; then the full toolchain loop (analyzer build, nullable `TreatWarningsAsErrors` build at the 84-error vendored-only baseline with no first-party regression, MSTest-with-coverage) per CLAUDE.md.
  - Verification (authoritative): after commit + push, the PR #182 "Format, build, analyze, and test" check completes GREEN (all steps, including nullable-as-errors and MSTest-with-coverage). Record the run URL and per-check status.

## Do Not Do (scope guard)

- Do NOT modify the logic of `IEnumerableExtensions.cs`; apply only CSharpier formatting output.
- Do NOT touch any other `.cs` source file; only this one file is unformatted per the CI log.
- Do NOT alter the analyzer-stack build-config delivered earlier in this feature.
- Do NOT introduce any CS8032 suppression or re-add SecurityCodeScan.
- Do NOT touch the two vendored projects (SVGControl, UtilitiesSwordfish.NET.General).
- Do NOT promote RS0030 or any analyzer rule from suggestion to warning/error.
- Do NOT modify `.claude/rules/` policy documents beyond the already-delivered `csharp.md`.
- Do NOT weaken or skip any CI gate to force green.

## Cycle Artifacts (this remediation cycle)

1. `remediation-inputs.2026-06-08T18-06.md` (this file) — authored at cycle entry by the orchestrator.
2. `remediation-plan.2026-06-08T18-06.md` — `atomic-planner` authors at cycle entry.
3. `code-review.<exit-ts>.md`, `feature-audit.<exit-ts>.md`, `policy-audit.<exit-ts>.md` — `feature-review` authors at cycle exit after CI is green.

## Handoff

Per `remediation-handoff-atomic-planner`: hand off to `atomic-planner` to author `remediation-plan.2026-06-08T18-06.md`, then `atomic-executor` preflight (`DIRECTIVE: PREFLIGHT VALIDATION ONLY`) and execution, then `feature-review` reaudit at the exit timestamp. Exit gate: `blocking_count == 0` (PR #182 CI GREEN; AC6 PASS, AC5 corroborated).
