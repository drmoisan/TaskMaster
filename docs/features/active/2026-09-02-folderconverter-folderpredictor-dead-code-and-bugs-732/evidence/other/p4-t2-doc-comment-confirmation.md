# P4-T2: MatchBestSpecialFolder Doc Comment Confirmation

Timestamp: 2026-09-03T11-51

Output Summary:
Read of TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs lines 85-96 (the XML doc
comment on the internal static `MatchBestSpecialFolder` helper) confirms it still
states, verbatim:

    Behavior is byte-for-byte identical to the original instance method body: a
    null/empty collection returns null; matching uses ordinal <c>string.Contains</c>;
    candidates are ordered by descending value length and the first key is returned
    (null when no candidate matches).

This sentence explicitly names `string.Contains` as ordinal substring matching, and
explicitly states a null/empty collection returns null. No edit is made to this file by
this or any other task in this plan.
