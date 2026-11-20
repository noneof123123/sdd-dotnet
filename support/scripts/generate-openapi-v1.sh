#!/bin/sh
set -eu

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
REPO_ROOT=$(CDPATH= cd -- "$SCRIPT_DIR/../.." && pwd)

SOLUTION_PATH="app/Realtor.sln"
OUTPUT_PATH="app/backend/src/RealtorApi/wwwroot/openapi/v1.json"
REDOCLY_CONFIG_PATH=".redocly.yaml"
NSWAG_CONFIG_PATH="support/scripts/openapi-v1.nswag.json"
CLIENT_OUTPUT_DIR="artifacts/openapi-client-smoke"
CLIENT_OUTPUT_FILE="$CLIENT_OUTPUT_DIR/RealtorApiClient.cs"

fail() {
    printf '[ERROR] %s\n' "$1" >&2
    exit 1
}

assert_command_available() {
    command -v "$1" >/dev/null 2>&1 || fail "Prerrequisito faltante: $1 no está disponible en PATH."
}

cd "$REPO_ROOT"

assert_command_available dotnet
assert_command_available node
assert_command_available npx

[ -f "$REDOCLY_CONFIG_PATH" ] || fail "Prerrequisito faltante: no existe $REDOCLY_CONFIG_PATH"
[ -f "$NSWAG_CONFIG_PATH" ] || fail "Prerrequisito faltante: no existe $NSWAG_CONFIG_PATH"

printf '%s\n' '[1/5] Compilando la solución y generando OpenAPI v1...'
dotnet build "$SOLUTION_PATH" --tl:off || fail "build y generación de OpenAPI v1"
printf '[OK] build y generación de %s\n' "$OUTPUT_PATH"

[ -s "$OUTPUT_PATH" ] || fail "No se generó o está vacío: $OUTPUT_PATH"
printf '[OK] archivo generado: %s\n' "$OUTPUT_PATH"

printf '\n%s\n' '[2/5] Validando con Redocly...'
npx --yes @redocly/cli lint --config "$REDOCLY_CONFIG_PATH" "$OUTPUT_PATH" || fail "redocly lint"
printf '%s\n' '[OK] redocly lint'

printf '\n%s\n' '[3/5] Restaurando herramientas locales...'
dotnet tool restore || fail "dotnet tool restore"
printf '%s\n' '[OK] dotnet tool restore'

printf '\n%s\n' '[4/5] Generando cliente tipado con NSwag...'
mkdir -p "$CLIENT_OUTPUT_DIR"
rm -f "$CLIENT_OUTPUT_FILE"
dotnet tool run nswag run "$NSWAG_CONFIG_PATH" || fail "nswag smoke client"
[ -f "$CLIENT_OUTPUT_FILE" ] || fail "NSwag no generó el cliente tipado esperado en: $CLIENT_OUTPUT_FILE"
printf '[OK] nswag smoke client: %s\n' "$CLIENT_OUTPUT_FILE"

printf '\n%s\n' '[5/5] Verificando ausencia de artefactos visuales...'
if [ -d "$CLIENT_OUTPUT_DIR" ] && [ -n "$(find "$CLIENT_OUTPUT_DIR" -type f ! -name 'RealtorApiClient.cs' -print -quit)" ]; then
    fail "artifacts/openapi-client-smoke contiene archivos inesperados"
fi
printf '%s\n' '[OK] sin artefactos visuales'

printf '\n%s\n' 'OK: OpenAPI v1 regenerado y validado'
printf '%s\n' "Documento: $OUTPUT_PATH"
