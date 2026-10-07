# P3-T13 System.Linq.AsyncEnumerable redirects (issue #973; AC9)

Timestamp: 2026-10-03T11-32
Command: EDIT-SLAE (EDIT-REDIRECT System.Linq.AsyncEnumerable 10.0.0.7 to VERSION 10.0.0.12) on the 15 carrier configs (one Edit-tool replacement per file, old_string the assemblyIdentity line plus its bindingRedirect line read by Grep -A 1); observations: CMD-PAIR-COUNT System.Linq.AsyncEnumerable 10.0.0.12 (with the full `oldVersion="0.0.0.0-10.0.0.12" newVersion="10.0.0.12"` text) and 10.0.0.7 over */app.config; CMD-NAME-COUNT System.Linq.AsyncEnumerable over */app.config; CMD-LINECOUNT and CMD-CRCOUNT over */app.config; git -C <execution-worktree-root> ls-files --eol -- <15 configs>; CMD-BOM for the 15 configs; git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- '*app.config'
EXIT_CODE: 0
Output Summary: all 15 carrier configs now redirect System.Linq.AsyncEnumerable 0.0.0.0-10.0.0.12 to 10.0.0.12; no 10.0.0.7 redirect remains; the name occurs in exactly the same 15 files (none added, none in SVGControl or SVGControl.Test); line counts unchanged from Phase 2 with CR counts equal, w/crlf, BOM unchanged; numstat P+1/P+5 (ADAL files) or P+1/P+1.

CMD-PAIR-COUNT System.Linq.AsyncEnumerable 10.0.0.12: 15 files, count 1 each
CMD-PAIR-COUNT System.Linq.AsyncEnumerable 10.0.0.7: no match
CMD-NAME-COUNT System.Linq.AsyncEnumerable: the same 15 files, count 1 each

SLAE Tags numstat=14/18 lines=235 cr=235 eol=w/crlf bom=239,187,191
SLAE TaskTree numstat=14/18 lines=235 cr=235 eol=w/crlf bom=239,187,191
SLAE TaskVisualization numstat=14/18 lines=235 cr=235 eol=w/crlf bom=239,187,191
SLAE QuickFiler numstat=13/17 lines=239 cr=239 eol=w/crlf bom=239,187,191
SLAE TaskMaster numstat=13/17 lines=426 cr=426 eol=w/crlf bom=239,187,191
SLAE ToDoModel numstat=13/17 lines=319 cr=319 eol=w/crlf bom=239,187,191
SLAE UtilitiesCS numstat=1/5 lines=289 cr=289 eol=w/crlf bom=239,187,191
SLAE VBFunctions.Test numstat=10/14 lines=347 cr=347 eol=w/crlf bom=239,187,191
SLAE Tags.Test numstat=9/9 lines=343 cr=343 eol=w/crlf bom=239,187,191
SLAE TaskTree.Test numstat=9/9 lines=343 cr=343 eol=w/crlf bom=239,187,191
SLAE QuickFiler.Test numstat=10/14 lines=363 cr=363 eol=w/crlf bom=239,187,191
SLAE TaskMaster.Test numstat=10/14 lines=355 cr=355 eol=w/crlf bom=239,187,191
SLAE TaskVisualization.Test numstat=10/14 lines=359 cr=359 eol=w/crlf bom=239,187,191
SLAE ToDoModel.Test numstat=10/14 lines=359 cr=359 eol=w/crlf bom=239,187,191
SLAE UtilitiesCS.Test numstat=2/6 lines=379 cr=379 eol=w/crlf bom=239,187,191

SVGControl/app.config 23/23 and SVGControl.Test/app.config 227/227: absent from the numstat list (unchanged).
