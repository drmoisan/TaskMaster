# Supplementary Pass Over Untracked, Unignored Files (P1-T5) — discharges AC-7

- **Issue:** #635
- **Plan task:** [P1-T5]

Timestamp: 2026-08-29T06-30

## Output Summary

Nine untracked, unignored files were present in the worktree when this pass ran, and all nine are this
item's own evidence artifacts. Five of them contain one or more of the thirteen identifiers, in every
case because the artifact quotes the identifiers it is auditing. No untracked, unignored file outside
this item's own feature folder and outside the .claude tree contains any of the thirteen. The claim
this item can make is therefore "no tracked file and no untracked, unignored file references a removed
member", not merely "no tracked file does".

UNTRACKED_FILES: 9
UNTRACKED_HIT_FILES_OUTSIDE_SCOPE: 0

## Command

Command:

```
pwsh -NoProfile -Command '$f = git ls-files --others --exclude-standard; Write-Output ("UNTRACKED_FILES=" + $f.Count); $f | ForEach-Object { Write-Output ("FILE " + $_) }; $outside = 0; foreach ($p in $f) { if (Test-Path -LiteralPath $p -PathType Leaf) { $m = @(Select-String -LiteralPath $p -SimpleMatch -Pattern "WireUpKeyboardHandler","AnyOpenDropDownsAsync","LoadGroups_02cAsync","LoadGroups_02bAsync","LoadGroup_03bAsync","LoadConversationsAndFoldersAsync","LoadItemGroup","LoadSequentialAsync","LoadGroupSequential","CacheTlpForMove","SwapTlp","CaptureTlpTemplate","_templateTlp" -ErrorAction SilentlyContinue); if ($m.Count -gt 0) { Write-Output ("HIT " + $p + " " + $m.Count); if (-not $p.StartsWith("docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/") -and -not $p.StartsWith(".claude/")) { $outside = $outside + 1 } } } }; Write-Output ("UNTRACKED_HIT_FILES_OUTSIDE_SCOPE=" + $outside)'
```

Output, verbatim:

```
UNTRACKED_FILES=9
FILE docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t2-requirements-inputs.2026-08-29T04-55.md
FILE docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t3-worktree-baseline.2026-08-29T04-55.md
FILE docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t4-identifier-derivation.2026-08-29T04-55.md
FILE docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t5-scope-census.2026-08-29T04-55.md
FILE docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/phase0-instructions-read.2026-08-29T04-55.md
FILE docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t1-partition-a-sweep.2026-08-29T04-55.md
FILE docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t2-partition-a-control.2026-08-29T04-55.md
FILE docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t3-partition-b-classification.2026-08-29T04-55.md
FILE docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t4-partition-c-enumeration.2026-08-29T04-55.md
HIT docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t2-requirements-inputs.2026-08-29T04-55.md 13
HIT docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t4-identifier-derivation.2026-08-29T04-55.md 36
HIT docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t1-partition-a-sweep.2026-08-29T04-55.md 4
HIT docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t3-partition-b-classification.2026-08-29T04-55.md 1
HIT docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t4-partition-c-enumeration.2026-08-29T04-55.md 72
```

EXIT_CODE: 0

The `pwsh -NoProfile -Command` wrapper exits `0` regardless of what runs inside it, so only the printed
values are asserted. The asserted value is `UNTRACKED_HIT_FILES_OUTSIDE_SCOPE=0`, which is the last
printed line.

## Enumerated list of files searched

The nine `FILE` lines above are the enumerated list of untracked, unignored files searched. All nine
are Markdown evidence artifacts under this item's own feature folder, written by [P0-T1] through
[P1-T4] before this task ran. The item's plan file and its specification do not appear in this list
because both are tracked; their modifications are visible to `git status --porcelain` and are recorded
by [P0-T3], [P4-T2] and [P4-T8], not here.

`git ls-files --others --exclude-standard` lists exactly the untracked-and-unignored set, so the
enumeration is complete for that set by construction.

## Enumerated hits

| File | Matching lines |
|---|---|
| docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t2-requirements-inputs.2026-08-29T04-55.md | 13 |
| docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/baseline/p0-t4-identifier-derivation.2026-08-29T04-55.md | 36 |
| docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t1-partition-a-sweep.2026-08-29T04-55.md | 4 |
| docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t3-partition-b-classification.2026-08-29T04-55.md | 1 |
| docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/evidence/other/p1-t4-partition-c-enumeration.2026-08-29T04-55.md | 72 |

Every hit file is one of this item's own artifacts. The [P0-T2] artifact lists the thirteen
identifiers as its search set; the [P0-T4] artifact quotes the removed declaration line for each of
them; the [P1-T1] artifact records the sweep command and its pattern list; the [P1-T3] artifact names
one identifier when explaining the stem-collision case; and the [P1-T4] artifact reproduces the
31-line hit set verbatim and enumerates it. None of these is a caller of any kind. Each is Markdown
prose that quotes the identifiers it is auditing.

## The two carve-outs from the outside-scope counter

Two path prefixes are excluded from the `UNTRACKED_HIT_FILES_OUTSIDE_SCOPE` counter, and each is
stated here with its reason:

1. A hit under `docs/features/active/2026-08-26-issue-468-residual-reflective-caller-risk-635/` is one
   of this item's own artifacts quoting the identifiers it is auditing. Counting it would make the
   measurement self-defeating: an artifact recording the search set necessarily contains the search
   set.
2. A hit under `.claude/` is agent-memory prose, which is exactly category E of the Phase 1
   classification recorded by [P1-T3]. It is authored bookkeeping, not a caller.

Neither carve-out hides a hit. Every hit file is printed by a `HIT` line before the carve-out test is
applied, and every printed `HIT` line is reproduced above. The carve-outs only exclude a file from the
outside-scope counter; they do not exclude it from the enumeration.

At this run, no file matched either carve-out condition from outside the feature folder: all five hit
files are under the feature folder and none is under the .claude tree.

## Auditable-absence record

SearchScope: the untracked, unignored files of this worktree, enumerated by `git ls-files --others --exclude-standard` and listed in full above as nine `FILE` lines. The measured scope size is 9 files. Each file was read with `Select-String -LiteralPath`, and the `Test-Path -LiteralPath ... -PathType Leaf` guard restricts the read to regular files.

SearchPatterns: the thirteen identifiers `WireUpKeyboardHandler`, `AnyOpenDropDownsAsync`, `LoadGroups_02cAsync`, `LoadGroups_02bAsync`, `LoadGroup_03bAsync`, `LoadConversationsAndFoldersAsync`, `LoadItemGroup`, `LoadSequentialAsync`, `LoadGroupSequential`, `CacheTlpForMove`, `SwapTlp`, `CaptureTlpTemplate`, `_templateTlp`, matched with `-SimpleMatch` so no character is interpreted as a regular-expression metacharacter. Identifier 7 is supplied as the bare stem, the broader form.

SearchResult: five files matched, all of them this item's own evidence artifacts under its own feature folder, enumerated with their per-file matching-line counts in the table above. Zero files matched outside this item's feature folder and outside the .claude tree.

## Host-identity hygiene of this command

Only the file path taken from the enumeration variable is printed. No resolved PowerShell provider
path is printed, because a resolved provider path carries the host account name. The `HIT` lines and
the `FILE` lines both print `$p` and `$_` directly, which hold the repository-relative paths that
`git ls-files` emits.
