# Notes — catching a bad parameter yourself

**Thirty minutes, so budget an hour** — but see below, because this one is unusually
cuttable and is designed to be handed out rather than run.

**Optional, and offered rather than pushed.** The decimal says so. Stretch two's floor is
`21-` and `22-`, which is the cap. This is the lab to hand somebody who finishes early,
or to give the room at a break with *"if you want one alternative to that open question
from this morning, here it is."*

**Polad: `21-`'s finished state** — identical to `22-`'s, since both continue the same
lab. `Orders` with the `GET /work-orders` endpoint, the `WorkOrders` singleton, and the
binding venue note the student wrote at the end of `21-`.

**It is genuinely standalone-ish.** A student who did `21-` and nothing else can do this
at a break without you in the room, which is the point of it existing.

## The one thing that doesn't get cut

**`AddProblemDetails()` in Production**, with a prediction before it.

Not the custom type — the custom type is a technique, and a good one, and they can read
it. The irreducible moment is the one where a fix they just watched work stops working
because they changed an environment variable and nothing else.

If you have ten minutes with somebody, do candidate two and skip candidate one entirely.
It stands alone and it is the better half.

## Where the closing abstraction went, and why it is arguable

The student writes the venue note at the end, so that is the closing abstraction and it
sits at the bottom — the prescribed-lab position, per `authoring-guide.md`.

**By its use it is closer to a menu lab**, and the guide puts the abstraction at the top
for those, so somebody choosing knows what they are choosing. The compromise here is a
third paragraph in the overview that says what the lab answers and roughly how long it
takes, without giving away either candidate.

If it turns out people are picking this up cold and bouncing off it, moving the
abstraction to the top is the fix, and it is a five-minute edit. Worth watching the first
time somebody takes it at a break.

## Before: what you set up

**The belief you plant, if you are handing it out:** nothing. Say *"there are two ways,
one of them is one line."* That is true, and it is the trap, and they will find it
themselves.

**If you are running it in the room:** plant that `AddProblemDetails()` is the obvious
right answer. It is the one they would find by searching, it is the one that appears to
work, and the lab's second half only pays if they believed the first half.

## After: what you land

The technique is theirs from the lab. What stays yours:

> Development is not a preview of production. It is a different program, and some of the
> differences are the ones that make broken things look fine.

That generalises past problem details and is the actual reason this lab is worth half an
hour. If you want a second sentence: **the environment that shows you the most detail is
the one least like the one your users get.**

## Going wrong on purpose

**Theirs, arranged, twice.** The `return false` break shows that `return true` was the
whole mechanism rather than a detail. The Production run shows the one-line fix is not
one.

**Yours, thirty seconds, if somebody asks why:** show that the Development body was coming
from `DeveloperExceptionPageMiddleware` — remove `AddProblemDetails()` and it is
`text/plain` with a stack trace; add it back and the *same middleware* renders
problem+json. Nothing else changed. That is the whole mechanism and it makes the
Production result obvious in hindsight.

## Where it goes wrong

- **Optional parameter ordering.** `PageNumber? page` is nullable with **no default**,
  and that is deliberate — a default of `= null` forces every parameter after it to have
  one too, and `WorkOrders orders` cannot. Somebody will add `= null` out of habit and
  hit a compiler error that does not explain itself well.
- **They put `AddProblemDetails()` after `builder.Build()`.** It is a service
  registration; the middleware pair goes after. Easy, and the error is clear.
- **`page=0` and `page=-4`.** Both bind fine and both return the first page, because
  `Skip` treats a negative count as zero. Not this lab's problem, explicitly not on the
  roles list, and a good question if it comes up — it is a *content* rule, which is
  `22-`'s second question.
- **"Why not just use `string? page` and parse it myself?"** Completely reasonable, and
  it is worth saying so out loud. It is the same trade with less ceremony and no reuse.
  The custom type earns its keep when there are four such parameters, not one.
- **Somebody finds `[FromQuery]`.** Unrelated — that changes *where* a value comes from,
  not what happens when it will not parse. Worth naming so it does not become a detour.

## Casting review

**After both candidates.** Two ways to fill one role, and they do not compete as cleanly
as they first look:

