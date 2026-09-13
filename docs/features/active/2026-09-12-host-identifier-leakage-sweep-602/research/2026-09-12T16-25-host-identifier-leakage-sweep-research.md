# Research — Repository-wide host-identifier leakage sweep (Issue #602)

- **Issue:** #602
- **Feature folder:** `docs/features/active/2026-09-12-host-identifier-leakage-sweep-602`
- **Work mode:** full-bug
- **Timestamp:** 2026-09-12T16-25
- **Branch measured:** `worktree-agent-ae73e8a4777540363` at merge commit `2405a829d6afd3b12eb7c228d57158a97cb4e2ca`
- **Supersedes:** the 2026-09-12T13-45 research artifact from a terminated attempt on another branch. Its
  findings were reused where they re-verified against this worktree; every count and citation below
  was re-measured here.

## Redaction notice for this document

This document is a tracked file. It never reproduces the three leaked literals. It refers to them by
class and, where a concrete token is needed, by the repository's established placeholders:

| Identifier class | Placeholder used here |
| --- | --- |
| Absolute Windows user-profile directory (drive letter, users segment, account leaf) | `<user-profile>` |
| Bare developer account name (the leaf of that profile directory) | `<user>` |
| Bare uppercase machine name (host name, full token) | `<host>` |
| Host-name stem (the host token with its trailing digit run removed) | `<host-stem>` |

Placeholder tokens appear only in Markdown prose, tables and fenced examples. No angle-bracket token
is written anywhere in this document in a position shaped like an XML attribute value or XML element
text. Where an example needs a value in such a position, a bracket-free neutral value is used, which
is also the required production substitution for that position. No raw before-value is recorded.

## Measurement method and its standing caveat

No shell was available in this research session (Read, Grep, Glob and WebFetch only). Every count
below was produced with the Grep tool, which is ripgrep. Ripgrep's file set differs from the
tracked-file set in four known ways, all of which apply to every figure in this document:

