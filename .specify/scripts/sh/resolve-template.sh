#!/usr/bin/env bash
# POSIX-shell entry point equivalent to powershell/resolve-template.ps1.
set -o pipefail

SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)
PYTHON_BIN=""
TEMP_FILES=()
TEMP_PATH=""

die() {
    printf 'ERROR: %s\n' "$1" >&2
    exit 1
}

cleanup() {
    local f
    for f in "${TEMP_FILES[@]}"; do
        rm -f -- "$f"
    done
}
trap cleanup EXIT

make_temp() {
    TEMP_PATH=$(mktemp "${TMPDIR:-/tmp}/resolve-template.XXXXXX") || die "Failed to create temporary file"
    TEMP_FILES+=("$TEMP_PATH")
}

find_python() {
    local override="${SPECKIT_PYTHON_EXECUTABLE:-${SPECKIT_PYTHON:-}}"
    local c
    if [ -n "$override" ] && command -v "$override" >/dev/null 2>&1; then
        if "$override" --version 2>&1 | grep -q 'Python 3'; then
            PYTHON_BIN=$override
            return 0
        fi
    fi
    for c in python3 python; do
        if command -v "$c" >/dev/null 2>&1 && "$c" --version 2>&1 | grep -q 'Python 3'; then
            PYTHON_BIN=$c
            return 0
        fi
    done
    return 1
}

py_helper() {
    if [ -z "$PYTHON_BIN" ]; then
        find_python || die "Python 3 is required to resolve templates on Ubuntu"
    fi
    "$PYTHON_BIN" - "$@" <<'PYCODE'
import json
import re
import sys

mode = sys.argv[1]


def norm_priority(value):
    if isinstance(value, bool):
        return 10
    if isinstance(value, str):
        text = value.strip()
        if not re.fullmatch(r'[+-]?[0-9]+(?:_[0-9]+)*', text):
            return 10
        value = int(text.replace('_', ''))
    if isinstance(value, float):
        if not value.is_integer():
            return 10
        value = int(value)
    try:
        number = int(value)
    except (TypeError, ValueError, OverflowError):
        return 10
    return number if number >= 1 else 10


def read_json(path):
    with open(path, encoding='utf-8') as handle:
        return json.load(handle)


def print_rows(rows):
    for priority, name in sorted(rows, key=lambda item: (item[0], item[1])):
        print(f'{priority}\t{name}')


if mode == 'presets':
    data = read_json(sys.argv[2])
    if not isinstance(data, dict) or not isinstance(data.get('presets'), dict):
        sys.exit(1)
    rows = []
    for name, info in data['presets'].items():
        if not isinstance(info, dict):
            continue
        if 'enabled' in info and not bool(info['enabled']):
            continue
        if not re.fullmatch(r'[a-z0-9-]+', name):
            continue
        rows.append((norm_priority(info.get('priority', 10)), name))
    print_rows(rows)

elif mode == 'extensions':
    registry = sys.argv[2]
    rows = []
    registered = set()
    if registry:
        data = read_json(registry)
        if not isinstance(data, dict):
            sys.exit(1)
        extensions = data.get('extensions', {})
        if not isinstance(extensions, dict):
            sys.exit(1)
        for name, info in extensions.items():
            registered.add(name)
            if not isinstance(info, dict):
                continue
            if 'enabled' in info and not bool(info['enabled']):
                continue
            if not re.fullmatch(r'[a-z0-9-]+', name):
                continue
            rows.append((norm_priority(info.get('priority', 10)), name))
    for name in sys.argv[3:]:
        if re.fullmatch(r'[a-z0-9-]+', name) and name not in registered:
            rows.append((10, name))
    print_rows(rows)

elif mode == 'manifest':
    manifest_path, template_name = sys.argv[2], sys.argv[3]
    try:
        import yaml
    except ImportError:
        print('yaml_missing', file=sys.stderr)
        sys.exit(2)
    try:
        with open(manifest_path, encoding='utf-8') as handle:
            data = yaml.safe_load(handle)
        if not isinstance(data, dict):
            raise ValueError('manifest root must be a mapping')
        provides = data.get('provides')
        if not isinstance(provides, dict):
            raise ValueError('manifest missing provides section')
        templates = provides.get('templates')
        if not isinstance(templates, list) or not templates:
            raise ValueError('manifest provides missing templates')
        valid_types = ('template', 'command', 'script')
        valid_strategies = ('replace', 'prepend', 'append', 'wrap')
        for entry in templates:
            if not isinstance(entry, dict):
                raise ValueError('manifest template entries must be mappings')
            for field in ('type', 'name', 'file'):
                if field not in entry or not isinstance(entry[field], str):
                    raise ValueError('manifest template entry missing or invalid: ' + field)
            if entry['type'] not in valid_types:
                raise ValueError('invalid manifest template type')
            strategy = entry.get('strategy', 'replace')
            if not isinstance(strategy, str):
                raise ValueError('manifest template strategy must be a string')
            strategy = strategy.lower()
            if strategy not in valid_strategies:
                raise ValueError('invalid manifest template strategy')
            if entry['type'] == 'script' and strategy not in ('replace', 'wrap'):
                raise ValueError('invalid manifest script strategy')
        for entry in templates:
            if entry.get('name') == template_name and entry.get('type', 'template') == 'template':
                strategy = entry.get('strategy', 'replace').lower()
                print('found\t' + strategy + '\t' + entry.get('file', ''))
                break
        else:
            print('absent\treplace\t')
    except Exception as exc:
        print(f'manifest_invalid: {exc}', file=sys.stderr)
        sys.exit(3)

elif mode == 'json':
    name, content_path = sys.argv[2], sys.argv[3]
    with open(content_path, encoding='utf-8') as handle:
        content = handle.read()
    payload = {'TEMPLATE_NAME': name, 'TEMPLATE_CONTENT': content}
    print(json.dumps(payload, separators=(',', ':'), ensure_ascii=True))

elif mode == 'wrap':
    layer_path, core_path = sys.argv[2], sys.argv[3]
    with open(layer_path, encoding='utf-8') as handle:
        layer = handle.read()
    with open(core_path, encoding='utf-8') as handle:
        core = handle.read()
    if '{CORE_TEMPLATE}' not in layer:
        print('Wrap strategy missing {CORE_TEMPLATE} placeholder', file=sys.stderr)
        sys.exit(3)
    sys.stdout.write(layer.replace('{CORE_TEMPLATE}', core))

else:
    sys.exit(1)
PYCODE
}

