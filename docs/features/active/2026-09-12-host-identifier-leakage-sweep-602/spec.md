# host-identifier-leakage-sweep (Spec)

- **Issue:** #602
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12
- **Status:** Approved for planning
- **Version:** 1.1 — supersedes the 1.0 draft produced by a terminated attempt on another branch.
  Every count and structural claim below was re-measured or re-verified in this worktree at merge
  commit 2405a829d; the orchestrator findings F1 through F8 recorded in this document override the
  1.0 draft wherever they differ.
- **Work Mode:** full-bug — this document is the sole authoritative acceptance-criteria source. The
  companion user story in this folder exists only to satisfy a mechanical artifact check and carries
  no checkboxes.

> **Formatting invariant — do not "fix" this.** A downstream scheduler derives this item's change
> footprint by harvesting whitespace-free tokens wrapped in Markdown backticks from this document and
> from the plan, and it has no notion of polarity. The Write Set section near the end of this document
> is therefore the only place where a repository path appears inside a code span. Everywhere else —
> including every path this item deliberately does not write — paths are written as plain prose, and
> literal file content is shown in four-space-indented blocks rather than in fenced or inline code
> spans. Adding backticks anywhere outside the Write Set creates a false contention edge against the
> twelve sibling items in the same concurrent run.

> **Redaction invariant.** This document is itself a tracked file. Writing the account name, the host
> name, the 8.3 short-name form of the account, or an absolute user-profile path into it would
> re-create the defect and falsify AC1 through AC4 by self-reference. All four are referred to by
> class, or by the placeholder tokens defined in the canonical convention record at
> .claude/agent-memory/\_shared\_no\_absolute\_host\_paths.md, which this document cites rather than
> restates. The escaped tokens \<user-profile\>, \<user\> and \<host\> appear only in Markdown prose
> and tables. No raw before-value is recorded anywhere in this feature folder.

> **XML invariant.** No angle-bracket token appears anywhere in this document in a position shaped
> like an XML attribute value or XML element text. The corresponding disposition rule for the change
> itself: when a tracked XML file carries a host identifier, the file is deleted rather than redacted
> in place. Per the maintainer decision on issue 671 of 2026-09-11 the project has moved to
> projection-only evidence, so deletion is the intended outcome for the raw test-result and coverage
> class, not a fallback.

## Context

### Summary of the defect and its impact

Tracked files across this repository embed developer-machine identifiers. Four spellings are live:

1. an absolute Windows user-profile path, in four separator spellings (doubled backslash, single
   backslash, forward slash, and the MSYS leading-slash form);
2. the bare account name that is the leaf of that profile directory;
3. the bare upper-case machine name, which also appears as a stem with its trailing digit run
   removed;
4. the 8.3 short-name form of the account name — the first six characters of the account name,
   upper-cased, followed by a tilde and a digit. This spelling was not known to the 1.0 draft
   (orchestrator finding F2). It appears both as the account segment of an absolute user-profile path
   and inside a flattened temporary-directory segment in which path separators have been replaced by
   dashes.

The standing repository convention prohibits all of these and prescribes portable placeholders or
repository-relative paths instead.

Impact:

- A committed artifact that hard-codes one developer's profile path is not reproducible by anyone
  else and documents a machine rather than a procedure.
- Account and host names are gratuitous identifying information in a repository that may be shared
  or made public.
- Because the default test-runner output naming reintroduces the account-and-host prefix on every
  local run, text-only cleanup regresses unless the reintroduction vector is closed.

The winformspumphost-suite-determinism feature (issue 511) sanitized its own folder on 2026-08-23 and
recorded the convention, but deliberately scoped its change to that folder so as not to break its own
three-file scope lock. This item is the remainder.

### Observed environment

Windows development workstation; Visual Studio and MSBuild toolchain; vstest console runner; VS Code
workspace. The defect is environment-independent once committed: the identifiers are static text in
tracked files and are visible to every clone.

### Severity and frequency

Medium. Deterministic — every affected file carries the identifier on every checkout. The population
grows with each feature merge that commits raw machine artifacts or cites a default-named test-result
file.

## Repro & Evidence

### Steps to reproduce

1. Clone the repository and check out merge commit 2405a829d.
2. Run a tracked-only, case-insensitive content search for each identifier class, using the safe
   search forms described under Test Strategy, excluding the staged promotion directory.
3. Observe non-zero file counts for every class.
4. List tracked files whose filename contains the account name or the host name; observe two
   test-result files.

### Expected versus actual

Expected: no tracked file contains an absolute user-profile path, a bare account name, a bare host
name, or the 8.3 short-name spelling, in its content or in its filename. Actual: over a thousand
tracked files contain at least one, and two tracked filenames embed both the account and the host.

### Frequency and determinism

Always. The counts are reproducible to the file on a fixed commit.

### Measured scope

The figures below were re-derived by the orchestrator in this worktree at merge commit 2405a829d with
tracked-only git search, case-insensitive, excluding the staged promotion directory. Tracked-only
search reads only files in the index and is therefore the authority for the phrase "no tracked file".
The research artifact's ripgrep figures are carried as an independent cross-check; where the two
disagree the tracked-only figure governs, and the executor's baseline capture supersedes both.

| Population | Tracked-only (governs) | ripgrep cross-check |
| --- | --- | --- |
| Tracked files, excluding the staged promotion directory | 15208 | not measurable without git |
| Absolute user-profile path, union of the four canonical spellings | 1094 | 1095 |
| Bare account name, unguarded | 1195 | 1195 |
| Bare account name with the negative preceding-character guard | 1187 | 1187 |
| Package-coordinate form (at-sign-prefixed account name) | 13 | 13 |
| Bare host name, full token | 183 | 183 |
| Host-name stem, trailing digit run removed | 186 | 186 |
| Union of guarded account name and host name | 1188 | 1188 |
| Union including the 8.3 short-name spelling | 1189 | not measured |
| 8.3 short-name spelling alone | 7 | not measured |
| Agent-memory documents carrying an identifier | 9 | 9 |
| Tracked filenames carrying the account name | 2 | 2 |
| Tracked filenames carrying the host name | 2 | 2 |

Extension decomposition of the 1188-file union, which sums exactly:

| Class | Files |
| --- | --- |
| Markdown | 954 |
| XML and test-result | 186 |
| Everything else (plain text, JSON, PowerShell, project file, TOML) | 48 |
| **Sum** | **1188** |

Directory decomposition of the same union, which also sums exactly:

| Subtree | Files |
| --- | --- |
| Archived features | 979 |
| Active features | 189 |
| Remainder | 20 |
| **Sum** | **1188** |

Deletion-class inventory, measured by tracked-file listing rather than by content search:

| Class | Tracked files |
| --- | --- |
| Test-result files with the TRX extension | 332 |
| XML under a feature-folder evidence path | 305 |
| XML outside the features tree (source and build configuration; none carries an identifier) | 4 |
| Whole XML-family inventory | 641 |
| Stray raw test output at the repository root | 1 |

The arithmetic 641 = 332 + 305 + 4 establishes that no tracked XML file under the features tree sits
outside an evidence path and that no file with the Visual Studio coverage-XML extension is tracked.

