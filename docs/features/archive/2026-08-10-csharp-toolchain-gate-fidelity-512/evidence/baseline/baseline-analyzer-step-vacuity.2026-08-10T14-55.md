# Baseline — the documented ANALYZER step is vacuous by the same mechanism (scope finding)

Timestamp: 2026-08-10T14-55
Branch: bug/csharp-toolchain-gate-fidelity-512 (from origin/epic/build-ci-coverage-gate-fidelity-integration @ edf3d34c)
MSBuild: C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe

This measurement was not requested by any of the four issues. It was taken because the required
outcome for this feature is that the documented toolchain commands "actually execute" and "actually
enforce", and step 2 of the same documented block was untested.

## Measurements

Metric definitions: `CoreCompileSkips` counts occurrences of `Skipping target "CoreCompile" because
all output files are up-to-date with respect to the input files.` in the MSBuild file log. Errors are
counted from node-prefixed lines only, to avoid the summary-block double count.

| Run | Command | EXIT | Elapsed | CoreCompileSkips | Errors |
|---|---|---|---|---|---|
| A1 | documented analyzer step, `/t:Build` | 0 | 22.1 s | 3 | 0 |
| A2 | **the same command again, immediately** | **0** | **1.5 s** | **18 of 18** | 0 |
| A3 | analyzer properties under `/t:Rebuild /m` | 0 | 19.0 s | 0 | 0 |
| A4 | documented nullable step, `/t:Build`, warm | 0 | 1.7 s | 18 of 18 | 0 |

Commands:

- A1/A2: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- A3: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- A4: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`

## Finding

**The documented analyzer step (`CLAUDE.md` step 2, `.claude/rules/csharp.md:15`,
`.claude/skills/csharp-qa-gate/SKILL.md:31`) is vacuous under exactly the same mechanism as the
type-check step.** Run A2 skipped `CoreCompile` on all 18 projects and returned EXIT 0 in 1.5 seconds
having compiled nothing. Analyzer diagnostics are produced during compilation, so a build that skips
compilation runs no analyzers.

The defect is not merely "a repeated build is fast". It is that **outputs produced under one property
set are silently accepted as validating a different property set.** MSBuild's up-to-date check
compares file timestamps and does not invalidate on a command-line `/p:` change. The mandatory
toolchain loop specified in `CLAUDE.md` § "After Making Changes" makes this concrete: the loop runs
format, then analyzer, then type-check, then test, and must restart from step 1 whenever any step
changes files. On every pass after the first compilation, whichever of steps 2 and 3 did not most
recently force a compile is validated against binaries built with the other one's properties.

## Why CI is not affected in the same way

`.github/workflows/ci.yml` runs its analyzer step with `/t:Build` on a fresh runner checkout where no
outputs exist, so that build genuinely compiles. Its subsequent nullable step uses `/t:Rebuild`
explicitly. CI therefore compiles under both property sets. The defect is specific to the documented
local command sequence, which is executed repeatedly in a warm working tree.

## Scope question raised for the spec

Issues #492, #509, #512 and #522 enumerate the format command and the type-check command. None
enumerates the analyzer command. Three positions are available:

1. **Correct the analyzer command too.** It sits in the same documented block, at the same sites, and
   fails the same required-outcome test ("actually execute, actually enforce"). Leaving it uncorrected
   ships a toolchain block in which step 3 is honest and step 2 is not.
2. **Leave it and file a follow-up issue.** The epic charter constrains this feature to "exactly the
   sites the issues enumerate" and warns that no child may edit a governance document outside its own
   issue's acceptance criteria.
3. **Correct it and record the widening explicitly**, on the basis that the epic's stated required
   outcome governs and that the analyzer command occupies the same lines being rewritten.

This decision belongs in `spec.md` and must be made explicitly rather than by default. Note that
position 2 still requires a decision about what the corrected step-3 text says, because a reader who
sees `/t:Rebuild` at step 3 and `/t:Build` at step 2 will reasonably infer the difference is
intentional. If the analyzer command is left as-is, the rationale for the asymmetry must be stated
in-line, or the documentation will be internally misleading in a new way.

## Measured cost input to the decision

A `/t:Rebuild` analyzer pass costs 19.0 s against 1.5 s for the vacuous warm `/t:Build` and 22.1 s for
a genuine cold `/t:Build`. If both step 2 and step 3 move to `/t:Rebuild`, a full toolchain loop pass
performs two full solution rebuilds at roughly 19-20 s each. Whether to collapse steps 2 and 3 into a
single `/t:Rebuild` invocation carrying both property sets is a design option the spec should evaluate
against the policy requirement that the stages remain distinguishable.
