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

## Case-sensitivity trap: sanitising a TRX clears the header but not `storage=`

Sanitising a TRX with a case-SENSITIVE substitution against the mixed-case workspace root looks
successful — the `<TestRun>` header, `runUser`, and `runDeploymentRoot` all come out clean — while
leaving the leak fully intact. `vstest` writes the `storage=` attribute of every `<UnitTest>`
element in **all-lower-case**, so a case-sensitive pass over a mixed-case root misses one path per
test. On issue #468 that was 946 leaked paths in a single TRX, and 16 committed TRX files needed
5,668 substitutions to clear.

**Always substitute case-insensitively, in binary mode**, and verify with a case-insensitive
fixed-string sweep (`grep -I -i -F`) rather than trusting the header.

## Angle-bracket placeholders corrupt XML: escape them, and re-parse after redacting

The four required placeholders are written with angle brackets, and a raw `<` is not legal inside
an XML attribute value. `vstest.console.exe` writes the account and host names into attribute
values in the TRX header (`<TestRun name=`, `runUser=`, `<Deployment runDeploymentRoot=`,
`computerName=`, `storage=`, `codeBase=`). A blind textual substitution therefore leaves every
redacted TRX **not well-formed**, failing to parse at line 2. Issue #662 shipped six corrupt TRX
files this way; the executor applied the plan's hygiene rule exactly as written.

Two rules:
1. When the target is an XML-family file (`.trx`, `.xml`, `.coveragexml`, `.cobertura.xml`),
   substitute the **escaped** form — `&lt;user&gt;`, `&lt;host&gt;`, `&lt;repo-root&gt;`,
   `&lt;user-profile&gt;`. The parsed attribute value is then still exactly the placeholder, so
   redaction is unchanged and the document parses. Blind replacement of the bracketed token is
   safe in a TRX because no TRX element is named `user`, `host`, `repo-root` or `user-profile`.
2. **`ResidualMatchCount=0` is not a sufficient hygiene gate.** It measures only that the
   identifiers were removed, never that the file it rewrote still parses, so a sweep that corrupts
   every XML artifact it touches still reports success. Always add an assertion that every
   rewritten XML-family file re-parses.

Related structural gap seen on the same issue: a hygiene sweep that excludes `plan.md` from its
residual scan by path cannot detect a host path reintroduced into the plan, and a sweep that
rewrites only `evidence/` while scanning the whole feature folder detects residuals it cannot fix.

Two further verification rules learned the same way:
- **Scope the verification sweep to the files your branch changed** (`git diff --name-only
  <base>..HEAD`). A repo-wide sweep returns thousands of pre-existing hits in other feature folders
  and drowns your own signal.
- **A sanitisation record must not quote the raw "before" values.** An evidence artifact that
  documents its own substitution with a `From` column or a `BEFORE:` line reintroduces into a
  committed file exactly the identifiers it just removed. Describe each substituted token by class
  (workspace-root prefix, user-profile path, `computerName` attribute, `runUser`) and keep only the
  `AFTER:` lines.
