# `-EnableNullable` is inert — deprecated no-op proof ([P5-T10])

Timestamp: 2026-08-11T00-14
Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -Target Rebuild -EnableNullable -TreatWarningsAsErrors 2>&1 | Tee-Object -FilePath coverage/task-deprecated-switch.log`
EXIT_CODE: 0

Issued from a PowerShell parent via
`pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-task-typecheck.ps1`. The transcript
capture is the explicit `2>&1 | Tee-Object` redirection, per [P5-T8].

## Measurements

| Metric | Value | Acceptance |
|---|---|---|
| `EXIT_CODE` | **0** | required 0 — PASS |
| Deprecation warning present in the transcript | **yes** | required — PASS |
| `Skipping target "CoreCompile"` count | 0 | the run compiled genuinely |
| Elapsed | 16.6 s | recorded |
| MSBuild summary | `0 Error(s)` | — |

## The deprecation warning, quoted from the transcript

```
WARNING: The -EnableNullable switch is deprecated and has no effect. This repository enforces
nullability per file via #nullable enable; /p:Nullable=enable is deliberately absent from CI and
makes the gate unpassable. See CLAUDE.md C#1 item 3.
```

The switch still **binds** (no parameter-binding error) and emits `Write-Warning`, not `Write-Host`,
so no new PSAvoidUsingWriteHost finding is created ([P2-T8] confirms the finding count is unchanged
at 16).

## Exit 0 is the discriminating signal

`EXIT_CODE: 0` is what proves the switch is inert. **If `Nullable=enable` were still emitted as an
MSBuild property, this run would have failed** with the `CS86xx` population measured in [P0-T13]:

> `FEATURE/evidence/baseline/baseline-nullable-debt.2026-08-10T22-57.md`, tail:
> DEBT-PROBE returns `EXIT_CODE: 1` with MSBuild reporting **195 Error(s)**, all attributed to
> `UtilitiesCS.csproj`, distributed CS8766 x130 / CS8618 x23 / CS8625 x12 / CS8600 x9 / CS8601 x8 /
> CS8604 x7 / CS8602 x3 / CS8603 x2 / CS8714 x1, with a zero `CoreCompile` skip count (the failure is
> genuine) and 22 of 73 `CoreCompile` headers executed before the build aborted.

DEBT-PROBE is `/t:Rebuild /m ... /p:TreatWarningsAsErrors=true` **plus** `/p:Nullable=enable` — the
exact property set this run would have produced had the switch not been neutralized. It returned
exit 1 with 195 errors. This run returned exit 0 with 0 errors, under the same `/t:Rebuild` and the
same zero skip count, so the difference is attributable to the property no longer being emitted.

## Corroborating direct evidence from the transcript

| Probe | Count | Interpretation |
|---|---|---|
| `grep -c 'nullable+' coverage/task-deprecated-switch.log` | **0** | `csc.exe` was **never** invoked with `/nullable+`; the transcript contains the full `csc.exe` command lines, so this is a positive absence, not an unobserved one |
| `grep -c 'Nullable=enable' coverage/task-deprecated-switch.log` | **1** | the single occurrence is inside the deprecation warning text itself, which names the flag in order to explain why it is absent — not an MSBuild argument |

## `.csproj` sync guard

| Capture | `git status --porcelain -- '*.csproj'` |
|---|---|
| Immediately before this task | (empty) |
| Immediately after this task | (empty) |

Sync console line emitted: `Sync-PackageReferences: All HintPaths are up to date` — it changed
nothing. **No `.csproj` was rewritten and no revert was required**; [P6-T9] is not invalidated.

## Output Summary

Invoking the corrected script with the retained `-EnableNullable` switch returns `EXIT_CODE: 0` and
emits the deprecation warning. Under the same `/t:Rebuild` shape, the property-bearing DEBT-PROBE
returns exit 1 with 195 errors, so exit 0 here proves the property is no longer emitted. Direct
transcript probes corroborate: zero `nullable+` occurrences in the `csc.exe` command lines, and the
single `Nullable=enable` occurrence is the warning text. Existing callers (including the
`.codex/codex-web-setup.sh:342` printed command that SD1 leaves uncorrected) keep working, with the
behaviour change made explicit rather than silent.
