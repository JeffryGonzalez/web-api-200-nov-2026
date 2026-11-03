# Notes — where parameters come from

**Twenty-five minutes, so budget fifty.** Ten `##` steps.

**Polad: its own, and it is built.** A single-service practice solution — `Orders`
under `Practice.AppHost`, no `Directory` — with a `WorkOrders` singleton holding
thirteen seeded rows across five departments, a page size of five, and no
`/work-orders` endpoint. That endpoint is the lab.

Nothing from stretch one is required. This is a stretch boundary, so the polad starts
fresh rather than continuing `12-`: carrying `Directory` and `DepartmentDirectory`
forward would put a service in the solution that this lab never uses, and an unused
service is a question to answer rather than something to teach. `31-` does the same
thing at the next boundary.

**`Page(page, department)` filters on `department is not null`, deliberately.** An
empty string is therefore a filter that matches nothing, which is the `department=`
finding in the break step. If anyone "fixes" that to `IsNullOrWhiteSpace`, the lab
loses its point.

Stretch 2, floor. Note the rename: `coverage.md` originally called this
`21-model-binding`. It is scoped to **binding only**, deliberately — see *Not verified*
at the bottom, which is the reason and is not a small one.

## The one thing that doesn't get cut

**The half of that 400 you can't see** — that a binding failure returns a bare `400`
with no `problem+json`, before any of their code runs.

Everything before it is mechanics they could get from documentation. That step is the
only moment where *my API has two kinds of error and I only designed one of them*
becomes a thing they saw.

If the clock is gone, cut *Work out where each one came from* and say it in a sentence.
Keep the venue note.

## Before: what you set up

**The belief you plant:** that binding is configuration. They will expect to declare
where things come from, because in most frameworks you do. Let them look for the
attribute that isn't there.

**Hand off on:** *"Three parameters, three different places, and you're not going to
write a single attribute. Work out what decided that."*

## After: what you land

The lab explains the binding rules and the `400`. What stays yours is the framing: the
default is doing a lot of work silently, and silence is fine right up until it makes a
decision you would not have made — `department=` being the one in this lab.

Worth saying once, after: **conventions are choices somebody else already made.** Good
ones are invisible and that is exactly what makes them hard to notice when they are
wrong for you.

## Going wrong on purpose

**Theirs, and it is the point of the lab:** `department=` returning `200` and filtering
on an empty string. Nothing errors. Nothing is red. The endpoint confidently returns
zero results for a filter nobody meant to apply, and in a real system that is a support
ticket six weeks later that reads *"search is broken sometimes."*

**Yours, thirty seconds:** move `page` into the route (`/work-orders/{page}`) and show
that the same handler now reads it from a different place with no code change. Route
wins over query. Then put it back.

## Where it goes wrong

- **They look for attributes.** Predictable and desirable. Do not rescue them for a
  couple of minutes.
- **Two complex parameters.** If someone adds a second body-bound type they get a
  runtime error, and it is a good one to hit. Only one parameter may come from the body.
- **`int page` versus `int? page`.** Somebody will ask why one is required. The type
  is the declaration; there is no other one.
- **"Where's the validation?"** The best question in the lab and it does not have a
  clean answer right now. See below. Do not improvise one.

## The demo this lab wants

**None to start.** They should meet the binder by using it.

Afterwards, the route-versus-query swap above, thirty seconds, because it makes the
precedence rule concrete in a way the reveal text cannot.

## Connective tissue

- **`venues/http.md`** said every error is `problem+json`. This lab is the first
  counter-example they meet, and it is a counter-instance *against the note* — which is
  what the note being falsifiable means in practice. Worth pointing at.
- **`12-`, if they did it.** Same shape one level up: a declaration that looks like a
  guarantee and isn't. There it was `string Name`; here it is the uniformity of the
  error contract.

## The seam into `20-`

The application lab consolidates a second intake channel, and every one of these
questions arrives with it: where does a phoned-in work order's data come from, what is
required, and what happens when a dispatcher submits a blank field.

Question one — *where would you put the check for an empty department* — is that
conversation, one day early and with no stakes.

## What this lab doesn't clarify

- **Validation.** Deliberately absent. See below.
- **`[AsParameters]`, `TryParse`, `BindAsync`.** All real, all latitude, none needed to
  make the point. Good candidates for `22-`.
- **How to make binding failures return `problem+json`.** Named as a decision in the
  venue note and not solved. It is a genuine design question and a fine tangent if the
  room wants it.

## Topics

Extracted into the take-home by `scripts/build-takehome.sh`. **Deliberately not in
`lab.md`** — naming the mechanism up front hands the room the answer to the question
the lab exists to pose. It is useful here for saying what is coming next.

- minimal APIs
- parameter binding
- query strings
- route values
- dependency injection in handlers
- binding failures
- 400 Bad Request

## Verified

Run against .NET 10, minimal APIs, on the exact parameter shapes the lab uses:

| Request | Result |
|---|---|
| required `int` absent from query | `400` |
| required `int` present but unparseable | `400` |
| optional `string?` sent empty (`department=`) | **`200`**, empty string bound |
| optional `string?` omitted | `200`, `null` |

And the body of those `400`s in Development is a `BadHttpRequestException` dump —
`Required parameter "int page" was not provided from query string` plus a stack trace —
not `problem+json`.

## Why this lab is binding only

Not a gap — a size decision. `21-` is nine steps and validation is its own lab, `22-`.
Together they are stretch two's floor, which is two labs and therefore at the cap.

Validation was briefly recorded here as blocked. It is not: `AddValidation()` works on
stock .NET 10 with no package and no MSBuild property. The reason it appeared not to
was that the probe's models were `internal` — declared without `public` — and the
source generator does not see those. That is the centrepiece of `22-` rather than a
footnote here.

- **"Where's the validation?"** is still the best question in this lab. The answer is
  *"next lab, and it is more interesting than you think."*

### Re-verified against the polad, 2026-09-01

Every row of the table above was re-run against this lab's polad on .NET 10, with the
endpoint added exactly as the lab has it. All four hold. Two details worth having:

- The Development body is `text/plain`, and the message is
  `Required parameter "int page" was not provided from query string` or
  `Failed to bind parameter "int page" from "abc"`. In Production it is a `400` with
  `Content-Length: 0` and **no `Content-Type` header at all** — which is a slightly
  stronger version of the point than "empty body."
- Students reach Development through the AppHost, which sets it. The Production shape
  is only reachable deliberately.
