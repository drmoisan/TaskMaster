---
name: msbuild-analyzer-gate-vacuous-without-rebuild
description: An msbuild /t:Build analyzer gate that follows any earlier build of the same tree compiles NOTHING and returns EXIT 0 — always use /t:Rebuild plus a csc.exe-count non-vacuity proof
metadata:
  type: project
---

MSBuild's legacy non-SDK up-to-date check is **timestamp-based and does not invalidate on a `/p:`
change**. So an analyzer gate written as

```
msbuild TaskMaster.sln /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

that runs after any earlier build of the same tree skips `CoreCompile` for every project and reports
`EXIT 0` having analyzed **nothing**. Measured on #505: **18 `Skipping target "CoreCompile"`, 0
`csc.exe` invocations, EXIT 0.** The same command as `/t:Rebuild` gives 18 `csc.exe` invocations.

This is vacuous *by construction* in any plan where the analyzer step follows an implementation-phase
build or a previous QC-loop iteration — which is essentially every plan. CI escapes it only because a
runner starts from a clean checkout; `.github/workflows/ci.yml:106-112` states this exact rationale
for its own `/t:Rebuild` type-check step.

**How to apply.** In every plan's analyzer and baseline-analyzer task:

1. Use `/t:Rebuild`, never `/t:Build`.
2. Add a file log and read a count back out of it as a mandatory acceptance condition:
   `/fl "/flp:logfile=<REPO>\coverage\analyzer.log;verbosity=normal"`, then count
   `PathToTool=.*csc\.exe` matches. **The count must be > 0 and must include the projects you
   touched.** `EXIT_CODE: 0` with a zero `csc.exe` count is a FAILED gate, not a passing one.
3. Apply the same treatment to the *baseline* task, or the final comparison silently depends on
   execution order.
4. `/nodeReuse:false` is worth adding: `/m` parallel rebuilds leave ~17 resident MSBuild worker
   processes that saturate the box and destabilize the subsequent test run.

**Why this matters beyond analyzers:** it is the general shape of a vacuous gate — a command whose
exit code is green because it did no work. Pair it with
[[preflight-catches-vacuous-gates]]: the structural MCP plan validator returns `ok:true` on a plan
full of these; only the `atomic-executor` preflight, *running the command and measuring*, catches
them. On #505 the preflight caught this empirically and it was the single highest-value finding of
the run.

Tool paths and the Bash-mangling caveat: [[bash-tool-mangles-msbuild-switches]].
