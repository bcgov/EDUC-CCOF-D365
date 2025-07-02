set -euo pipefail

readonly ENV_VAL=$1
readonly APP_NAME=$2
readonly OPENSHIFT_NAMESPACE=$3
readonly DYNAMICS_AUTHENTICATION_SETTINGS=$4
readonly D365_API_KEY_SCHEME=$5

D365_CONFIGURATION=$(jq << JSON
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AuthenticationSettings": {
    "Schemes": {
      "ApiKeyScheme": $(cat "$D365_API_KEY_SCHEME")
    }
  },
  "AppSettings": {
    "PageSize": 50,
    "MaxPageSize": 2000,
    "RetryEnabled": true,
    "MaxRetries": 5,
    "AutoRetryDelay": "00:00:08",
    "MinsToCache": 60
  },
  "D365AuthSettings": $(cat "$DYNAMICS_AUTHENTICATION_SETTINGS")
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

