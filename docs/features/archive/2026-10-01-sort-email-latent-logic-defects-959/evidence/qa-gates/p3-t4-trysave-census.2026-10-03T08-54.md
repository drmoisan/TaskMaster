# P3-T4 TrySaveAttachment Final State Census

Timestamp: 2026-10-03T08-54
Command: Write tool rewrote UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs from Listing L-T-FINAL (after reading the file; four leading spaces stripped); then CMD-CENSUS with PATHS-T and TOKENS-T, and CMD-USINGS
EXIT_CODE: 0 (scoped to the CMD-USINGS payload, the last invocation; process exit code)
Output Summary: the five-argument overload is a single forward to the private TrySaveAttachmentCoreAsync with isRetryAfterClear false; the core carries the YesToAll guard (logger.Error then a bare rethrow), the four logger calls, one catch clause and the recursion with isRetryAfterClear true; the outer rethrow and the Debug.WriteLine calls are gone; the five-directive using block is exact; the file has 204 lines.

CMD-CENSUS (single path; TOTAL lines):
- TOKEN [ExcludeFromCodeCoverage] @ TOTAL = 2
- TOKEN Debug.WriteLine( @ TOTAL = 0
- TOKEN catch(System.Exception){throw;} @ TOTAL = 0
- TOKEN catch( @ TOTAL = 2
- TOKEN catch(System.UnauthorizedAccessExceptione) @ TOTAL = 1
- TOKEN catch(System.Exceptioninner) @ TOTAL = 1
- TOKEN throw; @ TOTAL = 2
- TOKEN TrySaveAttachmentCoreAsync( @ TOTAL = 3
- TOKEN privatestaticasyncTask<bool>TrySaveAttachmentCoreAsync( @ TOTAL = 1
- TOKEN internalstaticasyncTask<bool>TrySaveAttachmentAsync( @ TOTAL = 0
- TOKEN internalstaticTask<bool>TrySaveAttachmentAsync( @ TOTAL = 3
- TOKEN boolisRetryAfterClear @ TOTAL = 1
- TOKEN isRetryAfterClear:false @ TOTAL = 1
- TOKEN isRetryAfterClear:true @ TOTAL = 1
- TOKEN isRetryAfterClear&&removeReadOnlyPrompt.Response==YesNoToAllResponse.YesToAll @ TOTAL = 1
- TOKEN logger.Warn( @ TOTAL = 2
- TOKEN logger.Error( @ TOTAL = 2
- TOKEN createDirectory(Path.GetDirectoryName(filePathSave)); @ TOTAL = 1
- TOKEN System.IO.Directory.CreateDirectory(path) @ TOTAL = 1
- TOKEN removeReadOnlyPrompt.ReleaseSingleAnswer(); @ TOTAL = 2
- TOKEN RemoveReadOnlyPrompt=new( @ TOTAL = 1
- TOKEN usingSystem.Diagnostics; @ TOTAL = 0
- TOKEN usingDeedle; @ TOTAL = 0
- TOKEN usingSDILReader; @ TOTAL = 0
- TOKEN usingOutlook= @ TOTAL = 0
- TOKEN usingUtilitiesCS; @ TOTAL = 0
- TOKEN #nullableenable @ TOTAL = 1
- LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs = 204
- SHA256 UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs = 63141294DEF46D2B34B4A8FB01A894C9DF2CF702C579EC1065CF3F0F35FBFECC

CMD-USINGS (A, S and M are not yet rewritten at this task; the T row is gated here):
- USINGS A count=18 exact=False firstline=True blankafter=True
- USINGS T count=5 exact=True firstline=True blankafter=True
- USINGS U count=10 exact=True firstline=True blankafter=True
- USINGS S count=18 exact=False firstline=True blankafter=True
- USINGS M count=18 exact=False firstline=True blankafter=True
- USINGS-EXACT-FILES: 2

Acceptance check: every TOKENS-T total equals the FINAL column (Debug.WriteLine( 0, catch(System.Exception){throw;} 0, catch( 2, throw; 2, TrySaveAttachmentCoreAsync( 3, isRetryAfterClear:true 1, isRetryAfterClear:false 1, logger.Warn( 2, logger.Error( 2, createDirectory(Path.GetDirectoryName(filePathSave)); 1, [ExcludeFromCodeCoverage] 2, usingSystem.Diagnostics; 0); USINGS T count=5 exact=True firstline=True blankafter=True; LINES 204 (at most 499). All three hold.
