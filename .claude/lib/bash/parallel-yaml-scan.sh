#!/usr/bin/env bash
# parallel-yaml-scan.sh: scanning half of the hand-written parser for the
# restricted block-YAML subset that a machine-authored parallel-run manifest is
# written in. It owns line splitting, frontmatter fence extraction, scalar
# lexing, and the node-table storage and accessors; the structural half that
# walks indentation and populates the table lives in parallel-yaml-emit.sh,
# which sources this file. The pair exists so the manifest validator can run in
# a destination workspace that has no Python, no yq, and no Node; it is
# deliberately NOT a general YAML implementation and fails closed on every
# construct outside the subset.
#
# The module is split across two files because the combined implementation
# exceeds the 500-line ceiling in .claude/rules/general-code-change.md.
#
# Accepted subset:
#   - block mappings `key: value` and `key:` followed by an indented block,
#   - block sequences `- scalar`, `- key: value` (compact mapping), and `-`
#     followed by an indented block,
#   - two-space indentation steps, spaces only,
#   - empty flow collections `[]` and `{}`,
#   - scalars: null (`~`, `null`, empty), the YAML 1.1 boolean words, decimal
#     integers matching `[-+]?(0|[1-9][0-9]*)`, single-quoted and double-quoted
#     strings without escape sequences, and plain strings,
#   - full-line `#` comments and blank lines.
#
# Rejected fail-closed (YP_STATUS=out_of_subset): non-empty flow collections,
# anchors, aliases, tags, block scalars (`|`, `>`), floats, unquoted dates and
# timestamps, `.inf`/`.nan`, escape sequences inside double-quoted scalars,
# trailing comments, and multi-document streams. Rejecting these is the point:
# silently mis-parsing one would produce a validator verdict that disagrees with
# the Python authority, which is worse than refusing to answer.
#
# Reported as a YAML error (YP_STATUS=yaml_error): unterminated quotes, tab
# indentation, misaligned indentation, and lines that are neither a mapping
# entry nor a sequence entry. The manifest validator renders these through the
# M1 "frontmatter is not valid YAML" message, whose text is a declared
# divergence class scoped to its prefix.
#
# Output model. A successful parse populates a node table addressed by path:
# `parallel`, `items`, `items[0]`, `items[0].blast_radius.paths[1]`. Document
# order is preserved in the indexed array YP_ORDER; the associative arrays are
# used for lookup only and are never iterated, so no hash order reaches output.
#
# shellcheck disable=SC2034
# SC2034 is disabled file-wide because this module is the storage half of a
# two-file library: the node table and the parse-status globals declared below
# are written here and read by parallel-yaml-emit.sh, parallel-items-validate.sh,
# and parallel-manifest-validate.sh, which shellcheck analyses as separate
# files and therefore cannot see the use.

# Node paths in document order. Every ordered traversal walks this array.
YP_ORDER=()

# Lexical type per path: map, seq, null, bool, int, float, or str.
declare -gA YP_TYPE=()

# Decoded scalar value per path; empty string for container nodes.
declare -gA YP_VALUE=()

# Mapping-key name per path, for nodes that came from a `key:` entry.
declare -gA YP_KEY=()

# Rendered path of the mapping that owns each key node; `<root>` at the top.
declare -gA YP_PARENT=()

# Child count per container path: sequence length or mapping key count.
declare -gA YP_COUNT=()

# Parse outcome: ok, not_a_mapping, yaml_error, or out_of_subset.
YP_STATUS="ok"

# Human-readable detail for a yaml_error or out_of_subset outcome.
YP_DETAIL=""

# Source lines produced by yp_split_lines.
YP_LINES=()

# Frontmatter body lines produced by yp_extract_frontmatter_body.
YP_BODY_LINES=()

# The single M1 error produced by a failed fence extraction.
YP_FENCE_ERROR=""

# Scratch outputs of yp_classify_scalar.
YP_SCALAR_TYPE=""
YP_SCALAR_VALUE=""

# The fence line that opens and closes a frontmatter block.
YP_FENCE="---"