find_specify_root() {
    local current
    current=$(pwd -P)
    while :; do
        if [ -d "$current/.specify" ]; then
            printf '%s\n' "$current"
            return 0
        fi
        [ "$current" = "/" ] && break
        current=$(dirname -- "$current")
    done
    return 1
}

get_repo_root() {
    local init_dir
    if [ -n "${SPECIFY_INIT_DIR:-}" ]; then
        init_dir=$SPECIFY_INIT_DIR
        case $init_dir in
            /*) ;;
            *) init_dir="$(pwd -P)/$init_dir" ;;
        esac
        if [ ! -d "$init_dir" ]; then
            die "SPECIFY_INIT_DIR does not point to an existing directory: $SPECIFY_INIT_DIR"
        fi
        init_dir=$(cd -- "$init_dir" && pwd -P)
        if [ ! -d "$init_dir/.specify" ]; then
            die "SPECIFY_INIT_DIR is not a Spec Kit project (no .specify/ directory): $init_dir"
        fi
        printf '%s\n' "$init_dir"
        return 0
    fi
    if find_specify_root; then
        return 0
    fi
    (cd -- "$SCRIPT_DIR/../../.." && pwd -P)
}

get_sorted_presets() {
    local presets_dir="$1"
    local registry="$presets_dir/.registry"
    local out d
    if [ -e "$registry" ] || [ -L "$registry" ]; then
        if [ -f "$registry" ]; then
            if out=$(py_helper presets "$registry" 2>/dev/null); then
                if [ -n "$out" ]; then
                    printf '%s\n' "$out"
                fi
                return 0
            fi
        fi
    fi
    for d in "$presets_dir"/*; do
        [ -d "$d" ] || continue
        d=${d##*/}
        if [[ $d =~ ^[a-z0-9-]+$ ]]; then
            printf '10\t%s\n' "$d"
        fi
    done | LC_ALL=C sort -t $'\t' -k1,1n -k2,2
}

