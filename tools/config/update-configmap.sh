set -euo pipefail

readonly ENV_VAL=$1
readonly APP_NAME=$2
readonly OPENSHIFT_NAMESPACE=$3
readonly DYNAMICS_AUTHENTICATION_SETTINGS=$4

D365_CONFIGURATION=$(jq << JSON
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DynamicsAuthenticationSettings": $(cat "$DYNAMICS_AUTHENTICATION_SETTINGS")
}
JSON
)
readonly D365_CONFIGURATION
echo "$D365_CONFIGURATION" > /tmp/appsettings.json

echo
echo Creating D365 config map "$APP_NAME-d365api-$ENV_VAL-config-map"
oc create -n "$OPENSHIFT_NAMESPACE" configmap \
  "$APP_NAME-d365api-$ENV_VAL-config-map" \
  --from-file="appsettings.json=/tmp/appsettings.json" \
  --dry-run -o yaml | oc apply -f -

