# Debt 2 — Batch: People — Remediated

Timestamp: 2026-07-20T00-50
Command: `MSBuild.exe UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true`
EXIT_CODE: 1 (solution-wide count still non-zero — remaining errors are entirely in
not-yet-remediated later batches. Zero errors remain for
`UtilitiesCS/EmailIntelligence/People/**`, confirmed by targeted grep returning no matches after
remediation.)

## Before/after (excluding the island lines, handled separately in P2-T14)

`PeopleScoDictionaryNew.cs`: CS8604:2, CS8600:1 -> 0. Total remaining solution-wide error count
after this batch: 32 (down from 35 after the OlFolderTools batch).

## Remediation approach

- `AddMissingEntries`: `recipients.Add(helper.Sender!)` — same `IRecipientInfo? Sender` pattern
  already established across this remediation (`AutoFile.cs`, `SortEmail.cs`).
- `RefineValidateCategory`: `InputBox.ShowDialog(...)`'s nullable `string?` return, assigned to
  the non-nullable `newPerson` parameter inside a `while` loop, required null-forgiving at
  **both** the assignment result (`)!;`) and the loop's next-iteration argument use
  (`DefaultResponse: newPerson!`) — Roslyn's loop-flow analysis re-widens a forgiven local's
  null-state at the loop-back edge on a conservative fixed-point pass, so a single forgiving
  operator at only one of the two sites was insufficient; both were required to reach a clean
  build (confirmed via two successive isolated rebuilds).

`PeopleScoDictionaryNewBackup.cs` (the dead, uncompiled duplicate documented in the epic's
Maintainer Decision Summary) remains outside the `.csproj` `<Compile Include>` set and untouched
by this batch, per the plan's own scope statement.

## Behavior-preservation confirmation

`git diff` for `PeopleScoDictionaryNew.cs` (excluding the island decision, recorded separately)
shows only the two described null-forgiving edits; no removed or altered method signatures, no
altered control flow.
