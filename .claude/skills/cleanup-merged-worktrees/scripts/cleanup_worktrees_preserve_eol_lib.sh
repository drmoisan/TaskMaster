#!/usr/bin/env bash
# cleanup_worktrees_preserve_eol_lib.sh: the line-ending and MEMORY.md index group of the
# preserve-file consolidation function set, split out of
# cleanup_worktrees_preserve_lib.sh to keep every file within the 500-line cap. The split
# is pre-authorized by the specification's placement decision, which names exactly this
# group: preserve_derive_line_ending, preserve_render_index_append, and
# preserve_index_has_entry, together with the per-record index decision that consumes
# them.
#
# Sourcing contract: this library defines functions only; it never runs work at source
# time, so the wrapper and the bats suites can source it without side effects. It MUST be
# sourced immediately BEFORE cleanup_worktrees_preserve_lib.sh, whose read-only planning
# phase calls the functions defined here.
#
# The convention used to write an index append is derived from the target file's current
# bytes at run time. The manifest's line_ending field is never read as an input to that
# decision; it is read for exactly one purpose, comparison, so that a stale advisory value
# produces a reported mismatch rather than a wrong write.

preserve_index_has_entry() {
	# Report whether the destination index already carries an entry for the given file.
	#
	# A duplicate exists when some line of the index holds a markdown link whose target -
	# the text between `](` and `)` - equals the basename of target_path. Index links in
	# this repository are bare filenames with no directory component, so the basename is
	# the whole comparison. Matching on the link target rather than on the whole line makes
	# the check independent of the description text, which the operator may have reworded.
	#
	# Args: $1 = index path, $2 = the basename to look for. Returns 0 on a duplicate.
	local index=${1-} base=${2-}
	[[ -f $index ]] || return 1
	local line target
	while IFS= read -r line || [[ -n $line ]]; do
		[[ $line == *"]("* ]] || continue
		target=${line#*](}
		target=${target%%)*}
		if [[ $target == "$base" ]]; then
			return 0
		fi
	done <"$index"
	return 1
}

preserve_line_terminator() {
	# Echo the raw bytes of the line terminator the given token selects.
	#
	# The selection lives in one place so that the writer and the tests read the same
	# answer. A token this function does not recognise falls to a line feed, which is the
	# only convention a newly tracked file in this repository can hold.
	#
	# Args: $1 = the token returned by preserve_derive_line_ending.
	case ${1:-lf} in
	crlf) printf '\r\n' ;;
	*) printf '\n' ;;
	esac
}

preserve_render_index_append() {
	# Append one index line to the destination index.
	#
	# The line's own bytes are written verbatim: no separator normalization is performed,
	# because the field is defined as the index line from the source worktree and
	# re-rendering the separator would drift from both conventions in use in this
	# repository. Only the line terminator is supplied here.
	#
	# Args: $1 = index path, $2 = the index line, $3 = the terminator token as returned by
	# preserve_derive_line_ending, $4 = `yes` when the target's final byte is not a line
	# feed and a leading terminator has to be written first.
	#
	# The terminator is selected from the token and from nothing else. An `absent` target
	# is created with a line feed, because .gitattributes guarantees a line feed for every
	# tracked path in this repository and a newly tracked file can hold no other
	# convention. A `mixed` target never reaches this function: it is refused in the
	# read-only phase.
	local index=${1-} line=${2-} token=${3:-lf} pre=${4:-no}
	local term
	term=$(
		preserve_line_terminator "$token"
		printf x
	)
	term=${term%x}
	{
		if [[ $pre == "yes" ]]; then
			printf '%s' "$term"
		fi
		printf '%s%s' "$line" "$term"
	} >>"$index"
}

preserve_derive_line_ending() {
	# Echo exactly one token describing the named file's line-ending convention:
	# `absent`, `lf`, `crlf`, or `mixed`.
	#
	# The convention is derived from the file's current bytes at run time. The manifest's
	# advisory line_ending field is never an input to this decision; it is compared against
	# the answer and never substituted for it.
	#
	# Only TERMINATED lines are counted. `read` returns non-zero on a final line that
	# carries no terminator, so the loop below does not count it, which is the correct
	# reading of the contract: an unterminated final line has no terminator to classify.
	# The separate question of whether one has to be written before an append is answered
	# by preserve_index_needs_terminator.
	#
	# A non-empty file holding no terminated line at all falls to `lf`. That case is
	# outside the specification's table, and `lf` is the safe answer because the
	# repository's .gitattributes guarantees a line feed for every tracked path, so a newly
	# terminated file can hold no other convention.
	local path=${1-}
	if [[ ! -f $path || ! -s $path ]]; then
		printf 'absent\n'
		return 0
	fi
	local line total=0 crlf=0
	while IFS= read -r line; do
		total=$((total + 1))
		if [[ $line == *$'\r' ]]; then
			crlf=$((crlf + 1))
		fi
	done <"$path"
	if ((total == 0 || crlf == 0)); then
		printf 'lf\n'
	elif ((crlf == total)); then
		printf 'crlf\n'
	else
		printf 'mixed\n'
	fi
}

