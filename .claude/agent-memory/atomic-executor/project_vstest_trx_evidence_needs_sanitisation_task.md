---
name: vstest-trx-evidence-needs-sanitisation-task
description: Plans that send /Logger:trx and msbuild tee output into evidence/ never include a sanitisation step, yet the repo-wide no-host-paths rule makes one mandatory before the final commit — budget it as a micro-action per artifact
metadata:
  type: project
---

Any plan task that writes `/Logger:trx` or tees msbuild output into
`<FEATURE>/evidence/<kind>/` produces an artifact that violates the repository-wide
`_shared_no_absolute_host_paths` rule, and no plan I have executed has ever carried a task to fix
it. Sanitisation is therefore a mandatory executor micro-action, not optional polish.

**Why:** the rule says "Applies to: every agent, every artifact" and binds independently of the
plan. The final commit task ("commit ALL evidence") is what actually lands the leak, so the leak
must be cleared before staging. Observed on #677 (2026-08-28): four TRX files needed 3610 / 94 / 34
/ 3661 substitutions and two teed msbuild logs needed 13070 / 13301 — an msbuild log leaks roughly
100x more than a small TRX because every project line repeats the absolute path.

**How to apply:**
- Budget one sanitise-and-rename step immediately after EACH vstest or teed-msbuild task, while the
  numbers are still in hand for the artifact. Doing it once at the end means re-deriving every count.
- The vstest DEFAULT FILENAME `<account>_<HOST>_<timestamp>.trx` leaks in the filename, which no
  content sweep can reach. Rename to a task-scoped name (`p4-t1-<scope>.trx`). This preserves an
  "exactly one TRX under .../pN-tM/" acceptance condition, so renaming does not break the gate.
- Do NOT try to avoid it by adding `--logger:trx;LogFileName=` — that changes the command the plan
  pins, and the plan's acceptance may quote the command verbatim. Rename after the fact instead.
- Substitute case-insensitively in binary mode, longest token first: workspace-root (BOTH separator
  spellings) -> user-profile (both spellings) -> host -> account. In a TRX write the placeholders
  XML-escaped (`&lt;repo-root&gt;`); in a plain-text log write them raw.
- Verify with THREE things, not one: a case-insensitive `grep -I -i -c -F` sweep per token, a
  `:\Users\` / `:/Users/` sweep, and a strict XML parse of the TRX whose `<UnitTestResult>` count
  must equal the run total. The parse is what catches a substitution that corrupted the document.
- `obj/**` build scratch (`*.csproj.FileListAbsolute.txt`) will show up in a repo sweep carrying the
  account name. It is gitignored, so confirm with `git check-ignore -v` and move on rather than
  editing it.

Related: [[project_trx_sanitisation_must_be_case_insensitive]],
[[project_bash_heredoc_collapses_doubled_backslashes]]