Script enumeration scope, measured by the orchestrator (finding F8):

| Enumerated class | Files |
| --- | --- |
| Active-features Markdown | 4367 |
| Archived-features Markdown | 6401 |
| Epics Markdown | 38 |
| One-off research Markdown | 4 |
| Agent-memory Markdown | 1047 |
| Feature-folder evidence plain text | 111 |
| Feature-folder evidence process-tree JSON | 3 |

Roughly 11971 files are enumerated in order to rewrite about 1000. The consequence for the script
design is recorded under Proposed Fix.

### The one unresolved measurement

The research artifact's working-tree scan puts the profile-path union one file higher than the
tracked-only figure (1095 against 1094), and one file sits in the archived-features bucket under
ripgrep but in the remainder bucket under tracked-only search. Three hypotheses were tested and
eliminated by the research; the cause was not established. The tracked-only figures govern, and the
executor's baseline capture, produced by the tracked-only command forms, supersedes both columns.

### Terminal-state derivation (orchestrator finding F4)

After the declared sweep plus the hand edit of the batch-budget state file (finding F1), the only
tracked file still carrying any identifier is the out-of-scope agent-governance settings file. The
orchestrator verified that this file carries the profile path and the account name and carries
neither the host name nor the 8.3 short-name spelling. The terminal values are therefore one file for
the profile path, one file for the guarded account name, zero for the host full token, zero for the
host-name stem, and zero for the short-name spelling. This derivation is what makes the acceptance
criteria known to be satisfiable.

The research record's Numeric Derivation Evidence covers one family only: the three files that carry
the host-name stem without the full token, derived twice by independent strategies with identical
member sets. The acceptance criteria below therefore state the profile-path and account-name
conditions as an exclusion — no tracked file other than the single named out-of-scope file — rather
than as a bare count, and state the host, stem and short-name conditions as absences. The figure one
in this subsection is informational.

### Filename-level leakage

Invisible to a content search. Exactly two tracked files carry the account name in their filename,
and the same two carry the host name. Both have the test-result extension and therefore fall inside
the deletion class, so filename leakage is fully resolved by the deletion and needs no separate task.
A filename assertion is nevertheless required, because a sweep validated only by content search
reports clean while leaving a leaking path on disk.

## Scope & Non-Goals

### In scope

The issue as filed carries five acceptance criteria. The maintainer narrowed this delivery item on
2026-09-11 to:

- AC1 — no tracked file contains an absolute user-profile path.
- AC2 — no tracked file contains the bare account name or the bare host name.
- AC3, editor-settings half only — the workspace editor settings file uses portable placeholders or
  environment references.

The 8.3 short-name spelling is treated as a spelling of the account name and falls under AC2.

### Out of scope, and why

The paths named in this subsection are written as plain prose without code spans, deliberately, so
that the blast-radius extractor does not record them as write targets. Do not add backticks to them.

1. **The agent-governance settings file under the agent directory.** Out of scope per the
   maintainer. It is published into this repository by a push-down from the upstream governance
   repository with zero templating, so an edit made here is reverted on the next sync. It must be
   corrected upstream. This item does not edit it and plans no task touching it. It carries one
   doubled-backslash profile path and therefore the account name; the orchestrator verified that it
   carries neither the host name nor the short-name spelling. It is the single file that makes the
   profile-path and account-name terminal values one rather than zero.

2. **The explicit results-directory and log-file-name change to the test runner (issue AC4).** A
   sibling item in the same concurrent run delivers it. See Ordering Risk below.

3. **The upstream governance-repository portion (issue AC5).** Same push-down reasoning as item 1.

4. **The staged promotion directory under the features tree.** Six identifier-bearing Markdown
   documents sit there. Every staging span, porcelain span and acceptance search in this item
   excludes that directory, so the criteria are asserted against the repository minus that directory.
   This is a construction requirement rather than a weakening: without it the criteria would be
   unsatisfiable, because an unscoped pathspec sweeps a sibling item's queued promotion file onto this
   branch. The six documents are the queued promotions of other items and are not this item's to
   rewrite.

5. **The package-coordinate occurrences in the two agent configuration files at the repository
   root.** Those are an npm package scope, not a host identifier. See the package-coordinate guard
   under Proposed Fix.

6. **Everything under the VS Code scripts directory, the agent instruction file at the repository
   root, and everything under the GitHub directory.** All were verified clean of all identifier
   classes.

7. **The concurrent-run documentation directory under the features tree.** Verified clean by the
   orchestrator. It is also the directory that cannot be named on a command line at all, per
   execution-environment precondition 3, so naming it as a write target would be both false and
   unexecutable.

8. **Git history.** The sweep is forward-only. See Rollout & Follow-up.

### Settled decisions carried in from the research

These are decisions, not options. They are not to be relitigated by the planner or the executor.

**Decision 1 — the worktree-cleanup skill document is swept.** The research had considered
excluding it on the same push-down grounds that exclude the governance settings file. The
maintainer's exclusion names exactly one file, and widening a maintainer-drawn boundary is not the
spec author's call. The upstream-reversion risk does apply to it, and an upstream correction is
recorded as a follow-up, but the sweep here is correct because the criterion is repository-wide. It
appears in the Write Set because the diff writes it.

**Decision 2 — the role-based deletion class.** The deletion set is every tracked test-result file
with the TRX extension, plus every tracked XML file under a feature-folder evidence path, plus the
stray raw test output at the repository root — not only files carrying the Cobertura suffix, and not
only the identifier-bearing members. Rationale: the maintainer's issue-671 decision moved the project
to projection-only evidence and names deletion as the intended outcome for the class. A
leaking-members-only deletion would satisfy the criteria while leaving several hundred raw machine
artifacts in the index for a later run to revisit. Four on-disk formats are present in the evidence
trees (Cobertura, the Visual Studio coverage export, JaCoCo, and Cobertura under a non-suffixed name)
and a suffix glob reaches only one of them.

The deletion set is decidable from the tracked-file listing alone, because the 641 = 332 + 305 + 4
arithmetic establishes that every tracked XML file outside the four source and build-configuration
documents sits under a feature-folder evidence path, and the research verified that every
identifier-bearing XML-family file is a coverage or test-result artifact. The executor derives the
deletion set by extension and path from the tracked-file listing and records the enumerated result,
using root-element inspection only as a spot check on the enumeration rather than as the selection
mechanism.

**Decision 3 — the publish-URL element in the add-in project file is emptied, not removed.** The
project file contains exactly one occurrence, in the publish-URL element's text content. Two adjacent
publish properties are already empty. Removal was considered and rejected on two grounds: it changes
ClickOnce publish configuration, and the same line is independently tracked by another issue.
Emptying eliminates the account name and, additionally, the employer organisation name carried by a
commercial cloud-storage folder segment on the same line (orchestrator finding F5); that second
disclosure was considered and is removed by the same edit. A placeholder token is prohibited in this
position because a raw angle bracket is illegal in XML character data exactly as it is in an
attribute value, and would produce a project file the build cannot load.

**Decision 4 — plain-text console logs under feature-folder evidence paths are edited in place.**
The projection-only decision names test-result and coverage XML specifically. Plain text accepts
angle-bracket placeholders safely.

