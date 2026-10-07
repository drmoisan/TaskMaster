#!/usr/bin/env bash
# cleanup_worktrees_detached_lib.sh: sourceable function group for detached-HEAD
# worktree registrations in the cleanup-worktrees tool. A branch-backed worktree is
# classified by branch name through classify_branch; a detached registration has no
# branch to classify, so this library classifies it on its own HEAD SHA and reports it
# as the five-field record WORKTREE|<path>|DETACHED|<state>|<flags>. The branch-backed
# four-field record WORKTREE|<path>|<branch>|<flags> is unchanged; the detached record
# replaces it for a detached registration rather than adding a second line.
#
# Sourcing contract: this file defines functions only; it never runs work at source
# time, so the wrapper and the bats suites can source it without side effects. It MUST
# be sourced after cleanup_worktrees_enumerate_lib.sh and cleanup_worktrees_lib.sh in
# every context, and additionally after cleanup_worktrees_actions_lib.sh in any context
# that calls remove_detached_worktree or apply_detached_worktrees, because it calls
# cleanup_wt_git, parse_worktree_list, normalize_wt_path, compute_protected, the four
# classification ladder rungs (classify_ancestry, classify_content_neutral,
# classify_cherry_equivalent, classify_residual_commit), and remove_worktree_safe. Bash
# resolves function names at call time, so run_report and run_apply may call into this
# file even though .claude/skills/cleanup-merged-worktrees/scripts/cleanup-worktrees.sh sources it last.
#
# Git exit-code capture rule: every git-backed read is captured in the PARENT shell with
# `|| rc=$?` and fails closed, matching the invariant documented at
# .claude/skills/cleanup-merged-worktrees/scripts/cleanup_worktrees_actions_lib.sh:19-35. A hard git failure always maps to
# the ANCESTRY_ERROR state and a non-zero return, never to a MERGED_* verdict and never
# to a removal.
#
# Deliberate non-reuse of classify_branch: its worktree lookup matches on branch name,
# and a detached record's branch field is the literal DETACHED, so the caller's own
# detached worktree would escape PROTECTED_CURRENT. It also emits into the BRANCH|
# namespace and can fabricate SHA-keyed COMMIT| records that cherry_pick_candidates
# would misread. Protection for a detached candidate is therefore decided BY NORMALIZED
# PATH against compute_protected's protected-path| records, and only the four ladder
# rungs (which accept a bare committish) are reused.
#
# Detached state vocabulary, exactly one token per classification:
#   MERGED_CLEAN | MERGED_CONTENT_NEUTRAL | MERGED_EQUIVALENT | NOT_MERGED |
#   HAS_UNIQUE_RESIDUALS | PROTECTED_CURRENT | ANCESTRY_ERROR
# The first three are the delete-eligible allowlist. Removal is never forced and
# `git worktree prune` is never invoked by this tool.

is_detached_candidate() {
	# Return 0 iff a porcelain flag set marks a detached-HEAD removal candidate.
	#
	# The predicate is the porcelain FLAG, not the branch field: emit_record writes the
	# literal DETACHED into the branch field whenever the branch accumulator is empty
	# (.claude/skills/cleanup-merged-worktrees/scripts/cleanup_worktrees_enumerate_lib.sh:115-116), which is also true of a
	# bare-repository stanza, and a local branch literally named DETACHED would produce
	# the same branch field with no `detached` flag.
	#
	# Returns 0 iff <flags> contains the `detached` token and contains neither `main`
	# (the main worktree is never a candidate) nor `bare`. Makes no git call.
	#
	# Args: $1 = comma-separated flag set (may be empty).
	local flags=${1:-}
	case ",$flags," in
	*,main,* | *,bare,*) return 1 ;;
	esac
	case ",$flags," in
	*,detached,*) return 0 ;;
	esac
	return 1
}

