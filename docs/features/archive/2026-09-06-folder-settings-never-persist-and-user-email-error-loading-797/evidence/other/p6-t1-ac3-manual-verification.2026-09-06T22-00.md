# AC3 — manual verification handoff

Timestamp: 2026-09-07T10-13
Task: [P6-T1]
Issue: #797

AC3-RESULT: BLOCKED-MANUAL

## Status

**BLOCKED, PENDING MANUAL.** AC3 is not checked off by this plan and its checkbox in spec.md and in
issue.md remains `- [ ]`.

AC3 reads: "A value saved in Folder Settings is present after an Outlook restart (manual
verification)." It requires a live Outlook process with this build of the VSTO add-in loaded, a real
user profile directory, and a full process teardown and restart. This execution environment has no
live Outlook host, and the executing agent is directed not to start one, load the add-in, or drive any
user interface. The criterion is therefore verifiable only by a person driving the add-in, and this
document is the handoff to the maintainer.

No AC3 result is fabricated. Every step of the procedure below is recorded as NOT PERFORMED, with the
reason, rather than given an invented observation.

## Why no note is written beside the AC3 checkbox

spec.md states that the eight criteria are reproduced verbatim from the criteria settled with the
maintainer on 2026-09-06 and are not renumbered, reordered, dropped, merged, split or reworded. The
acceptance-criteria-tracking skill independently permits exactly one edit to a criterion line:
changing `- [ ]` to `- [x]`. Annotating the AC3 line would violate both. The blocked status is
therefore recorded here and in the P6-T11 acceptance-criteria status summary instead of beside the
criterion.

## What the automated evidence does and does not establish

The automated tests delivered by this change establish the mechanism that produces the AC3 symptom and
the mechanism of its fix:

- The fresh-build path now adopts the loader's disk configuration, so the wrapper carries the
  resource-defined path rather than an empty one (AC1, proven by
  `LoadStoresAsync_WhenConfigDeserializesToNull_FreshWrapperAdoptsLoaderDiskConfiguration`).
- The serializer no longer returns silently on an empty or null path (AC2).
- An explicit Save writes inline rather than through the three-second deferred timer, so the write is
  not lost if the host exits inside that window (AC4).

They do not establish that the file appears on disk in a live VSTO host. That residual is exactly what
the procedure below covers. The fail-before requirement for AC3 is discharged by the exception dossier
at `evidence/regression-testing/fail-before-exception.2026-09-06T22-00.md`.

## The nine-step procedure, verbatim from the plan, with observed results

Each step records `NOT PERFORMED` together with the reason.

1. Confirm the settings file `StoresWrapper.json` does not exist under the local AppData TaskMaster
   directory, recording the check without recording the absolute path of the user profile.
   - **NOT PERFORMED.** Requires a real user profile directory on the machine that will run the
     add-in. The executing environment is a build worktree with no add-in installation.
2. Build and load the add-in and start Outlook.
   - **NOT PERFORMED.** No live Outlook host is available, and the executing agent is directed not to
     start one or load the add-in.
3. Open Settings, then Folder Settings, and record the rendered Archive Root Outlook, Archive Root
   File System, Junk Potential, Junk Email, User Email, Inbox and Root Folder values.
   - **NOT PERFORMED.** Requires the dialog, which requires the live host. Driving any user interface
     is out of scope for this execution.
4. Select an Archive Root Outlook value and click Save.
   - **NOT PERFORMED.** Same reason as step 3.
5. Confirm the settings file now exists.
   - **NOT PERFORMED.** Depends on step 4.
6. Close Outlook fully and reopen it.
   - **NOT PERFORMED.** Depends on step 2.
7. Reopen Folder Settings and confirm the saved value is present.
   - **NOT PERFORMED.** Depends on steps 4 and 6. This is the step that actually decides AC3.
8. Confirm the session log contains no serializer error and no line reporting an empty or null
   settings path.
   - **NOT PERFORMED.** Requires a session log from a live run.
9. Check the junk-folder rollout consideration recorded as risk 4 in spec.md by confirming whether the
   junk selections shown agree with the .NET user settings.
   - **NOT PERFORMED.** Requires the dialog and the live settings store.

## Notes for the verifier

- Step 9 is the check for the known, accepted rollout consequence of the AC5 reading. Once AC1 lands,
  the per-store JSON mechanism begins writing for the first time on affected machines while the .NET
  user settings already hold values written by the second mechanism. The two can disagree on a machine
  where junk folders were last selected under a non-default store, because one resolves relative paths
  against the selected store's root and the other against the default store's root. A first-run
  disagreement is expected and is not a defect introduced by this change.
- Step 3 will show the User Email label carrying either the mailbox SMTP address or a specific
  unavailability message that names the reason. It will no longer show the generic placeholder that
  the Inbox and Root Folder labels use, because AC6 replaced that literal for this label only.
- Steps 3 and 7 will show the Inbox and Root Folder values without the leading pair of backslash
  characters, per AC7.
- When all nine steps pass, AC3 may be checked off by changing `- [ ]` to `- [x]` on that line in
  spec.md and mirroring the same single-character change in issue.md, and nothing else. If any step
  fails, do not check it off; report the failure against issue #797.

No absolute host path, user account name or machine name appears in this artifact.

Output Summary: AC3 is recorded as BLOCKED-MANUAL. All nine procedure steps are NOT PERFORMED because
the criterion requires a live VSTO host that this execution environment does not have and is directed
not to create. The criterion is handed to the maintainer, and its checkbox is left unmarked and
byte-identical to its authored text in both spec.md and issue.md.