1. It includes untracked files that are not ignored. This worktree carried two such items at session
   start (one agent-memory document and one directory under the run's documentation folder); both
   were probed and carry none of the three classes, so they do not inflate any count here.
2. It excludes tracked files that match an ignore rule. Tracked files under `artifacts/` are one such
   population; they were probed by explicit path and carry no identifier, so they do not deflate any
   count here.
3. It stops reading a file at the first NUL byte, whereas `git grep -I` classifies a file as binary
   only from its leading bytes. A text file with an early NUL can be counted by one tool and not the
   other.
4. It transcodes UTF-16 files that carry a byte-order mark, whereas `git grep -I` treats them as
   binary.

The orchestrator re-derived every population in this worktree with tracked-only `git grep` at the same
commit, excluding the staged promotion directory `docs/features/potential`. Those figures are the
authority for the phrase "no tracked file". The ripgrep figures are carried as an independent
cross-check. Where the two disagree, the disagreement is reported, not resolved.

---

## 1. Re-derived population counts

All searches case-insensitive; all exclude `docs/features/potential`; counts are distinct files with at
least one occurrence.

| Population | Orchestrator (git grep, tracked-only) | This session (ripgrep) | Agreement |
| --- | --- | --- | --- |
| Tracked files excluding the staged promotion directory | 15208 | not measurable without git | — |
| Absolute user-profile path, union of all four spellings | 1094 | 1095 | **disagree by one file** |
| Bare account name, unguarded | 1195 | 1195 | agree |
| Bare account name, negative preceding-character guard (not `@`, alphanumeric, or `<`) | 1187 | 1187 | agree |
| Package-coordinate form (at-sign-prefixed account name) | 13 | 13 | agree |
| Bare host name, full token | 183 | 183 | agree |
| Host-name stem (trailing digit run removed) | 186 | 186 | agree |
| Union of guarded account name and host name | 1188 | 1188 | agree |

### Extension decomposition of the 1188-file union

| Class | Orchestrator | This session |
| --- | --- | --- |
| Markdown (`*.md`) | 954 | 954 |
| XML and test-result (`*.xml`, `*.trx`) | 186 | 186 |
| Plain text, JSON, PowerShell, project file, TOML (everything else) | 48 | 48 |
| **Sum** | **1188** | **1188** |

Both decompositions sum to the union exactly. The ripgrep Markdown figure was obtained as 960 files
repository-wide minus the 6 identifier-bearing Markdown files under `docs/features/potential/promoted`,
which is the same exclusion the orchestrator applied.

### Directory decomposition of the 1188-file union

| Subtree | Orchestrator | This session |
| --- | --- | --- |
| `docs/features/archive/**` | 979 | 980 |
| `docs/features/active/**` | 189 | 189 |
| Remainder (everything else) | 20 | 19 |
| **Sum** | **1188** | **1188** |

The sums agree exactly, but one file sits in the archive bucket under ripgrep and in the remainder
bucket under git grep. The cause could not be established without git. No file exists directly under
`docs/features/archive/` or directly under `docs/features/`, so a pathspec-shape difference on a
root-level file is ruled out. The most plausible remaining explanations are caveats 2 through 4 above
acting on two different files in opposite directions. **The tracked-only figures govern.** The
executor's baseline capture will supersede both columns and must be produced by the tracked-only
command forms in section 10.

The 19 remainder files under ripgrep, which are the complete set of identifier-bearing files outside
the two feature subtrees, are: the workspace editor settings file; the add-in project file; the stray
raw test output at the repository root; the mirrored PowerShell test fixture; one document under
`docs/research`; three documents under `docs/features/epics`; the worktree-cleanup skill document;
nine agent-memory documents; and the agent-governance settings file, which is out of scope.

### Unexplained profile-path delta

The ripgrep union of profile-path spellings is one file higher than the tracked-only figure. Three
hypotheses were tested and eliminated: (a) an untracked file — the untracked locations carry no
profile path; (b) a mixed-separator spelling that the four canonical spellings miss but a one-or-more
separator class catches — a targeted search for every mixed and triple-separator form found two files,
neither of which contains the account name in a profile-path shape (both quote a drive-and-users
fragment with a placeholder or an alternation); (c) a tracked-but-ignored file — inverted direction.
The remaining candidates are caveats 3 and 4. The delta is recorded, not resolved.

### Case sensitivity

The prior artifact measured the case-sensitive host-token count at 163 files against 183
case-insensitive. This corroborates the recorded mechanism in
`.claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md`: the test runner
lower-cases one TRX attribute while leaving another mixed-case. **Every acceptance search must be
case-insensitive.** The figure was not re-measured this session because the case-insensitive count
reproduced exactly.

---

## Numeric Derivation Evidence

- Complete Family: host-name stem, host-name full token
- Exhaustive Search Scope: the entire tracked repository index, excluding only the staged promotion directory
- Inclusion Rules: a tracked file is a member when a case-insensitive fixed-string search finds the host-name stem in it and the same search for the host-name full token does not
- Exclusion Rules: untracked files, ignored files, and every path under the staged promotion directory are excluded
- Primary Search Strategy or Query Expression: tracked-only case-insensitive fixed-string set difference, taking the host-name stem file listing and removing every path that also appears in the host-name full token file listing
- Primary Member Set: .claude/agent-memory/atomic-executor/project_bash_heredoc_collapses_doubled_backslashes.md, docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/qa-gates/redaction-sweep.2026-08-26T22-44.md, docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/policy-audit.2026-09-04T02-11.md
- Primary Count: 3
- Cross-check Search Strategy or Query Expression: per-file probe of each of the three candidate paths, asserting that the host-name stem matches the file and that the host-name full token does not match it, run as a path-restricted listing rather than as a set difference
- Cross-check Member Set: .claude/agent-memory/atomic-executor/project_bash_heredoc_collapses_doubled_backslashes.md, docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/qa-gates/redaction-sweep.2026-08-26T22-44.md, docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/policy-audit.2026-09-04T02-11.md
- Cross-check Count: 3
- Member-set Comparison: the primary and cross-check member sets are identical

### How both derivations were re-run in this session

Primary. The stem listing (186 files) and the full-token listing (183 files) were both measured. The
full-token set is a subset of the stem set by construction, because the full token contains the
stem, so the set difference has exactly 186 − 183 = 3 members. A third search for the stem followed
by any character other than the trailing digit, or by end of line, returned exactly three files; each
is in the stem listing and, by the per-file probe below, absent from the full-token listing, so those
three files are the difference set.

Cross-check. A path-restricted search over exactly the three candidate paths for the full token
returned zero files. A path-restricted content search over the same three paths for the stem
returned matches in all three (one line, one line, and three lines respectively). The two member
sets are identical.

Content character of the three members, recorded so the executor knows what it is rewriting: the
atomic-executor memory quotes a case-insensitive residual-count expression that names the stem as a
search alternative; the redaction-sweep evidence record tabulates the stem as a sweep pattern; the
policy-audit artifact quotes a `git grep` alternation naming the stem three times. All three carry the
stem *as a search pattern* rather than as a path, so the substitution is correct but the surrounding
prose should still read as an example afterwards.

---

## 2. Raw-evidence deletion class inventory (role-based definition)

Per the maintainer decision on issue 671 of 2026-09-11 the project is projection-only for evidence, so
tracked raw test-result and coverage XML is deleted, not redacted. The deletion set is defined by
role, not by filename suffix, and is decided over the whole class rather than the identifier-bearing
subset.

### 2a. Counts, all re-verified this session

| Class | Files | Method |
| --- | --- | --- |
| Tracked test-result files (`*.trx`) | 332 | ripgrep any-line match under glob; agrees with orchestrator |
| Tracked XML under a feature-folder evidence path (`docs/features/**/evidence/**/*.xml`) | 305 | same; agrees |
| Tracked XML outside the features tree | 4 | enumerated: `QuickFiler/FodyWeavers.xml`, `TaskMaster/ThisAddIn.Designer.xml`, `TaskMaster/Ribbon/RibbonExplorer.xml`, `.cr/team/Options/StorageStreams.xml`; agrees |
| Whole XML family (`*.xml` + `*.trx` + `*.coveragexml`) | 641 | agrees; equals 332 + 305 + 4 exactly |
| Stray raw test output at the repository root, `test-output.txt` | 1 | present on disk with seven occurrences; tracked status verified by the orchestrator with git (introduced by commit `59bb26366`) |

The arithmetic 641 = 332 + 305 + 4 has a consequence worth stating: **there is no tracked XML file
under `docs/features` that is outside an evidence path, and no `.coveragexml` file is tracked.** The
deletion set is therefore decidable from the tracked-file listing alone: every `*.trx`, every `*.xml`
whose path contains a feature-folder `evidence/` segment, plus the root stray file. Root-element
inspection is a spot check on the enumeration, not the selection mechanism.

### 2b. Root-element spot check

Of the 305 evidence XML files, 241 have a line beginning with a Cobertura `coverage` root and 62 have a
line beginning with a Visual Studio export, JaCoCo, or test-run root; two were not classified by those
two probes. 238 carry the `.cobertura.xml` suffix and 15 the `.jacoco.xml` suffix. The Cobertura root
count (241) exceeds the Cobertura suffix count (238), confirming the prior finding that suffix-based
selection under-reaches. This is why the role-based path rule, not a suffix glob, selects the set.

### 2c. Identifier-bearing subset

Of the 186 identifier-bearing XML-family files, all sit inside the deletion set (every one is a
`*.trx` or an evidence-path `*.xml`; none is among the four source and build-configuration XML files).
**No identifier-bearing XML document survives the deletion, so no XML-aware substitution is ever
required**, and the sweep script may refuse the XML family outright (section 7).

### 2d. Filename-level leakage

Invisible to a content search. Glob over the tree for a filename containing the account name returns
two files; for the host name, the same two files:

- `docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/regression-testing/` — one default-named `.trx`
- `docs/features/archive/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/qa-gates/trx/` — one default-named `.trx`

Both are `.trx` and fall inside the deletion set. Filename leakage is fully resolved by the deletion
and needs no separate task, but the acceptance criteria must include a filename assertion because a
content-only sweep reports clean while leaving a leaking path.

### 2e. Size

Not re-measurable without a shell. The prior artifact's line-count proxy (1,559,699 lines across the
332 TRX; 59,617,696 lines across the 238 Cobertura-suffixed files) reproduced exactly this session via
ripgrep's total-occurrence counter for an any-line pattern, and is consistent with the maintainer's
byte figures (~281 MB and ~3227 MB) at ~180 and ~57 bytes per line respectively. Treat the byte figures
as corroborated by proxy, not directly measured.

