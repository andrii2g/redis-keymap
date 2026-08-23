# Snapshot format

Snapshot schema version 1 is deterministic JSON with camel-case properties, lower-camel string enums, UTC timestamps, sorted arrays, masked examples, source metadata, scan completeness, patterns, trees, findings, and recommendations. Unknown additive v1 fields are tolerated; missing/zero/future schema versions are rejected.

Configuration fingerprints cover delimiter, ordered custom rules, collapse markers, and normalization algorithm version only.
