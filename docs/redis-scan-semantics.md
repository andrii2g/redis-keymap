# Redis scan semantics

The adapter executes SCAN cursor MATCH pattern COUNT page-size until cursor zero. COUNT is a hint and empty pages can precede completion. Redis may repeat keys, so hash64 deduplication is default; exact and none modes are available. Concurrent writes can make a scan inconsistent. Limited/cancelled scans are incomplete.

v1 rejects Cluster, Sentinel, and proxy topologies and never falls back to a whole-keyspace blocking command.
