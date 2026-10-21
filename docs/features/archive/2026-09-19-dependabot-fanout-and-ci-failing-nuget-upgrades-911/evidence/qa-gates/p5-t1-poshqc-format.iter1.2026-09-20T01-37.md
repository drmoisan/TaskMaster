# Final QA Step 1 — PoshQC Format, Iteration 1

- Timestamp: 2026-09-20T09-07-32
- Task: [P5-T1]
- Command: CMD-POSHQC-FORMAT, then CMD-REVERT-OUT-OF-SCOPE-FORMAT
- EXIT_CODE: 0

## The Exact `scan_folders` Argument

```json
["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]
```

MCP result: `ok:true`.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| **Post-revert hash-difference count, excluding derived-set members** | **0** | **0** | PASS |
| Post-revert `scripts/vscode` capture lists a derived-set member | no | **no** | PASS |

Because the count is **0**, the loop does **not** restart at [P5-T1]. This is the clean pass.

Per **gate rule 6** the tool's `Formatted N files` summary is **not** this count. The count is
the element-wise hash comparison below.

## Hash Sets, Before and After

| Measurement | Value |
|---|---|
| Files hashed, before | **46** |
| Files hashed, after | **46** |
| Keys whose SHA-256 differs | **0** |

**Manifest digest**, the SHA-256 over the sorted `path SHA256` manifest:

| Set | Manifest digest |
|---|---|
| Before this invocation | `8723C3E65C2CC479DD7A2E57033BE48F446DA1A5DCC94DF61874842FF2CFF15B` |
| After this invocation | `8723C3E65C2CC479DD7A2E57033BE48F446DA1A5DCC94DF61874842FF2CFF15B` |

Equal, by two independent methods.

The digest also equals the [P3-T12] value, which is the expected result: Phase 4 changed only
markdown under the feature folder and touched no PowerShell file, so the whole PowerShell tree
is byte-identical to its state at the end of Phase 3.

## Derived Pathspec

`REVERT-SET: empty`. The hash-difference set is empty, so the derived set — that set minus the
spec `## Write Set` members — is empty and `git checkout --` was **not run**.

## Porcelain, `scripts/vscode`

Pre-revert and post-revert, both:

```
(empty)
```

`scripts/vscode/Invoke-MSTest.ps1` and `scripts/vscode/Invoke-MSTestWithCoverage.ps1` are the
two files this revert exists to protect. Both are unformatted on `main`, clean under the PoshQC
ruleset CI runs, and untouched by this cycle. Neither was rewritten and neither entered the
derived set.

## Output Summary

The formatter rewrote **0 of 46** files on the first iteration. `REVERT-SET: empty`. Step 1 of
the final loop passes without a restart.
