# Redis KeyMap

Redis is fast, but its key schema is often invisible.

`redis-keymap` scans key names without reading values, normalizes dynamic segments, creates a sanitized structural snapshot, and detects key-schema drift over time.

## The problem

Redis schemas are encoded in key names and are often undocumented. Redis KeyMap turns naming hierarchy and structural patterns into reviewable artifacts; it does not infer relationships between stored values.

## Before and after

~~~text
user:123
user:123:orders
user:456:sessions
~~~

Technical tree:

~~~text
user
└─ {id}
   ├─ orders
   └─ sessions
~~~

Logical tree:

~~~text
user
├─ orders
└─ sessions
~~~

## Features

- Explicit cursor-based SCAN against one standalone Redis endpoint
- Strict UTF-8 offline input, one key per line
- Numeric, UUID, ULID, long hexadecimal, token, binary, and ordered custom normalization
- Technical/logical trees, masked examples, versioned JSON snapshots, Markdown reports
- Deterministic drift comparison and CI policy exit codes
- Cross-platform .NET tool and Native AOT release binaries

## Installation

~~~bash
dotnet tool install --global RedisKeyMap.Tool
redis-keymap --version
~~~

Release archives provide self-contained binaries for Windows x64, Linux x64/ARM64, and macOS x64/ARM64.

## Five-command quick start

~~~bash
redis-keymap analyze --input examples/sample-keys.txt --snapshot current.json --report current.md
redis-keymap render current.json --report rendered.md
redis-keymap scan --connection-env REDIS_CONNECTION --max-keys 10000 --snapshot live.json --report live.md
redis-keymap diff baseline.json live.json --report drift.md
redis-keymap check baseline.json live.json --config redis-keymap.json
~~~

> [!IMPORTANT]
> Redis KeyMap is read-only and explicitly uses `SCAN`, never `KEYS`. A full scan still consumes server and network resources. Prefer a non-production environment, use a bounded `--max-keys` sample first, and review reports before publishing them.

## Privacy

Examples are masked by default: `user:123` becomes `user:{id}`. Use `--no-examples` for none. Raw examples require `--include-raw-examples`, print a caution, and can contain sensitive identifiers. Reports can reveal namespace architecture even when examples are masked.

## Snapshot and drift workflow

Commit a reviewed baseline snapshot. Generate a current snapshot with the same normalization configuration, render `diff` for review, and use `check` in CI. Configuration fingerprints prevent misleading comparisons unless an explicit mismatch override is supplied.

## CI example

~~~yaml
- run: redis-keymap analyze --input keys.txt --snapshot current.json --report current.md
- run: redis-keymap check baseline.json current.json --config redis-keymap.json
~~~

## Configuration

~~~json
{
  "delimiter": ":",
  "normalization": {
    "customRules": [
      { "name": "tenant", "pattern": "^tenant-[0-9]+$", "replacement": "{tenant-id}", "ignoreCase": false }
    ]
  },
  "privacy": { "exampleMode": "masked", "examplesPerPattern": 3 }
}
~~~

Configuration is strict: unknown properties are rejected. Keep the same delimiter and
normalization rules when producing baseline and current snapshots so comparisons remain
meaningful.

## Normalization and snapshots

Segments are classified in deterministic order: empty, ordered custom rule, numeric ID,
UUID, ULID, long hexadecimal value, mixed token, then static text. Logical trees collapse
dynamic markers while technical trees preserve their positions.

Snapshot schema version 1 uses deterministic JSON with sorted arrays, UTC timestamps,
masked examples, source metadata, scan completeness, findings, and recommendations.
Configuration fingerprints cover the settings that affect normalization and prevent
accidental comparisons between incompatible snapshots.

## Exit codes

| Code | Meaning |
|---:|---|
| 0 | Success / policies passed |
| 1 | Operational failure |
| 2 | Usage or configuration error |
| 3 | Policy violation |
| 4 | Explicit partial result |
| 130 | Cancelled |

## SCAN limitations

COUNT is a hint. A full iteration can return duplicates, and concurrent mutation can omit or repeat keys. Limited, cancelled, or failed scans are marked incomplete. Redis Cluster and Sentinel discovery are not supported in v1; scan one standalone endpoint only.

## Development

~~~bash
dotnet restore
dotnet format --verify-no-changes
dotnet build -c Release --no-restore
dotnet test -c Release --no-build
dotnet publish src/RedisKeyMap.Cli -c Release -r win-x64 -p:PublishAot=true
~~~
