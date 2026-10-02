---
name: evidence-identity-hygiene-sweep-927
description: Issue #927 research lessons - raw evidence documents must be classified by XML root not by name (243 Cobertura incl. 2 with root tag on its own line, 23 dotnet-coverage <results>, 27 raw Pester JaCoCo of which 5 are named .jacoco.xml, 18 projections); _pester.yml path lists are explicit; a generic profile-path regex trips its own research doc
metadata:
  type: project
---

Raw-evidence classification for the hygiene guard must be content-based, and the guard regex
catches the researcher's own document unless examples are written as placeholders.

**Why:** Measured 2026-09-28 under `docs/features/**/*.xml` (311 files): 243 Cobertura roots
(3 with no `cobertura` in the name; 2 in the 139 folder put `<coverage` alone on a line so a
"`<coverage ` plus space" match misses them), 23 `dotnet-coverage` native `<results>` documents
that no name pattern catches, 45 JaCoCo `<report>` roots of which 27 contain `<class`/`<sourcefile`
(raw Pester output; 5 of those are named `*.jacoco.xml`) and 18 are package-level projections.
132 of 332 tracked `.trx` still carry a profile path; two `.trx` file NAMES embed account+host.
`_pester.yml` `Run.Path`/`CodeCoverage.Path` are explicit arrays (`dependencies`, `vscode`), so a new
`scripts/<folder>` is neither tested nor measured until both arrays are extended; the README row
for `_pester.yml` is already stale. The first draft of the research file contained two contiguous
drive-rooted `Users` literals (an upper-case fixture and a doubled-backslash fixture) and would
have failed the proposed guard.

**How to apply:** When counting or removing raw evidence, enumerate by root element with a
whitespace-or-`>` terminator and discriminate JaCoCo by child elements, never by suffix. When a
research or plan file must describe a profile path, write `<drive>:\Users\<segment>` or prose,
never a drive letter followed by a colon. No production code parses the `Users` segment (only
`SpecialFolder.UserProfile` is read at `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs:230`),
so C# fixtures can move to `C:\Fixtures\<segment>\...` without an allowlist. Related:
[[no-absolute-host-paths]], [[committed-cobertura-baselines]].
