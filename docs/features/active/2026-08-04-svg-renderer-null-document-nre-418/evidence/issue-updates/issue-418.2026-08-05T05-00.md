# Issue Update Mirror — Issue #418, Remediation Cycle 2

- Task: `[P2-T11]`
- Timestamp: 2026-08-05T00-28
- Issue: #418
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/418
- Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`
- Evidence series: `2026-08-05T05-00`
- Target: the **AC-10** entry in
  `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md`
- Nature of change: **append-only evidence note**. No AC text was rewritten; no checkbox changed state.

## PostedAs

```
PostedAs: body — local feature `issue.md` only
```

**POSTING NOT ATTEMPTED (GitHub).** The note was appended to the local feature `issue.md` body and
mirrored here. It was **not** posted to the GitHub issue.

Reason: `[P2-T11]` scopes this task to appending the note to the local `issue.md` and mirroring it to this
artifact. Neither the plan nor the execution directive instructs posting to GitHub, and the plan's Scope
Lock lists only the local `issue.md`, this plan file, and `evidence/**` as writable. Posting to the
remote issue is outside this cycle's scope and is left to the orchestrator or maintainer. No
`IssueUpdatedAt` timestamp and no comment URL exist, because no remote write occurred.

## Exact text appended, verbatim

Inserted at `issue.md:111`, inside the AC-10 block (which begins at line 107) and before AC-11 (line 112),
preceded by one blank line at line 110:

> Evidence-note amendment 2026-08-05 (remediation cycle 2, task `[P2-T11]`). **The criterion's text and
> its `[x]` state are unchanged.** This note records that the redirect's stated objective — the test host
> resolving `ExCSS` through the binding redirect rather than depending on the `AssemblyResolve` fallback
> to mask its absence — **is now achievable in the standalone `SVGControl.Test` host**, which is the one
> respect in which `feature-audit.2026-08-04T22-28.md` evaluated this criterion PARTIAL. The redirect
> value was already correct; what was missing was the assembly itself. `ExCSS.dll` is now present in
> `SVGControl.Test/bin/Debug` as of tasks `[P1-T1]` and `[P1-T2]`, which added an explicit `ExCSS`
> `<Reference>` (identity
> `ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a, processorArchitecture=MSIL`,
> `HintPath` `..\packages\ExCSS.4.3.2\lib\net48\ExCSS.dll`, `<Private>True</Private>`) plus the matching
> `<package id="ExCSS" version="4.3.2" targetFramework="net481" />` entry to
> `SVGControl.Test/packages.config`. A binding redirect presupposes the file is findable; the assembly is
> now on the probing path, so the redirect can do the work this criterion assigns it, and the
> `AssemblyResolve` fallback is no longer reached at all. The deployed assembly's identity was measured as
> `ExCSS, Version=4.3.2.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a` with `FileVersion=4.3.2.0`,
> matching the existing `oldVersion="0.0.0.0-4.3.2.0" newVersion="4.3.2.0"` redirect exactly. Evidence:
> `evidence/qa-gates/order-independence.2026-08-05T05-00.md` (standalone `SVGControl.Test` run at 75 total
> / 75 passed / 0 failed and the `SVGControl.Test`-first pair at 76/76/0, against 6 failed in both shapes
> before the fix) and `evidence/other/excss-copy-local.2026-08-05T05-00.md` (`ExCSS.dll` present in the
> output with its file version, `Svg.dll` still present, `Fizzler.dll` still absent, and the post-build
> `HintPath` verified unrewritten). **`SVGControl.Test/app.config` was not modified by this cycle** — no
> binding redirect was added, removed, or retargeted, per the binding `## Do Not Do` prohibition; the
> stale `Fizzler` and `Unsafe` redirect defects remain deferred to
> `docs/features/potential/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md`. This cycle modified
> exactly two files, both build configuration: `SVGControl.Test/SVGControl.Test.csproj` (five added lines)
> and `SVGControl.Test/packages.config` (one added line). No `.cs` file was changed and no assertion was
> weakened; the two `XmlException` assertions this criterion's failure mode implicated now hold with their
> original text.

## Append-only compliance, verified by measurement

```
Command: git diff --numstat -- docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md
Output:  2	0	docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md
```

**2 added lines, 0 removed or modified.** The two lines are one blank separator plus the note itself.

```
Command: git diff -U0 -- .../issue.md | grep -E '^[+-]' | grep -v '^(+++|---)' | grep -cE '^[+-]- \[.\]'
Output:  0
```

**Zero changed lines begin with a checkbox token.** No line beginning `- [ ]` or `- [x]` was added,
removed, or altered, so no criterion changed state in either direction.

Diff hunk header, confirming a pure insertion:

```
@@ -109,0 +110,2 @@
```

The `-109,0` side names zero source lines, which is the signature of an insertion with no deletion or
replacement.

## Checkbox state after the edit — AC-11 remains unchecked

```
Command: grep -oE '^- \[.\] \*\*AC-[0-9]+' .../issue.md
```

| AC | State |
|---|---|
| AC-1 | `- [x]` |
| AC-2 | `- [x]` |
| AC-3 | `- [x]` |
| AC-4 | `- [x]` |
| AC-5 | `- [x]` |
| AC-6 | `- [x]` |
| AC-7 | `- [x]` |
| AC-8 | `- [x]` |
| AC-9 | `- [x]` |
| **AC-10** | **`- [x]`** — unchanged; its existing check is now accurate on its own merits |
| **AC-11** | **`- [ ]`** — unchanged and **deliberately still unchecked** |

**AC-1 through AC-10 stay `[x]` and AC-11 stays `[ ]`**, exactly as `[P2-T11]` requires.

AC-11 is R-1: the human WinForms-designer runbook. It is excluded from this plan and represented by no
task, it cannot be executed by any agent, and it is tracked as ratified human-interaction requirements H-1
and H-2 with `response: "exception"` and a `runbook_path`. It may be checked off only after a human
capture exists at `evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md`, or after an explicit
maintainer waiver.

## Placement verification

| Element | `issue.md` line |
|---|---|
| AC-10 criterion line | 107 |
| AC-10 amendment of 2026-08-04 (pre-existing) | 109 |
| blank separator (added) | 110 |
| **this note (added)** | **111** |
| AC-11 criterion line | 112 |

The note is inside the AC-10 block and before AC-11. No other criterion's block was touched. File length
121 → 123 lines.

## Output Summary

An append-only evidence note was added to the **AC-10** entry of the local feature `issue.md` at line 111,
recording that the redirect's stated objective is now achievable in the standalone `SVGControl.Test` host
because `ExCSS.dll` is present in `SVGControl.Test/bin/Debug` as of `[P1-T1]`/`[P1-T2]`, citing
`evidence/qa-gates/order-independence.2026-08-05T05-00.md` and
`evidence/other/excss-copy-local.2026-08-05T05-00.md`, and stating that `SVGControl.Test/app.config` was
not modified by this cycle. The edit is verified append-only: `git diff --numstat` reports **2 added, 0
removed**, the hunk header is `@@ -109,0 +110,2 @@`, and **zero** changed lines begin with a checkbox
token. AC-1 through AC-10 remain `[x]` and **AC-11 remains `[ ]`**. `PostedAs: body — local feature
issue.md only`; the GitHub issue was **not** posted to, with the reason recorded above.