classify_detached_head() {
	# Classify one detached-HEAD worktree on its own HEAD SHA and echo one state token.
	#
	# Order (load-bearing): the PROTECTED_CURRENT check runs FIRST, by normalized path
	# against compute_protected's protected-path| records, and returns BEFORE any
	# classification git call, so the caller's own detached worktree is never probed for
	# ancestry. The remaining rungs are the ladder from cleanup_worktrees_lib.sh applied
	# to the bare SHA:
	#   1. ancestry           -> MERGED_CLEAN
	#   2. content-neutral    -> MERGED_CONTENT_NEUTRAL
	#   3. cherry equivalence -> MERGED_EQUIVALENT
	#   4. rename-aware blob fallback: all residuals CONTENT_ON_MAIN -> MERGED_EQUIVALENT
	#   5. remaining unique residuals -> HAS_UNIQUE_RESIDUALS when the HEAD was partially
	#      incorporated on main (a cherry `-` line or a CONTENT_ON_MAIN residual), else
	#      NOT_MERGED.
	# No BRANCH| or COMMIT| record is emitted, and classify_branch is never called.
	#
	# Args: $1 = detached HEAD sha, $2 = worktree path.
	# Returns 0 normally; 2 on any hard git failure, which always echoes ANCESTRY_ERROR.
	local head="$1" path="$2"
	local cpout cprc=0 pline norm
	# Guarded parent-shell capture: a compute_protected hard failure must fail closed as
	# ANCESTRY_ERROR, not degrade to a weakened (empty) protected set that could let the
	# caller's own worktree reach a delete-eligible verdict.
	cpout=$(compute_protected) || cprc=$?
	if ((cprc != 0)); then
		printf 'ANCESTRY_ERROR\n'
		return 2
	fi
	norm=$(normalize_wt_path "$path")
	while IFS= read -r pline; do
		[[ $pline == protected-path\|* ]] || continue
		if [[ -n $norm && ${pline#protected-path|} == "$norm" ]]; then
			printf 'PROTECTED_CURRENT\n'
			return 0
		fi
	done <<<"$cpout"
	local v
	v=$(classify_ancestry "$head")
	case "$v" in
	MERGED_CLEAN)
		printf 'MERGED_CLEAN\n'
		return 0
		;;
	ANCESTRY_ERROR)
		printf 'ANCESTRY_ERROR\n'
		return 2
		;;
	esac
	v=$(classify_content_neutral "$head")
	case "$v" in
	MERGED_CONTENT_NEUTRAL)
		printf 'MERGED_CONTENT_NEUTRAL\n'
		return 0
		;;
	CONTENT_NEUTRAL_ERROR)
		printf 'ANCESTRY_ERROR\n'
		return 2
		;;
	esac
	local ce
	ce=$(classify_cherry_equivalent "$head")
	if [[ $ce == "CHERRY_ERROR" || $ce == "DIFF_TREE_ERROR" ]]; then
		printf 'ANCESTRY_ERROR\n'
		return 2
	fi
	if [[ $ce == "MERGED_EQUIVALENT" ]]; then
		printf 'MERGED_EQUIVALENT\n'
		return 0
	fi
	# Residual `+` commits exist; resolve each via the rename-aware blob tier.
	local sha verdict unique_count=0 content_count=0
	while IFS= read -r pline; do
		[[ $pline == RESIDUAL\ * ]] || continue
		sha=${pline#RESIDUAL }
		verdict=$(classify_residual_commit "$head" "$sha")
		if [[ $verdict == "RESIDUAL_ERROR" ]]; then
			printf 'ANCESTRY_ERROR\n'
			return 2
		fi
		if [[ $verdict == UNIQUE\|* ]]; then
			unique_count=$((unique_count + 1))
		elif [[ $verdict == "CONTENT_ON_MAIN" ]]; then
			content_count=$((content_count + 1))
		fi
	done <<<"$ce"
	if ((unique_count == 0)); then
		# Every residual was content-on-main; the HEAD is content-equivalent-merged.
		printf 'MERGED_EQUIVALENT\n'
		return 0
	fi
	# Partial-merge signal, read from the already-captured cherry verdict rather than a
	# second `git cherry` invocation whose exit code would be discarded in a pipeline.
	local minus_present=0
	[[ $ce == *MINUS_PRESENT* ]] && minus_present=1
	if ((minus_present == 1)) || ((content_count > 0)); then
		printf 'HAS_UNIQUE_RESIDUALS\n'
	else
		printf 'NOT_MERGED\n'
	fi
	return 0
}

report_detached_worktrees() {
	# Emit one five-field detached registration record per detached candidate.
	#
	# Consumes parse_worktree_list output (the caller's already-guarded capture, so a
	# worktree-list hard failure has aborted upstream before this function is reached).
	# Records are emitted in parse_worktree_list order; no sort is applied, because the
	# branch-backed emission loop this replaces is likewise porcelain-ordered.
	#
	# The record's head field must be bound in the `read`; both pre-existing emission
	# sites discard it with the `_` placeholder.
	#
	# Args: $1 = parse_worktree_list output.
	# Returns the maximum classify_detached_head return code (0, or 2 when any candidate
	# hard-failed), so the caller can fold a detached hard failure into its own rc.
	local wlout="$1" record wpath whead wflags state crc rc=0
	while IFS= read -r record; do
		[[ -z $record ]] && continue
		IFS='|' read -r wpath whead _ wflags <<<"$record"
		is_detached_candidate "$wflags" || continue
		crc=0
		state=$(classify_detached_head "$whead" "$wpath") || crc=$?
		printf 'WORKTREE|%s|DETACHED|%s|%s\n' "$wpath" "$state" "$wflags"
		if ((crc > rc)); then
			rc=$crc
		fi
	done <<<"$wlout"
	return "$rc"
}