| | Fixes the shape | Can change the message | Tells absent from bad | Cost |
|---|---|---|---|---|
| `AddProblemDetails` + middleware | every framework error at once | no | no | changes every error response in the service |
| Custom `TryParse` type | one parameter | yes | yes | a check every handler must remember |

My ranking, said as a ranking: **do the global one always, and reach for the custom type
only when the distinction between absent and unparseable is a distinction your callers
actually care about.** Most of the time it is not, and a bare `400` that at least has a
body is fine.

The failure mode of the other order is a codebase with fourteen bespoke parameter types
and no consistent error shape, which is more work and a worse result.

Mark it as opinion. Their team may have a house `ProblemDetails` factory that settles it.

## Connective tissue

- **`21-`, directly.** This answers its last question one, and the venue note it produces
  is an addendum to the note that lab produced rather than a replacement.
- **`22-`.** Its validation failures come back as `application/json` with an `errors`
  dictionary — a *third* error shape, and not problem+json either. If somebody has done
  both, the honest summary is that this service has three error shapes and has now fixed
  one of them.
- **`32-`, if they get there.** `BackgroundServiceExceptionBehavior.Ignore` is the same
  species: correct-looking advice, applied faithfully, producing a worse outcome that
  nothing reports.

## Topics

Extracted into the take-home by `scripts/build-takehome.sh`. **Deliberately not in
`lab.md`** — naming the mechanism up front hands the room the answer to the question
the lab exists to pose. It is useful here for saying what is coming next.

- custom TryParse binding
- IParsable
- generic constraints
- static abstract members
- AddProblemDetails
- UseExceptionHandler
- UseStatusCodePages
- Development vs Production

## Verified

Verified 2026-09-02 against this lab's polad, .NET 10, `Orders` on 5181.

**Candidate one**, `PageNumber` and the generic `OptionalQuery<int>` — identical results:

| Request | Result |
|---|---|
| no `page` | `200`, first page. The handler's `?? 1` |
| `?page=2` | `200`, second page |
| `?page=abc` | `400`, `application/problem+json`, our title and detail |
| `?page=` | `400`, same. The empty-string case `21-` leaves open |
| `TryParse` returning `false` | bare `400` returns. The mechanism confirmed |

`OptionalQuery<T>` binds on the closed generic with the three-argument
`IParsable<T>`-shaped `TryParse`. Confirmed separately with `int` and `decimal` as two
parameters on one handler.

**Candidate two**, plain `int page`, `?page=abc`:

| | Development | Production |
|---|---|---|
| nothing added | `text/plain` + stack trace | **no content-type, empty body** |
| `AddProblemDetails()` only | **`application/problem+json`** | **no content-type, empty body** |
| `AddProblemDetails()` + both middleware | `application/problem+json` | `application/problem+json` |

The middle row is the lab. **It is environment-dependent, which is worse than either
simple story**, and it is why this is a lab rather than a footnote in `21-`.

### I nearly got this wrong

My first pass concluded `AddProblemDetails()` made no difference at all, because the
script stripping it out for the control run never matched the line it was meant to
remove — so all four "different" configurations were the same configuration. The result
was consistent, plausible, and meaningless.

Worth recording for the same reason `22-`'s note is: **a probe that changes nothing
produces clean results.** The tell was that all four rows agreed, which should have been
suspicious rather than reassuring.

### Not guarded by a claim

The Development-versus-Production difference is a platform behaviour that could change,
and the lab's whole second half depends on it. There is no tripwire in `claims/` for it.
Cheapest version: boot `Orders` twice under both environments, curl `?page=abc`, assert
the content types differ. Worth adding before this runs in front of anyone twice.

### Provenance

From `courses/web-api-200/cart/demo-query-string.cs` — Jeff's working-through of the
custom-`TryParse` idea, which already had `AddProblemDetails()`, `UseExceptionHandler()`
and `UseStatusCodePages()` at the top. The three-state behaviour is his design and it is
better than it looks: two nullables carrying two different questions.

The generic form answers the question in his comment. `IParsable<T>` rather than the
generic-math interfaces he was reaching for — the requirement is parsing, not arithmetic.
