# Evidence Identity Redaction — Pre-PR Sanitization

Timestamp: 2026-08-26T12-05
Author: child orchestrator for `bug/breadcrumb-router-navigation-defects-498`
Scope: this feature's evidence tree and research artifact only.

## Why this was done

The repository rule is that no artifact may embed an absolute host path, an operating-system account
name, or a machine name. Two categories of artifact in this feature folder violated it:

1. **Thirty-two `vstest.console.exe` TRX files.** vstest writes identity into the TRX body itself —
   `TestRun/@name`, `TestRun/@runUser`, `TestRun/@computerName` and `Deployment/@runDeploymentRoot` —
   and there is no switch that suppresses it. Controlling `/ResultsDirectory:` and `LogFileName=`
   changes the file name only. Failure stack traces additionally carried the absolute worktree path.
2. **One research artifact** carried the absolute preparation worktree path in its header.

A prior execution of this feature recorded a decision to leave TRX bodies unmodified, reasoning that
editing tool-generated evidence would falsify the artifact the plan's gates read. That reasoning is
sound about the *gate signal* but does not extend to the *identity attributes*, which no gate reads.
The sequencing below preserves the original concern while satisfying the rule.

## Sequencing

All plan gates in Phases 0 through 8 were executed and recorded against **unmodified** tool output.
This redaction was applied only after `P8-T8`, the final plan task, completed. No gate was re-run
against redacted input and no gate result was derived from a redacted file.

## Substitutions applied

| Original token | Replacement in `*.trx` | Replacement in Markdown |
|---|---|---|
| absolute worktree path | `REDACTED-REPO-ROOT` | `<repo-root>` |
| machine name | `REDACTED-HOST` | `<host>` |
| account name | `REDACTED-USER` | `<user>` |

Angle-bracket placeholders are used in Markdown but not in TRX. `<` is not legal inside an XML
attribute value, so bracket-free tokens are used in the XML artifacts to keep them well-formed.

## Verification

| Check | Command basis | Result |
|---|---|---|
| Residual account, machine, or user-profile path anywhere in the feature folder | recursive fixed-pattern search for the account name, machine name, and `<drive>:\Users\` | **0 matches** |
| TRX well-formedness after redaction | `[xml](Get-Content -Raw ...)` over every TRX | **32 checked, 0 malformed** |
| Aggregate test counters after redaction | `TestRun/ResultSummary/Counters` summed over every TRX | total 206, passed 192, failed 14 — the 14 are the seven `[expect-fail]` tasks at two tests each |
| Per-file outcome multiset unchanged | multiset of `outcome="..."` compared against the committed `HEAD` version of each file | **no file changed** |
| Per-file test-name set unchanged | sorted set of `testName="..."` hashed and compared against `HEAD` | **no file changed** |

The last two checks are the load-bearing ones: they prove the redaction altered no test identity and
no test result, so every gate conclusion recorded elsewhere in this evidence tree remains supported by
the artifact it cites.

## Files touched

Thirty-two `results.trx` files under `evidence/baseline/trx/`, `evidence/regression-testing/trx/` and
`evidence/qa-gates/trx/`; `evidence/other/orchestrator-cross-cutting-findings.2026-08-26T01-30.md`
(one quoted archive filename); and
`research/2026-08-24T09-50-breadcrumb-router-navigation-defects.md` (one header line).

No production or test source file was touched by this redaction.

## Note on the wider repository

Fifty TRX files elsewhere in the repository carry the same identity attributes. Those lie outside this
feature's ownership set and are not changed here. The durable fix remains a post-processing step in
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` that rewrites those attributes before a TRX is copied
into any evidence folder, applied repository-wide. That is recorded as a follow-up in
`evidence/other/orchestrator-cross-cutting-findings.2026-08-26T01-30.md`.
