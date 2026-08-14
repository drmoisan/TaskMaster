---
name: pwsh-git-gh-cli-gotchas
description: Three verified environment facts that silently break plan verification commands — jq is not installed, PowerShell will not concatenate $(...)..HEAD into one git argument, and an unanchored git pathspec like 'packages.config' matches nothing
metadata:
  type: project
---

Three verified facts about this Windows box that make plausible-looking verification
commands fail or, worse, pass vacuously. Verified 2026-08-14 during #553 preflight.

**1. `jq` is NOT installed.** `command -v jq` (git-bash) and `Get-Command jq` (pwsh 7.6.3)
both return nothing. `gh api ... --jq '<filter>'` DOES work because that filter is
compiled into `gh` — so a plan can use `--jq` freely but any standalone
`jq '<filter>' file.json` against a LOCAL file is unrunnable. Replace with
`Get-Content -Raw x.json | ConvertFrom-Json` / `ConvertTo-Json -Depth 20`
(the default `-Depth 2` silently truncates nested API objects such as a GitHub ruleset).

**2. PowerShell does not build `<sha>..HEAD` from `$(git merge-base ...)..HEAD`.**
Verified: `git diff --name-only $(git merge-base origin/main HEAD)..HEAD -- '*.cs'`
run under `pwsh -Command` makes git print its usage block and exit non-zero. The
subexpression and the trailing `..HEAD` are not concatenated into one argument. Use two
statements: `$base = git merge-base origin/main HEAD` then
`git diff --name-only "$base..HEAD" -- '*.cs'`. The bash form works; the pwsh form does not.

**3. A git pathspec with no wildcard is anchored to the repo root.** `git ls-files --
'packages.config'` returns 0 files and `'app.config'` returns 0, while
`'**/packages.config'` returns 18. `'*.cs'` DOES match at any depth (pathspec globbing
does not set FNM_PATHNAME), so `*.ext` forms are fine and bare-filename forms are not.
A "no C#/project-file changes" gate written with bare `packages.config` is vacuous.

**4. actionlint's `-color` is a BOOLEAN flag; `-color never` fails with exit 3.**
`actionlint -color never` makes Go's flag parser read `-color` as the boolean and
`never` as a positional FILE, producing `could not read "never": open never: The system
cannot find the file specified.` and exit 3 — which reads like a lint failure but is an
argument error. The suppression form is the separate boolean `-no-color`. Verifying that
a tool's download URL returns HTTP 200 is NOT verifying that its command line parses;
run `<tool> -h` during preflight when a plan hard-codes flags. Also note `-verbose`
prints `Collected N YAML files` / `Found 0 errors in N files`, which is how you prove a
lint run actually covered the file set instead of silently skipping it.

**5. `gh workflow run --ref <branch>` races `git push` and silently runs the OLD sha.**
Verified 2026-08-14 on #553: `git push && gh workflow run ci.yml --ref <branch>` produced
a run whose `head_sha` was the PREVIOUS commit, because GitHub resolved the ref before the
push replicated. For a fault-isolation probe this is the worst possible failure — the run
goes GREEN and looks like the probe proved the gate does not fire. Always verify
`gh run list --json headSha` (or `gh api .../runs/<id> --jq .head_sha`) equals the intended
sha BEFORE watching, cancel and re-dispatch if it does not, and put a few seconds plus a
`git ls-remote --heads origin <branch>` tip check between push and dispatch.

**Why:** all five produce a wrong result rather than an obvious error — #2 emits a usage
dump that a wrapper can read as "no output, therefore clean", and #3 reports an empty
diff for files that genuinely changed.

**How to apply:** when preflighting a plan, actually execute each verification command
shape (not just read it) before signing off, especially anything using `jq` on a file,
`$(...)` inside a git rev range, or a bare-filename pathspec. Related:
[[project-build-test-env]], [[verify-line-citations-with-numbered-output]].