**Decision 5 — the stray raw test output at the repository root is deleted.** The orchestrator
verified with git that it is tracked (introduced by commit 59bb26366). It is raw machine test output,
sits outside every feature folder, and carries several occurrences.

**Decision 6 — the host-name stem rewrites to the same \<host\> token as the full token.** The stem
carries no information that the projection should preserve, and a single token for one machine keeps
the residual searches and the transform aligned.

**Decision 7 — the batch-budget state file under the agent state directory is hand-edited
(orchestrator finding F1).** It carries the account name inside a flattened temporary-directory
segment and also carries the 8.3 short-name spelling. It sits outside every script-modified class,
so without a named disposition the terminal value for the account name would be two rather than one.
Two further facts were verified by the spec author and are recorded because they bear on durability:
the root ignore file already lists the agent state directory (once in the developer block with a
comment stating that this per-session file records absolute local paths and must never be committed,
and once in the managed-ignores block), so the file is tracked-but-ignored; and the batch-budget hook
regenerates the file with session-scoped local paths whenever a session's identifier resolves to the
default. The hand edit therefore satisfies this item's criteria at the branch tip but can be
re-dirtied locally by a later session. The durable remedy — removal from the index, after which the
existing ignore rule keeps it out — is recorded under Rollout & Follow-up as a maintainer decision
rather than taken here, because the Write Set disposition for this item is a hand edit.

## Root Cause Analysis

Two mechanisms, both confirmed rather than hypothesised.

**Mechanism one — default test-runner output naming.** The vstest console runner names its
test-result and coverage output with an account-and-host-and-timestamp prefix by default, so raw test
output embeds both identifiers in the filename as well as throughout the document body. Any evidence
record that cites such a file by name inherits both identifiers. This was verified by reading the
argument-list builders in the repository's two PowerShell test entry points, each of which returns
the assembly list plus a settings switch, an isolation switch and a test-case filter, and no
results-directory switch and no logger file-name switch. The durable fix — an explicit results
directory and a controlled log file name — is delivered by a sibling item in the same concurrent run
and is out of scope here.

**Mechanism two — agents copying raw machine artifacts into committed evidence trees.** The default
output lands in the runner's default results directory, which the root ignore rules already exclude,
so the default emission is not itself a tracked-file leak. The leak occurs when an agent copies a
default-named artifact out of that directory into a feature folder's evidence tree and commits it.
That copy is the provenance of every tracked test-result file, every tracked coverage document under
an evidence path, and both filename-level leaks. The root ignore rules exclude the default output
directory and the live coverage drop directory, but no rule matches the artifact once it has been
copied elsewhere.

A third, secondary contributor: the absolute profile path appears in a small number of live
configuration and test files where a repository-relative or environment-referenced value was
expressible, in one publish-configuration element whose value was captured from a local machine, and
in one per-session state file that the ignore rules were intended to keep out of the index.

### Affected components

The workspace editor settings file; the add-in project file; one mirrored PowerShell test fixture;
the batch-budget state file under the agent state directory; the repository ignore file; and the
Markdown, plain-text and JSON evidence corpus under the feature folders, the epics tree, the one-off
research directory, and the agent-memory tree.

## Proposed Fix

### Design summary

Four dispositions.

**Disposition A — delete the raw-evidence class.** Remove every tracked test-result file with the
TRX extension, every tracked XML file under a feature-folder evidence path, and the stray raw test
output at the repository root. Enumerate the resulting path list into an evidence record before
deleting. This is the disposition the maintainer's projection-only decision prescribes and the one
feature 511 validated at smaller scope. Every identifier-bearing XML document in the repository is a
coverage or test-result artifact; there is no identifier-bearing XML that must survive. That is what
makes "delete, never redact" sufficient for the XML family and removes the need for any XML-escaping
repair path. The convention record's rule about substituting escaped entity forms into XML-family
files is therefore not exercised by this item.

**Disposition B — rewrite Markdown, plain text and process-tree JSON by script.** After the
deletion, roughly a thousand files still require a textual substitution. That is two to three orders
of magnitude beyond what an executor can edit with individual file writes, so a script is required
rather than preferred. Design constraints:

1. Separate the pure transform — text and rule set in, text out — from a thin
   enumerate-read-transform-write driver, so that every ordering and idempotency case is a pure unit
   test with no filesystem access and no temporary file.
2. Refuse any path whose extension is in the XML family (the test-result extension, the XML
   extension, the Visual Studio coverage-XML extension, and the project-file extension) and throw.
   This makes the feature-488 and feature-662 corruption mechanically impossible rather than
   discouraged.
3. Default to detect mode; rewrite only under an explicit apply switch. Detect mode doubles as the
   recurrence guard and as the carrier for the acceptance counts.
4. Derive the account leaf from the user-profile environment variable, the host token from the
   computer-name environment variable, the stem by stripping the trailing digit run, and the 8.3
   short-name from the account leaf (first six characters upper-cased, a tilde, and a digit class),
   each with an explicit parameter override, so that the script contains none of the identifiers it
   removes. The short-name is a parameter, not a hard-coded literal (finding F2). Without this the
   script would be a new instance of the defect and would fail this item's own criteria.
5. Enumerate directories from inside the script. One member of the population is a document under
   the one-off research directory whose own filename contains a word the worktree-isolation shell
   filter refuses anywhere in a command string, including inside a pathspec operand (finding F6).
   That path can never be named on a command line; it must be reached by the script's own directory
   walk. No acceptance command, staging span or evidence artifact may spell it, and this document
   does not.
6. Write a file only when the transform changed its text (finding F8). About 11971 files are
   enumerated to rewrite about 1000. An unconditional rewrite would defeat the substitution-shape
   gate, because a diff that touches every enumerated file cannot be reconciled against the
   identifier-bearing population, and it would rewrite line endings across the whole corpus. The
   driver must also preserve each file's existing line-ending convention and encoding on the files it
   does rewrite.

**Disposition C — hand-edit the five files the script must not or cannot transform.** The
workspace editor settings file, the add-in project file, the mirrored PowerShell test fixture, the
batch-budget state file under the agent state directory, and the repository ignore file. Each is
described below.

**Disposition D — amend the repository ignore rules.** The content of the last hand edit closes the
copy vector identified as mechanism two.

### The ordered substitution rule set

The account name is a proper substring of the profile path, and the host-name stem is a proper
prefix of the host full token. A shorter rule applied first corrupts the longer form into a hybrid
that no later rule can repair. Longest-first ordering is mandatory. Apply in exactly this order,
every rule case-insensitive:

