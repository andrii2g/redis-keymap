# Redis KeyMap Report

Generated: 2026-01-01T00:00:00.0000000+00:00  
Tool version: 0.1.0

> [!IMPORTANT]
> This report contains key-name structure only; Redis values were not read. Masked reports can still reveal architecture.

## Summary

| Metric | Value |
|---|---:|
| Accepted unique keys | 15 |
| Unique normalized patterns | 12 |
| Top-level namespaces | 5 |
| Maximum key depth | 3 |
| Duplicate observations ignored | 0 |
| Complete scan | Yes |

## Scan metadata

- Observed items: 15
- Duration: 0 ms
- Duplicate handling: Hash64

## Top-level namespaces

| Namespace | Count |
|---|---:|
| user | 7 |
| product | 3 |
| cache | 2 |
| job | 2 |
| session | 1 |

## Technical key hierarchy

```text
cache  (2)
├─ homepage  (1)
└─ product  (1)
   └─ {id}  (1)

job  (2)
└─ {uuid}  (2)
   ├─ result  (1)
   └─ state  (1)

product  (3)
└─ {id}  (3)
   ├─ price  (1)
   └─ stock  (1)

session  (1)
└─ {token}  (1)

user  (7)
└─ {id}  (7)
   ├─ orders  (2)
   ├─ sessions  (1)
   └─ tokens  (2)
```


## Simplified logical hierarchy

```text
cache  (2)
├─ homepage  (1)
└─ product  (1)

job  (2)
├─ result  (1)
└─ state  (1)

product  (3)
├─ price  (1)
└─ stock  (1)

session  (1)

user  (7)
├─ orders  (2)
├─ sessions  (1)
└─ tokens  (2)
```

## Normalized patterns

| Pattern | Count | Examples |
|---|---:|---|
| `user:{id}` | 2 | `user:{id}` |
| `user:{id}:orders` | 2 | `user:{id}:orders` |
| `user:{id}:tokens` | 2 | `user:{id}:tokens` |
| `cache:homepage` | 1 | `cache:homepage` |
| `cache:product:{id}` | 1 | `cache:product:{id}` |
| `job:{uuid}:result` | 1 | `job:{uuid}:result` |
| `job:{uuid}:state` | 1 | `job:{uuid}:state` |
| `product:{id}` | 1 | `product:{id}` |
| `product:{id}:price` | 1 | `product:{id}:price` |
| `product:{id}:stock` | 1 | `product:{id}:stock` |
| `session:{token}` | 1 | `session:{token}` |
| `user:{id}:sessions` | 1 | `user:{id}:sessions` |

## Findings

- **RKM004** (Info): Namespace contains exactly one accepted key. `session`
- **RKM005** (Info): Pattern contains heuristic dynamic segments; confirm normalization. `cache:product:{id}`
- **RKM005** (Info): Pattern contains heuristic dynamic segments; confirm normalization. `product:{id}`
- **RKM005** (Info): Pattern contains heuristic dynamic segments; confirm normalization. `product:{id}:price`
- **RKM005** (Info): Pattern contains heuristic dynamic segments; confirm normalization. `product:{id}:stock`
- **RKM005** (Info): Pattern contains heuristic dynamic segments; confirm normalization. `session:{token}`
- **RKM005** (Info): Pattern contains heuristic dynamic segments; confirm normalization. `user:{id}`
- **RKM005** (Info): Pattern contains heuristic dynamic segments; confirm normalization. `user:{id}:orders`
- **RKM005** (Info): Pattern contains heuristic dynamic segments; confirm normalization. `user:{id}:sessions`
- **RKM005** (Info): Pattern contains heuristic dynamic segments; confirm normalization. `user:{id}:tokens`
- **RKM009** (Info): Namespace contains direct entity keys; document the stored base entity meaning. `product:{id}`
- **RKM009** (Info): Namespace contains direct entity keys; document the stored base entity meaning. `user:{id}`

## Recommendations

- Document the principal Redis key patterns in the owning application repository.
- Prefer a consistent key form such as entity:{id}:resource.
- Review this snapshot before Redis cleanup or naming refactors.

## Method and limitations

Redis KeyMap discovers naming hierarchy and structural patterns only. Live scans use cursor iteration; concurrent mutation can make counts approximate, and incomplete scans cannot prove removals.
