Timestamp: 2026-09-01T06-18
Command: pwsh -NoProfile -Command 'git grep -n -F "TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs:66-72" -- "UtilitiesCS/OutlookObjects/Store/StoreLaunchReadinessEvaluator.cs"'
EXIT_CODE: 0
Output Summary: exactly one line: StoreLaunchReadinessEvaluator.cs:14.

Full paragraph quoted (from the type-level XML doc, lines 8-17):
"Shared readiness evaluation for the store-settings dialogs: computes whether the store-wrapper model has finished loading and is safe to bind (issue #240), reused by both StoreWrapperController and DisabledStoresController so the readiness behavior is defined once. ModelUnavailable is also the terminal state for the remainder of an Outlook session once the store load has completed through its catch block: see TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs:66-72, where a caught load failure leaves StoresWrapper unset and there is no later retry that would move the session out of this state."

The paragraph states that ModelUnavailable is also the terminal state after the caught load failure, and cites the location. AC11 satisfied.
