# host-identifier-leakage-sweep (User Story)

- **Issue:** #602
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12
- **Status:** Narrative context only
- **Work Mode:** full-bug

> **Why this file exists, and what it is not.** Work mode for this item is full-bug, under which
> spec.md in this folder is the sole authoritative acceptance-criteria source. This document exists
> only to satisfy a mechanical artifact check that requires a user-story path. It contains zero
> checkboxes and must not be used as an acceptance-criteria source, tracked against, or checked off.
> Its content is narrative context for the operator-facing half of the defect.

> **Formatting invariant.** This document contains no Markdown code spans, because a downstream
> scheduler derives this item's change footprint from backticked tokens in the feature documents and
> has no notion of polarity. Do not add backticks here. The authoritative write set is the Write Set
> section of spec.md.

> **Redaction invariant.** The account name, the host name, the 8.3 short-name form of the account,
> and any absolute user-profile path are referred to by class only. Reproducing any of them here
> would re-create the defect in a tracked file. The canonical convention record lives under the
> agent-memory tree and is cited rather than restated.

## Audience and value

| Audience | Value delivered |
| --- | --- |
| Maintainer | A repository that can be shared or opened without disclosing an operator's account name, machine name, employer, or directory layout |
| Contributor cloning the repository | Evidence records and configuration that describe a procedure rather than one machine, and that resolve on any workstation |
| Agent session reading committed evidence | Consistent, portable placeholders, and a recurrence guard in detect mode that prevents the population from regrowing |
| Reviewer | Measured, falsifiable before-and-after counts rather than an unfalsifiable claim of cleanliness |

## Story 1 — Absolute profile paths reach their terminal state

Given tracked files across the repository embed an absolute Windows user-profile path in four
separator spellings, and in some of them the account segment is the 8.3 short-name rather than the
account leaf, when the raw-evidence class is deleted and the remaining Markdown, plain text and
process-tree JSON are rewritten by the ordered, longest-first transform, then a tracked-only,
case-insensitive search over the union of all spellings, excluding the staged promotion directory,
lists no file other than the out-of-scope agent-governance settings file that is published from the
upstream governance repository, and a non-zero pre-sweep baseline captured with the identical
command text is on record beside the post-sweep listing.

## Story 2 — The account name reaches its terminal state without breaking package resolution

Given the bare account name appears both as a host identifier and, in thirteen files, as the scope
segment of an npm package coordinate, when the transform applies the negative preceding-character
guard that refuses a match preceded by an at-sign, an alphanumeric, or an opening angle bracket,
then the guarded tracked-only search lists no file other than the same out-of-scope file, the
package coordinate is byte-identical to its prior value, and agent-tooling resolution continues to
work for every session.

## Story 3 — The host name reaches zero, including the stem form

Given the full host token appears in one population of tracked files and the host-name stem, with
its trailing digit run removed, appears in a slightly larger one, when the sweep runs the full-token
rule before the stem rule and both assertions are evaluated, then both counts are zero, each against
its own recorded non-zero pre-sweep baseline, so that no tracked file leaves the machine name
recoverable in either form.

## Story 4 — The 8.3 short-name spelling reaches zero

Given a fifth identifier spelling — the first six characters of the account name, upper-cased,
followed by a tilde and a digit — appears in seven tracked files, both as the account segment of an
absolute profile path and inside a flattened temporary-directory segment that no separator-based
rule can match, when the rule set gains a dedicated short-name rule, the profile-path rules accept
the short-name as an account segment, and the script takes the short-name as an explicit parameter
with an environment-derived default, then the fixed-string search for that spelling lists no file
against its recorded baseline of seven, and the script itself contains no hard-coded short-name.

## Story 5 — No tracked filename leaks an identifier

Given two tracked test-result files carry the account name and the host name in the filename
itself, which a content search cannot see, when the raw-evidence deletion removes the whole
test-result class, then listing tracked filenames for the account name, the host name, or the
short-name returns nothing, and no separate filename remediation task is required.

## Story 6 — The workspace editor settings resolve portably

Given one key in the workspace editor settings points at an absolute profile path while every other
key in the same file is already workspace-relative, when that value is replaced with the
workspace-folder variable reference followed by the in-repository symbols directory, then the file
contains no absolute path of any class and remains valid JSON, and the unverified question of whether
that particular extension performs variable substitution — the research found no substitution in the
extension's sources, so the setting is likely to become inert — is recorded in the change description
together with the documented fallback and its bounded blast radius.

## Story 7 — Raw machine artifacts leave the index

Given the project has moved to projection-only evidence under the maintainer's issue-671 decision,
and several hundred raw test-result and coverage documents remain tracked, when the deletion is
applied under the role-based definition — every test-result file with the TRX extension, every XML
file under a feature-folder evidence path, and the stray raw test output at the repository root — then
the distilled Markdown projections remain as the evidence of record, the deleted path list is
enumerated into an evidence artifact before deletion with counts that reconcile to the tracked-file
listing, and no later run has to revisit a residue of non-leaking raw artifacts.

