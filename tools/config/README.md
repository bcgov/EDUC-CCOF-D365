# D365 Config Map Updater

This readme serves as documentation for what secrets are used for deployment and
what their expected types are. Note that the output of the update script is a
valid JSON file, so these types should be in JSON.

| Key                              | Type     | REMARKS |
|----------------------------------|----------|
| DYNAMICS_AUTHENTICATION_SETTINGS | `Object` | NOT IN USE AFTER CHANGE IN API AUTHENTICATION SCHEME EFFECTIVE |
| D365_AUTHENTICATION_SETTINGS| `Object` | REPLACEMENT OF DYNAMICS_AUTHENTICATION_SETTINGS |
| D365_API_KEY_SCHEME | `Object` | STORES THE API KEYS FOR PORTAL AND CRM |

Each of these keys are environment specific, so make sure you update each
environment where applicable.
