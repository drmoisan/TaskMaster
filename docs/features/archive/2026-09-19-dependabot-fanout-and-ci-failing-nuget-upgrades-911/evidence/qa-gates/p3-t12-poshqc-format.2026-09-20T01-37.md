# PoshQC Format After Phase 3 — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T09-03-10
- Task: [P3-T12]
- Command: CMD-POSHQC-FORMAT, then CMD-REVERT-OUT-OF-SCOPE-FORMAT
- EXIT_CODE: 0

## The Exact `scan_folders` Argument

```json
["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]
```

MCP result: `ok:true`. Per **gate rule 6** the tool's summary is not the rewrite count.

## Rewrite Count

| Measurement | Value |
|---|---|
| Files hashed, before and after | **46** each |
| Files whose SHA-256 changed across the invocation | **0** |
| Derived out-of-scope pathspec | `REVERT-SET: empty` |
| **Post-revert hash-difference count, excluding derived-set members** | **0** |

Because the count is 0, the phase does **not** restart from [P3-T11].

## Hash Sets, Before and After

**Manifest digest**, the SHA-256 over the sorted `path SHA256` manifest:

| Set | Manifest digest |
|---|---|
| Before this invocation | `8723C3E65C2CC479DD7A2E57033BE48F446DA1A5DCC94DF61874842FF2CFF15B` |
| After this invocation | `8723C3E65C2CC479DD7A2E57033BE48F446DA1A5DCC94DF61874842FF2CFF15B` |

Equal, by two independent methods: element-wise comparison of all 46 keys, and the whole-manifest
digest.

The digest differs from the [P2-T8] value of
`DF69F6059117D42C5883A339A6725372E024DC8E4431550B7FC496E83579B435`, which is the positive check
that Phase 3 edited PowerShell files between the two gates — the two test files [P3-T13]
enumerates.

## CMD-REVERT-OUT-OF-SCOPE-FORMAT

`REVERT-SET: empty`. The hash-difference set is empty, so `git checkout --` was not run.
`scripts/vscode/Invoke-MSTest.ps1` and `scripts/vscode/Invoke-MSTestWithCoverage.ps1` are
unchanged and did not enter the derived set.

## Porcelain, `scripts/vscode`

Pre-revert and post-revert, both `(empty)`. No derived-set member is listed.

## A Note on Scope

The formatter's scan set is the four PowerShell folders. `.github/workflows/dependabot-repair.yml`
is YAML and is outside it, so the six workflow edits this phase made are not covered by this
gate. Their static validity is covered by [P3-T9]'s actionlint run instead.

## Output Summary

The formatter rewrote **0 of 46** files. The six tests and one helper Phase 3 added to the two
test files were formatter-clean as written, including the whitespace compaction [P3-T5] applied
to stay under the phase ceiling. `REVERT-SET: empty`. The phase does not restart.
