#!/usr/bin/env bash
# cleanup_worktrees_actions_lib.sh: sourceable function library for the mutating
# ("apply mode") half of the cleanup-worktrees tool. Provides consolidation onto the
# documentationandmemories branch (dedicated worktree creation, cherry-picking with
# conflict/empty handling, abort cleanup), the post-merge verification gate, and the
# deletion mechanics (base-branch refusal, same-process re-verification, no-force
# worktree removal, branch deletion) plus the apply-mode driver.
#
# Sourcing contract: this library depends on functions defined in
# .claude/skills/cleanup-merged-worktrees/scripts/cleanup_worktrees_lib.sh (cleanup_wt_git, parse_worktree_list,
# classify_ancestry, classify_branch, run_report). The wrapper
# .claude/skills/cleanup-merged-worktrees/scripts/cleanup-worktrees.sh sources that library first; the bats suites do
# the same. This file defines functions only and runs nothing at source time.
# git commands that legitimately return non-zero (cherry-pick on conflict, worktree
# remove on a dirty tree, merge-base --is-ancestor) are captured with `|| rc=$?` so
# an intended non-zero exit does not abort under set -euo pipefail. All consolidation
# git commands target the dedicated worktree via `git -C`, never the caller's tree.
#
# Guarded-read invariant: every git-backed read whose output or exit code is
# authoritative is captured in the parent shell (`out=$(cmd) || rc=$?`, iterated over
# `<<<"$out"`), never via `mapfile`/`< <(...)`. consolidation_worktree_path fails
# (non-zero, empty stdout) on a parse_worktree_list hard failure or an empty main path,
# so the malformed `-wt/documentationandmemories` derivation is impossible; both
# consumers (create_consolidation_worktree, cleanup_consolidation_on_abort) guard the
# capture. run_apply captures parse_worktree_list, enumerate_branches, and each
# classify_branch with rc up front and returns non-zero on any hard failure without
# mutating. reverify_delete_eligible and remove_worktree_safe capture their git-backed
# reads and fail closed (BLOCKED-REVERIFY / BLOCKED-DIRTY). Accepted best-effort sites
# (Design Decision 9), intentionally left as `|| true` because every failure direction
# is already fail-closed: `git fetch origin main` in verify_consolidation_merged (a
# stale main can only block deletion, never fabricate ancestry), `cherry-pick
# --skip`/`--abort` in cherry_pick_candidates (a failed skip/abort surfaces on the next
# pick as rc=1), the CHERRY_PICK_HEAD and ref-exists probes, and
# `vout=$(verify_consolidation_merged) || true` in run_apply (only the exact token
# MERGED_CLEAN unlocks deletion).
#
# Opt-in disposable-dirt clearing hook: when CLEANUP_WT_CLEAR_DISPOSABLE is 1,
# delete_candidate answers a BLOCKED-DIRTY removal by calling clear_disposable_dirt
# (.claude/skills/cleanup-merged-worktrees/scripts/cleanup_worktrees_dirt_lib.sh), then re-verifying delete eligibility in
# process, then retrying the SAME unforced removal. remove_worktree_safe is unmodified
# by that hook: it gains no force flag, no new argument, and no new call site, so the
# never-force-remove invariant holds on every path. The hook is off by default, runs in
# apply mode only, and refuses the clear when any per-file verdict for the worktree is
# UNIQUE, including the fail-closed UNIQUE assigned when a classification read errors.

CLEANUP_WT_CONSOLIDATION_BRANCH="documentationandmemories"

