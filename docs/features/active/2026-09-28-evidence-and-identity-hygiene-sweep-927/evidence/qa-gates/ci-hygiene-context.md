# P6-T37 CI hygiene context capture on the pull-request head (AC16)

Timestamp: 2026-09-29T23-35
Command: git rev-parse HEAD; pwsh -NoProfile -Command 'gh api repos/drmoisan/TaskMaster/commits/67a69cb23916878c436fa570d587847b7a0745fa/check-runs --jq ".check_runs[] | [.name, .conclusion] | @tsv"'; pwsh -NoProfile -Command 'gh api repos/drmoisan/TaskMaster/rulesets/18572843 --jq ".rules[] | .parameters.required_status_checks[]?.context"'; GATE6: pwsh -NoProfile -Command '& ./scripts/hygiene/Test-RepositoryHygiene.ps1 2>&1 | Tee-Object -FilePath coverage/logs/927-guard-run.log; exit $LASTEXITCODE' (run from the item worktree root inside a System.Diagnostics.Stopwatch that printed GUARD-SECONDS= and GUARD-EXIT=)
EXIT_CODE: 0
Output Summary:
- HEAD-SHA: 67a69cb23916878c436fa570d587847b7a0745fa (git rev-parse HEAD; equal to the head the orchestrator reported for pull request 943, CI workflow run 36664415704)
- Check-runs on the head, transcribed verbatim (name, a tab, conclusion):

```text
build-nullable / Build with nullable warnings treated as errors	success
actionlint / actionlint	success
pester / Run Pester suite with coverage	success
build-analyzers / Build with analyzers and code style enforcement	success
format-check / Verify formatting	success
hygiene / Repository hygiene guard	success
mstest-coverage / Run MSTest suite with coverage	success
```

- HYGIENE-LINES=1 (exactly one check-run name begins `hygiene / `)
- CAPTURED-CONTEXT: hygiene / Repository hygiene guard
- CAPTURED-CONCLUSION: success
- Pre-existing contexts: actionlint / actionlint success; format-check / Verify formatting success; build-analyzers / Build with analyzers and code style enforcement success; build-nullable / Build with nullable warnings treated as errors success; mstest-coverage / Run MSTest suite with coverage success; pester / Run Pester suite with coverage success (six of six)
- Ruleset 18572843 required contexts (read-only GET), transcribed verbatim:

```text
actionlint / actionlint
format-check / Verify formatting
build-analyzers / Build with analyzers and code style enforcement
build-nullable / Build with nullable warnings treated as errors
mstest-coverage / Run MSTest suite with coverage
pester / Run Pester suite with coverage
```

- RULESET-HYGIENE-CONTEXTS=0 (no required context begins `hygiene / `)
- RULESET-MODIFIED: no (this change issued no ruleset PUT and did not edit the ruleset)
- CONFIRMING-RUN: HYGIENE Findings=0 (GATE6 after the P6-T36 commit; GUARD-EXIT=0; GUARD-SECONDS=39; enumeration is git ls-files, so every path committed by P6-T36 was inside the scan)
- AC16: MET (checked off in spec.md; the ledger TOTAL recount printed 18)
