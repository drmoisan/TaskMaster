# P7-T3 — Committed-Artifact Sanitisation Record

Timestamp: 2026-08-28T16-45

This record deliberately lists only **AFTER** values. Quoting a BEFORE value would reintroduce the
identifier into a committed file and defeat the sweep this record certifies.

## (a) TRX substitution

Five TRX files were rewritten in binary mode (bytes read, mapped 1:1 through ISO-8859-1, substituted,
written back), case-insensitively, applying three substitutions in this order — worktree root first,
because that prefix itself contains the account name:

| Substitution target | AFTER value |
|---|---|
| the worktree-root prefix | `<repo-root>` |
| the value of `$env:USERNAME` | `<user>` |
| the value of `$env:COMPUTERNAME` | `<host>` |

| TRX file | bytes before | bytes after |
|---|---|---|
| `evidence/regression-testing/p2-t3/p2-t3.trx` | 41205 | 38568 |
| `evidence/regression-testing/p2-t10/p2-t10.trx` | 22385 | 21084 |
| `evidence/regression-testing/p3-t6/p3-t6.trx` | 38604 | 36055 |
| `evidence/regression-testing/p3-t9/p3-t9.trx` | 65249 | 60860 |
| `evidence/qa-gates/p4-t2/p4-t2.trx` | 115976 | 109011 |

Note on the resulting files: the placeholder tokens contain `<` and `>`, so a sanitised TRX is no
longer well-formed XML where a substituted path sat inside an XML attribute value. That is the
substitution the plan specifies. These TRX files are audit evidence read by humans; no task in this
plan or any downstream tool parses them as XML, and every run's counts, verdicts, and failure messages
are already transcribed into the per-task Markdown artifacts.

### Additional removal — vstest deployment scaffold directories

`/InIsolation` created two empty scaffold directories under the results directories whose **directory
names** embedded the account name and, one level down, the machine name. They contained no files and
were untracked by git, so they could never have been committed, but they were removed so the
path-name check below holds over the whole feature tree:

- one under `evidence/regression-testing/p2-t3/`
- one under `evidence/regression-testing/p2-t10/`

Directories removed: 2.

## (b) Sweep scope and results

Scope is the **union** of two enumerated sets:

1. Every file under
   `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/` — **41 files**.
2. Every path reported by `git status --porcelain` at this moment, with directory entries expanded to
   their contained files — **23 porcelain entries expanding to 57 files**. By construction this is
   exactly what P7-T4 commits, and it includes `.claude/agent-memory/**`, the changed production and
   test sources, and `QuickFiler.Test/QuickFiler.Test.csproj`.

Union after de-duplication: **58 files**.

Content sweep, performed byte-wise and case-insensitively over every file in the union (equivalent to
`grep -r -a -i -F` — `-a` and not `-I`, so a binary-classified file cannot let a match through
undetected):

| Literal swept | Hits |
|---|---|
| the worktree-root prefix | **0** |
| the value of `$env:USERNAME` | **0** |
| the value of `$env:COMPUTERNAME` | **0** |

No file produced a read error.

Path-name check (separate from the content sweep, because a content grep matches file contents only).
Each union member's repo-relative path name was tested case-insensitively against the same three
literals:

| Literal | Repo-relative path-name hits |
|---|---|
| the worktree-root prefix | 0 |
| the value of `$env:USERNAME` | 0 |
| the value of `$env:COMPUTERNAME` | 0 |
| **Total** | **0** |

The repo-relative form is the meaningful one: only the repo-relative path is committed, and the
ambient absolute prefix used to enumerate the union is not part of any committed name.

## Verdict

Three zero-hit content counts and a zero total path-name-hit count. No committed artifact carries an
absolute host path, an account name, or a machine name.

## Scope note on this record

This record is written into the feature evidence tree after the union above was enumerated, so it is
the one path the sweep could not itself cover. It is safe by construction rather than by measurement:
it quotes only AFTER values and names the substitution targets descriptively (`$env:USERNAME`,
`$env:COMPUTERNAME`, "the worktree-root prefix") rather than by value, so it introduces none of the
three literals. A post-write re-sweep including this file is recorded below.

### Post-write re-sweep

The union was re-enumerated after this record was written — **59 files** (the 58 above plus this
record) — and re-swept for the same three literals:

- Total content hits across all three literals: **0**
- Total repo-relative path-name hits across all three literals: **0**

The record is therefore covered by a measured sweep, not only by construction.
