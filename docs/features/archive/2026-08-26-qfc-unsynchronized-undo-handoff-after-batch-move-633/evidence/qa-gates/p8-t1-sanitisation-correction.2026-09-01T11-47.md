# Corrective evidence sanitisation sweep (correction to P8-T1)

Timestamp: 2026-09-01T11-47
Task: Corrective action against [P8-T1]. This is not a plan task and claims no task ID of its own;
`[P8-T2]` and `[P8-T3]` are the plan's commit and diff-scope tasks and are unrelated to this sweep.
Working directory: WORKTREE

Command: `pwsh -NoProfile -File <scratchpad>/sweep-hostpaths.ps1 -Root WORKTREE -ListFile <scratchpad>/changed.txt -Apply`
EXIT_CODE: 0

## Why this sweep was needed

The P8-T1 sweep substituted one value: the three spellings of this worktree's own absolute path. Its
reported remaining-match count of 0 was measured against that one value and was correct within that
scope, but its Output Summary generalised the result to every absolute host path in the tree. Every
other absolute host path and every bare host identifier was still present. The correction is recorded in
`p8-t1-sanitisation.2026-09-01T11-15.md`, which retains the original record and adds a dated section
stating the defect.

## Scope of this sweep

The file set is the branch's own changed set, derived mechanically rather than chosen:

```
git diff --name-only origin/main...HEAD
```

That command returned 74 paths. The sweep ran over exactly those paths and over nothing else. Sweeping
the whole repository was rejected deliberately: unrelated feature folders under `docs/` carry thousands
of pre-existing hits that this branch neither introduced nor owns, and including them would bury the
signal this gate is meant to produce.

## Substitution rules

Two identifier tokens were swept: the account name and the machine name. Neither is written in this
artifact, and no pre-substitution value is quoted anywhere in it. Quoting a removed identifier would
write it back into a committed file and would make this artifact a match on the next sweep, which is the
same failure mode that a `From:` column or a `BEFORE:` line in a sanitisation record produces.

Four token classes were matched, longest first, all case-insensitively, over the file decoded as UTF-8
rather than line by line:

| # | Token class matched | Replacement in Markdown and `.msbuild.txt` | Replacement in `.trx` |
|---|---|---|---|
| 1 | Drive-rooted path to the main checkout, either slash spelling | `<repo-root>` | `REDACTED_PATH` |
| 2 | Drive-rooted path to the user profile, either slash spelling | `<user-profile>` | `REDACTED_PATH` |
| 3 | Bare account name | `<user>` | `REDACTED_USER` |
| 4 | Bare machine name | `<host>` | `REDACTED_HOST` |

Two properties of the helper are load-bearing.

**The XML exception.** A TRX file is an XML document, and an angle-bracket placeholder written into an
attribute value makes that document malformed. The `.trx` branch of the substitution table therefore uses
bracket-free tokens. The distinction is by file extension, so it cannot be defeated by a Markdown file
that happens to contain XML.

**Case-insensitive matching in binary mode.** `vstest` writes the `storage=` attribute of every
`<UnitTest>` element entirely in lower case while writing the run-identity attributes in mixed case. A
case-sensitive pass over a mixed-case path therefore clears the visible header and leaves the lower-case
copy intact. Every match and every verification count in this artifact is case-insensitive, and
verification is a fixed-string sweep over file content rather than a read of the header.

The helper builds its path-separator character class at run time from a character code rather than
writing it as a literal, so a doubled separator cannot be silently de-doubled in transit and reduced to a
forward-slash-only pattern. The helper printed the character codes of the constructed class on each run
and they were confirmed to be the two-separator form before the sweep was applied. The helper lives in
the system scratchpad outside the repository and adds no file to the change footprint.

## Counts

Measured over the 74-path changed set with a case-insensitive fixed-string count of the two identifier
tokens, and separately with a case-insensitive regular-expression scan for any drive-rooted user-profile
path in any slash spelling.

| Measure | Before | After |
|---|---|---|
| Occurrences of the two identifier tokens | 366 | **0** |
| Files containing at least one such occurrence | 17 | **0** |
| Drive-rooted user-profile paths, any slash spelling | 289 | **0** |
| Files containing at least one such path | 9 | **0** |
| TRX files that parse as XML | 8 of 8 | **8 of 8** |

The 366 occurrences divide into 313 of the account token and 53 of the machine token.

## Files rewritten

17 files, all under `FEATURE/`. No file outside `docs/` was touched.

| Count | Files | Class of surviving token removed |
|---|---|---|
| 8 | `evidence/**/*.msbuild.txt` | Analyzer-configuration path into the main checkout, 36 occurrences per file |
| 8 | `evidence/**/*.trx` | Run-identity attributes naming the account and the machine |
| 1 | `plan.2026-08-31T19-35.md` | The note defining the `WORKTREE` constant by its literal absolute value |

The `.trx` run-identity attributes are `runUser`, `computerName`, the test-run `name`, and the
`runDeploymentRoot` on the `Deployment` element. The `storage=` attribute on each `<UnitTest>` element
already carried the `WORKTREE` token from the P8-T1 pass and was confirmed clean on entry.

The single plan-file occurrence was introduced by this plan's own execution: the note added to document
the `WORKTREE` substitution wrote the substituted value out in full. A sanitisation record that quotes
what it removed reinstates the identifier in a different file, which is why this artifact quotes none.

## Fixed point

The verification sweep was re-run after the substitution pass and reported 0 occurrences in 0 files, 0
drive-rooted user-profile paths, and 0 TRX parse failures. A sweep whose post-state count is 0 has no
further work to do, so that pass is the fixed point.

## Footprint

`git diff --name-only origin/main...HEAD` after the change lists 2 paths under `QuickFiler/`, 4 under
`QuickFiler.Test/`, and the remainder under `docs/`. The production and test file counts are unchanged by
this sweep, which modified no C# file and therefore required no toolchain re-run.

Output Summary: Corrective sweep over the branch's 74 changed paths rewrote 17 files and removed all 366
occurrences of the two host identifier tokens and all 289 drive-rooted user-profile paths. Post-sweep
counts are 0 for every measure, all 8 TRX files still parse as XML, and the branch footprint outside
`docs/` is unchanged at 2 production and 4 test files. EXIT_CODE 0.