reverify_detached_delete_eligible() {
	# Same-process re-verification immediately before a destructive action.
	#
	# Re-runs classify_detached_head in-process and confirms the FRESH state is still
	# delete-eligible: MERGED_CLEAN | MERGED_CONTENT_NEUTRAL | MERGED_EQUIVALENT. Any
	# other verdict, and any hard git failure, emits
	# ACTION|worktree-remove|<path>|BLOCKED-REVERIFY and returns 1. This is the detached
	# counterpart of reverify_delete_eligible, which re-verifies by branch name.
	#
	# Args: $1 = detached HEAD sha, $2 = worktree path.
	# Returns 0 when the fresh verdict is delete-eligible; 1 otherwise.
	local head="$1" path="$2" state crc=0
	# Guarded parent-shell capture: a hard failure maps explicitly to BLOCKED-REVERIFY
	# and return 1, in addition to the state-token allowlist below.
	state=$(classify_detached_head "$head" "$path") || crc=$?
	if ((crc != 0)); then
		printf 'ACTION|worktree-remove|%s|BLOCKED-REVERIFY\n' "$path"
		return 1
	fi
	case "$state" in
	MERGED_CLEAN | MERGED_CONTENT_NEUTRAL | MERGED_EQUIVALENT)
		return 0
		;;
	esac
	printf 'ACTION|worktree-remove|%s|BLOCKED-REVERIFY\n' "$path"
	return 1
}

remove_detached_worktree() {
	# Remove one delete-eligible detached worktree, in a load-bearing order:
	#   1. locked   -> emit ACTION|worktree-remove|<path>|BLOCKED-LOCKED, invoke NO git
	#                  command, return 1.
	#   2. prunable -> report-only: return 0 with no output and no git command. This tool
	#                  never executes worktree pruning.
	#   3. re-verify with reverify_detached_delete_eligible; return 1 on a flipped verdict.
	#   4. remove_worktree_safe "<path>" and propagate its return.
	# Steps 1 and 2 are pure string tests over the already-read porcelain flags and MUST
	# precede every git invocation, so "no removal was attempted" is observable as an
	# empty argv log rather than inferred from a report line.
	#
	# remove_worktree_safe is consumed as an opaque contract and is not modified: it owns
	# the non-forced `git worktree remove`, the DIRTY| lines, and the BLOCKED-DIRTY result
	# token. This function emits no DIRTY| line of its own. The forced-removal flag is
	# never passed.
	#
	# Args: $1 = worktree path, $2 = detached HEAD sha, $3 = porcelain flag set.
	# Returns 0 on successful removal or a prunable skip; 1 when blocked.
	local path="$1" head="$2" flags="$3"
	case ",$flags," in
	*,locked,*)
		printf 'ACTION|worktree-remove|%s|BLOCKED-LOCKED\n' "$path"
		return 1
		;;
	esac
	case ",$flags," in
	*,prunable,*)
		return 0
		;;
	esac
	reverify_detached_delete_eligible "$head" "$path" || return 1
	remove_worktree_safe "$path"
}

apply_detached_worktrees() {
	# Apply-mode driver half for detached registrations: classify, report, then remove
	# the delete-eligible ones.
	#
	# For each detached candidate it classifies once, emits
	# WORKTREE|<path>|DETACHED|<state>|<flags> from THAT verdict before deciding removal,
	# and only then acts. The record emission is part of this function's contract, not an
	# optional extra: the apply-mode emission loop in cleanup_worktrees_actions_lib.sh
	# skips detached candidates, so this is the only producer of a detached registration
	# record in apply mode and each mode therefore emits exactly one record per detached
	# registration. classify_detached_head is not called a second time for the record.
	# The separate re-classification inside reverify_detached_delete_eligible is the
	# same-process re-verification gate, not a duplicate of this reporting call.
	#
	# A hard failure sets rc=1 and performs no removal. A delete-eligible verdict calls
	# remove_detached_worktree and sets rc=1 if it fails. Any other state performs no
	# removal and emits no further line.
	#
	# Args: $1 = parse_worktree_list output.
	# Returns 0 when every candidate was handled without failure or block; 1 otherwise.
	local wlout="$1" record wpath whead wflags state crc rc=0
	while IFS= read -r record; do
		[[ -z $record ]] && continue
		IFS='|' read -r wpath whead _ wflags <<<"$record"
		is_detached_candidate "$wflags" || continue
		crc=0
		state=$(classify_detached_head "$whead" "$wpath") || crc=$?
		printf 'WORKTREE|%s|DETACHED|%s|%s\n' "$wpath" "$state" "$wflags"
		if ((crc != 0)); then
			# A hard classification failure never triggers a removal; its state is an
			# error state, not on the delete-eligible allowlist.
			rc=1
			continue
		fi
		case "$state" in
		MERGED_CLEAN | MERGED_CONTENT_NEUTRAL | MERGED_EQUIVALENT)
			remove_detached_worktree "$wpath" "$whead" "$wflags" || rc=1
			;;
		*) : ;;
		esac
	done <<<"$wlout"
	return "$rc"
}
