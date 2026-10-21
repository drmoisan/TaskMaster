# P0-T6 — Analyzer include path resolution

Timestamp: 2026-09-13T04-57
Command: CMD-ANALYZERPATHS, run verbatim from a session scratchpad script file
EXIT_CODE: 0

## How the command was invoked

CMD-ANALYZERPATHS contains two regular expressions whose literal text is a doubled backslash. The
Bash tool available to this executor collapses a doubled backslash when the payload is passed inline
to a native executable, which turns the first pattern into an invalid escape and makes the filter
throw once per project. That failure mode is silent in the sense that matters: every project is
filtered out, the enumeration inspects nothing, and the command reports zero missing paths. The
first inline attempt did exactly that and produced a vacuous zero.

The command was therefore written verbatim, including both doubled-backslash patterns, into a
session scratchpad script file outside the repository and invoked with `pwsh -NoProfile -File`. The
script adds two diagnostic counters so that a vacuous run is distinguishable from a genuine one:
the number of projects the filter admitted and the number of analyzer include items inspected. A run
that reports a nonzero count for both has actually inspected the tree.

## First run, before remediation

```
PROJECTS-ENUMERATED: 18
ANALYZER-ITEMS-CHECKED: 162
MISSING-COUNT: 15
MISSING: <worktree-root>\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll
   (repeated for 15 of the 16 first-party projects)
```

## Diagnosis

The skew is pre-existing on the merged base and is not produced by anything in this plan. In 15 of
the 16 first-party projects, the analyzer item group names Meziantou.Analyzer version 3.0.203 while
the same project's packages.config entry, its restore-guard Error condition and its props Import all
name version 3.0.235. One project, the add-in project, already carries 3.0.235 on its analyzer item
and accounts for the difference between 16 and 15. Verified example: the QuickFiler project file
carries the 3.0.235 guard at line 589 and the 3.0.203 analyzer item at line 595.

A missing analyzer path is compiler error CS0006 rather than a warning, so every later build gate in
this plan is unreachable while the skew stands.

## Remediation applied, exactly as the acceptance condition directs

The acceptance condition directs that a non-empty result be remedied by installing the named package
version into the packages directory, with the version derived from the project files rather than
written into the condition. The version the project files name is 3.0.203. That package version was
downloaded from the public package feed and extracted to the packages directory of this worktree at
`packages/Meziantou.Analyzer.3.0.203`, and the analyzer assembly the project files reference was
confirmed present at the extracted path.

No tracked file was edited to achieve this. The packages directory is matched by the repository
ignore file at ignore-file line 191 through the pattern for a packages directory, verified with
`git check-ignore -v`, so the install is invisible to every diff and porcelain gate in this plan and
widens no scope. The alternative remedy — rewriting the 15 project files to name 3.0.235 — was
rejected because 14 of those files are outside this item's Write Set.

## Second run, after remediation

```
PROJECTS-ENUMERATED: 18
ANALYZER-ITEMS-CHECKED: 162
MISSING-COUNT: 0
```

## Post-remediation porcelain status

`git status --porcelain --untracked-files=all` reported only the Phase 0 plan check-off and the five
Phase 0 evidence artifacts written so far. It reported no path under the packages directory and no
project file, which confirms the remediation touched no tracked content.

## Defect recorded for the orchestrator, not fixed here

The stale analyzer include version in 15 first-party project files is a pre-existing repository
defect that will recur in every fresh worktree until the project files are corrected upstream. It is
outside this item's Write Set and is recorded here for the orchestrator rather than repaired.

Output Summary: CMD-ANALYZERPATHS initially reported 15 lines beginning with the token `MISSING:`,
all naming the Meziantou analyzer assembly at version 3.0.203. The named version was installed into
the gitignored packages directory as the acceptance condition directs. The re-run inspected 18
project files and 162 analyzer include items and emitted zero lines beginning with `MISSING:`.
Acceptance met.
