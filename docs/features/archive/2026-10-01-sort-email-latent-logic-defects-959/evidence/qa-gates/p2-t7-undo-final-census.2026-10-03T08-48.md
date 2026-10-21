# P2-T7 UndoAndMoveLog Final State Census

Timestamp: 2026-10-03T08-48
Command: Write tool rewrote UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs from Listing L-U-FINAL (after reading the file; four leading spaces stripped); then CMD-CENSUS with PATHS-U and TOKENS-U, and CMD-USINGS
EXIT_CODE: 0 (scoped to the CMD-USINGS payload, the last invocation; process exit code)
Output Summary: the corrected Path.Combine order and condition, the single tab-joined header from MovedMailsHeader, SanitizeArray deleted, the exclusion removed from SanitizeArrayLineTSV and the ten-directive using block are in place; every TOKENS-U total equals the FINAL column; the file has 190 lines.

CMD-CENSUS (single path; TOTAL lines):
- TOKEN [ExcludeFromCodeCoverage] @ TOTAL = 4
- TOKEN Path.Combine(strFileName,strFileLocation) @ TOTAL = 0
- TOKEN fileExists(Path.Combine(strFileLocation,strFileName)) @ TOTAL = 1
- TOKEN SanitizeArray( @ TOTAL = 0
- TOKEN MovedMailsHeader @ TOTAL = 2
- TOKEN string.Join("\t",MovedMailsHeader) @ TOTAL = 1
- TOKEN "Triage" @ TOTAL = 1
- TOKEN "FlaggedAsTask" @ TOTAL = 1
- TOKEN string[14,2] @ TOTAL = 0
- TOKEN Func<string,bool>fileExists @ TOTAL = 1
- TOKEN Action<string,string[],string>writeTextFile @ TOTAL = 1
- TOKEN writeTextFile( @ TOTAL = 1
- TOKEN File.Exists @ TOTAL = 1
- TOKEN FileIO2.WriteTextFile @ TOTAL = 1
- TOKEN publicstaticvoidWriteCSV_StartNewFileIfDoesNotExist( @ TOTAL = 1
- TOKEN internalstaticvoidWriteCSV_StartNewFileIfDoesNotExist( @ TOTAL = 1
- TOKEN Debug.WriteLine( @ TOTAL = 0
- TOKEN usingSystem.Diagnostics; @ TOTAL = 0
- TOKEN usingSystem.Collections.Generic; @ TOTAL = 0
- TOKEN usingDeedle; @ TOTAL = 0
- TOKEN usingSDILReader; @ TOTAL = 0
- TOKEN usingOutlook= @ TOTAL = 0
- TOKEN usingUtilitiesCS; @ TOTAL = 0
- TOKEN usingUtilitiesCS.EmailIntelligence; @ TOTAL = 0
- TOKEN usingUtilitiesCS.OutlookExtensions; @ TOTAL = 0
- TOKEN #nullableenable @ TOTAL = 1
- LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs = 190
- SHA256 UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs = FFBF947FE5E2175C13BE3E879CA46358A153D0B1225BC5FCDBCB1CAF58575724

CMD-USINGS (A, T, S and M are not yet rewritten at this task; only the U row is gated here):
- USINGS A count=18 exact=False firstline=True blankafter=True
- USINGS T count=18 exact=False firstline=True blankafter=True
- USINGS U count=10 exact=True firstline=True blankafter=True
- USINGS S count=18 exact=False firstline=True blankafter=True
- USINGS M count=18 exact=False firstline=True blankafter=True
- USINGS-EXACT-FILES: 1

Acceptance check: every TOKENS-U total equals the FINAL column; USINGS U count=10 exact=True firstline=True blankafter=True; LINES 190 (at most 499). All three hold.
