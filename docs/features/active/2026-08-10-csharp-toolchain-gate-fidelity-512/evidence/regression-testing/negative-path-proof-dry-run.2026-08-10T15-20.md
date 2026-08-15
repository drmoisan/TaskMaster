# Negative-path proof — executed dry run (de-risks AC4 before planning)

Timestamp: 2026-08-10T15-20
Branch: bug/csharp-toolchain-gate-fidelity-512 (from origin/epic/build-ci-coverage-gate-fidelity-integration @ edf3d34c)
MSBuild: C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe

This is a **preparation-phase feasibility run**, executed to confirm that acceptance criterion AC4 is
achievable before an atomic plan is written around it. No repository file was left modified; the
perturbation was reverted within the same script under a `finally` block.

## The command under test (the proposed corrected type-check gate)

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

This is `.github/workflows/ci.yml`'s exact command. It deliberately omits `/p:Nullable=enable`.
Each run additionally passed `/nologo /v:m /fl "/flp:logfile=<path>;verbosity=normal"` to capture the
compile-execution assertion; those switches do not alter build semantics.

## Step 1 — positive control, unperturbed tree

Command: as above
EXIT_CODE: 0
Elapsed: 20.1 s
`Skipping target "CoreCompile"` occurrences: **0**
MSBuild summary: `0 Error(s)`
Output Summary: The gate compiles the whole solution genuinely and passes on a clean tree.

## Step 2 — perturbation

File: `UtilitiesCS/Extensions/QueueExtensions.cs` (19 lines, carries `#nullable enable` at line 9).

Appended one method inside the existing static class:

```csharp
        // TEMPORARY nullable-gate probe - reverted immediately after measurement.
        public static string ProbeNullableGate()
        {
            string? maybe = null;
            return maybe;
        }
```

`git diff --stat` reported `1 file changed, 8 insertions(+), 2 deletions(-)`.

## Step 3 — the same command against the perturbed tree

Command: as above
EXIT_CODE: **1**
Elapsed: 3.6 s
`Skipping target "CoreCompile"` occurrences: **0**
MSBuild summary: `1 Error(s)`
Diagnostic:

```
UtilitiesCS\Extensions\QueueExtensions.cs(24,20): error CS8603: Possible null reference return. [.\UtilitiesCS\UtilitiesCS.csproj]
```

All error codes present: `CS8603 x1`. The perturbation produced exactly the intended diagnostic and
nothing else.

## Step 4 — revert

`git checkout -- UtilitiesCS/Extensions/QueueExtensions.cs`
Post-revert `git status --porcelain` for that path: empty.
Verification that the probe method is gone: `ProbeNullableGate still present: False`.

## Verdict

**AC4 is achievable as specified.** Positive control EXIT 0, perturbed EXIT 1, clean revert.

## The decisive consequence for the design decision

This run settles the central question of issue #522 empirically. The corrected command **omits**
`/p:Nullable=enable`, yet it still failed on a nullable violation introduced into a file carrying a
`#nullable enable` pragma. Removing the flag therefore **loses no enforcement over any file that has
opted in**; it only stops conscripting the 1100-odd files that never opted in and were never written
for nullable analysis.

Combined with the measurement that the currently documented command compiles nothing at all
(`baseline-nullable-gate-vacuity.2026-08-10T14-25.md`, run M2: EXIT 0 in 1.8 s with 18 of 18
`CoreCompile` targets skipped), the enforcement delta of this change is **strictly positive**:

| | Current documented gate | Proposed corrected gate |
|---|---|---|
| Compiles? | No (18 of 18 skipped when warm) | Yes (0 skips, 74 CoreCompile executions) |
| Catches a real nullable violation in an opted-in file? | No | **Yes (CS8603, EXIT 1)** |
| Passes on a clean tree? | Vacuously | Yes, genuinely |
| Passes in CI on `main`? | n/a | Yes |

Any reviewer inclined to read the removal of `/p:Nullable=enable` as a relaxation should be directed
to this table. The change moves the gate from zero enforcement to real enforcement.

## Notes for the plan author

- Failure is fast: the perturbed run terminated in 3.6 s because `UtilitiesCS` is a foundational
  dependency compiled early. A perturbation in a leaf project would cost a longer build first.
- Assert non-vacuity with **zero occurrences of `Skipping target "CoreCompile"`** in the `/fl` log.
  Do **not** count `csc.exe` (absent at `verbosity=normal`) and do **not** count `CoreCompile:` header
  lines, which are printed even when the target is skipped. That header-line trap is the most likely
  explanation for the contradictory historical evidence artifact noted in the research document.
- Git reported `LF will be replaced by CRLF` when writing the perturbation. The plan should apply the
  perturbation in a way that preserves the file's existing line endings, or accept and revert it.
- The plan must retain the revert as a mandatory step with its own verification, not as a trailing
  assumption.
