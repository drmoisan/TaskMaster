# Subfolder Commit Tests CSharpier

Timestamp: 2026-07-21T23-12Z

Command: `csharpier format UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSubfolderSelectorSessionTests.cs QuickFiler.Test/Viewers/BreadcrumbSubfolderActivationTests.cs UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectorMessagesTests.cs`

EXIT_CODE: 0

Output Summary: The initial scoped pass formatted all three authorized batch-D test sources. The formatter output was retained. The identical command was rerun and completed with stable SHA-256 hashes for every file.

## Initial scoped pass

- `BreadcrumbSubfolderSelectorSessionTests.cs`: `AD424074EA44DCE43D65AE4A2F428239383C717B6B92E85FC2A15F5C2F573662` before, `BF57AA91BA20953505B2B085141602AD0663A79EB3F4E83280477E10DBA4561B` after.
- `BreadcrumbSubfolderActivationTests.cs`: `DD3F84AF30F4DAA62AAB14F08AEE40B7FA2AA778513E09B7D527532152842F8F` before, `BE49A0264312490EDC96386969B174F052251A8363EA990D656143B9901EA687` after.
- `BreadcrumbSelectorMessagesTests.cs`: `7F738ABC89287B6E368AFDD5DB367C09127F9FC8A605B7E8B8071C0EE08A1B01` before, `7413BF2963072B2BA5BCBBE6D50E661857C13B6B55EF00928296C4C0AB0F5DE6` after.

## Required rerun

- Rerun EXIT_CODE: 0.
- All three before/after hashes matched the final hashes recorded above.
- Final line counts: 147, 381, and 289, respectively; every file remains at most 500 lines.

The required rerun produced no further formatter delta.
