---
name: trx-needs-resultsdirectory
description: /Logger:trx writes to TestResults\ relative to cwd, so a TRX-existence acceptance clause needs an explicit /ResultsDirectory: — that directory must be private to the task, and a BARE /Logger:trx names the file after the account and host
metadata:
  type: feedback
---

Every `vstest.console.exe` task that passes `/Logger:trx` must also pass
`/ResultsDirectory:<the same canonical evidence directory the acceptance clause checks>`.

**Why:** `vstest.console.exe` writes the TRX into a `TestResults` folder relative to the working
directory unless `/ResultsDirectory:` is supplied. A task that says "write the ten TRX files to
`<FEATURE>/evidence/regression-testing/`" and makes "ten distinct TRX files exist" its acceptance
condition is then asserting against a file the stated command never produces at that location. The
#511 plan had ten such tasks and zero `ResultsDirectory` occurrences; preflight caught it.

**A shared results directory is a second, independent defect.** Supplying `/ResultsDirectory:` but
pointing every task at the same evidence `<kind>` folder makes a count-based acceptance ambiguous. In
the #511 plan eight tasks wrote into `evidence/regression-testing/`, so by the time `P4-T2` asserted
"ten distinct TRX files exist and each records a failed count of exactly 0", that folder already held
roughly 25 earlier TRX files — several of them legitimate FAILURES from `[expect-fail]` Phase 1 runs.
A later auditor cannot tell which ten the gate meant. Append a lower-case task-ID segment to each
value (`.../evidence/regression-testing/p4-t2`) and word the acceptance as "that subdirectory holds
exactly ten TRX files and no others". This also disambiguates two sibling ten-run tasks from each
other.

**How to apply:** When auditing a plan, grep `Logger:trx` and `ResultsDirectory` and confirm the two
line-number sets are identical AND that every `ResultsDirectory` VALUE is distinct. Match the parent
directory to each task's own evidence `<kind>` (`baseline`, `regression-testing`, `qa-gates`); the
task-ID segment goes underneath it, so the canonical `<FEATURE>/evidence/<kind>/` invariant still
holds. Renumbering a task means renaming its segment — re-check after any Delta that shifts IDs. A
stray `TestResults\` from an omission does not break a clean-tree acceptance — `.gitignore:39` is
`[Tt]est[Rr]esult*/` — so the clean-tree gate will NOT catch the missing-flag defect for you.

**A bare `/Logger:trx` is a third, independent defect: it puts the account and machine name in the
committed artifact's FILE NAME.** vstest's default TRX name is `<account>_<machine> <timestamp>.trx`.
`.claude/agent-memory/_shared_no_absolute_host_paths.md` prohibits an account or machine name in any
committed artifact, and a sanitisation sweep that rewrites file CONTENT never reaches a file name, so
the violation survives every sanitise-then-commit task a plan carries. Always pass an explicit name,
and always double-quote the whole switch: `"/Logger:trx;LogFileName=p5-t10.trx"`. The quotes are
load-bearing — an unquoted semicolon terminates the argument in `pwsh`, so the unquoted form silently
degrades to a bare `/Logger:trx` and restores the account-named file. Gate it: have the sanitisation
task count files AND DIRECTORIES under `<FEATURE>/evidence/` whose name contains the token produced by
`Split-Path -Leaf $env:USERPROFILE`, require 0, and never write that token into the artifact. A plan
that measures this count but explicitly declines to gate on it (#633 round 1 did, in two tasks) has
documented the violation rather than prevented it. Confirmed on #633 round 2 across eight scoped runs.

**Confirmed instance.** #464 round 3 inserted a task into Phase 5 and renumbered its tail; the
renamed `[P5-T12]` kept the segment `p5-t11` and round 4 caught it. The cheap audit is two `-o`
greps over the plan — one for `ResultsDirectory:[^ \`]*`, one for `^- \[[ x]\] \[P\d+-T\d+\]` — then
compare the two line-number-keyed lists pairwise. See [[project_464_efc_controller_plan_seams]].
