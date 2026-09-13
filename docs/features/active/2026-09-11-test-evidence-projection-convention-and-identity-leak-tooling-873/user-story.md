# 2026-09-11-test-evidence-projection-convention-and-identity-leak-tooling (User Story)

- **Issue:** #873
- **Work Mode:** full-bug
- **Last Updated:** 2026-09-12T11-30
- **Status:** Narrative only

> **This document contains no acceptance criteria and no checkboxes.** Work mode for issue #873 is
> full-bug, so spec.md in this folder is the single authoritative acceptance-criteria source. This
> file exists because the terminating hook for the feature-document step requires it, and because a
> short narrative is useful to a reviewer who has not read the research artifact. Nothing here
> creates an obligation; if a statement here appears to conflict with spec.md, spec.md governs.
>
> This document also deliberately contains no file paths. The change footprint for this item is
> defined solely by the Write Set section of spec.md.

## Narrative

**As** a developer or agent running the repository's test and coverage gates from the editor,
**I want** those gates to produce small, reviewable evidence artifacts that carry no personal or
machine identity,
**so that** committed evidence is useful to a reviewer, does not disclose an operator's account name,
host name, employer organization name or absolute checkout path, and does not add tens of megabytes
to the repository for every feature.

## Why this is being fixed now

Two independent problems meet in the same set of scripts.

The first is volume and disclosure. The Visual Studio test console names its output file after the
account, the host and a timestamp, and records the account, the host and the absolute checkout path
in six separate attributes of that document. The coverage collector writes a document measured in
tens of megabytes that carries absolute host paths on every class entry. Both are committed today on
most features. The maintainer settled the question of what should be committed instead on 2026-09-11:
a small package-level coverage projection plus the one-line first-party summary the scripts already
print, and a short pass or fail summary for the test run.

The second is that three configuration and documentation surfaces carry the same identifiers
independently of any test run — a publish destination in a project file, a symbols directory in the
editor settings, and five agent-memory notes. Those do not disappear when the historical evidence is
swept, because a fresh run or a fresh read reintroduces them.

## What a reviewer will notice after the change

- A coverage run prints the first-party coverage line as it does today, and additionally writes a
  small projection file beside the coverage output. The projection is derived from the same
  post-processed document the threshold assertion reads, so its totals reconcile to that document
  exactly, and the delivery treats that reconciliation as a required step rather than a courtesy.
- A test run prints a verdict, the counts, and the names of any failed tests. The summary states how
  the skipped figure is derived, because the test platform reports no skipped attribute; the raw
  not-executed and inconclusive figures are reported alongside it so no number is lost.
- Neither run leaves a file named after an account and a host. Both runs write into a results
  directory beneath a directory the repository already ignores, which means a raw document cannot be
  committed by accident even if a cleanup step fails.
- The raw coverage document is still present after an ordinary local run, because the editor coverage
  extension reads it from the ignored coverage directory. It is discarded only when the run was
  directed somewhere else, and only after every gate that needs the detail has run. This asymmetry is
  intentional; removing it would break the extension.
- The convention itself is written down once, in a repository-owned instructions document that every
  agent session already loads, rather than in a governance file that is overwritten from upstream on
  the next synchronization.

## What this story does not cover

The historical cleanup of evidence already committed across every feature folder is a separate item
and is sequenced after this one, so that a fresh run does not reintroduce what the sweep just
removed. The broader set of agent-memory notes that match a wider identifier pattern belongs to that
same sweep. Branch-coverage threshold work is owned by a concurrent item and is untouched here. No
redaction sweep is built as executable code by this item; the obligation it was meant to satisfy is
delivered as rule text in the shared host-path guidance that agents already read.

## Stakeholders

- **Maintainer** — recorded the convention and ratifies the resulting artifact shape.
- **Feature-review agents** — consume committed evidence; the per-file coverage detail they rely on
  remains available while the raw document is on disk, which is why the discard is sequenced last.
- **Developers running the editor tasks** — see one extra summary line and one extra small file, and
  no change to how the editor coverage extension behaves.
