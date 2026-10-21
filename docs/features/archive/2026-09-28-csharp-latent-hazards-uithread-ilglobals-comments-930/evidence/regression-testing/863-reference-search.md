# #863 reference search after deletion ([P1-T13])

Timestamp: 2026-09-29T09-16
Command: CMD-REF-SOURCE: pwsh -NoProfile -Command 'git grep -n -I -F -e "ILGlobals.Cache" -e "ILGlobals.modules" -e """Cache""" -e """modules""" -e ([char]39 + "Cache" + [char]39) -e ([char]39 + "modules" + [char]39) -- . ":(exclude)docs" ":(exclude).claude" ":(exclude)artifacts"; "REF_SOURCE_GREP_EXIT=$LASTEXITCODE"'
Command: CMD-REF-REPO: pwsh -NoProfile -Command '$hits = @(git grep -n -I -F -e "ILGlobals.Cache" -e "ILGlobals.modules" -e """Cache""" -e """modules""" -e ([char]39 + "Cache" + [char]39) -e ([char]39 + "modules" + [char]39) -- .); "REF_REPO_GREP_EXIT=$LASTEXITCODE"; "REF_REPO_TOTAL=$($hits.Count)"; ...; "REF_REPO_OTHER=$($rest.Count)"; $rest'
EXIT_CODE: 0
REF_SOURCE_GREP_EXIT=0
Output Summary:
- CMD-REF-SOURCE printed exactly one hit: `config/blast-radius.json:32:  "modules": {` (the blast-radius module map JSON key; the positive control proving the search can match). No hit in any .cs file.
- CMD-REF-REPO: REF_REPO_GREP_EXIT=0, REF_REPO_TOTAL=130, REF_REPO_DOCS=122, REF_REPO_GOVERNANCE=7, REF_REPO_OTHER=1; the only `$rest` entry is `config/blast-radius.json:32:  "modules": {`. Hits whose path ends in .cs: 0. Every other hit is under docs/ (122, including this feature's own evidence text, which quotes the removed members) or under .claude/ (7, the same governance paths recorded at baseline).
- The new test sources contain neither word: the Grep tool finds 0 occurrences of `Cache` or `modules` in UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs and 0 in UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs.
- Compared with the baseline census: the ILGlobals_Tests.cs line 267 hit is gone; no reflection-based or string-named consumer of either member remains in source scope.
