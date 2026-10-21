# P4-T1 — CSharpier format pass (Phase 4, loop iteration 1)

Timestamp: 2026-08-28T03-56
Task: [P4-T1]
LoopIteration: 1
Command: dotnet tool run csharpier format .
EXIT_CODE: 0

Run from the worktree root through `dotnet tool run`, so the manifest-pinned CSharpier 1.2.6 from the
repository-root `dotnet-tools.json` is used. No globally installed CSharpier was invoked.

## Command output

```
Formatted 1547 files in 2086ms.
```

**`Formatted 1547 files` is the PROCESSED count, not the rewritten count, and is not a gate.**
CSharpier reports how many files it parsed and formatted, whether or not the result differed from what
was on disk. Reading it as a rewrite count would report 1547 rewrites on a tree where nothing changed.
The rewritten count is therefore derived independently, below.

## Derived rewritten count — SHA-256 manifest comparison

A manifest of `SHA-256  <repo-relative path>` rows was built over **all tracked `*.cs`, `*.xml` and
`packages.config` files** (enumerated with `git ls-files`) immediately **before** the command and again
immediately **after** it. Both manifests were written to the system temporary directory, outside the
repository, so the measurement cannot dirty the tree it measures.

| | Before | After |
|---|---|---|
| Manifest rows | 1850 | 1850 |
| Aggregate SHA-256 of the manifest | `ac415e81b3d5ad61885fa1aac8063e2d79e3a2b3cea5145dce0c025b58024e44` | `ac415e81b3d5ad61885fa1aac8063e2d79e3a2b3cea5145dce0c025b58024e44` |

| Comparison | Count |
|---|---:|
| Files added | 0 |
| Files removed | 0 |
| Files whose SHA-256 changed (**rewritten**) | **0** |

The two aggregate hashes are identical, so not one of the 1850 hashed files differs by a single byte.

Corroboration: `git status --porcelain` immediately after the format pass printed **zero lines**. Both
independent signals agree that the format pass rewrote nothing.

This is the expected outcome. The two files this remediation edited were each checked against CSharpier
individually at the moment they were edited — `QfcItemController.EventWiringTests.Part2.cs` after P1-T1
and `QfcItemController.EventWiring.cs` after P2-T1, both reporting `Checked 1 files` with exit `0` —
because each new line mirrors the formatting of the neighbours it sits among.

## No restart is triggered

Convention 11 requires Phase 4 to restart from P4-T1 if any stage rewrites a file. The derived
rewritten count is 0, so no rewrite occurred and loop iteration 1 continues to P4-T2.

## Acceptance

| P4-T1 condition | Result |
|---|---|
| `EXIT_CODE: 0` | **Yes** — observed `0` |
| Derived rewritten count is 0 | **Yes** — 0 of 1850 hashed files changed; identical aggregate hashes; empty porcelain |

Output Summary: `dotnet tool run csharpier format .` exited **0**, reporting
`Formatted 1547 files in 2086ms.` — a processed count, deliberately not used as a gate. The gate figure
is the **derived rewritten count of 0**, obtained by SHA-256 manifest comparison over all 1850 tracked
`*.cs`, `*.xml` and `packages.config` files taken immediately before and after the command: 0 added, 0
removed, 0 changed, with byte-identical aggregate manifest hashes
`ac415e81b3d5ad61885fa1aac8063e2d79e3a2b3cea5145dce0c025b58024e44` on both sides, corroborated by an
empty `git status --porcelain`. No file was rewritten, so no Phase 4 restart is triggered and loop
iteration 1 proceeds.
