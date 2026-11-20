#!/bin/sh
set -eu

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
REPO_ROOT=$(CDPATH= cd -- "$SCRIPT_DIR/../.." && pwd)

PROJECT_PATH="$REPO_ROOT/app/backend/src/RealtorApi/RealtorApi.csproj"
OUTPUT_PATH="$REPO_ROOT/app/backend/src/RealtorApi/wwwroot/openapi/v1.json"
REDOCLY_CONFIG_PATH="$REPO_ROOT/.redocly.yaml"
NSWAG_CONFIG_PATH="$REPO_ROOT/support/scripts/openapi-v1.nswag.json"
CLIENT_OUTPUT_DIR="$REPO_ROOT/artifacts/openapi-client-smoke"
CLIENT_OUTPUT_FILE="$CLIENT_OUTPUT_DIR/RealtorApiClient.cs"

fail() {
    printf '%s\n' "$1" >&2
    exit 1
}

assert_command_available() {
    command -v "$1" >/dev/null 2>&1 || fail "$2"
}

cd "$REPO_ROOT"

assert_command_available dotnet '❌ Prerrequisito faltante: dotnet no está disponible en PATH.'
assert_command_available node '❌ Prerrequisito faltante: node no está disponible en PATH.'
assert_command_available npx '❌ Prerrequisito faltante: npx no está disponible en PATH.'

[ -f "$REDOCLY_CONFIG_PATH" ] || fail "❌ Prerrequisito faltante: no existe .redocly.yaml en $REDOCLY_CONFIG_PATH"
[ -f "$NSWAG_CONFIG_PATH" ] || fail "❌ Prerrequisito faltante: no existe configuración NSwag en $NSWAG_CONFIG_PATH"

printf '%s\n' '🔨 [1/4] Compilando RealtorApi...'
dotnet build "$PROJECT_PATH" --tl:off

[ -f "$OUTPUT_PATH" ] || fail "❌ No se generó OpenAPI en: $OUTPUT_PATH"
printf '%s\n' '✅ Documento OpenAPI generado'

printf '\n%s\n' '🔍 [2/4] Validando con Redocly...'
npx --yes @redocly/cli lint --config "$REDOCLY_CONFIG_PATH" "$OUTPUT_PATH"
printf '%s\n' '✅ Validación Redocly exitosa'

printf '\n%s\n' '🔧 [3/4] Generando cliente tipado con NSwag...'
mkdir -p "$CLIENT_OUTPUT_DIR"
rm -f "$CLIENT_OUTPUT_FILE"

dotnet tool restore || fail '❌ dotnet tool restore falló. Revise la salida previa.'
dotnet tool run nswag run "$NSWAG_CONFIG_PATH" || fail '❌ NSwag falló al generar el cliente smoke. Revise la salida previa.'

[ -f "$CLIENT_OUTPUT_FILE" ] || fail "❌ NSwag no generó el cliente tipado esperado en: $CLIENT_OUTPUT_FILE"
printf '%s\n' "✅ Cliente tipado generado: $CLIENT_OUTPUT_FILE"

printf '\n%s\n' '✅ [4/4] OpenAPI v1 completamente regenerado y validado'
printf '%s\n' "   📄 Documento: $OUTPUT_PATH"
printf '%s\n' '   🔐 Validación: Redocly + NSwag pasadas'
