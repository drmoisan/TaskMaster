# P5-T1 — Project-File Correction

Timestamp: 2026-09-13T06-18
Task: [P5-T1]

This artifact records counts only. It contains no account token, no host token, no organization name
and no absolute host path, which is the rule this delivery exists to enforce and therefore the rule its
own evidence must hold to. Both tokens are derived at run time — the account token as the leaf name of
the current user profile directory, the host token as the computer name — exactly as the Phase 0
baseline task derived them, and neither is hardcoded anywhere in the command or in this text.

## The change

The publish-destination element on line 37 of `TaskMaster/TaskMaster.csproj` carried an absolute
user-profile path. Its text is now the repository-relative value the other project file in this
repository already uses for the same property: the single segment `publish` followed by a trailing
backslash. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` line 29 carries that same value, so the two
project files now agree.

The value is XML element text, so no angle-bracket placeholder was used and no empty value was left: a
placeholder would be read as markup and an empty element would change the property's meaning rather
than redact it.

Command: `git diff --numstat -- TaskMaster/TaskMaster.csproj`
EXIT_CODE: 0
Output: `1	1	TaskMaster/TaskMaster.csproj` — one line added, one removed, so the edit touched only
the element this task names. Carriage-return line-feed endings were preserved: 586 such endings and
zero bare line feeds.

## The four acceptance observations

Command: pwsh -NoProfile -Command '<read the project file, derive the account token as the leaf of the user profile directory and the host token as the computer name, count case-insensitive fixed-string matches of the OneDrive literal, the account token, the host token and a drive-letter-rooted user-profile path, then load the file as an XML document>'
EXIT_CODE: 0

```
ONEDRIVE_COUNT: 0
ACCOUNT_TOKEN_COUNT: 0
HOST_TOKEN_COUNT: 0
USER_PROFILE_PATH_COUNT: 0
XML_LOADS_WITHOUT_ERROR: True
```

- `ONEDRIVE_COUNT: 0` where the Phase 0 baseline recorded a count greater than 0 for the same
  case-insensitive single-word literal in the same file. Phase 0 halted-if-zero on that baseline
  precisely so this after-state check would be falsifiable, and it was: the baseline count was 1.
- `ACCOUNT_TOKEN_COUNT: 0` and `HOST_TOKEN_COUNT: 0` from case-insensitive fixed-string searches for
  the two run-time-derived tokens.
- `USER_PROFILE_PATH_COUNT: 0` from a case-insensitive search for a drive-letter-rooted user-profile
  path, expressed as a drive letter followed by a colon and the user-profile directory segment.
- `XML_LOADS_WITHOUT_ERROR: True` — the file loads as an XML document. The absence counts alone are
  necessary but not sufficient: a substitution that broke the markup would still return zero matches,
  so the parse check is what distinguishes a correct edit from a destructive one.

All four observations hold.

## Scope

Only the publish-destination element text was changed. No analyzer version was touched anywhere. This
project file is not one of the fifteen carrying the analyzer version skew, so this edit has no
interaction with the C# build state, and no other project file in the repository was modified.

Output Summary: The publish-destination element now carries the repository-relative value the sibling
project file uses. The OneDrive count is 0 against a non-zero baseline, the account and host token
counts are 0, the drive-letter-rooted user-profile path count is 0, and the file loads as XML. One line
added, one removed, line endings preserved.