## Story 8 — No redaction corrupts a document

Given two prior features shipped unparseable test-result XML because an angle-bracket placeholder
was substituted into an attribute value, when the sweep script refuses XML-family extensions
outright, every identifier-bearing XML document is deleted rather than rewritten, and the two
structurally sensitive positions — the fixture's attribute value and the project file's publish-URL
element — receive a bracket-free value and an empty element respectively, then no angle-bracket
token appears in any XML attribute value or element text anywhere in the diff, and the failure mode
that survived a full feature review on a prior item is made mechanically impossible rather than
discouraged.

## Story 9 — Every rewritten file still works

Given a thousand-file textual rewrite can satisfy a zero residual count by destroying the files it
touched, when the project file is emptied of its publish path, the mirrored PowerShell fixture is
hand-edited under its three semantic invariants at all four occurrence positions, the batch-budget
state file is hand-edited, and the process-tree captures are rewritten, then both full MSBuild
rebuild gates load the project with no skipped compile target, the Pester suite covering the fixture
passes, and every rewritten or hand-edited JSON file parses.

## Story 10 — The sweep is repeatable and writes only what changed

Given the account-name rule emits a token whose preceding character is an opening angle bracket,
which would make the rule re-fire on its own output, and given roughly twelve thousand files are
enumerated to rewrite about one thousand, when the guard class is widened to refuse that character,
the driver writes a file only when the transform changed its text, and a second apply-mode run is
executed over the swept tree, then the second run produces an empty porcelain status against the
post-sweep commit, and the number of files the first apply run modified equals the number the detect
run reported.

## Story 11 — The leak cannot re-enter the index

Given the leak's provenance is an agent copying a default-named machine artifact out of an
already-ignored directory into a committed evidence tree, when the repository ignore rules gain
additive patterns for raw test-result and suffixed coverage artifacts and the deployment scratch
directory, then the copy vector is closed for those formats wherever the artifact is copied to, no
existing rule is deleted, weakened or duplicated, and the limitation that no safe glob exists for the
Visual Studio coverage export under an arbitrary name is stated rather than papered over.

## Story 12 — The blast radius is exactly what was declared

Given twelve other items may run concurrently and the scheduler derives concurrency solely from
declared write-set overlap, when the diff is reviewed against the Write Set section of spec.md, then
every changed path is inside the declared set, and the agent-governance settings file, the staged
promotion directory, the agent instruction file at the repository root, the VS Code scripts
directory, and the GitHub directory are untouched.

## Story 13 — The repository is correct in either landing order

Given the sibling item that gives the test runner an explicit results directory and controlled log
file name has no dependency edge to this one, so it may land before or after, when this item lands
first, then the identifier populations still reach their terminal state, new default-named output
still lands in an already-ignored directory, the added ignore rules still block reintroduction of the
copied artifact, and no criterion in this item asserts anything about the runner's argument list — so
the item neither fails early nor claims credit for work it did not deliver.

## Story 14 — The transform is proven before it is trusted

Given an untested thousand-file rewriter in the change path is the shape that produced two prior
corruptions, when the pure transform is separated from the input and output loop and covered by unit
tests for longest-first ordering, each live spelling including the short-name account segment,
doubled-separator preservation, idempotency, the package-coordinate guard, the XML-family refusal,
the short-name rule, and the stem rule, then every one of those behaviours is verified without
touching the filesystem and without any temporary file, and PoshQC is invoked with an explicit
scan-folder list because the configured defaults the PowerShell rule cites do not exist in this
repository.

## Story 15 — The fix does not contain the defect

Given a sweep script that hard-coded the identifiers it removes would itself be a new instance of
the defect and would fail this item's own criteria by self-reference, when the script derives the
account leaf, the short-name, the host token and the stem from environment values with explicit
parameter overrides, and every committed acceptance command does the same, then neither the script,
nor its test file, nor any evidence artifact this item writes contains an identifier literal, and
each sanitisation record describes substituted tokens by class while keeping only the after-values.

## Out of narrative scope

The agent-governance settings file is not edited here. It is published into this repository from
the upstream governance repository with zero templating, so a local edit is reverted on the next
sync, and it must be corrected upstream. It is the single file that makes the profile-path and
account-name terminal state one file rather than none. The explicit results-directory and
log-file-name change to the test runner belongs to a sibling item. The six identifier-bearing
documents under the staged promotion directory are other items' queued promotions and are excluded
from every search and staging span by construction. Git history is not rewritten; the criteria in
spec.md are assertions about the working tree at the branch tip.

The batch-budget state file under the agent state directory is hand-edited here, but the durable
remedy — removing it from the index so that the existing ignore rule for the agent state directory
takes effect — is recorded in spec.md as a maintainer decision for follow-up, because the hook that
owns the file regenerates it with local paths.
