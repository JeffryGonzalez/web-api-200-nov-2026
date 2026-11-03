# Notes — publishing a message

**Thirty minutes, so budget an hour.** Nine `##` steps. Wednesday.

**Polad: its own.** A fresh two-service solution with NATS in the AppHost and nothing
using it. Both services reference `WolverineFx.Nats` and **deliberately not**
`WolverineFx.RuntimeCompilation` — so the room hits the Roslyn error and the reveal
rescues them. Hitting it is worth ninety seconds: it is the clearest available
evidence that Wolverine writes code at startup, which is why a handler needs no
registration. Nothing from stretch one or
two is needed; a student who missed Monday can do this.

Stretch 3, floor. `30-` cannot run without it.

## The one thing that doesn't get cut

**Break it on purpose** — stop `crew`, publish, restart, and find the message gone.

Everything before it is wiring, and the wiring is genuinely easy. That step is the only
moment where *the caller got a 202 and the message does not exist* becomes a thing they
saw.

If the clock is gone, give them the built version and go straight to it.

## Before: what you set up

Docker first. NATS is a container and this is the second lab all week that needs one.

**The belief you plant:** that this is about getting two services talking. It is, for
twenty minutes, and it works on the first try — which is what makes the break step
land. Nobody expects a thing that just worked to have lost something.

**Hand off on:** *"Assigning work should not wait for the crew to be told. Make the
telling happen somewhere else."*

## After: what you land — and this half is yours

The lab explains at-most-once delivery. What stays yours is the framing.

Say, once: **they did not choose at-most-once. They chose NATS, and at-most-once came
with it.** Every transport has a delivery guarantee, most people never read it, and it
is invisible until the day it is not. That is the transferable sentence and it is worth
more than anything about NATS specifically.

Then stop. Outbox, JetStream and idempotency are all real and they are all later.

## Going wrong on purpose

**Theirs, arranged, and the point:** the lost message. Note that `orders` returns `202`
in both cases — the room will look for a difference in the response and there isn't one.

**Yours, thirty seconds, if they doubt it:** send one while `crew` is down, start
`crew`, show nothing arrives, then send another and show it arrives instantly. The
contrast is what convinces; either half alone reads as a fluke.

## Where it goes wrong

- **The Roslyn error at startup.** Very likely, because it is new. Core Wolverine 6 no
  longer ships the runtime compiler and `WolverineFx.RuntimeCompilation` has to be
  added. The reveal covers it.
- **Version mismatch on that package.** `dotnet add package` takes the newest, and the
  Wolverine packages are not always published in step — I hit `NU1102` complaining
  about a `WolverineFx` version that does exist, because `RuntimeCompilation` was a
  release ahead of `.Nats`. Pin both to the same version.
- **"Where do I register the handler?"** Nowhere. Wolverine finds it by shape — a
  static `Handle` method taking the message type. Somebody will not believe this.
- **They want a queue immediately.** Good instinct, wrong lab. It is question one.
- **The dashboard rather than the terminal** for `crew`'s logs. Say so; the output they
  need is not where they have been looking all week.

## The demo this lab wants

**None until the break step**, and then only if they doubt the result.

Worth showing afterwards if there is time: the Aspire dashboard's trace view, where a
publish and its handling appear as one distributed trace across two services. Thirty
seconds, no narration, and it makes *"somewhere else, later"* concrete in a way the
logs do not.

## Connective tissue

- **`venues/http.md`'s `202` entry** claims a `Location` header. Theirs has none. That
  is question two, and it is the third counter-instance they have found against that
  file this week — which is the note being useful rather than wrong.
- **`10-`, from Tuesday.** There they added a synchronous dependency to the write path
  and felt the cost. Here they add an asynchronous one and it costs nothing — until it
  loses something. Two shapes, both real, neither free.
- **Forward into `30-`.** The work-order app has NATS running and nothing publishing,
  exactly like this polad did.

## The seam into `30-`

Question one — *who finds out when the lost message was Harold's notification* — is
Wednesday's application lab and the outbox conversation behind it.

Do not answer it. Do not let them build a retry here.

## Topics

Extracted into the take-home by `scripts/build-takehome.sh`. **Deliberately not in
`lab.md`** — naming the mechanism up front hands the room the answer to the question
the lab exists to pose. It is useful here for saying what is coming next.

- Wolverine
- NATS
- message handlers
- convention-based handler discovery
- shared contracts projects
- at-most-once delivery
- 202 Accepted

## Verified

Built and run, 2026-08-30, .NET 10, Wolverine 6.30.3, NATS 2.14 via Aspire 13.5.3:

| | |
|---|---|
| Publish in `orders`, handle in `crew` | **Works.** `Crew Ted has 2026-0819 at N. Salyer at the culvert` |
| `orders` response | `202 Accepted`, immediately, empty body |
| Handler discovery | No interface, no registration. Wolverine finds a static `Handle` by shape |
| **Publish while `crew` is stopped** | **`202`. Message never delivered, and not waiting on restart. Gone** |
| Publish after restart | Arrives normally. The system looks healthy because it is |
| Startup without `WolverineFx.RuntimeCompilation` | Crashes with a clear message naming the package (GH-2876) |
| `dotnet add package` for it | Resolved 6.31.0 against `.Nats` 6.30.3 and failed `NU1102`. Pin both |

## Where this could go stale

The Roslyn/`RuntimeCompilation` split is recent. If Wolverine folds it back into the
core package, the reveal about it becomes wrong and a student following it adds a
package they do not need — harmless, but the lab would be lying. Worth a re-run before
each delivery.

The at-most-once behaviour is not a rough edge and will not change; it is what core
NATS is.
