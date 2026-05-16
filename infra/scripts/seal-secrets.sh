#!/usr/bin/env bash
set -e

INPUT_DIR="bootstrap/dev/plain-secrets"
OUTPUT_DIR="bootstrap/dev/sealed-secrets"
NAMESPACE="quizzer-dev"
CONTROLLER_NAME="sealed-secrets-controller"
CONTROLLER_NAMESPACE="quizzer-dev"

while [[ "$#" -gt 0 ]]; do
    case $1 in
        -InputDir|--input-dir) INPUT_DIR="$2"; shift ;;
        -OutputDir|--output-dir) OUTPUT_DIR="$2"; shift ;;
        -Namespace|--namespace) NAMESPACE="$2"; shift ;;
        -ControllerName|--controller-name) CONTROLLER_NAME="$2"; shift ;;
        -ControllerNamespace|--controller-namespace) CONTROLLER_NAMESPACE="$2"; shift ;;
        *) echo "Error: Unknown parameter passed: $1" >&2; exit 1 ;;
    esac
    shift
done

if ! command -v kubeseal &> /dev/null; then
    echo "Error: kubeseal was not found on PATH. Install kubeseal before sealing secrets." >&2
    exit 1
fi

if [ ! -d "$INPUT_DIR" ]; then
    echo "Error: Input directory '$INPUT_DIR' does not exist." >&2
    exit 1
fi

mkdir -p "$OUTPUT_DIR"

shopt -s nullglob
secret_files=("$INPUT_DIR"/*.yaml "$INPUT_DIR"/*.yml)

if [ ${#secret_files[@]} -eq 0 ]; then
    echo "Error: No YAML Secret files found in '$INPUT_DIR'." >&2
    exit 1
fi

for file in "${secret_files[@]}"; do
    base_name=$(basename "$file")
    name_no_ext="${base_name%.*}"
    output_file="$OUTPUT_DIR/${name_no_ext}.sealed.yaml"

    kubeseal \
        --format yaml \
        --scope strict \
        --namespace "$NAMESPACE" \
        --controller-name "$CONTROLLER_NAME" \
        --controller-namespace "$CONTROLLER_NAMESPACE" \
        < "$file" > "$output_file"

    echo "Wrote $output_file"
done