---

## 3. Repository ignore rules

### 3a. Current state (root `.gitignore`, read in full this session)

Relevant existing lines, verbatim:

```
# MSTest test Results
[Tt]est[Rr]esult*/
[Bb]uild[Ll]og.*
```

```
# Visual Studio code coverage results
*.coverage
*.coveragexml
```

```
# dotnet-coverage Cobertura output (read by Koverage extension)
coverage/*
!coverage/.gitkeep
```

**`*.trx` is not excluded at the root, and neither is any Cobertura or JaCoCo pattern.** The existing
rules exclude the runner's default output *directory* and the live coverage drop *directory*. Neither
matches an artifact once it has been copied into `docs/features/*/evidence/**`, which is the provenance
of all 332 TRX files, all 305 evidence XML files, and both filename leaks.

### 3b. Per-folder precedent

Exactly three per-feature `evidence/.gitignore` files exist (Glob `**/evidence/.gitignore`): features
511, 484 and 365. Feature 511's file, read in full this session, carries `*.trx`, `*.coverage`,
`*.coveragexml`, `Deploy_*/`, a date-stamped run-directory pattern and an `r1-p*-t*/` pattern, with a
comment recording that the `Deploy_*` scratch directory name itself embeds the account and host. The
per-folder approach does not generalise: every future feature folder would have to remember it.

### 3c. Additive lines for the root `.gitignore`

Place adjacent to the existing coverage block:

```
# Raw machine test-result and coverage artifacts are never committed (issues #602, #671).
# The evidence of record is the distilled markdown projection alongside these paths. The
# test runner's default TRX name also embeds the operator account and machine name, so
# committing one leaks a host identifier in the filename as well as in the document body.
*.trx
*.cobertura.xml
*.jacoco.xml
Deploy_*/
```

- `*.trx` is the load-bearing line: it matches the artifact wherever it is copied to and closes the
  vector behind both filename leaks.
- `*.cobertura.xml` and `*.jacoco.xml` cover the two suffixed coverage formats.
- **Honest limitation:** the Visual Studio coverage export is written under arbitrary names
  (`coverage-post.xml`, `coverage-baseline.xml`, `final-coverage-repository.xml`,
  `diagnostic-dotnet-coverage.xml` and others were enumerated this session). No glob distinguishes
  those from legitimate XML such as `RibbonExplorer.xml` or `FodyWeavers.xml`, so **no safe ignore
  rule exists for that format**. It must be caught by the residual search and by review, and the spec
  must say so.
- `Deploy_*/` is carried over from the feature-511 precedent with its rationale.
- `*.coverage`, `*.coveragexml`, `[Tt]est[Rr]esult*/` and `coverage/*` already exist and must not be
  duplicated or weakened. The `[Tt]est[Rr]esult*/` rule is currently the only thing keeping the
  runner's default emission out of the porcelain status (section 11).

