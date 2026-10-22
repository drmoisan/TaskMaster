# AC4 Verification (P2-T14)

Timestamp: 2026-09-01T16-51

Command: `git grep -n -F -- 'IsValidSelection keeps its "====" rejection' -- QuickFiler/Controllers/EfcFormController.cs`

EXIT_CODE: 1

ExpectedExitCode: 1

Output Summary: no output. This command asserts an absence, so exit 1 is the
passing outcome. The stale claim that `IsValidSelection` keeps a four-character
rejection is gone from the file.

Command: `git grep -c -F -- 'three-character rejection' -- QuickFiler/Controllers/EfcFormController.cs`

EXIT_CODE: 0

Output Summary — a count of 1:

```
QuickFiler/Controllers/EfcFormController.cs:1
```

Both figures hold.

## Full replacement comment text as it stands on disk (`:318-320`)

```csharp
        public string SelectedFolder
        {
            // Derived from the bridge router's selection tracking. IsValidSelection routes to
            // IsSelectableFolder, which composes IsBannerRow, matching the producers' "===="
            // prefix, with the guard's deliberately broader three-character rejection.
            get => _router?.SelectedFolderPath;
        }
```

The comment now describes the composition the code implements: `IsBannerRow`
matching the producers' four-character prefix, combined with the guard's
deliberately broader three-character rejection. It no longer asserts that
`IsValidSelection` keeps a four-character rejection, which was the inaccuracy
the issue reports and which the issue notes "invites a future contributor to
'correct' the guard in the dangerous direction."

The `get => _router?.SelectedFolderPath;` accessor and everything outside
`:318-320` are unchanged; the edit is comment-only. AC4 makes no
occurrence-count assertion against comment prose beyond the two commands above,
and its remaining clause — verification by AC9's clean toolchain pass — is
satisfied by the six AC9 artifacts, all recording `EXIT_CODE: 0` from the same
final loop pass.

**AC4 checked off in `issue.md`.**
