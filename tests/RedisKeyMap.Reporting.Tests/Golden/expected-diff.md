# Redis KeyMap Drift Report

Baseline: drift-example
Current: drift-example

## Compatibility warnings

None.

## Summary by severity/change kind

- Info: 7
- Warnings: 0
- Errors: 0

## Added patterns

- `billing:invoice:{id}`: Subject was added.
- `user:{id}:preferences`: Subject was added.

## Removed patterns

None.

## Count changes

None.

## Namespace changes

- `billing`: Subject was added.
- `user`: Count changed by 50.0%.

## Finding changes

- `RKM004:billing`: Finding introduced.
- `RKM005:billing:invoice:{id}`: Finding introduced.
- `RKM005:user:{id}:preferences`: Finding introduced.

## Policy result

Not evaluated.

## Limitations

Incomplete scans and changed normalization can make apparent drift inconclusive.
