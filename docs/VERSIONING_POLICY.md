# Versioning Policy (RentADad.Platform)

This project follows a conservative, respect-based approach to versioning and compatibility, aligned with ADR-004.

## Principles

- Backward compatibility is preserved whenever reasonably possible.
- Breaking changes are treated as costly decisions, not routine refactors.
- Deprecation happens only when it is economically justified.
- Error formats and failure semantics remain consistent across versions.

## Semantic Versioning

Where versions are exposed, we use Semantic Versioning:

- MAJOR: breaking changes
- MINOR: additive, backward-compatible changes
- PATCH: backward-compatible fixes

## API Compatibility Rules

- Prefer additive changes (new fields, endpoints, or optional parameters).
- Avoid removing or repurposing existing fields or endpoints.
- Do not change existing behavior or error semantics without a deprecation plan.
- Error responses must retain stable shapes and error codes.

## Deprecation Process

When a change would break compatibility:

- Document the deprecated behavior and the replacement.
- Provide a reasonable deprecation window (minimum 2 minor releases).
- Remove only with clear justification and a MAJOR version bump.

## Version Exposure

- API version is exposed via the route prefix (e.g., `/api/v1`).
- The default version is the latest stable major.
- Any behavioral change must be reflected in the versioning plan.
- Responses include `X-Api-Version` and ProblemDetails `version` for clarity.

## Notes

This policy does not require immediate public guarantees for early-stage APIs,
but it does require intentional, documented evolution once consumers rely on behavior.