consolidation_worktree_path() {
	# Echo the consolidation worktree path. CLEANUP_WT_CONSOLIDATION_PATH overrides;
	# otherwise the path is <main-worktree-path>-wt/documentationandmemories, where the
	# main worktree path is the first `git worktree list --porcelain` stanza.
	#
	# A parse_worktree_list hard failure aborts with git's exit code and no stdout; an
	# empty main worktree path aborts with return 1 and no stdout. Either way the
	# malformed `-wt/documentationandmemories` derivation from an empty main path is
	# impossible.
	local override=${CLEANUP_WT_CONSOLIDATION_PATH:-}
	if [[ -n $override ]]; then
		printf '%s\n' "$override"
		return 0
	fi
	local out rc=0 first_record main_wt=""
	# Guarded parent-shell capture instead of `mapfile < <(...)`, which would swallow a
	# git hard failure and yield a malformed path.
	out=$(parse_worktree_list) || rc=$?
	if ((rc != 0)); then
		printf 'cleanup-worktrees: cannot derive consolidation path: git worktree list failed (rc=%s)\n' "$rc" >&2
		return "$rc"
	fi
	first_record=${out%%$'\n'*}
	main_wt=${first_record%%|*}
	if [[ -z $main_wt ]]; then
		printf 'cleanup-worktrees: cannot derive consolidation path: empty main worktree path\n' >&2
		return 1
	fi
	printf '%s-wt/%s\n' "$main_wt" "$CLEANUP_WT_CONSOLIDATION_BRANCH"
}

create_consolidation_worktree() {
	# Create the documentationandmemories branch in a dedicated worktree off main.
	#
	# Precondition: refs/heads/documentationandmemories must NOT already exist. If it
	# does (a prior aborted run), stop and report to stderr rather than reuse silently.
	# Otherwise run `git worktree add <path> -b documentationandmemories main` with the
	# path derived by consolidation_worktree_path.
	#
	# Returns 0 on success (emits ACTION|worktree-add|<path>|OK); non-zero on a
	# consolidation-path derivation failure, the pre-existing-branch guard, or a failed
	# worktree add.
	local path rc=0 prc=0
	# Guarded capture: a consolidation-path derivation failure aborts before the ref
	# probe and before any git mutation.
	path=$(consolidation_worktree_path) || prc=$?
	if ((prc != 0)); then
		return "$prc"
	fi
	if cleanup_wt_git rev-parse --verify --quiet \
		"refs/heads/$CLEANUP_WT_CONSOLIDATION_BRANCH" >/dev/null 2>&1; then
		printf 'cleanup-worktrees: refs/heads/%s already exists; refusing to reuse a prior consolidation branch. Remove it or resume the prior run.\n' \
			"$CLEANUP_WT_CONSOLIDATION_BRANCH" >&2
		return 1
	fi
	cleanup_wt_git worktree add "$path" -b "$CLEANUP_WT_CONSOLIDATION_BRANCH" main || rc=$?
	if ((rc != 0)); then
		printf 'ACTION|worktree-add|%s|FAILED\n' "$path"
		return "$rc"
	fi
	printf 'ACTION|worktree-add|%s|OK\n' "$path"
	return 0
}