yp_trim() {
	# Echo a string with leading and trailing whitespace removed.
	#
	# Args: $1 = the raw string. Mirrors Python's str.strip() for the ASCII
	# whitespace the manifest subset can contain.
	local value="$1"
	value=${value#"${value%%[![:space:]]*}"}
	value=${value%"${value##*[![:space:]]}"}
	printf '%s' "$value"
}

yp_split_lines() {
	# Populate YP_LINES by splitting text on any of the three line terminators.
	#
	# The CRLF replacement runs first so a carriage-return/line-feed pair is
	# consumed as one terminator rather than as a CR followed by an empty line,
	# matching the ordered alternation in the Python module's regex.
	#
	# Args: $1 = raw document text.
	local text="$1"
	text=${text//$'\r\n'/$'\n'}
	text=${text//$'\r'/$'\n'}
	YP_LINES=()
	local line
	# A here-string appends one newline, so a text that already ends in a
	# terminator yields the same trailing empty element Python's split does.
	while IFS= read -r line; do
		YP_LINES+=("$line")
	done <<<"$text"
}

yp_extract_frontmatter_body() {
	# Populate YP_BODY_LINES with the leading frontmatter block (invariant M1).
	#
	# Args: $1 = raw document text.
	# Returns 0 on success. On failure returns 1 and sets YP_FENCE_ERROR to the
	# single M1 error string, using the caller-supplied context prefix in $2.
	local text="$1" context="$2"
	yp_split_lines "$text"
	YP_BODY_LINES=()
	YP_FENCE_ERROR=""
	if ((${#YP_LINES[@]} == 0)) || [[ $(yp_trim "${YP_LINES[0]}") != "$YP_FENCE" ]]; then
		YP_FENCE_ERROR="$context must open with a '$YP_FENCE' frontmatter fence."
		return 1
	fi
	local index
	# Scan forward for the first closing fence: everything between the two
	# fences is the body, and no second fence means the block is unterminated.
	for ((index = 1; index < ${#YP_LINES[@]}; index++)); do
		if [[ $(yp_trim "${YP_LINES[index]}") == "$YP_FENCE" ]]; then
			local body_index
			for ((body_index = 1; body_index < index; body_index++)); do
				YP_BODY_LINES+=("${YP_LINES[body_index]}")
			done
			return 0
		fi
	done
	YP_FENCE_ERROR="$context frontmatter block is not terminated by '$YP_FENCE'."
	return 1
}

yp_reject() {
	# Record an out-of-subset rejection and return 1.
	#
	# Args: $1 = detail describing the refused construct.
	YP_STATUS="out_of_subset"
	YP_DETAIL="$1"
	return 1
}

yp_yaml_error() {
	# Record a YAML-level parse failure and return 1.
	#
	# Args: $1 = detail describing the malformed input.
	YP_STATUS="yaml_error"
	YP_DETAIL="$1"
	return 1
}

yp_classify_scalar() {
	# Classify one scalar token, setting YP_SCALAR_TYPE and YP_SCALAR_VALUE.
	#
	# Args: $1 = the raw token with surrounding whitespace already trimmed.
	# Returns 0 when the token is inside the subset, 1 otherwise, with
	# YP_STATUS and YP_DETAIL set by yp_reject or yp_yaml_error.
	local raw="$1"
	YP_SCALAR_TYPE=""
	YP_SCALAR_VALUE=""

	# Quoted forms are decided first: a quote character makes every later
	# lexical rule inapplicable, and an unterminated quote is a YAML error
	# rather than an out-of-subset construct.
	if [[ $raw == '"'* ]]; then
		if [[ ${#raw} -lt 2 || $raw != *'"' ]]; then
			yp_yaml_error "unterminated double-quoted scalar: $raw"
			return 1
		fi
		local body=${raw:1:${#raw}-2}
		local backslash=$'\\'
		if [[ $body == *"$backslash"* || $body == *'"'* ]]; then
			yp_reject "escape sequences inside double-quoted scalars are outside the subset"
			return 1
		fi
		YP_SCALAR_TYPE="str"
		YP_SCALAR_VALUE="$body"
		return 0
	fi
	if [[ $raw == "'"* ]]; then
		if [[ ${#raw} -lt 2 || $raw != *"'" ]]; then
			yp_yaml_error "unterminated single-quoted scalar: $raw"
			return 1
		fi
		local single_body=${raw:1:${#raw}-2}
		YP_SCALAR_TYPE="str"
		YP_SCALAR_VALUE="${single_body//\'\'/\'}"
		return 0
	fi

	# Structural sigils that introduce constructs this parser does not model.
	case "$raw" in
	'&'* | '*'* | '!'* | '|'* | '>'* | '?'* | '%'* | '@'* | '\`'*)
		yp_reject "value begins with the unsupported YAML indicator: $raw"
		return 1
		;;
	'[]' | '{}') ;;
	'['* | '{'*)
		yp_reject "non-empty flow collections are outside the subset: $raw"
		return 1
		;;
	esac

	# Null, then boolean, then integer: the resolver order PyYAML applies to a
	# plain scalar, restricted to the members the manifest subset admits.
	case "$raw" in
	'' | '~' | null | Null | NULL)
		YP_SCALAR_TYPE="null"
		YP_SCALAR_VALUE=""
		return 0
		;;
	true | True | TRUE | yes | Yes | YES | on | On | ON)
		YP_SCALAR_TYPE="bool"
		YP_SCALAR_VALUE="true"
		return 0
		;;
	false | False | FALSE | no | No | NO | off | Off | OFF)
		YP_SCALAR_TYPE="bool"
		YP_SCALAR_VALUE="false"
		return 0
		;;
	.inf | .Inf | .INF | -.inf | -.Inf | -.INF | .nan | .NaN | .NAN)
		yp_reject "special float values are outside the subset: $raw"
		return 1
		;;
	esac

	if [[ $raw =~ ^[-+]?(0|[1-9][0-9]*)$ ]]; then
		YP_SCALAR_TYPE="int"
		YP_SCALAR_VALUE="${raw#+}"
		return 0
	fi

	# A token that looks numeric but is not a clean decimal integer is refused
	# rather than silently demoted to a string, because PyYAML would resolve
	# many of these to int, float, or datetime.
	if [[ $raw =~ ^[-+]?[0-9][0-9_]*$ ]] ||
		[[ $raw =~ ^[-+]?0[xXoObB] ]] ||
		[[ $raw =~ ^[-+]?[0-9]*\.[0-9]* ]] ||
		[[ $raw =~ ^[-+]?[0-9]+[eE][-+]?[0-9]+$ ]] ||
		[[ $raw =~ ^[0-9]{4}-[0-9]{2}-[0-9]{2} ]]; then
		yp_reject "numeric, float, or timestamp scalar outside the subset: $raw"
		return 1
	fi

	YP_SCALAR_TYPE="str"
	YP_SCALAR_VALUE="$raw"
	return 0
}

yp_node_add() {
	# Register one node in the table and append it to the document order.
	#
	# Args: $1 = path, $2 = type, $3 = value, $4 = owning-map path or empty,
	# $5 = mapping key name or empty.
	local path="$1" type="$2" value="$3" parent="$4" key="$5"
	YP_ORDER+=("$path")
	YP_TYPE["$path"]="$type"
	YP_VALUE["$path"]="$value"
	if [[ -n $key ]]; then
		YP_KEY["$path"]="$key"
		YP_PARENT["$path"]="$parent"
	fi
	if [[ $type == map || $type == seq ]]; then
		YP_COUNT["$path"]=0
	fi
}

yp_reset() {
	# Clear the node table so a fresh parse starts from an empty state.
	YP_ORDER=()
	YP_TYPE=()
	YP_VALUE=()
	YP_KEY=()
	YP_PARENT=()
	YP_COUNT=()
	YP_STATUS="ok"
	YP_DETAIL=""
}

yp_has() {
	# Return 0 when a path exists in the node table.
	#
	# Args: $1 = path.
	[[ -n ${YP_TYPE["$1"]+set} ]]
}

yp_type_of() {
	# Echo the lexical type at a path, or `absent` when the path is unknown.
	#
	# Args: $1 = path.
	if yp_has "$1"; then
		printf '%s' "${YP_TYPE["$1"]}"
	else
		printf 'absent'
	fi
}

yp_value_of() {
	# Echo the decoded scalar value at a path, or the empty string when absent.
	#
	# Args: $1 = path.
	if yp_has "$1"; then
		printf '%s' "${YP_VALUE["$1"]}"
	fi
}

yp_count_of() {
	# Echo the child count at a container path, or 0 when the path is absent.
	#
	# Args: $1 = path.
	if [[ -n ${YP_COUNT["$1"]+set} ]]; then
		printf '%s' "${YP_COUNT["$1"]}"
	else
		printf '0'
	fi
}
