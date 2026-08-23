# Scope Guard Before the QC Loop

- Task: `[P1-T7]`
- Timestamp: 2026-08-05T00-04
- Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`
- Evidence series: `2026-08-05T05-00`
- EXIT_CODE: 0 (both commands returned 0)

## Command 1 — `git status --porcelain`

```
 M SVGControl.Test/SVGControl.Test.csproj
 M SVGControl.Test/packages.config
 M docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/remediation-plan.2026-08-05T05-00.md
?? docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/other/excss-copy-local.2026-08-05T05-00.md
?? docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/regression-testing/order-paired-after.2026-08-05T05-00.md
?? docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/regression-testing/order-standalone-after.2026-08-05T05-00.md
?? docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/remediation-baseline/ac-source-check.2026-08-05T05-00.md
?? docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/remediation-baseline/build-basis.2026-08-05T05-00.md
?? docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/remediation-baseline/coverage-basis.2026-08-05T05-00.md
?? docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/remediation-baseline/cycle-inputs-read.2026-08-05T05-00.md
?? docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/remediation-baseline/order-paired.2026-08-05T05-00.md
?? docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/remediation-baseline/order-standalone.2026-08-05T05-00.md
?? docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/remediation-baseline/phase0-instructions-read.2026-08-05T05-00.md
?? docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/remediation-baseline/reference-census.2026-08-05T05-00.md
?? docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/remediation-baseline/toolchain-bootstrap.2026-08-05T05-00.md
?? docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/remediation-baseline/tree-state.2026-08-05T05-00.md
?? docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/remediation-baseline/vstest-path.2026-08-05T05-00.md
```

Counts, measured: **3** ` M` entries, **14** `??` entries. Zero staged, added, deleted, or renamed
entries — verified by `git status --porcelain | grep -vE '^( M|\?\?)'` returning nothing.

## Command 2 — `git diff --stat`

```
 SVGControl.Test/SVGControl.Test.csproj             |  5 ++++
 SVGControl.Test/packages.config                    |  1 +
 .../remediation-plan.2026-08-05T05-00.md           | 34 +++++++++++-----------
 3 files changed, 23 insertions(+), 17 deletions(-)
```

Arithmetic reconciliation, so no figure is unexplained: the two build-configuration files contribute
5 + 1 = **6 insertions and 0 deletions**. The plan file's 34 changed lines are 17 insertions plus 17
deletions, because each task check-off replaces one `- [ ]` line with the same line reading `- [x]`.
6 + 17 = **23 insertions**; 0 + 17 = **17 deletions**. Both totals match exactly. **17 check-offs is
exactly the number of tasks completed at this point** — `[P0-T1]`..`[P0-T11]` (11) plus
`[P1-T1]`..`[P1-T6]` (6).

## The two functional changes, confirmed

| File | Added lines | Removed / modified | Content |
|---|---|---|---|
| `SVGControl.Test/SVGControl.Test.csproj` | **5** | **0** | the four-line `ExCSS` `<Reference>` block (`[P1-T1]`) plus one `<Private>True</Private>` on the existing `Svg` reference (`[P1-T3]`) |
| `SVGControl.Test/packages.config` | **1** | **0** | `<package id="ExCSS" version="4.3.2" targetFramework="net481" />` (`[P1-T2]`) |

Both match the Scope Lock exactly. No other property, item, or target changed in either file.

## Required negative assertions — all three verified by measurement

```
Command: git diff --name-only | grep -c '\.cs$'
Output:  0
```
**No `.cs` file appears in the diff.** This cycle changes no production and no test source, as the Scope
Lock requires and as `[P1-T5]`'s "no assertion weakened" claim depends on.

```
Command: git diff --name-only | grep -ci 'app\.config$'
Output:  0
```
**No `app.config` appears in the diff.** Neither `SVGControl.Test/app.config` nor `SVGControl/app.config`
nor any other was modified, honouring the binding `## Do Not Do` prohibition and Design Decision 4.

```
Command: git diff --name-only | grep -c -E 'plan\.2026-08-04T14-36\.md|remediation-plan\.2026-08-05T01-50\.md'
Output:  0
```
**Neither `plan.2026-08-04T14-36.md` nor `remediation-plan.2026-08-05T01-50.md` appears in the diff.**
Both remain read-only for this cycle, as `[P0-T5]` invariant (b) established at entry. `[P2-T12]`
re-confirms this at exit.