cherry_pick_candidates() {
	# Cherry-pick each UNIQUE candidate onto the consolidation worktree.
	#
	# Reads COMMIT|<branch>|<sha>|UNIQUE|... records from stdin. Source branches are
	# processed in LC_ALL=C order and commits oldest-first within each branch; that
	# ordering is inherited from run_report's COMMIT emission (enumerate_branches is
	# LC_ALL=C sorted and select_cherry_pick_candidates emits oldest-first). One
	# `git -C <wt> cherry-pick -x <sha>` invocation per commit (never a multi-SHA
	# invocation): -x records provenance and default cherry-pick preserves authorship.
	# Every git command targets the consolidation worktree via -C, never the caller's
	# worktree. Conflict/empty handling is implemented in this function (see below).
	#
	# Conflict handling (Planner Decision 4): on a non-zero cherry-pick whose result is
	# a conflict (CHERRY_PICK_HEAD present), run `git -C <wt> cherry-pick --abort`,
	# record the commit as CONFLICT, skip the remaining commits of that source branch
	# (intra-branch dependency safety), and continue with the next branch. On a "now
	# empty" result, run `git -C <wt> cherry-pick --skip` and reclassify the commit as
	# droppable (CONTENT_ON_MAIN). Conflicts are never auto-resolved; --allow-empty and
	# --keep-redundant-commits are never used.
	#
	# Args: $1 = consolidation worktree path.
	local wt="$1" line branch sha prc cp_out rc=0 skip_branch=""
	while IFS= read -r line; do
		[[ $line == COMMIT\|* ]] || continue
		IFS='|' read -r _ branch sha _ <<<"$line"
		# A conflict on an earlier commit of this branch skips its remaining commits.
		if [[ -n $skip_branch && $branch == "$skip_branch" ]]; then
			printf 'ACTION|cherry-pick|%s|SKIPPED-BRANCH\n' "$sha"
			continue
		fi
		prc=0
		# Capture combined output for reliable "now empty"/conflict detection (real
		# git emits those diagnostics on stderr). The recording stub multiplexes its
		# `stub-git:` argv log onto the same streams, so re-surface those lines to
		# stderr for argv observation; this is a no-op under real git.
		cp_out=$(cleanup_wt_git -C "$wt" cherry-pick -x "$sha" 2>&1) || prc=$?
		if [[ $cp_out == *"stub-git: "* ]]; then
			printf '%s\n' "$cp_out" | grep '^stub-git: ' >&2 || true
		fi
		if ((prc == 0)); then
			printf 'ACTION|cherry-pick|%s|OK\n' "$sha"
			continue
		fi
		# "Now empty" means the content already landed on main: skip and reclassify.
		if printf '%s' "$cp_out" | grep -qi 'now empty'; then
			cleanup_wt_git -C "$wt" cherry-pick --skip >/dev/null || true
			printf 'COMMIT|%s|%s|CONTENT_ON_MAIN|reclassified-empty\n' "$branch" "$sha"
			printf 'ACTION|cherry-pick-skip|%s|OK\n' "$sha"
			continue
		fi
		# Otherwise a conflict: abort (never auto-resolve), record, skip the branch.
		if cleanup_wt_git -C "$wt" rev-parse --verify --quiet CHERRY_PICK_HEAD >/dev/null 2>&1; then
			cleanup_wt_git -C "$wt" cherry-pick --abort >/dev/null || true
		fi
		printf 'COMMIT|%s|%s|CONFLICT|\n' "$branch" "$sha"
		printf 'ACTION|cherry-pick|%s|CONFLICT\n' "$sha"
		skip_branch="$branch"
		rc=1
	done
	return "$rc"
}

cleanup_consolidation_on_abort() {
	# Tear down a partially-built consolidation on a hard failure: remove the
	# consolidation worktree and delete the documentationandmemories branch, reporting
	# each action. No silent leftovers are permitted. The forced-removal flag is never
	# used (that policy is global to this library); a failed removal is reported.
	#
	# Returns 0 (best-effort cleanup; each step's result is reported via ACTION lines).
	# When the consolidation path cannot be derived (a parse_worktree_list hard failure),
	# the worktree removal is skipped and reported as ACTION|worktree-remove||FAILED (path
	# unknown), and the branch deletion is still attempted (best-effort contract).
	local path wrc=0 brc=0 prc=0
	# Guarded capture: on a path-derivation failure, skip the removal (path unknown) and
	# continue to the branch-deletion step.
	path=$(consolidation_worktree_path) || prc=$?
	if ((prc != 0)); then
		printf 'ACTION|worktree-remove||FAILED\n'
	else
		cleanup_wt_git worktree remove "$path" >/dev/null || wrc=$?
		if ((wrc == 0)); then
			printf 'ACTION|worktree-remove|%s|OK\n' "$path"
		else
			printf 'ACTION|worktree-remove|%s|FAILED\n' "$path"
		fi
	fi
	cleanup_wt_git branch -D "$CLEANUP_WT_CONSOLIDATION_BRANCH" >/dev/null || brc=$?
	if ((brc == 0)); then
		printf 'ACTION|branch-delete|%s|OK\n' "$CLEANUP_WT_CONSOLIDATION_BRANCH"
	else
		printf 'ACTION|branch-delete|%s|FAILED\n' "$CLEANUP_WT_CONSOLIDATION_BRANCH"
	fi
	return 0
}

