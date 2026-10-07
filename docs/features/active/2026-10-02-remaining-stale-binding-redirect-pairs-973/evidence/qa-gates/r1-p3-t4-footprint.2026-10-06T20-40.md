# Remediation cycle 1, P3-T4: footprint gate

Timestamp: 2026-10-06T20-40
Command: (1) git -C <execution-worktree-root> diff --name-only d873200e87df1e8e79c2fec18a758cdf8150b134 HEAD -- '*.cs' '*.csproj' '*.props' '*.targets' '*.config'; (2) git -C <execution-worktree-root> diff --name-only d873200e87df1e8e79c2fec18a758cdf8150b134 HEAD -- ':!docs/*' ':!.claude/*'; (3) git -C <execution-worktree-root> status --porcelain --untracked-files=all; git -C <execution-worktree-root> diff --numstat d873200e87df1e8e79c2fec18a758cdf8150b134 HEAD -- tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1; git -C <execution-worktree-root> diff --name-only d873200e87df1e8e79c2fec18a758cdf8150b134 HEAD -- docs
EXIT_CODE: 0

HEAD at measurement: 5868df29d49357c8c16dd62aece10542331a8cc1

(1) C#, project, props, targets and config paths (verbatim): (no output)

(2) Paths outside docs/ and .claude/ (verbatim):
tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1

(3) Porcelain (verbatim): (empty; no path remains after removing .claude/agent-memory/ paths)

NUMSTAT: 1	2	tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1

docs paths added or changed this cycle (recorded, not gated):
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/p5-t17-ac17-checkoff.2026-10-06T20-34.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/r1-p1-t1-azure-core-refscan-taskmaster.2026-10-06T20-31.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/r1-p1-t2-azure-core-refscan-utilitiescs-control.2026-10-06T20-31.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/r1-p1-t3-azure-core-version-presence.2026-10-06T20-32.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/r1-p1-t4-azure-core-redirect-census.2026-10-06T20-33.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/r1-p1-t6-plan-p5-t17-checkoff.2026-10-06T20-35.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/r1-p2-t1-cr2-assertion-fold.2026-10-06T20-36.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/r1-poshqc-analyze.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/r1-poshqc-format.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/r1-poshqc-test.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/regression-testing/fail-before-exception.2026-10-06T20-36.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/remediation-baseline/r1-p0-t1-instructions-read.2026-10-06T20-24.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/remediation-baseline/r1-p0-t2-anchors.2026-10-06T20-25.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/remediation-baseline/r1-p0-t3-amendment-verification.2026-10-06T20-26.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/remediation-baseline/r1-p0-t4-cr1-base-pattern-count.2026-10-06T20-27.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/remediation-baseline/r1-poshqc-analyze-baseline.2026-10-06T20-29.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/remediation-baseline/r1-poshqc-format-baseline.2026-10-06T20-28.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/remediation-baseline/r1-poshqc-test-baseline.2026-10-06T20-29.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/plan.2026-10-02T22-16.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/remediation-plan.2026-10-06T19-30.md
docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/spec.md

LOOP-FIX-COMMIT: none

No C# toolchain step was run in this cycle because the diff carries no `.cs`, `.csproj`, `.props`, `.targets` or `.config` file (plan D-R3); the C# gates recorded at the 2026-10-06 base run remain the C# evidence.

Output Summary:
- Command (1) printed nothing: no C#, project, props, targets or config file changed since the cycle base; no CSHARP-FOOTPRINT stop.
- Command (2) printed exactly tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1.
- Numstat 1 added, 2 deleted for that file (no formatter re-lay, LOOP-FIX-COMMIT: none).
- Porcelain empty. No FOOTPRINT-MISMATCH.
- The docs changes are confined to the feature folder (21 paths).
