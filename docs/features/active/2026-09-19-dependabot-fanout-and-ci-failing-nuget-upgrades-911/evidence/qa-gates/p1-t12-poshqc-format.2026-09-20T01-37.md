# PoshQC Format After Phase 1 — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-47-20
- Task: [P1-T12]
- Command: CMD-POSHQC-FORMAT, then CMD-REVERT-OUT-OF-SCOPE-FORMAT
- EXIT_CODE: 0

## Toolchain Re-Run Note

This gate was run twice and the figures below are from the **second** run.

The first run also reported 0 rewrites of 46. [P1-T13] then reported 22 analyzer findings against
the baseline of 13 — nine `PSReviewUnusedParameter` warnings in the seam delegates this phase had
added. Those were corrected in
`tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`, and per the toolchain rule in `CLAUDE.md`
the loop restarted at the formatter. This second run is that restart. The corrected file is
formatter-clean as written, so the rewrite count is 0 again and the phase does not restart from
[P1-T2].

## The Exact `scan_folders` Argument

```json
["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]
```

MCP result: `ok:true`. Per **gate rule 6**, the tool's summary is not the rewrite count.

## Rewrite Count

| Measurement | Value |
|---|---|
| Files in the hash set, before | **46** |
| Files in the hash set, after | **46** |
| Files whose SHA-256 changed across the invocation | **0** |
| Derived out-of-scope pathspec | `REVERT-SET: empty` |
| **Post-revert hash-difference count, excluding derived-set members** | **0** |

Because the post-revert rewrite count is **0**, the phase does **not** restart from [P1-T2].

## How the Hash Sets Are Recorded

The 46-element before and after sets are recorded in three complementary ways rather than by
reprinting the [P0-T6] table, which they equal element for element except where this phase's own
edit moved a file.

**1. Manifest digest.** The SHA-256 over the sorted `path SHA256` manifest, which is a single
value identifying the whole set:

| Set | Manifest digest |
|---|---|
| Before this invocation | `00A25502FF151F86613571C0E2C2FA8A62E71A1046BB35F5E7F5ED5B0085DD23` |
| After this invocation | `00A25502FF151F86613571C0E2C2FA8A62E71A1046BB35F5E7F5ED5B0085DD23` |

The two digests are equal, which is the same fact as the rewrite count of 0 and is independent of
it: the count was computed by element-wise comparison, the digest by hashing the whole manifest.

**2. The element-wise comparison.** Every one of the 46 keys was compared individually across the
invocation. Differing keys: **0**.

**3. The delta against the [P0-T6] baseline set.** Exactly one file moved between the [P0-T6]
after set and this invocation's before set, and it is the one file Phase 1 edited:

| Path | SHA-256 at [P0-T6] | SHA-256 before this invocation |
|---|---|---|
| `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` | `CD465C973E473FA5AFA7121B38ABC8F29ED193D0B180EBF8FDA5BEB3E29BADB7` | `FF029FC1402034C897FAEF6F09D2C2699E6878A9B2D752FA1B6E58FA13F9B818` |

The other 45 entries are byte-identical to the [P0-T6] table, which records all 46 in full. That
single moved entry is the positive check that this phase actually edited the file it claims to
have edited: a Phase 1 that added no test would show zero moved entries here.

## CMD-REVERT-OUT-OF-SCOPE-FORMAT

`REVERT-SET: empty`. The derived set is the hash-difference set minus the spec `## Write Set`
members; the hash-difference set is empty, so the derived set is empty and `git checkout --` was
not run.

`scripts/vscode/Invoke-MSTest.ps1` and `scripts/vscode/Invoke-MSTestWithCoverage.ps1` are
unchanged across this invocation and did not enter the derived set.

## Porcelain, `scripts/vscode`

Pre-revert:

```
git status --porcelain --untracked-files=all -- scripts/vscode
```

```
(empty)
```

Post-revert: identical, `(empty)`. No derived-set member is listed.

## Output Summary

The formatter rewrote **0 of 46** files. The eight `It` blocks Phase 1 added to
`tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` were already formatter-clean as written.
`REVERT-SET: empty`. The phase does not restart.