## Every untracked path is evidence under this feature's `evidence/` tree

```
Command: git status --porcelain | grep '^??' | sed 's/^?? //' | grep -v '^docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/'
Output:  (empty)
```

All 14 untracked paths are under
`docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/`, in the canonical kinds
`remediation-baseline/` (11), `regression-testing/` (2), and `other/` (1). Zero paths under
`artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`,
`artifacts/coverage/`, or `artifacts/evidence/`. Every one is a product of a task in this plan and is
authorized by the Scope Lock clause `evidence/**`.

## Disclosed: a third tracked file is modified, and why that is not a scope violation

`[P1-T7]` asks this artifact to confirm that **exactly two** tracked files are modified. The measured
state is **three**. This discrepancy is recorded rather than transcribed away, and it is **not** a scope
violation.

The third file is
`docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/remediation-plan.2026-08-05T05-00.md`
— **this plan itself**. Its modification is:

1. **Explicitly authorized by this plan's own Scope Lock**, which lists it under "Documentation and
   evidence" as permitted for "checkbox state and preflight revision only". The change is exclusively
   checkbox state: 17 lines, each a `- [ ]` → `- [x]` flip on a completed task, verified by the
   17-insertions/17-deletions symmetry above. No task text, no heading, and no other content changed.
2. **Mandatory under the executor protocol**, which requires check-offs to be written to the canonical
   plan file on disk as each task's verification passes. A run in which the plan file were *unmodified*
   at `[P1-T7]` would mean no task had been checked off, which would itself be a protocol failure.
3. **Required to be consistent by `[P2-T12]`**, whose acceptance states "checkbox state in this plan file
   matches the evidence recorded" — which is only satisfiable if this file is modified.

The "exactly two" phrasing in `[P1-T7]` is therefore best read as *exactly two tracked files carrying
functional change*, which holds precisely. Read literally as a count of all ` M` entries it cannot be
satisfied by any conforming execution.

Neither prohibited response was triggered: nothing was reverted, and no halt was required. The
`[P1-T7]`/`[P0-T5]` prohibition on acting on another agent's file did not engage, because **no file
belonging to another agent appeared** — every changed path is attributable to a specific task in this
plan. No concurrent writer landed a file between `[P0-T5]` and this task.

### Line-ending note, disclosed

`git diff` emits `warning: in the working copy of '...remediation-plan.2026-08-05T05-00.md', LF will be
replaced by CRLF the next time Git touches it`. Measured: the working copy has **350 of 350 lines ending
CRLF** (`grep -c $'\r$'` = 350, `wc -l` = 350), so there are no mixed or corrupted endings. The file's
git attribute is `text: auto`, meaning git stores the blob LF-normalized and materializes CRLF on
checkout; `git show HEAD:<path>` confirms the stored blob is LF. The warning is that standard
normalization notice and predates this cycle's edits. `git diff --stat --ignore-cr-at-eol` returns the
same 17/17 figure, confirming no line-ending-only change is being counted.

## Verdict

**PASS.** Exactly two tracked files carry functional change — `SVGControl.Test/SVGControl.Test.csproj`
(five added lines) and `SVGControl.Test/packages.config` (one added line). No `.cs` file, no
`app.config`, and neither prior plan file appears in the diff. All 14 untracked paths are canonical
evidence under this feature's `evidence/` tree. The one additional tracked modification is this plan's
own checkbox state, authorized by its Scope Lock and required by the executor protocol. The QC loop may
begin at `[P2-T1]`.

## Output Summary

`git status --porcelain` shows 3 modified tracked files and 14 untracked; `git diff --stat` shows
23 insertions and 17 deletions across 3 files, reconciling exactly as 6 functional insertions plus 17
checkbox-flip line replacements. The two functional files are `SVGControl.Test/SVGControl.Test.csproj`
(+5/-0) and `SVGControl.Test/packages.config` (+1/-0). Measured **0** `.cs` paths, **0** `app.config`
paths, and **0** prior-plan-file paths in the diff. All untracked paths are canonical feature evidence.
The third tracked modification is this plan file's checkbox state, disclosed above as Scope-Lock
authorized rather than a violation; nothing was reverted and no halt was required.
