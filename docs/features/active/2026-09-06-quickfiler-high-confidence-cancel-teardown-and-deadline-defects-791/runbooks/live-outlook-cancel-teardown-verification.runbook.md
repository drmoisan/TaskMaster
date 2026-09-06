# Human-Exception Runbook — Live-Outlook Cancel Teardown Verification (Issue #791, AC2)

## Cue

Act on this runbook when the executor or feature reviewer for issue #791 reaches the manual-verification
item of acceptance criterion AC2 ("Cancel teardown completes cleanly... plus a manual live-Outlook
evidence note"), or when a pull request for issue #791 is opened. A live Outlook process, a live WebView2
runtime, and real user-driven Cancel/Undo clicks cannot be driven by an agent, so this step is resolved as
a permitted `exception` and performed by a human.

## Prerequisites

- A Windows machine with Microsoft Outlook installed and the TaskMaster VSTO add-in already registered for
  the current user (an HKCU Outlook add-in manifest entry pointing at a `TaskMaster.vsto` deployment).
- A local checkout of the issue #791 feature branch. The confirmed working deployment path on this
  machine's registered manifest is `C:\Users\DanMoisan\repos\TaskMaster\TaskMaster\bin\Debug\TaskMaster.vsto`.
  Either build the feature branch in that exact checkout, or update the HKCU manifest to point at the
  checkout actually used, before testing. Rebuilding a checkout that is already registered updates the
  assembly Outlook loads without any re-registration step (see Source and Citation).
- Read access to the feature folder's atomic plan file, `plan.2026-09-06T12-57.md`, to obtain the exact
  Cancel-path log line markers as implemented. The marker text is defined by the plan, not by this
  runbook, and may differ from any provisional wording used during planning.
- An Outlook Explorer view containing enough mail items to file two rounds of High Confidence suggestions.
- Write access to create the evidence file under `docs/features/active/2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791/evidence/other/`.

## Step-by-step Instructions

1. Close Outlook if it is running.
2. Build the issue #791 feature branch in the checkout that deploys to
   `TaskMaster\bin\Debug\TaskMaster.vsto` (or confirm the HKCU manifest has been updated to the checkout
   you are using instead).
3. Open `plan.2026-09-06T12-57.md` in the feature folder and note the exact text of every Cancel-path log
   line the plan introduces (for example, lines logging token cancellation, loader await/stop, handler
   unregistration, `KbdActive` reset, focus park, breadcrumb cancel, and ribbon release). Keep this list
   at hand for step 8.
4. Open the deploy directory's log folder (`<deploy dir>\logs\`) and note the last line and timestamp of
   today's `debug_yyyy-MM-dd.log`, if the file already exists. This marks where new lines from this test
   begin.
5. Launch Outlook. Confirm the add-in loaded (the QuickFiler ribbon group, including the High Confidence
   button, is visible).
6. Select an Explorer view with mail items. Launch QuickFiler via the ribbon **High Confidence** button.
7. File a first round of suggestions (commit at least one item). File a second round, then click **Undo**
   repeatedly in a quick burst (10 or more clicks in rapid succession) to reproduce the reported scenario.
8. Press **Cancel**.
9. Immediately click into a native Outlook window (an Explorer view or an open Inspector) and type.
   Confirm keystrokes are received normally and Outlook is responsive.
10. Open `<deploy dir>\logs\debug_yyyy-MM-dd.log` and read the lines written after the marker noted in
    step 4. Confirm each Cancel-path log line from step 3 appears, in the order the plan specifies.
11. In the same lines, confirm the following string does NOT appear anywhere after the Cancel press:
    `ERROR QuickFiler.Controllers.QfcDatamodel - LoadRemainingEmailsToQueue Error. Delegate to an instance
    method cannot have null 'this'.`
12. Because the original defect was sporadic (observed once directly, once via a surviving background
    loader crashing on the next launch), repeat steps 6-11 at least once more if time allows. Record every
    pass performed.
13. Write the evidence file at
    `docs/features/active/2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791/evidence/other/manual-verification.<yyyy-MM-ddTHH-mm>.md`
    (timestamp format per the evidence-and-timestamp-conventions skill) with these fields:
    - `Timestamp:` ISO-8601 timestamp of the test session.
    - `Build (commit SHA):` the commit the tested build was compiled from.
    - `Steps performed:` which of steps 6-12 were executed and how many passes.
    - `Observed log lines:` the Cancel-path lines actually observed, quoted verbatim.
    - `Keyboard state after Cancel:` what was observed in step 9, for each pass.
    - `Result: PASS` or `Result: FAIL` (FAIL if the null-`this` error appears, if any expected Cancel-path
      line is missing, or if the keyboard is left unusable in any pass).
    - `Tester:` name of the person who performed the verification.

## Verification

- The evidence file described in step 13 exists at the canonical path and contains all required fields.
- `Result: PASS` requires: the Outlook keyboard remained usable in native Outlook windows after every
  Cancel press performed; every Cancel-path log line named in the plan file was observed in the log, in
  the specified order, for every pass; and the string
  `Delegate to an instance method cannot have null 'this'.` does not appear anywhere in the log after any
  Cancel press.
- If any pass fails these conditions, record `Result: FAIL` with the specific missing line, out-of-order
  line, error occurrence, or keyboard symptom observed, rather than omitting the failing pass.

## Source and Citation

- Build-to-load mechanism (third-party UI/tooling background, web-second — no MCP documentation tool is
  wired into this repository at this time; see the two-axis-model-selection spec, Out of Scope):
  Microsoft Learn, "Create Visual Studio Tools for Office Add-ins: Outlook mail" — "When you build the
  project, the code is compiled into an assembly that is included in the build output folder for the
  project. Visual Studio also creates a set of registry entries that enable Outlook to discover and load
  the VSTO Add-in." Source URL:
  https://learn.microsoft.com/en-us/visualstudio/vsto/walkthrough-creating-your-first-vsto-add-in-for-outlook
  — updated_at: 2026-04-24 (fetched 2026-09-06).
- Defect signature, deploy path, log location, and threshold facts (primary, in-repo source): `issue.md`
  in this feature folder, captured 2026-09-06 (session in which the deploy path, log file location, and
  the exact `null 'this'` error text were verified directly against a live log).
- Exact Cancel-path log marker text (primary, in-repo source, authoritative at test time): the atomic plan
  file `plan.2026-09-06T12-57.md` in this feature folder — read at the time of testing, since the markers
  are implementation output, not fixed by this runbook.
- Evidence file naming and location: `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`,
  canonical `<FEATURE>/evidence/other/` path and `yyyy-MM-ddTHH-mm` timestamp format.
