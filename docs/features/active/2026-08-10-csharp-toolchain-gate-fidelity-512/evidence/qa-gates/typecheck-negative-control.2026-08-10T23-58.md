# TYPECHECK negative control — the #512 negative-path proof ([P5-T5], [expect-fail]; AC4)

Timestamp: 2026-08-10T23-58
Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /nologo /v:m /fl "/flp:logfile=coverage/qa-typecheck-negative.log;verbosity=normal"`
EXIT_CODE: 1

`MSBUILD` = `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe`,
invoked via `pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-typecheck-negative.ps1`.

This is an `[expect-fail]` task: a non-zero exit is the **required** outcome. The command is
byte-identical to the one that returned `EXIT_CODE: 0` in the [P5-T4] positive control; only the
tree differs.

## The perturbed file

`UtilitiesCS/Extensions/QueueExtensions.cs` — 21 lines at the merge base, UTF-8 with BOM, CRLF line
terminators, carrying `#nullable enable` at line 9 and a concrete
`public static class QueueExtensions` at lines 11-20.

## The exact perturbation

Appended inside the existing static class body, after the closing brace of `DequeueChunk` and before
the class closing brace, preserving the file's existing CRLF line endings and BOM:

```csharp

        // TEMPORARY nullable-gate negative-control probe - reverted by [P5-T6].
        public static string ProbeNullableGate()
        {
            string? maybe = null;
            return maybe;
        }
```

`git diff --stat` reported `1 file changed, 7 insertions(+)`; `file` reported the encoding and line
terminators unchanged (`UTF-8 (with BOM) text, with CRLF line terminators`).

**Form rationale.** `spec.md` § "Negative-path proof design" prescribes the equivalent one-line
`public static string NullableGateNegativeControl() => null;` form. The local-variable form is
adopted instead because it is the **measured** one: it is exactly the probe validated by
`FEATURE/evidence/regression-testing/negative-path-proof-dry-run.2026-08-10T15-20.md`, which
confirmed it produces `CS8603` and nothing else. The member is `public` (no unused-member
diagnostic), `static` (required — `QueueExtensions` is a static class), and adds no field (so it
cannot produce `CS8618` and confuse attribution).

## Measurements

| Metric | Value | Acceptance |
|---|---|---|
| `EXIT_CODE` | **1** | required 1 — PASS |
| MSBuild summary | `1 Error(s)` | — |
| Node-prefixed `error CS` count | 1 | — |
| `Skipping target "CoreCompile"` count | **0** | required 0 — PASS |
| Elapsed | 3.4 s | recorded |

## The diagnostic, quoted verbatim

```
19>C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ac1a08c3569adb7eb\UtilitiesCS\Extensions\QueueExtensions.cs(25,20): error CS8603: Possible null reference return. [C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ac1a08c3569adb7eb\UtilitiesCS\UtilitiesCS.csproj]
```

It matches `error CS8603`, is attributed to the perturbed file
`UtilitiesCS\Extensions\QueueExtensions.cs`, and is attributed to `UtilitiesCS.csproj`, as required.
It is the **only** error produced.

## Non-vacuity: the perturbed file's project was genuinely compiled

Asserted **from the log**, not assumed:

```
1>Project "...\TaskMaster.sln" (1) is building "...\UtilitiesCS\UtilitiesCS.csproj" (19) on node 3 (Rebuild target(s)).
19>Done Building Project "...\UtilitiesCS\UtilitiesCS.csproj" (Rebuild target(s)) -- FAILED.
```

`UtilitiesCS.csproj` appears among the projects the run built, under the `Rebuild` target, and the
diagnostic carries its node prefix `19>`. The `Skipping target "CoreCompile"` count for the whole log
is **0**, so no project short-circuited. The hazard the [P5-T4] positive control rules out — an
earlier project aborting the graph before the perturbed project compiles — did not occur.

## AC2 counting-mechanism deviation (restated)

The non-vacuity assertion is a **zero** count of `Skipping target "CoreCompile"`, substituted for
AC2's `csc.exe` parenthetical (zero at `verbosity=normal` even for genuine compiles). `CoreCompile:`
header lines are not counted. Recorded in `spec.md` § "The non-vacuity assertion mechanism".

## The decisive consequence for issue #522

The corrected command **omits** `/p:Nullable=enable`, yet it still failed on a nullable violation
introduced into a file carrying a `#nullable enable` pragma. Removing the flag therefore **loses no
enforcement over any file that has opted in**; it only stops conscripting files that never opted in.
Contrast the documented gate measured at [P0-T11]: `EXIT_CODE: 0` in 1.8 s with 18 of 18
`CoreCompile` targets skipped — it would not have caught this violation at all.

| | Documented gate at the merge base | Corrected gate |
|---|---|---|
| Compiles? | No (18 of 18 skipped when warm) | Yes (0 skips) |
| Catches a real nullable violation in an opted-in file? | No | **Yes (CS8603, EXIT 1)** |
| Passes on a clean tree? | Vacuously | Yes, genuinely ([P5-T4]) |

## Output Summary

The corrected type-check gate returns `EXIT_CODE: 1` with exactly one diagnostic —
`error CS8603: Possible null reference return.` at
`UtilitiesCS\Extensions\QueueExtensions.cs(25,20)`, attributed to `UtilitiesCS.csproj` — against a
deliberately perturbed opted-in production file, with a **zero** `CoreCompile` skip count and the
perturbed file's project proven from the log to have been recompiled. AC4's negative-path proof is
satisfied; the revert confirmation is appended below by [P5-T6].

---

## Revert confirmation ([P5-T6])

Timestamp: 2026-08-11T00-00
Command: `git checkout -- UtilitiesCS/Extensions/QueueExtensions.cs`
EXIT_CODE: 0

| Check | Command | Result | Acceptance |
|---|---|---|---|
| Working-tree status | `git status --porcelain UtilitiesCS/Extensions/QueueExtensions.cs` | **(empty)** | required empty — PASS |
| Probe method absent | `grep -c 'ProbeNullableGate' UtilitiesCS/Extensions/QueueExtensions.cs` | **0** | required 0 hits — PASS |
| Line count restored | `wc -l UtilitiesCS/Extensions/QueueExtensions.cs` | **21** | equals the merge-base line count of 21 — PASS |

**The perturbation is never committed.** The file is byte-identical to its merge-base content, as
proven by an empty `git status --porcelain` for that path.

Output Summary: the negative-control perturbation was reverted immediately after measurement.
`git status --porcelain` for the file is empty, a grep for the probe method name returns zero hits,
and the file's line count (21) equals its merge-base line count. AC4's revert-confirmation
requirement is satisfied. The failing `/t:Rebuild` deleted every project's `bin`/`obj`; [P5-T7] is
the mandatory restoration build.