verify_consolidation_merged() {
	# Post-merge gate: after `git fetch origin main`, is the consolidation branch an
	# ancestor of main? Uses the same 0/1/>1 merge-base --is-ancestor handling as
	# classify_ancestry. Exit 0 is the ONLY state that unlocks deletion of branches
	# whose unique content was consolidated. Echoes MERGED_CLEAN / NOT_ANCESTOR /
	# ANCESTRY_ERROR and returns 0 / 1 / 2 respectively.
	local mrc=0
	# Tip-equality pre-check, ahead of the best-effort fetch so no network call is made
	# in the blocked case. A consolidation branch created at main and not yet committed
	# to has a tip identical to main's; merge-base --is-ancestor answers 0 for that
	# shape, which would wrongly present a zero-commit branch as delete-eligible. An
	# empty or unresolvable tip on either side is a HARD FAILURE, never an equality
	# match: two empty strings compare equal, so treating them as equality would block a
	# genuinely merged branch. A commit-counting guard was rejected for this check: the
	# count is also zero after the consolidation PR merges with a merge commit, which
	# would permanently block the documented post-merge cleanup step.
	local cons_tip main_tip ctrc=0 mtrc=0
	cons_tip=$(cleanup_wt_git rev-parse "$CLEANUP_WT_CONSOLIDATION_BRANCH") || ctrc=$?
	main_tip=$(cleanup_wt_git rev-parse main) || mtrc=$?
	if ((ctrc != 0)) || ((mtrc != 0)) || [[ -z $cons_tip || -z $main_tip ]]; then
		printf 'ANCESTRY_ERROR\n'
		return 2
	fi
	if [[ $cons_tip == "$main_tip" ]]; then
		printf 'NOT_ANCESTOR\n'
		return 1
	fi
	cleanup_wt_git fetch origin main >/dev/null 2>&1 || true
	cleanup_wt_git merge-base --is-ancestor "$CLEANUP_WT_CONSOLIDATION_BRANCH" main >/dev/null 2>&1 || mrc=$?
	if ((mrc == 0)); then
		printf 'MERGED_CLEAN\n'
		return 0
	elif ((mrc == 1)); then
		printf 'NOT_ANCESTOR\n'
		return 1
	fi
	printf 'ANCESTRY_ERROR\n'
	return 2
}

reverify_delete_eligible() {
	# Same-process re-verification immediately before a destructive action.
	#
	# Re-runs the classification ladder for <name> (which re-checks ancestry against
	# main, and the content-neutral/equivalence verdicts) and confirms the FRESH state
	# is still delete-eligible: MERGED_CLEAN | MERGED_CONTENT_NEUTRAL | MERGED_EQUIVALENT.
	# Any mismatch (the branch drifted, main moved, or a recorded verdict no longer
	# holds) emits ACTION|delete|<name>|BLOCKED-REVERIFY and returns 1; otherwise 0.
	#
	# Args: $1 = branch name; $2 = recorded state (advisory, for callers' context).
	local name="$1" line state="" out crc=0
	# Guarded parent-shell capture: a classify_branch hard failure (non-zero rc) maps
	# explicitly to BLOCKED-REVERIFY/return 1, in addition to the state-token allowlist.
	out=$(classify_branch "$name") || crc=$?
	if ((crc != 0)); then
		printf 'ACTION|delete|%s|BLOCKED-REVERIFY\n' "$name"
		return 1
	fi
	while IFS= read -r line; do
		[[ $line == BRANCH\|* ]] || continue
		IFS='|' read -r _ _ state <<<"$line"
		break
	done <<<"$out"
	case "$state" in
	MERGED_CLEAN | MERGED_CONTENT_NEUTRAL | MERGED_EQUIVALENT)
		return 0
		;;
	*)
		printf 'ACTION|delete|%s|BLOCKED-REVERIFY\n' "$name"
		return 1
		;;
	esac
}

remove_worktree_safe() {
	# Remove a worktree without the forced-removal flag. A dirty worktree (modified
	# tracked or any untracked files) makes `git worktree remove` fail; that blocks
	# removal — the worktree's `git -C <path> status --porcelain` output is reported as
	# DIRTY| lines and the worktree is left in place. An empty path means the branch
	# has no worktree: skip silently and return 0. The main worktree is never passed in.
	#
	# Args: $1 = worktree path (may be empty).
	# Returns 0 on successful removal or no-worktree; 1 when a dirty worktree blocks it.
	local path="$1" rc=0 line sout srrc=0
	[[ -z $path ]] && return 0
	cleanup_wt_git worktree remove "$path" >/dev/null || rc=$?
	if ((rc == 0)); then
		printf 'ACTION|worktree-remove|%s|OK\n' "$path"
		return 0
	fi
	# Guarded parent-shell capture of the diagnostic status read: a hard failure emits no
	# DIRTY lines (the removal failure is already reported) but still blocks removal.
	sout=$(cleanup_wt_git -C "$path" status --porcelain) || srrc=$?
	if ((srrc == 0)) && [[ -n $sout ]]; then
		while IFS= read -r line; do
			[[ -z $line ]] && continue
			printf 'DIRTY|%s|%s\n' "$path" "$line"
		done <<<"$sout"
	fi
	printf 'ACTION|worktree-remove|%s|BLOCKED-DIRTY\n' "$path"
	return 1
}

