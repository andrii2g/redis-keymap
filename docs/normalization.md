 Normalization

Precedence: empty, ordered custom rule, numeric ID, canonical UUID, uppercase Crockford ULID, 12+ hexadecimal, 16–256 ASCII mixed token, then static. Static casing is preserved. Custom replacements must be lowercase marker forms such as {tenant-id}; regexes have a 100 ms timeout.

Logical trees collapse configured dynamic markers. If all segments collapse, the first marker remains. Ambiguous numeric/hex/token recognition is heuristic and produces findings.
