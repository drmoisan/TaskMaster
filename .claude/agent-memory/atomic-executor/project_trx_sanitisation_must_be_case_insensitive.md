---
name: trx-sanitisation-must-be-case-insensitive
description: vstest writes the TRX `storage=` attribute in all-lower-case, so a case-SENSITIVE host-path substitution clears the header and leaves one leaked path per test; also the Deploy_ dir name itself carries the account and machine name
metadata:
  type: project
---

Sanitise every TRX **case-insensitively**, and sweep with a case-insensitive fixed-string search
afterwards rather than trusting the substitution.

**Why:** On 2026-08-26 a sweep of `qfc-collection-controller-defects-468` found the account name and
the absolute worktree path in **16 already-committed TRX files** — 946 occurrences in one full-suite
TRX. The earlier phase had substituted only the mixed-case spelling of the workspace root. That
cleared `<TestRun name=...>`, `runUser`, `computerName` and `runDeploymentRoot`, so a
machine-name search returned zero and the artifact recording the sanitisation truthfully reported
"0 hits" for the machine name. But vstest writes the `storage=` attribute of **every**
`<UnitTest>` element in all-lower-case, so one leaked absolute path survived per test. A stack
trace in a failing TRX leaks the source path of the throwing frame too, which is why a failing TRX
needs more substitutions than a passing one of the same size.

**How to apply:**
- Substitute in this order, all case-insensitive: workspace-root prefix -> `<repo-root>`,
  user-profile path -> `<user-profile>`, machine name -> `<host>`, account name -> `<user>`. A
  shorter token applied first eats the prefix of a longer one.
- Read and write the TRX through a raw byte layer (`open($fh, '<:raw', ...)`) so BOM and CRLF state
  are untouched. Verify the substitution count is a plausible multiple of the test count, not a
  whole-file rewrite: `git diff --stat` should show roughly `2 x substitutions` changed lines.
- vstest also creates an empty `Deploy_<account> <timestamp>_<pid>/In/<MACHINE>/` scaffolding
  directory inside every `/ResultsDirectory:`. Git does not track empty directories so it never
  reaches a commit, but delete it anyway with `rmdir` rather than relying on that.
- **An artifact that documents a sanitisation must not quote the raw tokens.** The same feature had
  a committed QA artifact whose substitution table's *From* column and `BEFORE:` examples
  reproduced every identifier the sanitisation had removed. Name the token **class**
  (workspace-root prefix, user-profile path, `computerName` attribute, `runUser`) and keep only the
  `AFTER:` lines.

See [[_shared_no_absolute_host_paths]].

## The `<repo-root>` placeholder must be XML-escaped inside the TRX

A plan task that says "replace every absolute filesystem path prefix with the literal `<repo-root>`"
produces **invalid XML** if followed literally. XML forbids a raw `<` in text nodes *and* in
attribute values, so writing the five characters `<repo-root>` into a `<StdOut>` body or into a
`storage="..."` attribute makes the document unparseable.

**Why:** On 2026-08-27 (feature 444, `[P4-T7]`) the raw substitution produced 13444 occurrences and
`ElementTree` reported `mismatched tag: line 638` — the parser had read the placeholder as an
element start tag (`</StdOut> closes <repo-root>`). The four host-value assertions the task gates on
(`:\Users\`, `$env:COMPUTERNAME`, `$env:USERNAME`, `computerName="host"`) all still passed, so the
gate did not catch it and an unparseable 8 MB TRX would have been committed.

**How to apply:** write the placeholder as `&lt;repo-root&gt;`. An XML reader decodes that back to
the literal `<repo-root>`, so the required substitution is satisfied and the document still parses.
Verify with a strict parser (`xml.etree.ElementTree.parse`), not with `[xml]` in PowerShell — the
`[xml]` cast's failure message is truncated and does not name the offending tag. After the fix,
confirm the decoded attribute value: `storage` should read `<repo-root>\...`, and the
`<UnitTestResult>` count should equal the run's test total.

### Catch the raw `<repo-root>` spelling at PREFLIGHT, not at execution

The escaping fix above is unusable once execution reaches the sanitisation task, because that task is
normally the second-to-last in the plan and the close-out task immediately after it forbids writing
anything to disk after the commit. An executor that reads this memory only when it reaches the
sanitisation task has already lost the chance to apply it without violating the clean-tree gate.

**Why:** on 2026-08-28 (feature 680, `[P7-T3]`) the plan text said "substitute ... the worktree-root
prefix -> `<repo-root>`" with the placeholder written raw. Following it literally produced five TRX
files that are no longer well-formed XML wherever a substituted path sat in an attribute value. The
task's own acceptance is three zero-hit greps plus a zero name-hit count, and all four passed, so
nothing in the task could detect it. The defect was noticed only after `[P7-T4]` had committed, at
which point amending would have dirtied the tree the close-out gate requires clean.

**How to apply:** during preflight, treat a sanitisation task that quotes a placeholder containing a
raw `<` or `>` and targets an XML-shaped artifact (TRX, coverage XML, `.csproj`) as a required plan
delta: the plan must say `&lt;repo-root&gt;`, or must state that the artifact is not required to
remain parseable. Raise it as a preflight revision, not as an execution-time judgment call.

## `/EnableCodeCoverage` drops a host-named binary INTO the evidence tree

A vstest run that combines `/EnableCodeCoverage` with an explicit `/ResultsDirectory:` pointing at
`<FEATURE>/evidence/<kind>/` writes two extra directories there, and one holds a `.coverage` binary
whose **filename** is `<account>_<MACHINE>_<date>.<time>.coverage`. The sibling directory is named
`<account>_<MACHINE>_<timestamp>` outright.

**Why:** sanitisation operates on file *contents*. A filename carrying the account and machine name
defeats every content sweep, and a `.coverage` file is an opaque binary that cannot be redacted at all.
On 2026-08-28 (feature 489, `[P0-T13]`) both directories landed inside `evidence/baseline/` and would
have been committed by the next `git add -A` on the feature folder.

**How to apply:** after any coverage-enabled vstest run that targets an evidence directory, delete the
attachment directories before staging. The plan's acceptance for such a task is normally the TRX plus
recorded integers, so nothing references them. If a plan genuinely needs the coverage data, direct
`/ResultsDirectory:` at a scratch path and copy only the TRX into evidence.
