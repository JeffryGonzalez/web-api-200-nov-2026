# HTTP

What this codebase promises callers over HTTP.

## Errors are problem+json

**The role:** tell a caller what went wrong in a form they can rely on.

**How we cast it:** `Results.Problem` and `Results.ValidationProblem`, so every failure we
return has the same fields in it.

## A 202 says we took it, not that we did it

**The role:** answer a caller when the work will finish after the response does.

**How we cast it:** `Results.Accepted()`.

Worth knowing because **a `202` is supposed to carry a `Location`** pointing somewhere the
caller can look to find out how it went. Ours does not, and there is nothing at the other
end of such a link yet. That is a gap rather than a decision, and it is written here so
nobody assumes otherwise.