---

## 4. Workspace editor settings file

`.vscode/settings.json` is 29 lines and contains **exactly one** occurrence of any identifier class:
line 27, the first (and only) element of `powerquery.client.additionalSymbolsDirectories`, a
forward-slash profile path with a lower-case drive letter, ending in `/repos/TaskMaster/.vscode/excel-pq-symbols`.
Every other key (`dotnet.defaultSolution`, `koverage.coverageFilePaths`, `koverage.coverageFileNames`,
both `chat.tools.terminal.autoApprove` regexes) is already workspace-relative. The target directory
exists inside the repository (`.vscode/excel-pq-symbols/excel-pq-symbols.json`, confirmed by Glob).

### Portable replacement

```
    "${workspaceFolder}/.vscode/excel-pq-symbols"
```

An angle-bracket placeholder is wrong in this position: this is a live configuration value consumed
by an extension, not documentation. AC3's own wording is "portable placeholders **or environment
references**", and the workspace-folder variable is an environment reference.

### Whether the key participates in variable substitution — partially resolved, still not executed

The prior artifact recorded this as unknown because the extension's README returned 404. This session
reached the extension's sources instead (WebFetch, 2026-09-12, `microsoft/vscode-powerquery` master):

- The extension manifest describes the key as "One or more absolute file system paths to directories
  containing M language symbols in json format." It says nothing about variables or relative paths.
- The client (`client/src/extension.ts`) reads the array with a plain configuration `get` and passes it
  unchanged to the library symbol manager.
- The library symbol manager (`client/src/librarySymbolManager.ts`) applies only `path.normalize`,
  de-duplicates, and converts each string with `vscode.Uri.file`. **No variable substitution and no
  workspace-relative resolution exists in either file.**
- VS Code core substitutes predefined variables only in a documented, select set of setting keys; this
  key is not among them.

Consequence: it is **likely** that neither the variable form nor a bare relative form resolves for
this extension, and that the setting becomes inert after the edit. This has not been verified by
execution. The blast radius is bounded: the extension is not among the three workspace-recommended
extensions (`.vscode/extensions.json` names only the C# Dev Kit, the C# extension and the pull-request
extension), no toolchain gate depends on it, and the symbol source is an optional editor convenience.

Recommendation: use the variable form because it is what AC3 names and it reads correctly to a human;
record in the change description that the extension's sources show no substitution so the key is
expected to be inert; and note that the functional alternative — a user-scope setting outside the
repository — is out of this item's scope. Do not use a bare relative path as the fallback: the symbol
manager would hand it to `vscode.Uri.file`, which produces a root-relative URI, so the relative form
does not improve on the variable form and the prior artifact's fallback ordering should not be relied
upon.

---

## 5. Ordered substitution rule set and idempotency

The account name is a proper substring of the profile path. An account-name rule applied first
corrupts every profile path into a hybrid no later rule can repair. **Longest-first ordering is
mandatory.** Apply in this order, every rule case-insensitive:

| Order | Matches | Replacement | Rationale |
| --- | --- | --- | --- |
| 1 | Drive-letter profile path, doubled-backslash spelling | `<user-profile>` with doubling preserved | Longest form; must precede rule 2 or rule 2 consumes half of it |
| 2 | Drive-letter profile path, single-backslash spelling | `<user-profile>` | |
| 3 | Drive-letter profile path, forward-slash spelling | `<user-profile>` | |
| 4 | MSYS spelling: leading slash, drive letter, users segment | `<user-profile>` | Missed entirely by a naive leading-slash search from Git Bash |
| 5 | Bare account name **not preceded by an at-sign, an alphanumeric, or an opening angle bracket** | `<user>` | Guard protects the package coordinate and makes the rule idempotent |
| 6 | Bare host name, full token | `<host>` | Disjoint from 1–5 |
| 7 | Host-name stem | `<host-stem>` or `<host>` per the spec's choice | Must run **after** rule 6, or rule 6 never matches; three files carry the stem alone |

Two corrections to a naive reading are load-bearing.

**Separator classes use a one-or-more quantifier** so a single expression handles both the single and
the doubled backslash; ordering alone is then not the only defence against a reordering mistake.

**The idempotency trap in rule 5.** Rules 1–4 emit a token containing neither a drive letter nor a
users segment, so they cannot re-fire. Rule 5 emits a token whose preceding character is always an
opening angle bracket. If the guard class excludes only the at-sign and alphanumerics, rule 5 re-fires
on its own output on the second pass and the transform is not idempotent. The guard must therefore
also exclude `<`. The same guard must appear in the account-name acceptance search, or that search
fires on its own remediation. This must be covered by a dedicated unit test and by the repository-scale
second-run empty-diff assertion.

**Rule 7 ordering.** The stem is a proper prefix of the full token, so a stem rule placed before the
full-token rule would rewrite every full token into a placeholder followed by a digit run. The stem
rule runs last.

---

## 6. The package-coordinate false positive

