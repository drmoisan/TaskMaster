# `$LASTEXITCODE` Hygiene Review — Issue #553

- Timestamp: 2026-08-14T11-14 (local) / 2026-08-14T15:14:22Z (UTC)
- Task: [P5-T2]
- Governing rule: `.claude/rules/ci-workflows.md`
- Corroborating research: Q9 of
  `research/2026-08-14T13-30-ci-parallel-job-split-research.md`

## Scope

Six of the seven workflow files are enumerated: the five callees plus the
orchestrator `ci.yml`.

`.github/workflows/codex-web-setup-test.yml` is explicitly **EXCLUDED** from
enumeration because it declares no `shell: pwsh` or `shell: powershell` step, so
the `.claude/rules/ci-workflows.md` pattern cannot apply to it. The exclusion is
recorded here so the enumeration is consistent with its own scope statement. That
file is also untouched by this change.

## Step-by-step enumeration

Every `shell: pwsh` step in scope, located by scanning each file for a
`shell: pwsh` line and attributing it to the nearest preceding `- name:`:

| File | pwsh steps | Steps (line: name) |
| --- | --- | --- |
| `ci.yml` | **0** | none — the orchestrator contains no `steps:` at all |
| `_actionlint.yml` | **0** | none — its single script step uses `shell: bash` |
| `_format-check.yml` | 2 | L35: `Setup CSharpier`; L39: `Verify formatting` |
| `_build-analyzers.yml` | 2 | L43: `Restore solution`; L47: `Build with analyzers and code style enforcement` |
| `_build-nullable.yml` | 2 | L43: `Restore solution`; L47: `Build with nullable warnings treated as errors` |
| `_mstest-coverage.yml` | 3 | L43: `Restore solution`; L47: `Build solution`; L54: `Run MSTest suite with coverage` |

Total: **9 `pwsh` steps** and **1 `bash` step** across the six files.

## (a) No step uses the deliberately-failing-nested-command pattern

`.claude/rules/ci-workflows.md` applies to a step whose `run:` block
**intentionally invokes a command expected to fail** — for example a negative-path
self-validation that asserts a gate catches a synthetic regression. Reviewing all
nine steps:

| Step | Last external command on the success path | Deliberately-failing? | Verdict |
| --- | --- | --- | --- |
| `Setup CSharpier` (`dotnet tool restore`) | `dotnet tool restore` | no | compliant; failure should propagate |
| `Verify formatting` (`dotnet csharpier check .`) | the gate command itself | no | compliant; a non-zero exit **is** the gate signal and must not be reset |
| `Restore solution` ×3 (`nuget restore`) | `nuget restore` | no | compliant |
| `Build with analyzers…` | `msbuild`, then an explicit exit guard | no | compliant |
| `Build with nullable…` | `msbuild`, then an explicit exit guard | no | compliant |
| `Build solution` (MSTest job's plain build) | `msbuild`, then an explicit exit guard | no | compliant |
| `Run MSTest suite with coverage` | `vstest.console.exe`, then `throw` on non-zero | no | compliant; on success the script ends after a passing `if`, leaving exit 0 |

**No step in the pipeline intentionally invokes a failing nested command, so the
rule's mandatory reset (`$LASTEXITCODE = 0`) or explicit `exit 0` is not required
anywhere.** This matches research Q9's conclusion for the pre-split pipeline and
confirms the split introduced no such step.

The rule becomes load-bearing only if a future change adds negative-path
self-validation (for example a step asserting that a gate catches a seeded
violation). This feature deliberately exercised its seeded violations as
**temporary probe commits** ([P4-T1]–[P4-T3]), which were reverted, precisely so
that no deliberately-failing nested command enters the committed pipeline — the
method the spec's seeded-conditions section prescribes.

Note the distinction the rule turns on: the `if ($LASTEXITCODE -ne 0) { exit
$LASTEXITCODE }` guards below exist to **propagate** a genuine failure, not to
suppress an expected one. Resetting `$LASTEXITCODE` in a gate step would silently
disable that gate.

## (b) Guards and throws present, with match counts

| Check | Command | Count | Expected | Locations |
| --- | --- | --- | --- | --- |
| msbuild exit guards | `Select-String -Path _build-analyzers.yml,_build-nullable.yml -Pattern 'if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }' -SimpleMatch` | **2** | 2 | `_build-analyzers.yml:L53`, `_build-nullable.yml:L60` |
| MSTest failure throw | `Select-String -Path _mstest-coverage.yml -Pattern 'throw "MSTest execution failed' -SimpleMatch` | **1** | 1 | `_mstest-coverage.yml:L85` |
| Zero-assembly throw | `Select-String -Path _mstest-coverage.yml -Pattern 'throw "No test assemblies found' -SimpleMatch` | **1** | 1 | `_mstest-coverage.yml:L79` |

All four match counts are as required.

`-SimpleMatch` is used so the literal `$`, `(`, `)`, `{`, and `}` characters are
matched as text rather than interpreted as regex metacharacters.

Note: `_mstest-coverage.yml` also carries a third exit guard at L51, in the new
plain `Build solution` step. It is not part of the required counts above (that
step has no pre-split counterpart) but it applies the same propagate-on-failure
discipline, so a build failure in the MSTest job fails the job rather than
proceeding to a test run against stale binaries.

## Behavioural corroboration

The guards were exercised, not merely inspected. In the [P4-T2] probe run
(31811211865) the nullable gate's msbuild invocation failed with
`error CS8603`, the exit guard propagated the non-zero code, and the
`build-nullable` job concluded `failure`. Had the guard been absent or the exit
code reset, the step would have reported success and the gate would have been
silently disabled.

## Acceptance ([P5-T2])

- Artifact records the step-by-step table and all four match counts.
- Spec seeded-condition checkbox 8 ("No `pwsh` step leaks a residual non-zero
  `$LASTEXITCODE` per `.claude/rules/ci-workflows.md`") is checked off with this
  artifact as the evidence pointer.
