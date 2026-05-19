# Remediation Inputs — appointment-item-test-coverage-79

## Finding
`UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs` is **748 lines**, exceeding the 500-line policy cap in `.github/instructions/general-code-change.instructions.md`.

## Remediation Action
Split `MeetingItemHelperTests.cs` into two `partial class` files:
- `MeetingItemHelperTests.cs` — tests 1-20 (FolderRoot, CompressPlainText, Constructor, ToSerializableObject, ToggleDark, SetSender, GetHtml, LoadRecipients, LoadAll, EmailHeader2) — target ≤ 500 lines
- `MeetingItemHelperTests.Part2.cs` — tests 21-28 (PropertySetters, PropertyChangedSetters, GetHeadersExtendedMapi, Tokenizer, UnReadSetter, LoadInternetCodepage, Equals, ToMatchableObject) + all private helpers + inner class — target ≤ 500 lines

## Required changes
1. Modify `MeetingItemHelperTests.cs`: add `partial` keyword to class declaration; truncate after `EmailHeader2_ShouldIncludeProjectedTextFields` test; add closing braces.
2. Create `MeetingItemHelperTests.Part2.cs`: same using directives; `partial class` declaration; remaining test methods and all private helpers.
3. Update `UtilitiesCS.Test/UtilitiesCS.Test.csproj`: add `<Compile Include="OutlookObjects\AppointmentItem\MeetingItemHelperTests.Part2.cs" />` after the existing MeetingItemHelperTests entry.
4. Re-run CSharpier, analyzer build, nullable build, MSTest+coverage, and coverage delta gates.
5. Re-run reduced audit.

## Constraints
- All test methods must remain identical in content (no logic changes).
- MSTest/Moq/FluentAssertions style must be preserved.
- Compile includes must be updated in `UtilitiesCS.Test.csproj`.
- Inner class `MeetingItemHelperCopyProbe` moves to Part2.cs; it is accessible in Part1 via partial class.