`.mcp.json` line 5 and `.codex/config.toml` line 5 each reference an npm package whose scope segment
equals the account name (`@<user>/drm-copilot-mcp`, the second with a pinned version). Thirteen tracked
files carry the at-sign-prefixed form; eleven of them are Markdown documents that quote the same
coordinate. A blind case-insensitive substitution on the bare account token rewrites the coordinate
and **breaks MCP server resolution for every agent session in this repository.**

The guard must be the structural negative preceding-character class of rule 5, not a path allow-list.
An allow-list naming the two configuration files would already be wrong today, because eleven further
documents quote the coordinate, and it would fail again the next time a document quotes it. The guard
appears in both the transform and the acceptance search so the two agree on what counts as a leak.

Measured consequence: unguarded 1195 minus guarded 1187 is 8, not 13, because five of the thirteen
coordinate-carrying files also carry an unguarded occurrence elsewhere and remain in the guarded set.

---

## 7. Scripted sweep versus manual edits

### The arithmetic forces a script

After the deletion removes the 186 identifier-bearing XML-family files, 1002 files remain that need a
textual substitution (954 Markdown plus 48 other). That is far beyond what an executor can edit with
individual Write/Edit calls. **A PowerShell sweep script is required.**

Applicable constraints, verified: `.claude/hooks/enforce-powershell-batch-budget.ps1` states "3
production PowerShell files per batch, 3 test PowerShell files per batch"; the 500-line file cap; tests
mirror the production tree under `tests/`; PSScriptAnalyzer and Pester apply. `scripts/dev-tools/`
exists (it holds `run-actionlint.ps1`), so a production script at
`scripts/dev-tools/Repair-HostIdentifierLeak.ps1` with a mirrored test at
`tests/scripts/dev-tools/Repair-HostIdentifierLeak.Tests.ps1` consumes one slot in each bucket. This
repository has no Python toolchain and no developer-tools Python tree; do not propose one.

Design constraints that keep one production file sufficient:

1. **Separate the pure transform from the I/O.** One exported pure function taking text and the
   ordered rule set and returning text carries all testable behaviour; the driver is a thin
   enumerate-read-transform-write loop. Temporary files are prohibited in tests, so the transform must
   accept and return text rather than paths.
2. **Refuse the XML family and throw** (`.trx`, `.xml`, `.coveragexml`, `.csproj`). Section 2c shows no
   surviving file needs an XML-aware substitution, so this removes the whole XML-escaping code path
   and makes the feature-488/662 corruption mechanically impossible.
3. **Detect by default; rewrite only under an explicit apply switch.** Detect mode doubles as the
   recurrence guard and as the runner for the acceptance counts (section 10).
4. **Derive the tokens from the environment** (`$env:USERPROFILE` leaf, `$env:COMPUTERNAME`, stem by
   stripping the trailing digit run) with explicit parameter overrides, so the script contains none of
   the identifiers it removes. Without this the script would fail the item's own AC1/AC2.
5. **Enumerate directories from inside the script.** The run's documentation directory cannot be
   named on a Bash command line (its name contains a refused word); the script's own directory walk
   reaches it without naming it on any command line.

### The four structurally sensitive files that must be hand-edited

**(a) `.vscode/settings.json`** — one line; section 4.

**(b) `TaskMaster/TaskMaster.csproj` line 37** — the `PublishUrl` element's text content is an absolute
publish directory carrying the account name and, through a commercial cloud-storage folder segment,
the employer organisation name. This is element text, not an attribute value, but a raw `<` is
illegal in XML character data just as in an attribute value, so a placeholder here produces a project
file MSBuild cannot load. Adjacent `InstallUrl`, `PublisherName` and `SupportUrl` elements (lines 38,
46, 47) are already empty. **Settled by the spec (Decision 3): empty the element**, matching the three
neighbours; removal was rejected because it changes ClickOnce configuration and the line is shared
with another issue. Verify with a full MSBuild rebuild afterwards.

**(c) `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`** — four occurrences in three
structural positions, re-verified this session:

| Line | Position |
| --- | --- |
| 41 | single-quoted string, `$worktreeRoot` (profile path, worktree-suffixed leaf) |
| 42 | single-quoted string, `$canonicalRoot` (profile path, leaf `TaskMaster`) |
| 104 | `filename` attribute of a `class` element inside a here-string cast to `[xml]` on line 124 |
| 124 | single-quoted `-RepoRoot` argument (same worktree root as line 41) |

