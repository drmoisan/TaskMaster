# `CLAUDE.md` § UT2 protected-section guard ([P3-T6])

Timestamp: 2026-08-10T23-42
Command: `git show <MERGE_BASE>:CLAUDE.md | sed -n '/^### UT2. Coverage and Scenarios/,/^### UT3. Test Structure and Diagnostics/p' | sha256sum` and `sed -n '/^### UT2. Coverage and Scenarios/,/^### UT3. Test Structure and Diagnostics/p' CLAUDE.md | sha256sum`
EXIT_CODE: 0

`MERGE_BASE` = `a5e336e5ae3443d4197caf5f87036fae1d538f89`
(`FEATURE/evidence/baseline/baseline-git-context.2026-08-10T22-35.md`).

## Extraction boundaries

Both extracts run from the line `### UT2. Coverage and Scenarios` through the line
`### UT3. Test Structure and Diagnostics`, inclusive of both boundary lines.

## Comparison

| Measurement | Merge base | Working tree | Identical |
|---|---|---|---|
| SHA-256 of the extract | `d4d95bbfc15578320e4996cf0d9872c4cee1ea6d9a9ae08cc282acdf62dbdf68` | `d4d95bbfc15578320e4996cf0d9872c4cee1ea6d9a9ae08cc282acdf62dbdf68` | **YES** |
| Line count | 28 | 28 | YES |
| Byte count | 2262 | 2262 | YES |

**Differing lines: 0.** The two extracts are byte-identical.

## Recorded measurement-method correction

A first attempt compared the two extracts inside PowerShell using `Compare-Object` over
`(& git show ...)` output. That comparison reported **2** differing lines, both being the same line
rendered with a hyphen in the merge-base side and an em dash (`—`) in the working-tree side. That was
a **console output-encoding artifact** of piping `git show` through PowerShell, not a content
difference: the byte-exact SHA-256 and byte-count comparison above, performed without any
re-encoding step, shows the two extracts are identical. The method is recorded so the false positive
is not repeated and so the negative claim here is auditable.

## Corroboration from the full-file diff

`git diff <MERGE_BASE> -- CLAUDE.md` produces exactly four hunks, at these locations, none of which
falls inside § UT2:

| Hunk | Section | Owning task |
|---|---|---|
| `@@ -185,25 +185,31 @@` | § C#1 items 1, 2 and 3 (format, analyzer, type-check) | [P3-T1], [P3-T2], [P3-T3] |
| `@@ -378,9 +384,9 @@` | § CUT3 numbered commands | [P3-T4] |
| `@@ -396,9 +402,9 @@` | § "C# Toolchain (run in this exact order)" | [P3-T5] |

(The first `@@` block covers the contiguous § C#1 edits made by [P3-T1] through [P3-T3].)

## Output Summary

`CLAUDE.md` § UT2 is **byte-identical** to its merge-base text: identical SHA-256
(`d4d95bbf...`), identical 28-line / 2262-byte extents, **zero** differing lines. The protected
section was not touched, so no revert is required and Phase 4 may begin. This check is re-run against
the final working tree by [P5-T13] (AC9).
