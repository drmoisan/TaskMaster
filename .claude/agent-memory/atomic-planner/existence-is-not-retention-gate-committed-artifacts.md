---
name: existence-is-not-retention-gate-committed-artifacts
description: "For any artifact an AC requires to be COMMITTED, an on-disk existence clause is not enough — a .gitignore-matched file exists identically and never reaches git; pair it with a git ls-files trackedness clause, which itself needs a preceding git add -N"
metadata:
  type: feedback
---

When an acceptance criterion says an artifact is *retained as evidence*, *committed*, or *recorded in
the delivery commit*, an acceptance clause of the form "the file exists at the named path" does not
measure that requirement. A path matched by a `.gitignore` pattern sits on disk exactly as a compliant
one does: the existence clause passes, every `git add -A` in the plan silently skips it, and the AC
fails with every gate reporting a pass.

**Why:** found on #736 round 5. Four msbuild non-vacuity logs were named `*.min.log`; `.gitignore:84`
is the bare pattern `*.log` with no negation anywhere in the file. Four separate tasks gated the logs,
and all four checked only existence. The AC required the logs to be retained. See
[[project-736-efc-archiveroot-boundary-sink-plan-seams]] point 21.

**How to apply:**

1. Before writing an evidence path into a plan, check the artifact's extension and path against
   `.gitignore` — including bare `*.ext` patterns far from any directory context. `*.log`, `*.tmp`,
   `*.coverage`, `*.coveragexml`, `*.binlog`, `coverage/*` are all live in TaskMaster.
   `.trx`, `.md`, and `.txt` are not matched.
2. If the extension collides, **rename the artifact** (e.g. `.log` → `.log.txt`). Do not edit
   `.gitignore` to add a negation: `.gitignore` is almost never in a ratified Write Set, so the edit
   breaches the scope-containment AC. Never `git add -f` it either — see
   [[gitignore-does-not-untrack-indexed-paths]].
3. Pair the existence clause with a trackedness clause: the path appears in the output of
   `git ls-files -- <path>`.
4. That clause is **unsatisfiable on its own** for a file the plan just created: `git ls-files` reads
   the index, and a new untracked file is absent from the index whether or not it is ignored. Precede
   it with `git add -N <path>` — intent-to-add, no content staged. It must be `-N` and not `-f`,
   because an un-forced add of an ignored path exits non-zero, and that non-zero exit is the
   discriminator the gate needs. Related: [[untracked-file-and-linecount-gate-seams]].
5. After the delivery commit, a scoped `git ls-files -- <evidence dir>` is sufficient without any
   staging step, because the commit already put the path in the index.