| Order | Matches | Replaces with | Rationale |
| --- | --- | --- | --- |
| 1 | Drive-letter profile path, doubled-backslash spelling; the account segment is either the account leaf or its 8.3 short-name | \<user-profile\>, doubling preserved | Longest form; must precede rule 2 or rule 2 consumes half of it |
| 2 | Drive-letter profile path, single-backslash spelling; same account-segment alternation | \<user-profile\> | |
| 3 | Drive-letter profile path, forward-slash spelling; same alternation | \<user-profile\> | |
| 4 | MSYS spelling: leading slash, drive letter, users segment; same alternation | \<user-profile\> | Six files; missed entirely by a naive leading-slash search |
| 5 | Bare account name not preceded by an at-sign, an alphanumeric, or an opening angle bracket | \<user\> | The guard protects the package coordinate and makes the rule idempotent |
| 6 | Bare 8.3 short-name spelling, with the same negative preceding-character guard | \<user\> | Catches the flattened temporary-directory form, which rules 1 to 4 cannot match because it has no separators (finding F2) |
| 7 | Bare host name, full token | \<host\> | Disjoint from rules 1 to 6 |
| 8 | Host-name stem | \<host\> | Must run after rule 7, or rule 7 never matches; three files carry the stem alone |

Three corrections to a naive reading of that table are load-bearing.

**Correction one — the separator classes use a one-or-more quantifier**, so that one expression
handles both the single-backslash and the doubled-backslash spelling and doubling is preserved by
capturing the separator run rather than by a separate rule. Preferring the quantifier over rule
ordering alone makes the rule set robust to a reordering mistake.

**Correction two — rule 5 must refuse a match preceded by an opening angle bracket.** Rules 1 to 4
emit a token containing neither a drive letter nor a users segment, so they cannot re-fire on their
own output. Rule 5 emits a token whose preceding character is always an opening angle bracket; if the
guard class excludes only the at-sign and alphanumerics, rule 5 re-fires on its own output and the
transform is not idempotent. This is the one case that longest-first ordering does not cover, and it
must be covered by a dedicated unit test. The same guard must appear in the account-name acceptance
search, or that search fires on its own remediation. Rule 6 carries the same guard for consistency;
its output token contains no tilde, so it cannot re-fire on its own output in any case.

**Correction three — the account-segment alternation in rules 1 to 4.** The short-name spelling
appears as the account segment of an absolute profile path. If rules 1 to 4 accepted only the account
leaf, such a path would fall through to rule 6, which would rewrite only the leaf and leave the drive
letter and users segment in place — a partial redaction that the profile-path acceptance search, which
also accepts the alternation, would still count.

### The package-coordinate guard

Two live configuration files at the repository root reference an npm package whose scope segment
equals the account name. Thirteen tracked files carry the at-sign-prefixed form; eleven of them are
Markdown documents that quote the coordinate. A blind case-insensitive substitution on the bare
account token rewrites the coordinate and breaks agent-tooling resolution for every session in this
repository.

The guard is the structural negative preceding-character class in rule 5, not a path allow-list. An
allow-list naming the two configuration files would already be wrong today, because eleven further
documents quote the coordinate, and would fail again the next time a document quotes it. The same
guard appears in the acceptance search so that the residual count and the transform agree on what
counts as a leak. Measured consequence: unguarded 1195 minus guarded 1187 is eight, not thirteen,
because five of the thirteen coordinate-carrying files also carry an unguarded occurrence elsewhere
and remain in the guarded set.

### The structurally sensitive files

**The workspace editor settings file.** Verified: it contains exactly one identifier occurrence, the
sole element of the Power Query client's additional-symbols-directories array, a forward-slash
profile path with a lower-case drive letter. Every other key in the file is already
workspace-relative. The in-repository symbols directory it should point at is verified to exist. The
replacement is the VS Code workspace-folder variable reference followed by the in-repository symbols
directory:

    "${workspaceFolder}/.vscode/excel-pq-symbols"

The issue's AC3 names "portable placeholders or environment references" and the variable form is an
environment reference. An angle-bracket placeholder is wrong here: it is a documentation device, and
this is a live configuration value that an extension consumes.

Whether that particular settings key participates in editor variable substitution could not be
verified and is recorded as unknown. The research reached the extension's sources and found that the
client reads the array with a plain configuration get and that the symbol manager applies only path
normalisation before converting each string to a file URI, so it is likely that the setting becomes
inert after the edit; execution was not attempted. The blast radius is bounded: the extension is not
among the three workspace-recommended extensions, no toolchain gate depends on it, and the symbol
source is an optional editor convenience. The documented fallback is the plain workspace-relative
form that the same file already uses for two other extensions; the research notes that the fallback
is unlikely to resolve any better than the variable form. The executor records the
unverified-resolution risk in the change description and in the QA evidence.

**The add-in project file.** One occurrence, in the publish-URL element's text content. Disposition
per Decision 3: empty the element. The disposition is verified by a full rebuild rather than an
incremental build, per the recorded MSBuild up-to-date-check behaviour.

**The mirrored PowerShell test fixture.** Verified: 494 lines, exactly four occurrences, in three
structurally different positions:

- one is the value of a filename attribute on a class element inside a here-string that the very
  next statement casts to an XML type. An angle-bracket placeholder there reproduces the
  feature-488 and feature-662 corruption exactly, surfacing as a Pester failure rather than as silent
  evidence damage. The correct substitution is a bracket-free neutral root, and sibling test cases in
  the same file already establish that convention. The required shape, shown in an indented block so
  that no code span and no bracket appear:

      filename="C:\fake\repos\TaskMaster\ToDoModel\Data Model\ToDo\ToDoItem.cs"

- three are PowerShell single-quoted string literals — a worktree root, a canonical root, and a
  repo-root argument passed to the helper under test — that are semantically pinned by the production
  helper. That helper hard-codes the literal directory leaf that is the repository name and derives
  the canonical root by joining that leaf to the parent of the supplied repo root.

Three invariants must survive any substitution, or the two affected tests turn green for the wrong
reason or red:

1. the worktree root and the canonical root continue to share a parent directory;
2. the canonical root's leaf remains exactly the repository name;
3. the worktree root's leaf remains something other than the repository name.

Orchestrator finding F3 applies: the 1.0 draft's fixture-invariant check read only the two root
variable assignments near the top of the file, but two of the four occurrences are further down — the
attribute value inside the here-string and the repo-root literal passed to the helper. Those two must
continue to share a parent directory with the two root assignments, and the attribute path must
continue to begin with the canonical root, or the affected test passes for the wrong reason. This
spec extends the invariant check to all four positions: the same neutral prefix is substituted
identically in every position, the repo-root argument remains equal to the worktree root literal, and
the attribute path begins with the canonical root whose leaf is the repository name. The Pester
regression gate remains the behavioural cover; the four-position invariant check is the structural
cover, and both are required. This file is excluded from the scripted sweep and hand-edited.

**The batch-budget state file under the agent state directory.** JSON. It carries the account name
inside a flattened temporary-directory segment and the 8.3 short-name spelling. It is hand-edited
rather than scripted because it sits outside every script-modified class and because it is a
structured file whose parse must be re-verified after the edit. The substitution replaces the
flattened profile segment with the \<user\> token inside the string value and leaves every key and
every other value unchanged. The file must remain valid JSON and is included in the JSON parse gate.
See Decision 7 for the durability caveat.

