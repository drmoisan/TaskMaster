Timestamp: 2026-09-03T14-19
AC5 verification.

`catch (UnauthorizedAccessException` occurrence count in UtilitiesCS/To Depricate/FileIO2.cs: 0
`UnauthorizedAccessException` occurrence count in UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs: 0

Both confirm no new handling was added for UnauthorizedAccessException (it is not an IOException subtype and is already outside the retry set), and no test references it.

AC5 checked off in spec.md.
