# PoshQC Format After Phase 2 — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-54-00
- Task: [P2-T8]
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

Because the count is 0, the phase does **not** restart from [P2-T7].

## Hash Sets, Before and After

**Manifest digest**, the SHA-256 over the sorted `path SHA256` manifest:

| Set | Manifest digest |
|---|---|
| Before this invocation | `DF69F6059117D42C5883A339A6725372E024DC8E4431550B7FC496E83579B435` |
| After this invocation | `DF69F6059117D42C5883A339A6725372E024DC8E4431550B7FC496E83579B435` |

Equal. That is the same fact as the rewrite count of 0, reached independently: the count by
element-wise comparison of all 46 keys, the digest by hashing the whole manifest.

The digest differs from the [P1-T12] value of
`00A25502FF151F86613571C0E2C2FA8A62E71A1046BB35F5E7F5ED5B0085DD23`, which is the positive check
that Phase 2 did in fact edit files between the two gates. The five files it edited are the ones
[P2-T9] enumerates.

## CMD-REVERT-OUT-OF-SCOPE-FORMAT

`REVERT-SET: empty`. The hash-difference set is empty, so the derived set is empty and
`git checkout --` was not run. `scripts/vscode/Invoke-MSTest.ps1` and
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` are unchanged and did not enter it.

## Porcelain, `scripts/vscode`

Pre-revert and post-revert, both:

```
(empty)
```

No derived-set member is listed.

## Output Summary

The formatter rewrote **0 of 46** files. Every Phase 2 edit — the extracted function, the
threaded resolver call, the two call-site comments and the two new tests — was formatter-clean as
written. `REVERT-SET: empty`. The phase does not restart.
