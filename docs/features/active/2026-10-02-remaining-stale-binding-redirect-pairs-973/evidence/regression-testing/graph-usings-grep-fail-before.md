# P3-T16 AC19 fail-before record (issue #973) [expect-fail]

Timestamp: 2026-10-03T11-35
Command: CMD-GRAPH-USINGS (pwsh -NoProfile -Command 'Set-Location -LiteralPath "<execution-worktree-root>"; ... Get-ChildItem -Recurse -Filter "*.cs" (excluding \.claude\, bin, obj, packages, .dotnet-sdk) | Select-String -Pattern "^using Microsoft\.Graph[.;]" ...; exit 1 when any hit'), paired with the Grep tool pattern `^using Microsoft\.Graph[.;]` type cs -n over <execution-worktree-root>; git -C <execution-worktree-root> status --porcelain -- '*.cs' CLAUDE.md; CMD-LINECOUNT and CMD-CRCOUNT for the five Part F files
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: taken before any .cs edit; six Microsoft.Graph using directives in five files, exactly the fact 10 file and line pairs; both enumeration routes agree (6 and 6); the five files read 306/306, 270/270, 539/539, 356/356, 188/188; porcelain over .cs and CLAUDE.md is empty.

HIT \UtilitiesCS\EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs:18:using Microsoft.Graph.Security.AttackSimulation.Trainings.Item.LanguageDetails;
HIT \UtilitiesCS\EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs:11:using Microsoft.Graph.Communications.OnlineMeetings.GetAllRecordingsmeetingOrganizerUserIdMeetingOrganizerUserIdWithStartDateTimeWithEndDateTime;
HIT \UtilitiesCS\EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs:12:using Microsoft.Graph.Drives.Item.Items.Item.GetActivitiesByInterval;
HIT \UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogic.cs:10:using Microsoft.Graph.Models;
HIT \UtilitiesCS\OutlookObjects\Folder\FolderMinimalWrapper.cs:6:using Microsoft.Graph.Drives.Item.Items.Item.SearchWithQ;
HIT \UtilitiesCS\OutlookObjects\Store\StoreWrapper.cs:7:using Microsoft.Graph.Models.TermStore;
GRAPH-USING-HITS: 6
GRAPH-USING-GREP-HITS: 6 (StoreWrapper.cs:7, FolderMinimalWrapper.cs:6, Triage_OlLogic.cs:10, ManagerAsyncLazy.cs:18, CategoryClassifierGroup.cs:11, CategoryClassifierGroup.cs:12)

Counts (LINECOUNT/CRCOUNT): StoreWrapper.cs 306/306; Triage_OlLogic.cs 270/270; CategoryClassifierGroup.cs 539/539; ManagerAsyncLazy.cs 356/356; FolderMinimalWrapper.cs 188/188
PORCELAIN '*.cs' CLAUDE.md: (empty)
