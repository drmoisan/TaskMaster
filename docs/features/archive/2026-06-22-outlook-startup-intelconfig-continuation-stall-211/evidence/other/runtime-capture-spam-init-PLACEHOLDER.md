# Runtime Capture (PLACEHOLDER) — SpamBayes-init `[spam-init]` Lines (issue #211)

Timestamp: 2026-06-24T15-10

Status: MAINTAINER-GATED / RUNTIME — NOT CI-automatable.

This artifact is a placeholder. It must be completed by the maintainer from a live, non-debugger
slow cold start of Outlook, per
`coldstart-spam-init-capture-instructions-2026-06-24T15-10.md` in this same `evidence/other/`
folder. No automated agent or CI job can populate it, because it requires a live Outlook process
and a reproduction of the ~113 s Spam-init freeze.

## How to complete

1. Follow `coldstart-spam-init-capture-instructions-2026-06-24T15-10.md`.
2. Paste the six raw `[spam-init]` lines from the slow cold start into the section below.
3. Set the capture `Timestamp:` to the ISO-8601 time of the cold start.
4. Name the dominant sub-step and folder.

---

## Capture (to be filled in by maintainer)

Capture Timestamp: <PLACEHOLDER — ISO-8601 of the slow cold start>

Build/branch under test: <PLACEHOLDER — e.g., bug/outlook-startup-latency-211 @ commit>

Raw `[spam-init]` lines (paste verbatim from log4net Debug output):

```
[spam-init] step=ValidatePathsSet.JunkCertain ms=<PLACEHOLDER>
[spam-init] step=ValidatePathsSet.JunkPotential ms=<PLACEHOLDER>
[spam-init] step=ValidatePathsSet.Inbox ms=<PLACEHOLDER>
[spam-init] step=ValidatePathsSet ms=<PLACEHOLDER>
[spam-init] step=ValidateSpamClassifier ms=<PLACEHOLDER>
[spam-init] step=InitAsync(modelLoad) ms=<PLACEHOLDER>
```

Dominant sub-step (accounts for the ~113 s freeze): <PLACEHOLDER>

Dominant COM folder (if ValidatePathsSet dominates): <PLACEHOLDER>

Notes / interpretation: <PLACEHOLDER>
