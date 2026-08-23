# Never embed absolute host paths in any file

**Applies to:** every agent, every artifact — evidence records, plans, specs, research,
checkpoints, agent memory, commit messages, PR bodies.

## Rule

No file committed to this repository may contain an absolute host path or a host identifier.
Absolute paths leak the operator's account name, machine name, and directory layout, and they
are not reproducible on any other machine.

Prohibited: `C:\Users\<account>\...`, `C:/Users/<account>/...`, `/c/Users/<account>/...`, a bare
account name (for example in an `ls -l` owner column), and a bare machine name.

## Required placeholders

| Real value | Write this instead |
| --- | --- |
| repository root | `<repo-root>` |
| user profile directory (for caches outside the repo, e.g. NuGet) | `<user-profile>` |
| account name | `<user>` |
| machine / host name | `<host>` |

Compose longer paths from the placeholder: `<repo-root>\.claude\worktrees\agent-<id>\.dotnet-sdk`.
Prefer a repo-relative path (`packages/Foo/bar.dll`) whenever one is expressible — a placeholder is
the fallback for paths that genuinely sit outside the repository.

## The vstest TRX trap

`vstest.console.exe` names its TRX and `.coverage` output `<account>_<HOST>_<timestamp>.trx` by
default, so raw test output embeds both identifiers **in the filename**. Any evidence artifact that
cites a TRX by name inherits them.

Two mitigations, both required:
1. Pass an explicit `/ResultsDirectory:` plus a `--logger:trx;LogFileName=` that you control, or
   rename the produced files before citing them.
2. When citing a TRX in a markdown evidence record, cite the sanitized filename.

## Recurrence record

Issue #511 (`winformspumphost-suite-determinism-511`) accumulated 140 untracked evidence paths
carrying a `<account>_<HOST>_` filename prefix, 10 committed markdown files citing those names, and
91 absolute-path occurrences across 27 committed files. All were sanitized on 2026-08-23 at
maintainer instruction. Roughly 146 files in other and archived feature folders still carry the
prefix, and about 157 still carry the bare host name; that remainder is tracked as its own issue
because sanitizing it inside a bug-fix child would break the child's scope lock.