get_sorted_extensions() {
    local ext_dir="$1"
    local registry="$ext_dir/.registry"
    local reg="" d
    local -a dirs=()
    if [ -e "$registry" ] || [ -L "$registry" ]; then
        if [ ! -f "$registry" ]; then
            die "Invalid extension registry $registry: not a regular file"
        fi
        reg=$registry
    fi
    for d in "$ext_dir"/*; do
        [ -d "$d" ] || continue
        dirs+=("${d##*/}")
    done
    local out
    if ! out=$(py_helper extensions "$reg" "${dirs[@]}" 2>/dev/null); then
        die "Invalid extension registry $registry"
    fi
    if [ -n "$out" ]; then
        printf '%s\n' "$out"
    fi
}

resolve_template_content() {
    local name="$1" repo_root="$2" output="$3"
    local base="$repo_root/.specify/templates"
    local override="$base/overrides/$name.md"
    if [ -e "$override" ]; then
        cp -- "$override" "$output"
        return 0
    fi

    local -a layers=() strategies=()
    local effective_base_found=0
    local presets_dir="$repo_root/.specify/presets"
    if [ -d "$presets_dir" ]; then
        local preset_list line priority preset strategy manifest manifest_file manifest_declared
        local result found candidate mf cf
        preset_list=$(get_sorted_presets "$presets_dir")
        while IFS=$'\t' read -r priority preset; do
            [ -n "$preset" ] || continue
            strategy=replace
            manifest_file=""
            manifest_declared=0
            manifest="$presets_dir/$preset/preset.yml"
            if [ -f "$manifest" ]; then
                if ! result=$(py_helper manifest "$manifest" "$name" 2>&1); then
                    local code=$?
                    if [ "$code" -eq 2 ]; then
                        die "PyYAML is required to resolve preset template composition"
                    fi
                    die "Invalid preset manifest $manifest"
                fi
                IFS=$'\t' read -r found strategy manifest_file <<< "$result"
                if [ "$found" = "found" ]; then
                    manifest_declared=1
                else
                    strategy=replace
                    manifest_file=""
                fi
            fi

            candidate=""
            if [ -n "$manifest_file" ] && [[ $manifest_file != /* && $manifest_file != *../* && $manifest_file != *..\\* ]]; then
                mf="$presets_dir/$preset/$manifest_file"
                [ -f "$mf" ] && candidate=$mf
            else
                manifest_file=""
            fi
            if [ -z "$candidate" ] && [ "$manifest_declared" -eq 0 ]; then
                cf="$presets_dir/$preset/templates/$name.md"
                [ -f "$cf" ] && candidate=$cf
                if [ -z "$candidate" ]; then
                    cf="$presets_dir/$preset/$name.md"
                    [ -f "$cf" ] && candidate=$cf
                fi
            fi
            if [ -n "$candidate" ]; then
                layers+=("$candidate")
                strategies+=("$strategy")
                if [ "$strategy" = "replace" ]; then
                    effective_base_found=1
                    break
                fi
            fi
        done <<< "$preset_list"
    fi

    local ext_dir="$repo_root/.specify/extensions"
    if [ "$effective_base_found" -eq 0 ] && [ -d "$ext_dir" ]; then
        local ext_list ext_priority ext_id candidate
        ext_list=$(get_sorted_extensions "$ext_dir") || exit $?
        while IFS=$'\t' read -r ext_priority ext_id; do
            [ -n "$ext_id" ] || continue
            candidate="$ext_dir/$ext_id/templates/$name.md"
            [ -f "$candidate" ] || candidate="$ext_dir/$ext_id/$name.md"
            if [ -f "$candidate" ]; then
                layers+=("$candidate")
                strategies+=(replace)
                effective_base_found=1
                break
            fi
        done <<< "$ext_list"
    fi

    local core="$base/$name.md"
    if [ "$effective_base_found" -eq 0 ] && [ -f "$core" ]; then
        layers+=("$core")
        strategies+=(replace)
    fi

    if [ "${#layers[@]}" -eq 0 ]; then
        return 1
    fi

    if [ "${strategies[0]}" = "replace" ]; then
        cp -- "${layers[0]}" "$output"
        return 0
    fi

    local has_composition=0 s i
    for s in "${strategies[@]}"; do
        if [ "$s" != "replace" ]; then
            has_composition=1
            break
        fi
    done
    if [ "$has_composition" -eq 0 ]; then
        cp -- "${layers[0]}" "$output"
        return 0
    fi

    local base_idx=-1
    for ((i = 0; i < ${#layers[@]}; i++)); do
        if [ "${strategies[$i]}" = "replace" ]; then
            base_idx=$i
            break
        fi
    done
    if [ "$base_idx" -lt 0 ]; then
        die "Template '$name' has composing layers but no replace base"
    fi

    local tmp_current tmp_layer tmp_new
    make_temp; tmp_current=$TEMP_PATH
    make_temp; tmp_layer=$TEMP_PATH
    make_temp; tmp_new=$TEMP_PATH
    cp -- "${layers[$base_idx]}" "$tmp_current"

    for ((i = base_idx - 1; i >= 0; i--)); do
        cp -- "${layers[$i]}" "$tmp_layer"
        case "${strategies[$i]}" in
            replace)
                cp -- "$tmp_layer" "$tmp_current"
                ;;
            prepend)
                cat -- "$tmp_layer" > "$tmp_new"
                printf '\n\n' >> "$tmp_new"
                cat -- "$tmp_current" >> "$tmp_new"
                cp -- "$tmp_new" "$tmp_current"
                ;;
            append)
                cat -- "$tmp_current" > "$tmp_new"
                printf '\n\n' >> "$tmp_new"
                cat -- "$tmp_layer" >> "$tmp_new"
                cp -- "$tmp_new" "$tmp_current"
                ;;
            wrap)
                py_helper wrap "$tmp_layer" "$tmp_current" > "$tmp_new" || die "Failed to apply wrap strategy for '$name'"
                cp -- "$tmp_new" "$tmp_current"
                ;;
            *)
                die "Unknown strategy: ${strategies[$i]}"
                ;;
        esac
    done

    cp -- "$tmp_current" "$output"
    return 0
}

TEMPLATE_NAME=""
JSON_MODE=0
HELP=0
while [ "$#" -gt 0 ]; do
    case "$1" in
        -Json|--json|-json) JSON_MODE=1 ;;
        -Help|--help|-help|-h) HELP=1 ;;
        --) shift; break ;;
        -*) die "Unknown option: $1" ;;
        *)
            if [ -n "$TEMPLATE_NAME" ]; then
                die "Unexpected argument: $1"
            fi
            TEMPLATE_NAME=$1
            ;;
    esac
    shift
done

if [ "$HELP" -eq 1 ]; then
    printf 'Usage: resolve-template.sh <template-name> [-Json]\n'
    exit 0
fi

if [ -z "$TEMPLATE_NAME" ]; then
    die "Template name is required"
fi

REPO_ROOT=$(get_repo_root) || exit $?
make_temp; CONTENT_FILE=$TEMP_PATH

if ! resolve_template_content "$TEMPLATE_NAME" "$REPO_ROOT" "$CONTENT_FILE"; then
    die "Could not resolve required $TEMPLATE_NAME from the template override stack for $REPO_ROOT"
fi

if [ "$JSON_MODE" -eq 1 ]; then
    py_helper json "$TEMPLATE_NAME" "$CONTENT_FILE" || exit $?
else
    cat -- "$CONTENT_FILE"
fi
