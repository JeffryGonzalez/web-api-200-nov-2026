# Notes — when the whole response is wrong

**Thirty minutes, so budget an hour.** Eleven `##` steps.

**Optional, stretch one.** The first thing on Monday for somebody who finished `12-`
early, and until this existed there was nothing — stretch one's floor was two labs with
no latitude above it at all, on the day the spread in the room is widest.

**Polad: `12-`'s finished state**, with one addition. `Department` is
`(string Code, string? Name, string? Contact)` — the nullable landing `12-` argues for.
`Directory` now takes `POST /mode/{value}` so it can be told to answer badly, and the
venue note says plainly that nothing outside a practice repo should have that endpoint.

## The one thing that doesn't get cut

**The prediction about `?? []`.** *"Under what circumstances is `result` null?"* — asked
before anything is run, written down.

Almost everyone answers *when the response is empty*, or *when something went wrong*.
The real answer is *only when the other service sends the literal JSON `null`*, which
nobody guesses, and the gap between those is the lab.

Everything after it is consequence. If the clock is gone, ask the question, run the four
modes, and stop before the fix.

## Before: what you set up

**The belief you plant:** that `?? []` is defensive hygiene. It reads as hygiene, it was
pasted as hygiene two labs ago, and nobody has looked at it since. Do not draw attention
to it before the prediction step — the line has to have been invisible for the reveal to
land.

**Hand off on:** *"Nothing new to build in this one. You are going to find out what the
code you already have does when the other end has a bad day."*

## After: what you land

The lab covers the mechanism. What stays yours is the general form, and it is worth
saying slowly:

> **Wherever a failure and a legitimate empty answer end up in the same variable,
> something downstream is going to believe the empty one.**

That is not about JSON. It is about `FirstOrDefault`, about a cache miss that looks like
a cache hit of nothing, about a repository returning an empty list when the database was
unreachable. Once somebody has the shape they start finding it in their own code, which
is the point.

Second thing, shorter: **`500` is a claim about you.** Most services return one for a
dependency's failure and most of them should not.

## Going wrong on purpose

**Theirs, arranged:** four predictions, then four runs. The `null` mode is the one that
matters and it should be run last, because the three `500`s set up an expectation that
breakage is loud.

**Yours, thirty seconds, after the fix:** run `emptylist` and `null` back to back on the
*unfixed* code and show the two responses are byte-identical. It makes "these must not
share a variable" concrete in a way the prose does not.

## Where it goes wrong

- **They fix it by catching `Exception`.** Tempting, and it swallows the cancellation
  from a client that hung up, plus anything genuinely wrong in their own code. Worth
  catching in review rather than pre-empting — it is a better conversation once written.
- **`using System.Text.Json;`** is missing from `DepartmentDirectory.cs` and the error is
  clear, but it interrupts. Mentioned in the lab text for that reason.
- **"Should a `204` really be a `502`?"** Excellent question with no clean answer. A
  `204` from a list endpoint is arguably a legitimate "nothing here" and arguably a
  contract violation, and which one depends on a specification this service does not
  have. Say that; do not resolve it.
- **Somebody points out the fix loses the reason.** Correct — all four failure modes
  collapse into one `502`. That is a deliberate simplification and it is on the roles
  list as *something true about which of those happened*, not *everything*. If they want
  the detail preserved, that is a genuinely good extension and it belongs in their own
  time.
- **`emptylist` after the fix returns `200` and `[]`.** Somebody will read that as still
  broken. It is the one case that is not broken, and that is the whole point of having
  separated them.

## Casting review

**After both options are on the table**, marked as opinion.

| | Handled at the call site | Handled centrally |
|---|---|---|
| Names the dependency | yes | not without more work |
| Covers calls added later | no | yes |
| Risk | somebody forgets to wrap a new call | a `JsonException` from parsing *inbound* data also becomes "a dependency is broken" |

My ranking: **at the call site, until there are enough call sites that people forget —
then both.** The central handler is a safety net and a bad primary, because the thing it
cannot do is the thing you most want in an incident, which is know which dependency.

Their team may already have an exception-to-status convention, in which case this is
settled for them and the interesting question is whether it distinguishes inbound from
outbound failures. Most do not.

## Connective tissue

- **`12-`, directly.** That lab was one bad field in a good response. This one is a
  response that was never a department list. Same call, different failure class, and the
  answer to the first does nothing for the second.
- **`21.1-`, if they get there on Tuesday.** A query parameter that will not parse being
  treated as a parameter nobody sent is the same collapse, at the other end of the
  service. Worth naming if both have been done — it is the strongest cross-day rhyme in
  the course.
- **`11-`'s thirty-second hang.** Named in question one and deliberately not answered
  here. That is `11.1-`'s territory.

## The seam into `11.1-`

Question one asks what a `502` would mean if the directory never replied at all. The
honest answer is that nothing in this lab helps: a hang is not a bad answer, it is the
absence of one, and it is handled somewhere else entirely — by the resilience pipeline
they have not touched yet.

Do not answer it. It is the opening of the other optional lab on this day.

## Topics

Extracted into the take-home by `scripts/build-takehome.sh`. **Deliberately not in
`lab.md`** — naming the mechanism up front hands the room the answer to the question
the lab exists to pose. It is useful here for saying what is coming next.

- GetFromJsonAsync
- JsonException
- content negotiation
- problem details
- 502 Bad Gateway
- status codes for dependency failure

## Verified

Verified 2026-09-02 against this lab's polad, .NET 10, `orders` 5181, `directory` 5182.

**Before the fix**, `GET /departments-we-know-about`:

| `directory` mode | Result |
|---|---|
| `ok` | `200`, five departments |
| `empty` (`204`) | `500` — `JsonException: The input does not contain any JSON tokens` |
| `html` (`200`, `text/html`) | `500` — `JsonException: '<' is an invalid start of a value` |
| `object` (valid JSON, wrong shape) | `500` — `JsonException: The JSON value could not be converted` |
| `null` (literal `null`) | **`200` and `[]`** |
| `emptylist` (`[]`) | `200` and `[]` — byte-identical to the row above |

**After the fix:** `ok` and `emptylist` return `200`; the other four return `502` with
`application/problem+json`.

### I had this wrong when I designed the lab

I expected the `204` to be the silent one — that `GetFromJsonAsync` would hand back
`null` for an empty body and `?? []` would swallow it. **It throws.** So does the HTML
page, and so does the wrong-shaped document. The only route to `null` is the literal JSON
value `null`, which is much narrower and much less likely, and it makes the lab better:
the silent case is rare enough that nobody has ever seen it, which is exactly why the
line survived three labs unexamined.

Worth recording because it is the same mistake the lab is about — assuming you know what
a call returns without checking, on the strength of the name of the thing.

### Not guarded by a claim

The mapping in the first table is platform behaviour and the lab depends on all six rows.
`GetFromJsonAsync`'s treatment of a `text/html` content type is the one I would expect to
change — it currently parses it and fails on the content rather than refusing on the
content type. A tripwire that asserts those six status codes would be cheap and would
cover `12-` as well.
