---
name: msbuild-log-token-search-matches-csc-command-line
description: A plan gate that greps an msbuild log for a source filename matches the csc.exe command line, not diagnostics, so byte-exact set equality breaks whenever the plan adds a Compile Include
metadata:
  type: project
---

A plan gate of the form "extract every line of the msbuild log containing `<SomeFile>.cs`, then
assert the post-change line set is byte-identical to the baseline set" does **not** measure
diagnostics. At MSBuild's default verbosity the only lines containing a source filename are the
`csc.exe` command line emitted by `CoreCompile` and its companion `BuildResponseFile = '...'` echo.
Each is 30-60 KB and enumerates the compiling project's entire `/reference:` set **and** its entire
source-file set.

Consequences, all measured on #493 (2026-08-27):

- The **count** condition survives: adding source files lengthens an existing line rather than
  adding one, so the count stays 2 per token per log.
- The **byte-exact set equality** condition fails for any token whose containing project gains a
  `<Compile Include>` entry. Each affected line grew by exactly the length of the added path strings
  (123 chars for two files). Symmetric difference at token granularity was exactly the two added
  source-file arguments.
- A token belonging to a project the change does not touch (`UiThread.cs` in `UtilitiesCS`) keeps
  byte-exact equality, so the gate is real for *that* file and vacuous-then-broken for the changed
  project's files.

**Why:** the plan author intends the gate to discharge an AC clause like "no analyzer diagnostic is
raised at either call site", and reaches for a filename grep because a diagnostic line does contain
the filename. But so does the compiler invocation, and there is no diagnostic to find.

**How to apply:** at preflight, reject a byte-exact msbuild-log line-set gate scoped to a file in a
project the plan modifies. Substitute a gate on the diagnostic-bearing subset — partition the matches
on `warning <CODE>` / `error <CODE>` — or simply assert zero `warning CS` / `error CS` lines in the
whole log plus a byte-identity hash on the unowned file. During execution it is too late to block:
record the partition and the symmetric difference, state which sub-condition failed and why, and
check the AC off only if the criterion's own text is independently satisfied. Also record the hazard
in the **baseline** artifact before the change is made, so the later failure reads as disclosed rather
than as a surprise.

Do not paste the matched lines verbatim into a committed artifact: four lines is ~356 KB of reference
list. Record per line the log line number, redacted character length, and SHA-256 of the redacted
text, and write the full redacted lines to the git-ignored `TestResults/plan-logs/` tree so the
comparison stays byte-exact and reproducible.

Related: [[verify-line-citations-with-numbered-output]],
[[preflight-selfderived-gate-thresholds-are-blind]].
