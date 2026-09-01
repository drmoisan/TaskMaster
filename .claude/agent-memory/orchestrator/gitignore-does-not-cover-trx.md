---
name: gitignore-does-not-cover-trx
description: .gitignore covers *.coverage and *.coveragexml but NOT *.trx, so a committed TRX leaks the account name, machine name and worktree root; plan a sanitisation sweep before any git add of vstest evidence
metadata:
  type: project
---

`.gitignore` carries `*.coverage` (`:140`), `*.coveragexml` (`:141`), `coverage/*` (`:144`) and
`TestResult.xml` (`:44`), and **no `*.trx` entry at all**. `.csharpierignore:8` does list `*.trx`,
which is easy to misread as coverage — it is a *formatting* exclusion, not a commit exclusion.

So any plan that runs `vstest.console.exe` with `/Logger:trx` into an evidence directory and then
runs `git add` over that directory **commits the TRX as produced**.

A TRX carries host identifiers in four places:
- `runUser` — the account name
- `computerName` — the machine name
- `runDeploymentRoot` — the worktree root
- the `storage` attribute of **every** `<UnitTest>` element — the worktree root, one per test

The Cobertura copy is a second channel: every `<class filename>` is an absolute path whenever the
document is the raw pre-processed one, which happens on any run where
`Invoke-MSTestWithCoverage.ps1` throws at `:236` before the post-processing write at `:343`.

**How to apply.** Any plan whose evidence includes a vstest run needs an explicit sanitisation
sweep before each `git add`, not just a hope that `.gitignore` handles it. Three details that are
easy to get wrong and were all found by review rather than by execution:

1. **The substitution must be case-insensitive.** `vstest.console.exe` writes the `storage`
   attribute in lower case while the worktree root is mixed case, so a case-sensitive pass clears
   the TRX header and leaves one path per test intact.
2. **Filter the rewrite scope by `git check-ignore -q`.** `/EnableCodeCoverage` writes a *binary*
   `.coverage` into each results directory. It is gitignored and never committed, and a text
   rewrite corrupts it.
3. **A UTF-16LE identifier does not match a UTF-8 read.** In a real `.coverage`, the account name
   occurs 0 times as ASCII and 23 times as UTF-16LE, so a `File.ReadAllText` + ordinal search finds
   nothing. That makes a residual-count-zero gate *attainable*, but it is attainable for the wrong
   reason — do not read it as proof the file is clean.

One channel a content sweep cannot reach: a vstest run with an explicit `/ResultsDirectory:`
creates a `Deploy_<account> .../In/<machine>/` directory whose **name** carries both identifiers.
Content rewriting does not touch a directory name. Those directories are empty in practice and git
cannot commit an empty directory, so nothing leaks today — but it is the residual gap.

Related: [[../_shared_no_absolute_host_paths]], [[angle-bracket-redaction-breaks-trx-xml]].