**The repository ignore file.** Additive amendment only, placed adjacent to the existing coverage
block. Verified current state: the file already carries the bracketed test-results directory rule and
the build-log rule, the Visual Studio coverage and coverage-XML rules, and the Cobertura drop-directory
rule with its keep exception. It does not ignore a raw test-result file copied into a feature-folder
evidence tree, which is the copy vector. The lines to add, with a comment recording why:

    # Raw machine test-result and coverage artifacts are never committed (issues 602, 671).
    # The evidence of record is the distilled markdown projection alongside these paths.
    # The default test-result file name also embeds the operator account and machine name,
    # so committing one leaks a host identifier in the filename as well as in the body.
    *.trx
    *.cobertura.xml
    *.jacoco.xml
    Deploy_*/

Notes on the amendment:

- The test-result line is the load-bearing one. It closes the vector that produced both filename
  leaks, because it matches the artifact wherever it is copied to.
- The two suffixed coverage lines cover two of the four formats present. They do not cover the
  Visual Studio coverage export, which is written under arbitrary names. No safe glob exists for that
  format without risking a match on legitimate XML such as the ribbon-explorer or weaver
  configuration documents, so it must be caught by the residual searches rather than by an ignore
  rule. This limitation is stated deliberately.
- The deployment-scratch line is carried over from the feature-511 per-folder precedent with its
  recorded rationale: the deployment scratch directory name itself embeds the account and host.
- The existing rules are neither duplicated nor weakened. The test-results directory rule is
  currently the only thing keeping the default emission out of the porcelain status.

### Considered and rejected

- **History rewriting.** Out of scope; a maintainer decision, not a mechanical one. The criteria
  speak about the working tree at the branch tip.
- **Removing the publish-URL element rather than emptying it.** Rejected per Decision 3.
- **Deleting the Markdown evidence instead of editing it.** The projections are the evidence of
  record that the XML deletion depends on.
- **A throwaway in-session rewriter deleted before commit.** Permitted by the temporary-script
  exemption to the file-size rule, but it consumes the same batch-budget slot, leaves no recurrence
  guard, and puts an untested thousand-file rewriter in the change path — the shape that produced
  the feature-488 and feature-662 corruptions.
- **Suffix-based deletion.** Reaches 238 of the 305 evidence XML files; rejected for the role-based
  definition.
- **Path allow-listing the package coordinate.** Already wrong today and brittle.
- **Excluding the worktree-cleanup skill document on push-down grounds.** Rejected per Decision 1.
- **Removing the batch-budget state file from the index in this item.** Deferred to follow-up per
  Decision 7; the Write Set disposition is a hand edit.

## Assumptions, Constraints, Dependencies

### Execution-environment preconditions (hard)

These were verified this run and are preconditions, not preferences.

1. **The execution child for this item must run without worktree isolation.** Under worktree
   isolation the Bash tool refuses any command carrying a shell variable expansion and refuses the
   PowerShell executable outright. The acceptance searches derive their tokens from environment
   variables precisely so that the committed command text contains no literal identifier, and the
   sweep script requires PowerShell, so neither can run under isolation.
2. **A preflight reviewer running under isolation cannot execute the acceptance commands** and must
   validate them by construction — by reading the expressions and confirming their shape — and say so.
   A preflight report that claims to have executed them under isolation is not evidence.
3. **The word naming the GNU concurrency utility is refused by the isolation filter anywhere in a
   command string, including inside a pathspec operand.** The concurrent-run documentation directory
   and one document under the one-off research directory therefore cannot be named on a command line.
   Both are reached only by the sweep script's own enumeration; the former is clean, the latter is a
   member of the population (finding F6).
4. **A leading forward slash in a search literal is rewritten into a Windows path by Git Bash before
   git sees it**, so a search for a literal beginning with a forward slash returns no matches and a
   non-zero exit against a file that demonstrably contains it. Six files carry the MSYS spelling.
   Wrap any leading slash in a bracketed character class; a bracket expression is not path-shaped and
   survives the rewrite. Keep the class in the PowerShell-emitted forms too, so the same expression is
   valid from either shell.
5. **Every staging and porcelain span must exclude the staged promotion directory**, or an unscoped
   pathspec sweeps a sibling item's queued promotion file onto this branch.
6. **The Bash allowlist admits only git, gh, pwsh, poetry run and three library scripts, and checks
   every chained segment**, so a bare shell variable assignment is itself a non-allowlisted segment.
   The cleanest carrier for the acceptance counts is the sweep script's detect mode invoked through
   the PowerShell executable, which derives the tokens internally and shells out to tracked-only git
   search.
7. **PoshQC invocations must pass an explicit scan-folder list (finding F7).** The PowerShell rule
   file cites a Pester runsettings file under the PowerShell tooling directory that does not exist in
   this repository (verified: no files exist under that directory), and the scan-configuration file
   the PoshQC test tool reads is also absent. No PoshQC format, analyze or test invocation may rely on
   a configured default; each must name the production script directory and the mirrored test
   directory explicitly.

### Policy constraints

- The PowerShell batch budget permits three production and three test PowerShell files per batch,
  counted per distinct path per session. A one-production-plus-one-test design consumes one slot in
  each bucket. The state file that records that budget is itself a member of the write set for an
  unrelated reason (Decision 7); the hand edit to it must not alter the budget counters.
- No production, test or reusable script file may exceed 500 lines. Markdown is exempt.
- Test files mirror the production tree under the tests directory. The mirrored directory for the
  dev-tools scripts does not yet exist (verified by listing) and is created by this item together
  with the test file; the production dev-tools directory exists and already holds one script.
- Temporary files in tests are prohibited, which is why the transform accepts and returns text
  rather than paths.
- PSScriptAnalyzer and Pester apply to any PowerShell file written; the C# toolchain applies to the
  project-file edit.

### Dependencies

None blocking. The sibling item that delivers the runner's explicit results directory and log file
name is expected to land first but is not a dependency; see Ordering Risk.

## Data / API / Config Impact

- **User-facing or API changes:** none. No production C# behaviour changes. The project-file edit
  empties a ClickOnce publish property that no CI workflow invokes.
- **Configuration changes:** one workspace editor settings value moves from an absolute path to an
  environment reference; the repository ignore rules gain four additive patterns; one per-session
  state file loses a flattened absolute path from one string value.
- **Data considerations:** several hundred raw machine artifacts are deleted from the index. They
  remain retrievable from history.
- **Logging and telemetry:** none.
- **Compatibility:** the detect mode of the sweep script is reusable as a recurrence guard and is
  intended to be adopted as one. The transform's parameters (account leaf, short-name, host token,
  stem) are explicit so that a future operator on a different machine can run detect mode against
  this operator's identifiers without editing the script.

## Ordering Risk

The concurrent orchestration surface in this repository has no dependency edges between items. It
derives concurrency solely from blast-radius overlap, and a dependency relation is not expressible.
This item is therefore expected but not guaranteed to run after the sibling item that delivers the
runner's explicit results directory and controlled log file name. If this item executes first, the
criteria that sibling delivers are not yet in place, and the sweep must still leave the repository
self-consistent. This document and its acceptance criteria are written to be correct in both orders.

### What the repository looks like if this item executes first

