# D365 Config Map Updater

This readme serves as documentation for what secrets are used for deployment and
what their expected types are. Note that the output of the update script is a
valid JSON file, so these types should be in JSON.

| Key                              | Type     |
|----------------------------------|----------|
| DYNAMICS_AUTHENTICATION_SETTINGS | `Object` |

Each of these keys are environment specific, so make sure you update each
environment where applicable.
