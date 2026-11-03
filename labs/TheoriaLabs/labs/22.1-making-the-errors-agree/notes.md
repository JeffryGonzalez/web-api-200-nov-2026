# Notes — making the errors agree

**Forty minutes, so budget eighty.** Twelve `##` steps. The longest of the optionals and
the one that most rewards somebody who is already comfortable.

**Optional, stretch two.** With `21.1-` it gives Tuesday two things above the floor. Of
the two, give this one to somebody who has shipped an API that other teams consume — the
motivation is *a caller writing one error path*, and that lands very differently
depending on whether you have ever been that caller.

**Polad: `22-`'s finished state.** `NewWorkOrder` with its attributes, `AddValidation()`,
the `POST` endpoint, and the `GET` from `21-`.

**It does not require `21.1-`.** The lab names it once, where the binding failure comes
up, and works either way — a student who did `21.1-` finds one of the three shapes
already fixed, which is a better outcome, not a broken lab.

## The one thing that doesn't get cut

**The filter that never runs.**

Not the replacement — that is a technique and they can read it. The moment is the one
where a correct-looking filter, attached the documented way, produces no output at all
for the request it was written for. It is not subtle once seen and it is invisible
before.

If the clock is gone: run the three-shapes step, do the filter that never runs, explain
why, and stop. That is twenty minutes and it is a complete thought.

## Before: what you set up

**The belief you plant:** that an endpoint filter wraps the endpoint. It is what the name
says and what the documentation implies, and it is true right up until something else got
there first.

**Hand off on:** *"You have three error shapes. Filters wrap endpoints. This should take
ten minutes."*

## After: what you land

The mechanism is in the lab. What stays yours is the general rule, and it is worth saying
in a form that has nothing to do with ASP.NET:

> **You can only wrap something you are outside of.** Every interception mechanism has a
> position, and the useful question is never *can I hook this* — it is *where does my hook
> sit relative to the thing I want to change*.

That is the same question as middleware ordering, as an exception handler that never sees
an exception, as a decorator registered too late.

Second, shorter, and worth landing because the stretch has now shown it three times: a
record without `public`, a fix that only works in Development, and validation becoming
opt-in. **Turning something off is a change with no error message.**

## Going wrong on purpose

**Theirs, arranged:** the filter that logs nothing. Insist on the prediction — the value
is entirely in having expected two log lines.

**Yours, sixty seconds, and it is worth it:** the route-group attempt. The lab mentions
it; doing it live is better, because the room's own next idea is the one that also fails,
and watching it fail is what makes *position* rather than *syntax* the lesson.

## Where it goes wrong

- **They look for the filter's registration order.** Reasonable, and a dead end — no
  ordering of *their* filters helps, because the generated one is not one of theirs.
- **They find `DisableValidation()` early and skip the discovery.** Fine, and worth
  asking what it is for. Somebody who reaches for it before understanding why has an
  endpoint with no validation and has not noticed.
- **`Results.ValidationProblem` versus `Results.Problem`.** The first produces the
  `errors` dictionary and is what makes the output comparable to the built-in shape. If
  somebody uses `Results.Problem`, they lose per-field errors and the response gets worse
  in a way they may not spot.
- **The `is NewWorkOrder` test in a generic filter.** They should smell it. There is a
  reveal in the lab admitting it, and it is a genuinely good thing to be caught on —
  the honest arrangement is a second, non-generic filter composed after this one.
- **"Why is the built-in one outside?"** No good answer beyond *that is where the
  framework puts it*. Say so. It is not a design principle, it is a fact about this
  version, and a claim would be the way to find out if it changes.
- **Somebody applies this to the whole app.** Worth a caution: the trade is only worth it
  for a service whose error contract is consumed by people you cannot talk to.

## Casting review

**After the binding-failure step**, marked as opinion. The role is *one error shape*, and
the candidates do not compete — they cover different ground:

| | Reaches | Misses |
|---|---|---|
| **Built-in `AddValidation()`** | every validatable type, automatically | shape of the response is not yours; content rules impossible |
| **Own filter** | validation and content rules, response shape entirely yours | binding failures; and it is opt-in per endpoint |
| **`AddProblemDetails()` + middleware** | binding failures and unhandled exceptions | validation output, which is a returned result rather than a failure |

My ranking, as a ranking: **take the global one always — it is three lines and it fixes
the framework's own errors. Take the filter only when your API has consumers you cannot
phone.** For a service used by one team down the corridor, the built-in shape is fine and
the obligation to remember is not worth it.

The thing I would not do is what the lab does for teaching purposes: put shape and content
rules in one generic filter. That is fine for forty minutes and wrong for a codebase.

## Connective tissue

- **`22-`, directly.** Its question two — where a *content* rule lives — is answered here,
  and it is the reason this lab has a fourth role.
- **`21.1-`, if they did it.** It fixes the third shape, and this lab says so where the
  binding failure comes up. Together they are the complete answer and neither is complete
  alone, which is a decent argument for offering both to the same person.
- **`12.1-` on Monday.** Same species, other end of the service: a dependency's bad answer
  becoming our `500` is an error contract problem too, and the fix there is also *decide
  what the caller is told* rather than *make the error go away*.

## Topics

Extracted into the take-home by `scripts/build-takehome.sh`. **Deliberately not in
`lab.md`** — naming the mechanism up front hands the room the answer to the question
the lab exists to pose. It is useful here for saying what is coming next.

- endpoint filters
- IEndpointFilter
- route group filters
- filter ordering
- DisableValidation
- Validator.TryValidateObject
- Results.ValidationProblem
- problem details

## Verified

Verified 2026-09-02 against this lab's polad, .NET 10, `orders` 5181, Development.

| | |
|---|---|
| Endpoint filter, valid request | runs, sees `Created<WorkOrder>` |
| Endpoint filter, **validation failure** | **never runs** |
| **Route group** filter, validation failure | **never runs either** |
| Malformed body (binding failure) | no filter of any kind runs; `400`, `text/plain` |
| `.DisableValidation()` + own filter, invalid | `400`, **`application/problem+json`**, both fields named, our `title` |
| Own filter, content rule, well-formed body naming a resident | `400`, `problem+json`, `Description` error naming the match |
| Own filter, valid request | `201` |

`.DisableValidation()` exists on `RouteHandlerBuilder` in .NET 10 and compiles without a
package.

I also emitted the generated validation source to check where the response comes from.
The generator writes only type descriptions and an `AddValidation` interceptor — the
filter that returns the `400` is framework code, which is why nothing in their project
explains the ordering they just hit.

### Not guarded by a claim

Two behaviours here are version-specific and the lab is built on both: that the built-in
validation filter runs outside endpoint *and* route group filters, and that
`.DisableValidation()` turns it off per endpoint. If the framework ever moves the
validation filter inward, the first three steps of this lab stop failing and the lab needs
a new opening rather than a new number.

That is a cheap tripwire — one request, assert the filter did not run — and it is the same
shape as `check-validation-placement.sh`.