| Aspect | State after this item, sibling not yet landed |
| --- | --- |
| Tracked files containing an identifier, outside the excluded promotion directory | The declared terminal state: only the out-of-scope governance settings file, carrying the profile path and account name |
| Tracked filenames containing an identifier | None |
| Test-result files newly emitted by a local run | Still default-named, but written into the runner's default directory, which the existing test-results directory rule already ignores |
| Can a raw artifact re-enter the index by copy? | No for the test-result and suffixed-coverage formats — the added ignore rules match them wherever they are copied to. Yes for a Visual Studio export under an arbitrary name — caught only by the residual searches |
| Does an evidence author have to remember anything? | Yes — to cite a sanitized file name in Markdown, until the sibling lands |

### What makes the sweep self-consistent in either order

1. **The ignore-rule amendment closes the reintroduction vector independently of the sibling.** It
   is inside this item's scope and depends on nothing the sibling delivers. That is why it is the
   load-bearing self-consistency task rather than an incidental hygiene addition.
2. **No acceptance criterion in this item asserts anything about the test runner's argument
   list.** Such a criterion would make this item fail whenever it lands first, and would make it claim
   credit for a condition it did not deliver whenever it lands second. None of the criteria below
   mentions the runner's arguments.
3. **The existing test-results directory ignore rule must not be deleted or weakened.** It is what
   keeps the default emission out of the porcelain status in the interim.
4. **The residual authoring obligation is recorded under Rollout & Follow-up**, so that if the
   sibling is delayed the convention survives as documented practice.

## Known Contention

The scheduler serialises on declared write-set overlap, so the overlap must be declared accurately
rather than defensively.

Two siblings in the same concurrent run are relevant. One rewrites the PowerShell test entry points
under the VS Code scripts directory and also edits the workspace editor settings file, the agent
instruction file at the repository root, and several agent-memory documents. Another adds CI coverage
and Pester gates and also edits scripts under the same VS Code scripts directory.

Verified facts that narrow the apparent overlap:

- The agent instruction file at the repository root is clean of every identifier class. This item
  does not write it, so the first sibling's edit there does not overlap.
- Nothing under the VS Code scripts directory carries any identifier. This item writes nothing there,
  so neither sibling overlaps this item on that directory.
- Everything under the GitHub directory is clean.

The real overlap is three surfaces: the mirrored PowerShell test fixture under the tests tree, which
this item hand-edits and which either sibling may touch; the agent-memory documents; and the
workspace editor settings file, which the first sibling also edits.

**The agent-memory delta, recorded explicitly.** Nine agent-memory documents carry an identifier.
The first sibling declares six. The two sets do not agree, and because the criterion is
repository-wide a partial correction leaves it unmet. Whichever item lands second must re-measure
rather than assume. One of the nine quotes the identifiers as search patterns in five places while
documenting a search defect; the substitution is still correct, because the placeholder communicates
the same shape, but its surrounding prose may need a one-line adjustment so that the example still
reads as an example.

## Test Strategy

### Safe search forms

Five separate searches — one per class plus the filename assertion — not one combined expression.
Every search is case-insensitive: the case-sensitive count of the host class is twenty files below the
case-insensitive count, because the test runner lower-cases one attribute while leaving another
mixed-case. Every search excludes the staged promotion directory. Every search uses tracked-only
search rather than ripgrep. Every search derives its tokens from environment values inside the sweep
script's detect mode so that the committed command text carries no identifier literal. Any leading
forward slash sits inside a bracketed character class. The account-name search carries the same
negative preceding-character guard as rule 5. The profile-path search accepts the same
account-segment alternation as rules 1 to 4. The short-name search is a fixed-string search for the
tilde-bearing token with a digit class.

### Why a zero residual count is necessary but not sufficient

A post-sweep residual count of zero is satisfied equally by three states of the world: a correct
sweep; a sweep that destroyed or emptied the files it rewrote; and a search expression that never
matched anything under any conditions. The feature-662 recurrence is the concrete case: its approved
plan mandated bracketed placeholders, a correct executor shipped six unparseable test-result files,
and the plan's gate asserted only a zero residual count, which a document-destroying rewrite
satisfies perfectly.

The mitigation is mandatory and has two halves. First, record a non-zero pre-sweep baseline using the
identical command text, which converts the post-sweep figure from an unfalsifiable claim into a
measurement of a transition. Second, pair the counts with the format-validity and behaviour gates
below, which distinguish a correct sweep from a destructive one. The pre-sweep populations are
non-vacuous by construction: each returns a three- or four-digit file count against the current tree,
and the short-name search returns seven.

### Required gates

1. **Pure unit tests for the transform**, filesystem-free and temporary-file-free, covering at
   least: longest-first ordering, so that a profile path is not corrupted when the account rule is
   also armed; each live spelling of the profile path rewriting to the same token, including the
   spelling whose account segment is the short-name; doubled-separator input preserving its doubling
   in the output; idempotency, so that applying the transform twice equals applying it once — the case
   that catches the rule-5 self-match; the negative-context guard leaving an at-sign-prefixed package
   coordinate untouched; an XML-family extension throwing rather than rewriting; the 8.3 short-name
   rule rewriting both the bare flattened form and the profile-path account segment; and the stem
   rule firing on a bare stem and not on an already-rewritten full token.
2. **Regression gate on the hand-edited PowerShell fixture.** Re-run its Pester suite after the
   edit, invoking PoshQC with an explicit scan-folder list per precondition 7. It is the only existing
   test in the repository whose assertions depend on the literal content being changed, and therefore
   the only place where a substitution can silently alter behaviour. The four-position invariant
   check of finding F3 accompanies it.
3. **Build gate on the hand-edited project file.** Run the analyzer and nullable MSBuild commands
   exactly as the agent instruction file prescribes, both with the rebuild target, after the
   publish-URL disposition, to prove the project still loads. The non-vacuity condition recorded in
   agent memory applies: the build log must show no skipped compile target. The raw output of any
   subsequent test step stays in the runner's default directory and is not copied into evidence; the
   evidence records summary lines only and cites no default-named file. That is an evidence-authoring
   rule for this item, not an assertion about the runner's argument list.
4. **Residual-count assertions per identifier class**: profile path, guarded account name, host full
   token, host-name stem, and 8.3 short-name, each paired with its recorded non-zero pre-sweep
   baseline captured by the identical command text; plus the filename assertion with its baseline of
   two.
5. **A repository-scale idempotency assertion**: run the sweep in apply mode a second time over the
   swept tree and assert an empty porcelain status, excluding the staged promotion directory, against
   the post-sweep commit. This is the cheapest end-to-end check of the ordering argument and is what
   would have caught the rule-5 self-match.
6. **A JSON parse gate** over every rewritten or hand-edited JSON file: the workspace editor
   settings file, the batch-budget state file, and the three process-tree captures.
7. **A substitution-shape gate**: the number of files modified by the apply run equals the number of
   files reported by the preceding detect run, and no file outside the identifier-bearing population
   appears in the diff. This is what finding F8's write-only-on-change requirement makes checkable.
8. **A tracked-versus-ripgrep reconciliation**: confirm the tracked-only counts agree with the
   research artifact's ripgrep cross-check modulo the known one-file deltas, which discharges the
   research artifact's standing caveat.
