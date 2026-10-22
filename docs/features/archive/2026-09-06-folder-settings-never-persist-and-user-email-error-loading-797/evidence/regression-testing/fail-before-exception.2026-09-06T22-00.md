# Fail-before Exception Dossier — AC3 (Issue #797)

Timestamp: 2026-09-07T09-40

Criterion: AC3 — "A value saved in Folder Settings is present after an Outlook restart (manual
verification)."

WhyFailingRunImpossible: AC3 asserts that a saved value survives a restart of the Outlook host
process, which requires a live VSTO host with the add-in loaded, a real user profile directory, and a
process teardown and restart. No automated test in this repository can reproduce that: the unit test
policy prohibits external processes and temporary files, the settings write is exercised only through
an injectable stream-writer seam, and no test harness can start or restart Outlook. A failing
automated run for AC3 is therefore structurally impossible rather than merely absent.

## Alternative proof that the defect is real and present before the fix

The runtime log on the reporting machine records the same pair of lines once per Outlook start, at
17:29:59, 19:09:20 and 19:26:35 on 2026-09-06:

```text
[VSTA_Main] WARN  TaskMaster.AppOlObjects - StoresWrapper config deserialized to null; rebuilding from live stores.
[VSTA_Main] ERROR UtilitiesCS.OutlookObjects.Store.StoreWrapper - Error retrieving PrimarySmtpAddress from secondary inbox. The operation failed.
```

The warning recurs on every start, which is only possible if the settings file was never written. A
filesystem check on 2026-09-06 confirmed that the local application data TaskMaster directory contains
the other TaskMaster JSON files while the stores-wrapper settings file is absent, and a recursive
search of the user profile found it nowhere. The log lives outside the repository and was read
read-only; no absolute host path, user account name or machine name is reproduced here.

The automated fail-before evidence recorded in
`p1-t18-fail-before.2026-09-06T22-00.md` covers the mechanism that produces the AC3 symptom: the
fresh-build path does not adopt the loader's disk configuration (AC1), and the serializer's guard
returns silently on the resulting empty path (AC2). Those two failing tests establish, without a live
host, that no write can occur. What they do not establish is that the file appears on disk in a live
VSTO host, which is exactly the residual that AC3's manual procedure covers.

## Negative-evidence record

SearchScope:
- docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/regression-testing/
- docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/other/
- docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/
- docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/qa-gates/

SearchPatterns:
- `fail-before-exception.*.md`
- `*.trx` results containing a test whose name references an Outlook restart or a settings-file
  round trip

SearchResult: none. No automated fail-before run exists for AC3 anywhere under this feature folder's
evidence tree, and this dossier is the only `fail-before-exception.*.md` file present.

## Disposition

AC3 is verified by the written manual procedure recorded in P6-T1. No automated gate in this plan
claims to prove it.
