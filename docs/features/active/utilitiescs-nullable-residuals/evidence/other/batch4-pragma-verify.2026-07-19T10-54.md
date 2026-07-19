# Batch 4 Pragma Verification (P5-T3)

Timestamp: 2026-07-19T10-54

Batch 4 opted-in files (2, OneDrive helpers):
1. UtilitiesCS/OneDriveHelpers/AngleSharpParsedEmailBody.cs — `Html`/`_html` → `string?`;
   `Links`/`_links` and `FilteredLinks`/`_filteredLinks` → `IEnumerable<(string,string)>?`;
   `FilterLinksByDomain` return → `AngleSharpParsedEmailBody?` (has `return null`); setter-assigned
   `_parser = null!`. `Links ??=` guard preserved.
2. UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs — `TryGetUrlStreamAsync` and
   `TryGetFileStreamWriter` returns → `Task<Stream?>` (callers already null-check); setter-assigned
   `_client = null!` and `_clientGetAsync = null!`. Existing `?.Dispose()` preserved.

## Trustworthy isolated CS86xx gate

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168 -p:BuildProjectReferences=false`

EXIT_CODE: 0

Output Summary: Build succeeded. 0 errors, 0 CS86xx, 15 pre-existing out-of-scope warnings.
Consistency with pinned upstream contracts (AC5) is confirmed by construction: NO null handling was
added around `response.IsSuccessStatusCode` or the returned stream, because
`TimeOutTask.RunWithTimeout` returns non-null `Task<TResult>` (#369) so `response` is non-null, and
`StreamExtensions.TryCopyToAsyncWithTimeout` returns `Task<bool>` (#363, value type). If those returns
had been nullable, the build would have required `!` or a guard — it did not. No new runtime guard
added.