9. **A well-formedness gate for the XML family is deliberately absent**, and the reason is recorded
   so that its absence reads as a decision rather than an oversight: the sweep refuses XML-family
   extensions and throws, and every identifier-bearing XML document is deleted rather than rewritten,
   so there is no rewritten XML to parse. Format validity for the files that are rewritten is asserted
   instead through the project build gate, the Pester regression gate, and the JSON parse gate.

### The host-stem assertion

A search for the full host token matches 183 tracked files; a search for the stem matches 186. The
three-file difference set is the one population for which the research record carries complete
Numeric Derivation Evidence: the primary derivation was a set difference between the two listings,
the cross-check was a per-file probe of each candidate, and the member sets are identical. Those three
files are named individually in the Write Set so that the diff is expected to touch them. All three
carry the stem as a search pattern rather than as a path, so the substitution is correct but the
surrounding prose should still read as an example afterwards.

### Fail-before evidence

The bugfix workflow requires a failing regression test first. For this item the fail-before evidence
takes two forms: the recorded non-zero pre-sweep baselines, which are the repro; and the pure
transform unit tests, which fail before the script exists and pass after. Both are recorded under
this feature folder's regression-testing evidence directory.

### Waived verification convention

The convention record instructs that a verification sweep be scoped to the files the branch changed,
because a repository-wide sweep otherwise returns thousands of pre-existing hits in unrelated folders
and drowns the signal. That instruction is explicitly waived here, because this item's branch changes
essentially every file in the population, so the repository-wide sweep is the changed-file sweep. The
waiver is stated so that a reviewer does not read it as an undeclared deviation.

### Sanitisation-record constraint

Every evidence artifact this item produces is bound by the same rule that binds this document: a
sanitisation record must not quote the raw before-values. Describe each substituted token by class and
keep only the after-values. Evidence artifacts are written only under this feature folder's evidence
tree in the canonical sub-directories — baseline, regression-testing, qa-gates, other — per the
evidence-and-timestamp-conventions skill. No evidence is written to any other location.

### Toolchain

For the two PowerShell files: PoshQC format, PoshQC analyze, then PoshQC test, each with an explicit
scan-folder list naming the production dev-tools directory and its mirrored test directory. For the
project-file edit: the C# toolchain in the order the agent instruction file prescribes, with the two
rebuild gates as the load-bearing steps for this item. If any step changes files or fails, restart
from the first step.

## Acceptance Criteria

Check-off protocol: this file is the sole authoritative acceptance-criteria source for this item.
Change only the box marker when checking an item off; do not edit the criterion text. Leave any
criterion that cannot be verified unchecked and record the gap.

- [ ] AC1: After the sweep, a tracked-only, case-insensitive search over the union of the four
  canonical profile-path spellings, whose account segment accepts both the account leaf and its 8.3
  short-name, excluding the staged promotion directory, lists no tracked file other than the
  out-of-scope agent-governance settings file named in Scope & Non-Goals; a non-zero pre-sweep
  baseline captured with the identical command text is recorded under this feature folder's baseline
  evidence directory, and the post-sweep listing is recorded alongside it so that the result is a
  measured transition.
- [ ] AC2: After the sweep, a tracked-only, case-insensitive search for the bare account name with
  the negative preceding-character guard that refuses an at-sign, an alphanumeric and an opening angle
  bracket, excluding the staged promotion directory, lists no tracked file other than the same
  out-of-scope file, with its own recorded non-zero pre-sweep baseline; and the package-coordinate
  occurrences are unchanged — the at-sign-prefixed search lists the same files before and after, and
  the coordinate lines in the two agent configuration files at the repository root do not appear in
  the diff.
- [ ] AC3: After the sweep, the tracked-only, case-insensitive search for the bare host full token
  lists no file, and separately the search for the host-name stem with its trailing digit run removed
  lists no file, each against its own recorded non-zero pre-sweep baseline.
- [ ] AC4: After the sweep, the tracked-only, case-insensitive fixed-string search for the 8.3
  short-name spelling of the account lists no file, against its recorded non-zero pre-sweep baseline,
  and the sweep script accepts the short-name as an explicit parameter with an environment-derived
  default rather than as a hard-coded literal.
- [ ] AC5: After the sweep, the tracked-file listing filtered case-insensitively for the account
  name, the host name and the short-name in the filename returns nothing, against the recorded
  pre-sweep baseline of two test-result files.
- [ ] AC6: The workspace editor settings file's additional-symbols-directories value is the
  workspace-folder variable reference followed by the in-repository symbols directory, the file
  contains no absolute path of any class, the file parses as JSON, and the unverified-resolution risk
  for that settings key is recorded in the change description and in the QA-gate evidence together
  with the documented fallback.
- [ ] AC7: The raw-evidence class is deleted under the role-based definition — every tracked
  test-result file with the TRX extension, every tracked XML file under a feature-folder evidence
  path, and the stray raw test output at the repository root — and the deleted path list is
  enumerated into an evidence artifact under this feature folder's other-evidence directory before
  deletion, with per-class and per-directory counts that reconcile to the pre-deletion tracked-file
  listing.
- [ ] AC8: No angle-bracket placeholder token is introduced into any XML attribute value or any XML
  element text anywhere in the diff: every XML-family path in the diff is a deletion except the
  add-in project file, whose only hunk empties the publish-URL element; the hand-edited fixture's
  attribute value is bracket-free; and the sweep script's refusal of XML-family extensions is covered
  by a passing unit test.
- [ ] AC9: Every rewritten or hand-edited file remains valid in its own format: the add-in project
  file loads under both full MSBuild rebuild gates from the repository toolchain with no skipped
  compile target; the Pester suite covering the hand-edited fixture passes with the four-position
  invariant intact; and the workspace editor settings file, the batch-budget state file, and the
  three process-tree captures parse as JSON.
- [ ] AC10: The transform is idempotent at repository scale: a second apply-mode run over the swept
  tree yields an empty porcelain status, excluding the staged promotion directory, against the
  post-sweep commit.
- [ ] AC11: The repository ignore file is amended additively with the test-result, Cobertura-suffix,
  JaCoCo-suffix and deployment-scratch patterns, and its diff contains additions only — no existing
  rule is deleted, weakened or duplicated, and the test-results directory rule is intact.
- [ ] AC12: The delivered diff is confined to the paths declared in the Write Set. It does not touch
  the agent-governance settings file, anything under the staged promotion directory, the agent
  instruction file at the repository root, anything under the VS Code scripts directory, or anything
  under the GitHub directory; and the number of files the apply run modified equals the number the
  preceding detect run reported.
- [ ] AC13: The repository is self-consistent after the sweep in either landing order relative to
  the sibling that changes the test runner's arguments, and no criterion in this list asserts
  anything about the test runner's argument list.
- [ ] AC14: Pure unit tests for the transform exist under the mirrored test path declared in the
  Write Set and pass under PoshQC invoked with an explicit scan-folder list, covering longest-first
  ordering, each live profile-path spelling including the short-name account segment,
  doubled-separator preservation, idempotency, the package-coordinate guard, the XML-family refusal,
  the 8.3 short-name rule, and the stem rule, with no filesystem access and no temporary files.
- [ ] AC15: Neither the sweep script nor its test file contains any identifier literal: the same
  five searches used for AC1 through AC4, restricted to those two paths, list nothing, and both files
  derive the tokens from environment values or explicit parameters.