Line 104 is a genuine XML attribute value; an angle-bracket placeholder there reproduces the
feature-488/662 corruption as a Pester failure. The production helper
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` lines 73–78 derive the canonical root by taking
the *parent* of the supplied repo root and joining the literal leaf `TaskMaster` whenever the supplied
leaf is not itself `TaskMaster`. **Three semantic invariants any substitution must preserve:**

1. the worktree root and the canonical root share a parent directory;
2. the canonical root's leaf is exactly `TaskMaster`;
3. the worktree root's leaf is not `TaskMaster`.

Flattening both roots to one neutral value violates 1 or 3; rewriting the leaf violates 2. Replace only
the profile-path prefix, identically in all four positions, with a bracket-free neutral root that
follows the file's own convention (lines 31, 60, 411–420 already use `C:\repo` and `C:\fake`), for
example a prefix of the shape `C:\fake\repos`. That prefix contains no users segment and no account
name, so it is clean under every acceptance search, and it remains a Windows-shaped path so
`Split-Path` behaves identically. The line-104 attribute then reads, bracket-free:

```
filename="C:\fake\repos\TaskMaster\ToDoModel\Data Model\ToDo\ToDoItem.cs"
```

Exclude the file from the scripted sweep and re-run its Pester suite as a regression gate.

**(d) `.gitignore`** — additive only; section 3c.

---

## 8. Settled decisions carried forward from the spec (do not relitigate)

The spec in this feature folder records five decisions on the prior artifact's open questions. They
are restated here only so this document is self-contained:

1. `.claude/skills/cleanup-merged-worktrees/SKILL.md` (two forward-slash profile paths at lines 456
   and 466, inside JSON example blocks, re-verified) **is swept**; the maintainer's push-down exclusion
   names exactly one file. An upstream correction should follow.
2. The deletion class is the **role-based** definition of section 2.
3. The publish-URL element is **emptied**, not removed.
4. Plain-text console logs under evidence paths are **edited in place**.
5. `test-output.txt` is **deleted**.

---

## 9. Contention surface against the two relevant siblings

Verified facts that narrow the apparent overlap:

- `scripts/vscode/**`: thirteen files, zero occurrences of any class (re-verified). This item writes
  nothing there.
- `CLAUDE.md` at the repository root: clean (re-verified). This item does not write it.
- `.github/**`: clean (re-verified).
- The run's documentation directory: verified clean by the orchestrator; not a write target. Naming it
  as one would create a false contention edge against every item in the run.

The real overlap is three surfaces: the mirrored PowerShell test fixture under `tests/scripts/vscode/`
(this item hand-edits it; either sibling may touch it), the agent-memory documents, and
`.vscode/settings.json` (the first sibling also edits it).

**Agent-memory delta, re-verified: nine documents, not six.** Under `.claude/agent-memory/`:
`atomic-executor/project_bash_heredoc_collapses_doubled_backslashes.md`,
`epic-orchestrator/feedback_measure_whole_volume_before_blaming_worktrees.md`,
`epic-planner/reference_isolated_worktrees_cut_from_main_not_session_head.md`,
`feature-review/project_464-review-residuals.md`, `feature-review/project_488-review-residuals.md`,
`orchestrator/angle-bracket-redaction-breaks-trx-xml.md`,
`orchestrator/bash-tool-collapses-double-backslash-in-sed.md` (five occurrences),
`orchestrator/collect-pr-context-lands-in-main-checkout.md`,
`orchestrator/preparation-child-cwd-is-session-root-not-item-worktree.md`. Whichever item lands second
must re-measure rather than assume, because AC2 is repository-wide. Two of the nine (the two
"collapses backslash" memories) quote the identifiers as search patterns while documenting a search
defect; the substitution is still correct, but the prose may need a one-line adjustment so the example
still reads as an example.

Out of scope and written in plain prose deliberately: the agent-governance settings file under the
agent directory (one doubled-backslash profile path at line 75, no host name, re-verified), which is
published by a push-down from the upstream governance repository with zero templating and must be
corrected upstream; the explicit results-directory and log-file-name change to the test runner,
delivered by a sibling item; and the upstream governance-repository portion.

---

## 10. Safe search forms for the acceptance conditions

### Constraints these forms honour

- Under worktree isolation the Bash tool refuses any git command containing a shell variable
  expansion, refuses `pwsh`, and refuses `git -C` against another checkout. The execution child must
  therefore run **without** isolation; a preflight reviewer under isolation validates the forms by
  construction and must say so rather than claim to have run them.
- Git Bash rewrites a leading forward slash in a search literal into a Windows path before `git` sees
  it. Any leading slash must sit inside a bracketed character class, `[/]c`, which is not path-shaped
  and survives the rewrite. Six files carry the MSYS spelling, so this is not hypothetical.
- The run's documentation directory can never be named on a command line. None of the forms below
  names it; they search the whole index minus the staged promotion directory.
- `| wc -l` is accepted, so counts can be produced without dumping hundreds of paths.
- Committed evidence must contain no identifier literal, so the tokens are derived from the
  environment. **Correction to the prior artifact:** `basename "$USERPROFILE"` does not yield the
  account leaf under Git Bash, because the variable carries a backslash path and `basename` splits
  only on forward slashes. Use a parameter expansion that strips through the last separator of either
  kind, or derive the tokens in PowerShell.

### Recommended carrier

Because the TaskMaster Bash allowlist admits only `git`, `gh`, `pwsh`, `poetry run` and three library
scripts, and checks every chained segment, a bare shell assignment is itself a non-allowlisted segment.
The cleanest carrier is therefore the sweep script's **detect mode**, invoked as
`pwsh -NoProfile -File scripts/dev-tools/Repair-HostIdentifierLeak.ps1 -Detect`, which derives the
tokens from `$env:USERPROFILE` (leaf via `Split-Path -Leaf`) and `$env:COMPUTERNAME` (stem via a
trailing-digit-run regex replace), shells out to tracked-only `git grep`, and prints one count per
class. The committed evidence then records that literal-free command line and the counts. From
PowerShell the MSYS rewrite does not occur, but keep `[/]c` anyway so the same expression is valid from
either shell.

### The expressions, one per class, tracked-only, case-insensitive

Written here as the `git grep` forms the script must emit, with `ACCT`, `HOSTTOK` and `STEM` standing
for the environment-derived tokens and `EXCL` for `:(exclude)docs/features/potential`:

Class 1 — absolute user-profile path, all four spellings in one expression:

```
git grep -I -i -l -E "([cC]:|[/]c)[\\/]+[uU]sers[\\/]+ACCT" -- . EXCL | wc -l
```

Class 2 — bare account name with the structural guard (excludes the package coordinate and an
already-placed placeholder):

```
git grep -I -i -l -E "(^|[^@<a-zA-Z0-9])ACCT" -- . EXCL | wc -l
```

Class 3a — bare host name, full token, fixed-string:

```
git grep -I -i -l -F -- HOSTTOK -- . EXCL | wc -l
```

Class 3b — host-name stem, fixed-string (the supplementary assertion; three files today):

```
git grep -I -i -l -F -- STEM -- . EXCL | wc -l
```

Filename assertion — `git ls-files` piped through a case-insensitive match on the account name and on
the host name, expected empty; the two default-named TRX files are the current non-zero baseline.

### Non-vacuity

Each form returns a non-zero count against the current tree (1094, 1187, 183, 186 tracked-only). The
plan must require the executor to run all four **before** the sweep with the identical command text
and record the non-zero baseline as an evidence artifact under this feature folder's `evidence/`
tree. A post-sweep zero without a recorded pre-sweep non-zero is an unfalsifiable claim.

### Terminal values

Because the agent-governance settings file is out of scope and carries the profile path and the
account name but not the host name, the terminal values are **one** for class 1, **one** for class 2,
**zero** for classes 3a and 3b, and **zero** for the filename assertion. The spec already states this;
a plan that asserts zero for classes 1 and 2 is wrong in both landing orders.

---

## 11. Ordering risk: if this item lands before the runner-arguments sibling

The concurrent orchestration surface has **no dependency edges**; it derives concurrency solely from
blast-radius overlap. This item is expected but not guaranteed to run after the sibling that gives the
runner an explicit results directory and controlled log file name.

What reintroduces the prefix today, re-verified: `scripts/vscode/Invoke-MSTest.ps1` line 54 returns
the assembly list plus `/Settings:`, `/InIsolation` and a test-case filter, and
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` line 76 does the same. Neither directory contains a
`/ResultsDirectory:` or a `LogFileName=` token. The runner keeps emitting default-named TRX on every
local run until the sibling lands.

Why that does not break this item: the default output lands in the runner's default results directory,
which the root `[Tt]est[Rr]esult*/` rule already ignores, so the default emission is not itself a
tracked-file leak. The leak occurs when an artifact is **copied** into an evidence tree and committed,
and the `*.trx` root ignore rule in section 3c closes that vector wherever the copy lands. That rule is
inside this item's scope and depends on nothing the sibling delivers.

| Aspect | State if this item lands first |
| --- | --- |
| Tracked files containing an identifier (outside the excluded promotion directory) | the terminal values of section 10 |
| Tracked filenames containing an identifier | zero |
| TRX newly emitted by a local run | still default-named, in an ignored directory |
| Can a new raw artifact re-enter the index by copy? | no — `*.trx`, `*.cobertura.xml`, `*.jacoco.xml` at root |
| Can a Visual Studio export under an arbitrary name re-enter? | yes — no safe glob; caught only by the residual search |
| Must an evidence author remember anything? | yes — cite a sanitized name in Markdown until the sibling lands |

Self-consistency requirements: add the ignore lines; never assert anything about the runner's argument
list in this item's criteria (it would fail when landing first and over-claim when landing second);
never weaken `[Tt]est[Rr]esult*/`; state that the sweep is forward-only (history retains everything;
AC1/AC2 are working-tree assertions at the tip); record the residual authoring obligation in rollout
notes.

---

## 12. Prior art

**Convention record.** `.claude/agent-memory/_shared_no_absolute_host_paths.md` is the canonical
statement, read in full this session. It already carries the placeholder table used here, prefers a
repo-relative path where expressible, documents the TRX naming trap and its two mitigations, records
the feature-511 history, quantifies the case-sensitivity trap (946 leaked paths in one TRX; 16 TRX
needing 5,668 substitutions), and records the issue-645 Cobertura leak (2,007 `filename=` occurrences
per report). Cite it; do not restate it in the spec.

**Feature 511 validated deletion over redaction at smaller scope.**
`docs/features/active/2026-08-21-winformspumphost-suite-determinism-511/evidence/other/raw-vstest-artifact-disposition.2026-08-23T21-40.md`
(read this session) records the deletion of 56 TRX (358.3 MB) and 42 binary coverage attachments
(822.2 MB) rather than scrubbing them, on the reasoning that the distilled Markdown is the evidence of
record; it independently re-derived the decisive figures from the raw TRX before deletion to prove the
Markdown was a faithful distillation, and it added the per-folder ignore file of section 3b. This item
applies the same disposition repository-wide under the issue-671 decision. The disposition record is
the template for this item's deletion evidence artifact: counts by class, per-directory counts, a
faithfulness cross-check, and an explicit reversibility statement.

**Feature 488 XML corruption.** Recorded in
`.claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md`: a bracketed placeholder
substituted into TRX `storage=` and `codeBase=` attributes made all nineteen committed TRX files
unparseable at line 2, and a case-sensitive sweep reported clean because the runner lower-cases
`storage=`. The corruption survived a full feature review because the review re-derived coverage from
Cobertura rather than TRX.

**Feature 662 recurrence, worse mechanism.** The approved plan itself mandated the four bracketed
placeholders, so a correct executor shipped six unparseable TRX files, and the plan's gate asserted only
a zero residual count, which a document-destroying rewrite satisfies perfectly.

**How this research discharges the inherited rules:** every identifier-bearing XML file is deleted, so
no bracketed token is ever written into XML (section 2c) and the script refuses the XML family
(section 7); every acceptance search is case-insensitive (section 10); the machine token is swept
separately from the account token, and the stem separately from the full token (section 10); a zero
residual is paired with a recorded non-zero baseline and format-validity gates (section 13); no
sanitisation record quotes a before-value; and the convention's "scope the sweep to changed files"
instruction is explicitly waived because this item changes essentially every file in the population.

---

## 13. Test strategy

- **Pure Pester unit tests for the transform**, filesystem-free and temporary-file-free: longest-first
  ordering leaves a profile path intact when the account rule is armed; each of the four spellings
  rewrites to the same placeholder; doubled-backslash input preserves its doubling; **idempotency —
  applying the transform twice equals applying it once** (catches the rule-5 self-match); the guard
  leaves an at-sign-prefixed coordinate untouched; the stem rule does not fire on a full token already
  rewritten and does fire on a bare stem; an XML-family extension throws rather than rewriting.
- **Regression gate on the hand-edited fixture:** re-run the `Invoke-MSTestWithCoverage.Helpers`
  Pester suite. It is the only existing test whose assertions depend on the literal content being
  changed.
- **Build gate on the project file:** the analyzer and nullable MSBuild commands from the C# toolchain
  with `/t:Rebuild`, since an incremental build skips compilation and cannot fail.
- **Four residual-count assertions** (classes 1, 2, 3a, 3b) plus the filename assertion, each paired
  with its recorded non-zero pre-sweep baseline captured by the identical command text.
- **Repository-scale idempotency:** a second apply-mode run over the swept tree yields an empty diff.
- **Format validity for rewritten files:** JSON parse of `.vscode/settings.json` and the two
  `*.process-tree.json` evidence captures; the Pester and MSBuild gates above. A well-formedness gate
  for the XML family is deliberately absent, and the reason is recorded: the sweep refuses XML-family
  extensions and every identifier-bearing XML document is deleted rather than rewritten.
- **Tracked-versus-ripgrep reconciliation:** confirm the tracked-only counts agree with the ripgrep
  cross-check modulo the known one-file deltas of section 1, which discharges this document's standing
  caveat.

### Why a zero residual count is necessary but not sufficient

A post-sweep zero is satisfied equally by a correct sweep, by a sweep that emptied or destroyed the
files it rewrote, and by a search expression that never matched anything. Feature 662 is the concrete
case. The mitigation has two halves and both are mandatory: a recorded non-zero pre-sweep baseline by
the identical command (turns the zero into a measured transition), and the format-validity and
behaviour gates above (distinguish a correct sweep from a destructive one). The stem assertion adds a
third half specific to this item: a full-token zero alone leaves the machine name recoverable in three
files.

---

## 14. Rejected alternatives (brief)

- **History rewriting** — a maintainer decision, not mechanical; AC1/AC2 are working-tree assertions.
- **Deleting Markdown evidence instead of editing it** — the projections are the evidence of record
  that the XML deletion depends on.
- **A throwaway in-session rewriter** — same budget slot, no recurrence guard, and an untested
  thousand-file rewriter in the change path, which is the feature-488/662 shape.
- **Suffix-based deletion (`*.cobertura.xml` only)** — reaches 238 of 305 evidence XML files; rejected
  for the role-based definition.
- **Path allow-listing the package coordinate** — already wrong today (thirteen carriers) and brittle.
- **A bare workspace-relative path as the editor-settings fallback** — the extension hands the string
  to `vscode.Uri.file` unchanged, so it does not resolve any better than the variable form.
- **Removing the publish-URL element** — rejected per the spec's Decision 3.

## 15. Residual unknowns

- The one-file profile-path delta and the one-file archive/remainder bucket shift between ripgrep and
  tracked-only search (section 1). The tracked-only figures govern; the executor's baseline supersedes
  both.
- Whether the Power Query extension's setting resolves the workspace-folder variable at runtime. The
  sources show no substitution; execution was not attempted.
- Whether `tests/scripts/dev-tools/` already exists; `tests/scripts/vscode/` is the layout precedent.
