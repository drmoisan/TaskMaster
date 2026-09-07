# Preflight round 1 delta, adjudicated by the orchestrator

Timestamp: 2026-09-06T23-40

Reviewer signal: `PREFLIGHT: REVISIONS REQUIRED` with `CONVERGENCE: FURTHER ROUNDS LIKELY`.

## How to use this document

Apply each ACCEPTED item using the reviewer's verbatim replacement text from the preflight report,
reproduced in the delegation prompt. For every item, report one disposition:
`applied-verbatim`, `applied-with-mechanical-reassembly`, or `not-applied-with-reason`.
Do not silently substitute your own wording. If you judge an accepted item wrong, leave it unapplied
and report the disagreement for the orchestrator to adjudicate.

## Orchestrator adjudication summary

The reviewer stated plainly that it could not execute a single command: the PreToolUse guard refuses
every `pwsh` invocation inside this isolated agent worktree. Its claims about runtime behaviour are
therefore reasoning rather than observation. The orchestrator adjudicated each such claim against the
committed evidence of the completed issue #791 run, which contains real recorded invocations on this
same host.

### ACCEPTED, and confirmed by recorded observation

- B5 and B6, and M4. On a fully passing run `vstest.console.exe` prints
  `Test Run Successful. Total tests: 1339, Passed: 1339, Total time: 13.2586 Seconds.` and prints no
  `Failed:` line at all. Source: the #791 baseline artifact `p0-t10-quickfiler-tests.md` lines 14-22,
  which itself derives its failed count from the TRX `ResultSummary/Counters` element rather than
  from the console. Any acceptance condition in this plan that reads `Failed: 0` from console output
  is unsatisfiable on a green run. Apply the reviewer's replacements in full, including the
  `EXIT-CODE-UT:` and `EXIT-CODE-QFT:` split with a single roll-up `EXIT_CODE:` field.

### ACCEPTED on reading, verified independently by the orchestrator

- B2 and B3. Nullable annotations. The three named files do open with `#nullable enable`, and the
  nullable gate runs with warnings as errors, so the declared seams must carry `?`.
- B4. Author the new QuickFiler.Test file in C# 7.3-compatible syntax. The orchestrator verified that
  `QuickFiler.Test/QuickFiler.Test.csproj` declares no `LangVersion` while
  `UtilitiesCS.Test/UtilitiesCS.Test.csproj` declares `Latest` at its line 18, that no `.cs` file in
  QuickFiler.Test carries a `#nullable enable` directive, and that 25 files in UtilitiesCS.Test do.
  The two apparent modern constructs in QuickFiler.Test are inside comments and compile nothing.
  Whether or not the compiler default is literally 7.3, authoring the new file in the conservative
  syntax costs nothing and removes a real build risk, so this is applied as a precaution.
- B7. The double-quoted PowerShell pattern does not parse, and zero matches is this task's success
  outcome, so the artifact needs `ExpectedExitCode: 1`. The reviewer observed the exit code directly
  through an allowed git invocation.
- B8. The command block assigns four variables and never derives or prints the two values its
  acceptance reads.
- B11. The AC7 row-suppression branch is delivered by a task but executed by no test, so it would
  ship at zero hits and be checked off on the strength of provider-level tests that cannot reach it.
  This is the most consequential finding in the report and it is an acceptance-criterion delivery
  gap, not a style issue.
- B12. The 21-line headroom is not achievable, and the later ceiling gate then has no remedy.
- M1, M2, M3, M5. Counting and census errors, each checkable by reading.
- M6, M7, M8, m1, m2, m3, m4. Apply as written.

### REJECTED, refuted by recorded observation

- B9. The claim that full-framework MSBuild is not on PATH, and the consequent rewrite of eight
  tasks to resolve and invoke a resolved MSBuild path, is refuted. The #791 baseline artifact
  `p0-t3-nuget-restore.md` records the command
  `msbuild TaskMaster.sln /t:Restore /m /p:RestorePackagesConfig=true /p:Configuration=Debug "/p:Platform=Any CPU"`
  with `EXIT_CODE: 0` on this host, invoked as a bare command name, and the #791 plan's later gate
  builds use the same bare form and are recorded complete. Do NOT apply B9. Keep the bare `msbuild`
  invocations. The reviewer could not test PATH and asserted an unverifiable negative.

### NARROWED

- B1. The refusal of `pwsh` is a property of the isolated agent-worktree sandbox the reviewer ran in,
  not a property of this plan. Execution of this plan happens later, in the execution phase, and the
  identical command shapes are recorded as successfully executed in the #791 run. Do NOT add the
  proposed POSIX download-and-extract substitute for the repository SDK installer: it hard-codes a
  download URL, duplicates a maintained script, and widens scope. Apply only this narrowed clause,
  which records the requirement and forbids silent substitution:

  ```markdown
  **R11b — Execution-environment clause.** Every command block in this plan is a PowerShell block and
  requires a session in which `pwsh` may be invoked. A worktree-isolated agent session refuses every
  Bash invocation of `pwsh`, in both the `-Command` and the `-File` form. The executor records, in
  `<FEATURE>/evidence/baseline/p0-t3-sdk.md`, the derived line `EXEC-ENVIRONMENT: pwsh-permitted` once
  it has confirmed a PowerShell block runs. An executor that cannot obtain such a session reports
  BLOCKED at that task and stops; it must not substitute an unrecorded command shape for a documented
  one, because a substituted shape is unreviewed and its success-case output is unobserved.
  ```

- B10. The claim that the global-tool directory must be prepended to PATH is unproven here: the #791
  artifact `p0-t5-dotnet-coverage.md` records `dotnet-coverage --version` exiting 0 with the
  probe-only branch taken, so the tool resolves on this host without any PATH amendment. Apply only
  the `Get-Command` probe correction, which is sound on its own terms because an unresolvable command
  name raises a terminating error rather than setting an exit code, so the existing branch condition
  reads a value that does not exist. Do not assert in the plan that the PATH amendment is required;
  state it as a conditional applied only when the install branch is taken.

## Reviewer determinations the orchestrator confirms and does not reopen

D5, D7 and D8 were each checked by the reviewer against the tree and confirmed. The AC7 zero-candidate
restriction, the AC8 verification-only disposition, the relocation ordering that keeps the 500-line
file within its ceiling, and the absence of any unsatisfiable no-growth gate were all verified. The
ordering-integrity sweep across all 74 tasks found no violation and no task body swap is required.
