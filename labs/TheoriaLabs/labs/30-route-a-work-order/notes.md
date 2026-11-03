# Notes — route a work order

**Application lab. Fifty minutes for the floor, so a hundred.** Fifteen `##` steps, and
the last third — break, outbox, venue note — is where the value is.

**No polad.** Like `10-` and `20-`, this happens in `work-orders/`. Students work in the
village's codebase and the changes are theirs to keep.

**The last piece of the spine.** `31-`'s question two, `31.1-`'s question two and `32-`'s
question one all end on the same unanswered thing: *you returned `202` and the
announcement was never made.* This is the answer, and it is the only lab that gives it.

**Wednesday, end of day.** It is fine for this to be the lab somebody takes home — the
first two thirds stand on their own, and the outbox half reads well cold.

## What this lab deliberately is not

The application has Marten, Wolverine and NATS in it after today, and there is a version
of this lab that becomes a distributed-systems course: transaction semantics, projections,
idempotency, dead-letter management, event sourcing. **All of that is out.**

The line held here: this is a lab about **what a response means** when work continues
after it is sent. The outbox is in because three earlier labs ask for it and because
without it the week's central question stays open. Everything past it belongs to a course
about the Critter Stack, not to Web API 200.

If somebody wants to go further, that is a genuinely good conversation to have at the end
of the day, and it is a conversation rather than a lab.

## The one thing that doesn't get cut

**The work order that exists and will never be routed.**

`2026-0822`, in the database, `department: null`, caller told `500`. Not the outbox — the
orphan. The outbox is the answer to a problem they have to feel first, and a room that is
handed `IntegrateWithWolverine()` without seeing the orphan has learned a line of
configuration.

If the clock is gone: build the flow, watch the `201` with a null department, do the
crash, and stop. Say that the fix is one line and that it is in the lab text, and let them
read it on the train.

## Before: what you set up

**The belief you plant:** that the ordering is a detail. Save then publish, publish then
save — everyone has an instinct and nobody has a reason. Let them pick one without
comment.

**Hand off on:** *"Two services have been sitting in this solution doing nothing since
Monday. Today they get a job."*

That framing matters. `Routing` and `Notifications` have been visible in the dashboard all
week, green, doing nothing, and several people will have wondered. This is the payoff for
having left them there.

## After: what you land

The lab covers the mechanism and the abstraction is written. What stays yours is the
sentence about the whole week, and this is the moment for it:

> Everything you built before today answered with the finished truth. This endpoint
> answers with a claim about a moment. That is not a worse API — it is a different
> promise, and the only real mistake is making it by accident.

And the one that transfers furthest: **whenever a system writes to two places, one of them
will eventually be missed.** Not through carelessness — because there is no transaction
spanning them. Being careful about the order does not help, which is the part people
resist.

## Going wrong on purpose

**Theirs, arranged:** the orphaned work order.

**Yours, ninety seconds, and it is worth doing:** after the outbox is in, ask how they
would have found `2026-0822` in production. Nothing alerts. Nothing retries. The row is
valid, the API is healthy, and the only symptom is a resident who never heard back — which
arrives as a phone call to Marguerite, weeks later, and never gets connected to a
deployment that happened on a Tuesday.

That is the whole argument for the fourth role being on the list when nobody asked for it.

## Where it goes wrong

- **`WolverineFx.RuntimeCompilation` is needed in all three services.** Same error as
  `31-` and they have seen it. Pin every Wolverine package to the same version or restore
  fails with `NU1102`.
- **`Routing` and `Notifications` need the contracts project reference.** Easy to miss for
  two of the three, and the error is clear.
- **Handler discovery.** A class named `Handlers` finds nothing — the convention wants a
  name ending in `Handler` or `Consumer`, and the only signal is one `warn` at startup.
  Bit me building `31.1-`.
- **`outbox.Session!`.** The null-forgiving operator is real and somebody will ask. The
  session is populated by Wolverine when the endpoint is invoked; the property is nullable
  because the same interface is used where there is not one.
- **They leave the `CRASH` check in.** It is in the application's code and it will still
  be there tomorrow. The lab says to remove it. Check.
- **Two services listening to one subject.** `Routing` and `Notifications` both take
  `work-orders.recorded` and both get it, which is right here and is exactly what
  `31.2-` warns about when the handler has a side effect. If somebody has done `31.2-`,
  ask them which of these two would need a queue group. Notifications would.
- **Timing.** The department lands in a second or two. Somebody checking instantly will
  see `null` and think it failed — which is the lesson, so let it happen before explaining
  it.

## Connective tissue

- **`31-`, `31.1-`, `32-`.** All three end on Harold not hearing back. This is where that
  is answered, and the Harold table in `narrative.md` has a row for it.
- **`20-`.** Same endpoint family, and if they did the numbering race, the same
  `Numbering.NextAsync` is now being called on a session that comes from the outbox.
  Nothing changes about the race; worth a sentence if it comes up.
- **`12.1-` and `31.3-`.** Same family of finding, three ways: a failure and an empty
  answer sharing a variable, a fault and an outcome sharing an exception type, and here a
  write and an announcement sharing nothing at all.

## The beats this unlocks

From `narrative.md`, both gated on this lab existing:

- **Harold Mink expects to hear back** — the last row of that table is now true, and the
  sentence to protect until now (*durable messaging fixes "the listener was not there" and
  does nothing about "the announcement was never made"*) can finally be said.
- **What if they published events instead?** The closing whiteboard. It only pays after
  three days of inline calls and this lab's `201`. Still do not build it.

## Topics

Extracted into the take-home by `scripts/build-takehome.sh`. **Deliberately not in
`lab.md`** — naming the mechanism up front hands the room the answer to the question
the lab exists to pose. It is useful here for saying what is coming next.

- Wolverine
- NATS
- publish and subscribe
- cascading messages
- Marten
- transactional outbox
- IMartenOutbox
- eventual consistency
- 202 Accepted

## Verified

Verified 2026-09-02 against a scratch copy of `work-orders/` — .NET 10, Aspire 13.5.3,
Marten 9.30.0, `WolverineFx.Nats` / `.Marten` / `.RuntimeCompilation` 6.30.3, Postgres and
NATS from the AppHost.

| Step | Result |
|---|---|
| Website form submission | `201`, body has **`"department": null`** |
| Same work order a few seconds later | **`"department": "STR"`** |
| Routing / Notifications / API logs | `DECIDED`, `TOLD`, `ROUTED` |
| Crash between save and publish | `500`, and **`2026-0822` exists with `department: null`** |
| Same crash, with `IMartenOutbox` | `500`, and **no work order at all** |
| Normal submission with `IMartenOutbox` | `201`, then `department: STR` as before |

The orphan row is the lab. It survives a restart, it is valid, and nothing in the system
is looking for it.

### Not guarded by a claim

`IntegrateWithWolverine()` plus `IMartenOutbox` is the load-bearing pair, and the whole
last third of the lab depends on the crash producing no work order. That is one integration
test away from being guarded — publish a crashing submission, assert the count is
unchanged — and it would run without a broker if the intake path were exercised directly.

Worth having, because a Wolverine or Marten upgrade that changed outbox enrolment would
break this quietly: the endpoint would still return `500`, and the work order would come
back.
