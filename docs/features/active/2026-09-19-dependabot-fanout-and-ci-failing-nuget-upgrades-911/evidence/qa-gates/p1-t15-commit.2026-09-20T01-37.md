# Phase 1 Commit — Remediation Cycle 1, Issue #911

- Timestamp: 2026-09-20T08-48-30
- Task: [P1-T15]
- Finding: R2
- EXIT_CODE: 0

## Commit

Head SHA after the commit: **`7cda4543995f52b8f2f41165de086c6b2eefb036`**

The value [P0-T2] recorded was `4043b913468f913649be3e6aa189b1be8310df00`. The head SHA differs,
which is the check that the commit actually landed.

## Pathspec

Explicit, not `-A`:

```
git add -- tests/scripts/vscode/Sync-PackageReferences.Tests.ps1 docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/
```

## Commit Message

A single `-m` argument containing no `<`, `>`, `$` or backtick character:

```
test(deps): cover nine untested negative and error paths in Sync-PackageReferences

Discharges remediation finding R2 for issue 911. Adds eight Pester tests driving the
existing injected seam, covering lines 151, 180, 248, 290, 293, 330, 336, 337 and 345.
Per-file line coverage moves from 95 of 127 to 104 of 127. Line 248 is the issue 902
rejection handler. Also records the Phase 0 remediation baseline for this cycle.

Claude-Session: https://claude.ai/code/session_01QaUVgY37zfbsTvSTPd7wsr
```

**Attribution note.** The session attribution guidance asks for a `Co-Authored-By:` trailer whose
address is wrapped in angle brackets. This plan forbids `<` and `>` anywhere in the commit
argument, and the repository's pre-implementation gate rejects a commit command containing them.
The `Claude-Session:` line carries no forbidden character and is included; the `Co-Authored-By:`
trailer is omitted for that reason and the omission is recorded here rather than left silent.

## `git status --porcelain --untracked-files=all` After the Commit, Verbatim

```
(empty)
```

No entry at all, so no entry outside `coverage/`. The tree is clean.

The plan checkbox for this task was ticked **before** the commit, so the plan file was part of the
committed set rather than left modified afterwards. Ticking after the commit would have left the
plan dirty and this capture non-empty, which is the fixpoint that breaks a clean-tree gate.

The gitignored working files this cycle produced — `coverage/p0-t3-rows.xml`, the two hash-set
documents, the four JaCoCo and MSBuild artifacts, and the throwaway helper at
`coverage/helpers/hostpath-census.ps1` — all sit under `coverage/`, which `.gitignore:144`
ignores, so none appears in the capture.

## `git show --name-only --format= HEAD`

**31 paths.**

| Check | Required | Measured | Result |
|---|---|---|---|
| Lists `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1` | yes | **yes** | PASS |
| Paths under the feature folder | at least 12 | **30** | PASS |
| Paths under `scripts/` | none | **0** | PASS |

The 30 feature-folder paths are the modified plan, 15 `evidence/remediation-baseline/` artifacts
including the two copied non-markdown evidence forms, 9 `evidence/regression-testing/` artifacts
including the fail-before exception dossier, and 5 `evidence/qa-gates/` artifacts.

The only non-documentation path is the test file. Zero paths under `scripts/`, which is the check
that Phase 1 edited no production file — the invariance the nine line citations depend on.

`31 files changed, 2591 insertions(+), 28 deletions(-)`. The 28 deletions are the 28 plan
checkbox lines this phase ticked, each rewritten from `- [ ]` to `- [x]`.

## Line-Ending Note

`git add` emitted `LF will be replaced by CRLF` warnings for every added file. That is the
repository's configured normalisation for text files and is not a content change; the committed
blobs carry the normalised form, which is what every other file in this feature folder already
carries.

## Output Summary

Phase 1 committed at `7cda4543`. 31 paths, 30 under the feature folder and one test file, none
under `scripts/`. Working tree clean after the commit.