delete_branch() {
	# Delete a branch with `git branch -D` (not -d). -d's merge check is HEAD-relative
	# and this tool runs off-main, so -d would misfire; safety comes from the caller's
	# preceding reverify_delete_eligible re-check against main. Emits an ACTION line.
	#
	# Args: $1 = branch name.
	local name="$1" rc=0
	cleanup_wt_git branch -D "$name" >/dev/null || rc=$?
	if ((rc == 0)); then
		printf 'ACTION|branch-delete|%s|OK\n' "$name"
	else
		printf 'ACTION|branch-delete|%s|FAILED\n' "$name"
	fi
	return "$rc"
}

delete_candidate() {
	# Delete one candidate in the fixed order:
	#   0. refuse the base branch CLEANUP_WT_BASE_BRANCH: emit
	#      ACTION|delete|<name>|BLOCKED-PROTECTED-BASE and return 1 before any other
	#      step runs. The constant is defined in
	#      .claude/skills/cleanup-merged-worktrees/scripts/cleanup_worktrees_enumerate_lib.sh, which every caller sources
	#      first.
	#   1. reverify_delete_eligible (same-process ancestry/equivalence re-check),
	#   2. remove_worktree_safe (only when the candidate has a worktree),
	#   3. delete_branch (git branch -D).
	# Any step's failure stops the sequence for that candidate (a dirty worktree, for
	# example, blocks the branch deletion because git refuses to delete a branch still
	# checked out in a worktree). Prunable registrations are report-only; this tool
	# never executes worktree pruning.
	#
	# Args: $1 = branch name, $2 = worktree path (may be empty), $3 = recorded state.
	local name="$1" wt_path="$2" state="$3"
	# Refuse the base branch before re-verification, worktree removal, or branch
	# deletion. run_apply never reaches this path for the base, because compute_protected
	# already classifies it PROTECTED_CURRENT; this is the backstop for any other caller.
	if [[ $name == "$CLEANUP_WT_BASE_BRANCH" ]]; then
		printf 'ACTION|delete|%s|BLOCKED-PROTECTED-BASE\n' "$name"
		return 1
	fi
	reverify_delete_eligible "$name" "$state" || return 1
	if [[ -n $wt_path ]]; then
		# Opt-in clear-and-retry, off unless CLEANUP_WT_CLEAR_DISPOSABLE is 1. A
		# BLOCKED-DIRTY removal is retried once, and only after clear_disposable_dirt has
		# emptied a worktree whose every per-file verdict is disposable and
		# reverify_delete_eligible has re-confirmed eligibility in process. The retry calls
		# the same unforced remove_worktree_safe, so no force flag and no new removal call
		# site is introduced. Any step's failure stops the sequence and returns 1.
		if ! remove_worktree_safe "$wt_path"; then
			((CLEANUP_WT_CLEAR_DISPOSABLE == 1)) || return 1
			clear_disposable_dirt "$wt_path" || return 1
			reverify_delete_eligible "$name" "$state" || return 1
			remove_worktree_safe "$wt_path" || return 1
		fi
	fi
	delete_branch "$name"
}