preserve_index_needs_terminator() {
	# Report whether a terminator has to be written before the append, which is the case
	# when the file exists, is not empty, and its final byte is not a line feed.
	#
	# `wc -l` counts newline characters, so a file whose last line carries no terminator
	# would otherwise have the new index line concatenated onto its existing last entry.
	# The trailing `printf x` is required: command substitution strips trailing newlines,
	# so without a sentinel the line-feed case and the no-final-byte case would be
	# indistinguishable.
	#
	# Args: $1 = path. Returns 0 when a leading terminator is needed.
	local path=${1-} last
	[[ -f $path && -s $path ]] || return 1
	last=$(
		tail -c 1 -- "$path" 2>/dev/null
		printf x
	)
	last=${last%x}
	[[ $last != $'\n' ]]
}

# SC2034 is disabled for this function only. The four PRESERVE_INDEX_* variables and
# PRESERVE_EXIT are written here and read by preserve_plan_one and preserve_commit_plan in
# the sibling cleanup_worktrees_preserve_lib.sh, which shellcheck analyses as a separate
# file and therefore cannot see. They are deliberately globals rather than return values
# because a shell function returns one integer and this decision carries four fields.
# shellcheck disable=SC2034
preserve_plan_index() {
	# Decide what happens to one record's destination index. Performs no write.
	#
	# The destination index is the MEMORY.md sibling of target_path, derived from
	# target_path's own directory and never from the source's, because the index line
	# belongs to the destination namespace and re-namespacing during consolidation is
	# permitted.
	#
	# Args: $1 = target_path, $2 = the index line, $3 = the advisory line_ending value,
	# $4 = the consolidation worktree path.
	#
	# Sets PRESERVE_INDEX_ACTION (`none`, `append`, or `create`), PRESERVE_INDEX_PATH,
	# PRESERVE_INDEX_TERM, and PRESERVE_INDEX_PRE for the writing phase to consume.
	# Returns 0 to proceed with the record and 1 to refuse it outright.
	local tgt=${1-} advisory=${3-} cwt=${4-}
	local tdir=""
	[[ $tgt != */* ]] || tdir="${tgt%/*}/"
	PRESERVE_INDEX_ACTION="none"
	PRESERVE_INDEX_PATH="$cwt/${tdir}MEMORY.md"
	PRESERVE_INDEX_TERM="lf"
	PRESERVE_INDEX_PRE="no"
	local derived
	derived=$(preserve_derive_line_ending "$PRESERVE_INDEX_PATH")
	if [[ $derived != "$advisory" ]]; then
		# The advisory value disagreed with the target's own bytes. Report it and proceed
		# with the re-derived value; the advisory value never changes what is written.
		printf 'ACTION|preserve-eol|%s|ADVISORY-MISMATCH\n' "$PRESERVE_INDEX_PATH"
		printf 'cleanup-worktrees: advisory line_ending %s disagrees with the derived %s: %s\n' \
			"$advisory" "$derived" "$PRESERVE_INDEX_PATH" >&2
	fi
	if [[ $derived == "mixed" ]]; then
		# Refuse rather than normalize. There is no convention to normalize to, and a
		# majority rule would silently rewrite the operator's file.
		printf 'ACTION|preserve-index|%s|EOL-MIXED\n' "$PRESERVE_INDEX_PATH"
		printf 'cleanup-worktrees: refusing an index with mixed line endings: %s\n' \
			"$PRESERVE_INDEX_PATH" >&2
		PRESERVE_EXIT=1
		return 1
	fi
	if [[ $derived == "absent" ]]; then
		PRESERVE_INDEX_ACTION="create"
		printf 'ACTION|preserve-index|%s|CREATED\n' "$PRESERVE_INDEX_PATH"
		PRESERVE_EXIT=1
		return 0
	fi
	if preserve_index_has_entry "$PRESERVE_INDEX_PATH" "${tgt##*/}"; then
		printf 'ACTION|preserve-index|%s|SKIPPED-DUPLICATE\n' "$PRESERVE_INDEX_PATH"
		return 0
	fi
	PRESERVE_INDEX_ACTION="append"
	PRESERVE_INDEX_TERM="$derived"
	if preserve_index_needs_terminator "$PRESERVE_INDEX_PATH"; then
		PRESERVE_INDEX_PRE="yes"
	fi
	printf 'ACTION|preserve-index|%s|OK\n' "$PRESERVE_INDEX_PATH"
	return 0
}
