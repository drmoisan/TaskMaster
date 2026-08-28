#!/usr/bin/env bash
set -euo pipefail

greet_user() {
	local name="${1:-Demo}"
	echo "Hello, ${name}!"
}