run_apply() {
	# Apply-mode driver. Emits the report (WORKTREE and per-branch BRANCH/COMMIT lines)
	# and then performs deletion for delete-eligible states only. The eligible-state
	# gate is a single explicit allowlist: MERGED_CLEAN | MERGED_CONTENT_NEUTRAL |
	# MERGED_EQUIVALENT. NOT_MERGED, HAS_UNIQUE_RESIDUALS, PROTECTED_CURRENT, and
	# ANCESTRY_ERROR never trigger a destructive action. The main worktree and the base
	# branch CLEANUP_WT_BASE_BRANCH are never candidates: compute_protected protects the
	# base by name in every checkout topology, so classify_branch resolves it to
	# PROTECTED_CURRENT, and delete_candidate refuses it as a backstop. Deletion of the
	# consolidation branch (whose unique content was consolidated) is gated on
	# verify_consolidation_merged() returning MERGED_CLEAN. Returns non-zero if any
	# candidate's deletion failed or was blocked, or when the worktree listing, branch
	# enumeration, or any branch classification hard-fails (in which case no mutation is
	# performed for that failure).
	local rc=0 name record wpath wbranch wflags cb_out state
	local wlout wlrc=0 ebout ebrc=0
	# Guarded parent-shell captures up front, before check_main_freshness and any output:
	# a hard failure of the worktree listing or branch enumeration aborts apply with git's
	# exit code and performs no mutation.
	wlout=$(parse_worktree_list) || wlrc=$?
	if ((wlrc != 0)); then
		return "$wlrc"
	fi
	ebout=$(enumerate_branches) || ebrc=$?
	if ((ebrc != 0)); then
		return "$ebrc"
	fi
	check_main_freshness
	local -A wt_of=()
	while IFS= read -r record; do
		[[ -z $record ]] && continue
		IFS='|' read -r wpath _ wbranch wflags <<<"$record"
		# A detached registration is handled by apply_detached_worktrees instead: it
		# emits that registration's single WORKTREE record and owns its removal decision.
		is_detached_candidate "$wflags" && continue
		printf 'WORKTREE|%s|%s|%s\n' "$wpath" "$wbranch" "$wflags"
		[[ -n $wbranch && $wbranch != DETACHED ]] && wt_of[$wbranch]=$wpath
	done <<<"$wlout"
	apply_detached_worktrees "$wlout" || rc=1
	# Consolidation merge gate: unlock the consolidation branch's own deletion only
	# when documentationandmemories is merged into main.
	local consolidation_ok=1 vout
	if cleanup_wt_git rev-parse --verify --quiet \
		"refs/heads/$CLEANUP_WT_CONSOLIDATION_BRANCH" >/dev/null 2>&1; then
		vout=$(verify_consolidation_merged) || true
		[[ $vout == MERGED_CLEAN ]] && consolidation_ok=0
	fi
	# One shared classification pass for every branch, identical to the one report mode
	# runs, so the two modes can never disagree about a branch's state. A non-zero driver
	# return means at least one branch hard-failed; that propagates to rc here and the
	# per-branch allowlist check below still gates every individual deletion.
	local crc=0 all_out
	all_out=$(classify_all_branches) || crc=$?
	if ((crc != 0)); then
		rc=1
	fi
	while read -r name _; do
		[[ -z $name ]] && continue
		if [[ $name == "$CLEANUP_WT_CONSOLIDATION_BRANCH" ]] && ((consolidation_ok != 0)); then
			printf 'ACTION|delete|%s|BLOCKED-CONSOLIDATION-UNMERGED\n' "$name"
			continue
		fi
		# Extract this branch's own lines from the shared driver's combined output. The
		# name is the second pipe-delimited field of each of the three per-branch record
		# types, so the filter is name-scoped and cannot pick up a sibling's records.
		cb_out=$(printf '%s\n' "$all_out" | awk -F'|' -v n="$name" \
			'($1 == "BRANCH" || $1 == "CHILD_OF" || $1 == "COMMIT") && $2 == n')
		printf '%s\n' "$cb_out"
		state=$(printf '%s\n' "$cb_out" | awk -F'|' '/^BRANCH\|/{print $3; exit}')
		if [[ $state == "ANCESTRY_ERROR" ]]; then
			# A branch classification hard failure never triggers deletion (its state is an
			# error state, not on the allowlist) and propagates a non-zero driver rc.
			rc=1
			continue
		fi
		case "$state" in
		MERGED_CLEAN | MERGED_CONTENT_NEUTRAL | MERGED_EQUIVALENT)
			delete_candidate "$name" "${wt_of[$name]:-}" "$state" || rc=1
			;;
		*) : ;;
		esac
	done <<<"$ebout"
	return "$rc"
}