## Write Set

Every path or path class this item's diff creates, modifies or deletes. This is the only section of
this document in which a repository path appears inside a code span.

Deleted:

- `docs/features/**/*.trx`
- `docs/features/**/evidence/**/*.xml`
- `test-output.txt`

Modified by script:

- `docs/features/active/**/*.md`
- `docs/features/archive/**/*.md`
- `docs/features/epics/**/*.md`
- `docs/research/**/*.md`
- `.claude/agent-memory/**/*.md`
- `.claude/skills/cleanup-merged-worktrees/SKILL.md`
- `docs/features/**/evidence/**/*.txt`
- `docs/features/**/evidence/**/*.process-tree.json`

Named individually because the host-stem assertion depends on them:

- `.claude/agent-memory/atomic-executor/project_bash_heredoc_collapses_doubled_backslashes.md`
- `docs/features/active/2026-08-26-efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614/evidence/qa-gates/redaction-sweep.2026-08-26T22-44.md`
- `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/policy-audit.2026-09-04T02-11.md`

Modified by hand:

- `.gitignore`
- `.vscode/settings.json`
- `TaskMaster/TaskMaster.csproj`
- `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1`
- `.claude/state/powershell-batch-budget.default.json`

Created:

- `scripts/dev-tools/Repair-HostIdentifierLeak.ps1`
- `tests/scripts/dev-tools/Repair-HostIdentifierLeak.Tests.ps1`

This item's own feature folder, covering criterion check-off and every evidence artifact:

- `docs/features/active/2026-09-12-host-identifier-leakage-sweep-602/`

## Risks & Mitigations

| Risk | Likelihood | Impact | Mitigation |
| --- | --- | --- | --- |
| A blind substitution rewrites the package coordinate and breaks agent-tooling resolution for every session | Moderate without the guard | High | The structural negative preceding-character guard in rule 5, a dedicated unit test, and the same guard in the acceptance search |
| An angle-bracket token is written into XML and corrupts the document, repeating the feature-488 and feature-662 outcome | Low with the mitigations, high without | High | The script throws on XML-family extensions; every identifier-bearing XML document is deleted; the two structural positions use bracket-free values; AC8 asserts it |
| The transform is not idempotent because rule 5 re-fires on its own output | Certain without the bracket guard | Moderate | The widened guard class, a dedicated unit test, and the repository-scale second-run assertion |
| The short-name spelling survives because no rule matches it | Certain without rule 6 and the alternation in rules 1 to 4 | Moderate | Rule 6, the account-segment alternation, the dedicated unit test, and the AC4 residual assertion |
| A substitution in the PowerShell fixture silently turns two tests green for the wrong reason | Moderate | Moderate | The four-position invariant check, hand-editing rather than scripting that file, and the Pester regression gate |
| The project file no longer loads after the publish-element edit | Low | High | Emptying rather than removing, plus both full MSBuild rebuild gates |
| A post-sweep zero is achieved by destroying files rather than rewriting them | Low | High | Recorded non-zero pre-sweep baselines plus the format-validity, behaviour and substitution-shape gates |
| The host-name sweep reports clean while the machine-name stem survives | Certain without the stem rule and assertion | Moderate | Rule 8, the stem assertion, and the three named files |
| An unconditional rewrite touches all 11971 enumerated files and rewrites line endings | Certain without the write-only-on-change requirement | Moderate | Finding F8's requirement, the line-ending preservation rule, and the substitution-shape gate |
| An unscoped pathspec sweeps a sibling's queued promotion file onto this branch | Moderate | Moderate | The staged promotion directory is excluded from every staging span, porcelain span and acceptance search |
| A sibling lands a partial agent-memory correction and the repository-wide criterion is left unmet | Moderate | Moderate | The nine-versus-six delta is declared; whichever item lands second re-measures |
| The batch-budget hook regenerates the hand-edited state file with local paths after the sweep | Likely over time | Low at the tip, moderate if later staged | Decision 7 records the mechanism; index removal is recorded as a follow-up for maintainer decision |
| The editor settings key does not resolve the workspace-folder variable and the symbol source becomes inert | Likely per the extension's sources; unverified | Low | Recorded as unknown; bounded blast radius; fallback documented |
| The deleted raw artifacts are needed later | Low | Low | Deletion is forward-only; history retains every deleted artifact |
| The execution child is launched under worktree isolation and cannot run the acceptance commands | Moderate | High | Stated as a hard precondition; a preflight reviewer under isolation validates by construction and says so |
| A PoshQC invocation relies on a configured default that does not exist | Certain without an explicit scan-folder list | Moderate | Precondition 7 |

## Rollout & Follow-up

### Rollout

1. Capture the five pre-sweep baselines and the filename baseline as evidence artifacts under the
   baseline evidence directory, with the exact command text and exit code per the evidence schema.
2. Enumerate and record the deletion path list under the other-evidence directory, then delete.
3. Run the sweep in detect mode, record the report, then run it in apply mode and record the
   modified-file count.
4. Hand-edit the five structurally sensitive files.
5. Re-run the five counts and the filename assertion, the Pester regression gate with the
   four-position invariant check, both MSBuild rebuild gates, the JSON parse gate, the
   substitution-shape gate, and the repository-scale idempotency assertion; record each under the
   QA-gates or regression-testing evidence directory as appropriate.
6. Check off the criteria above individually as each is verified.

### Follow-up obligations

- **Upstream governance-repository correction.** The agent-governance settings file is the single
  surviving carrier of the profile path and the account name after this sweep, and it must be
  corrected in the upstream governance repository, not here. The worktree-cleanup skill document is
  swept here per Decision 1 but is subject to the same push-down reversion, so an upstream correction
  for it should follow as well; otherwise the next sync reintroduces the leak without any local
  change.
- **Batch-budget state file index removal.** The root ignore file already excludes the agent state
  directory and its own comment says the per-session file must never be committed. Removing the file
  from the index, so that the existing rule takes effect, is the durable remedy and is recorded here
  as a maintainer decision.
- **Residual authoring obligation.** Until the sibling item lands its explicit results directory and
  controlled log file name, evidence authors must cite a sanitized file name rather than the default
  one. This is recorded so that the practice survives a delay to that sibling.
- **Recurrence guard.** The detect mode of the sweep script is intended for adoption as a standing
  recurrence check.

### The sweep is forward-only — an explicit reading instruction

Git history retains every deleted artifact and every pre-sweep revision of every rewritten file. This
item does not rewrite history, and history rewriting is recorded as a maintainer decision rather than
a mechanical one. The criteria above are assertions about the working tree at the branch tip, not
about the repository's history. A reviewer must not read them as history claims, and must not treat
the continued presence of the identifiers in history as a failure of this item.

### Links

- GitHub issue: https://github.com/drmoisan/TaskMaster/issues/602
- Research artifact: this feature folder's research directory, timestamped 2026-09-12T16-25
- Convention record: the shared no-absolute-host-paths memory under the agent-memory tree
- Prior art: the winformspumphost-suite-determinism feature folder (issue 511), which validated the
  deletion-over-redaction disposition at smaller scope
