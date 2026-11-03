# Notes — messages that wait

**Thirty minutes, so budget sixty.** Thirteen `##` steps, and two of them are pure
observation.

**Optional.** The decimal says so. Stretch three's floor is `31-` alone, and this is the
lab that answers the question `31-` deliberately leaves open. Push it when there is
time; it is also the best demo in the course, and there is a demo path below.

**Polad: `31-`'s finished state, plus one thing.** Same solution — `Orders` publishing
to `work-assigned` over core NATS, `Crew` listening, the handler, the venue note from
`31-`. Added on top: a `Notifications` service registered with `.WithExplicitStart()`,
a second contract `ShiftNoteAdded` that nothing publishes yet, and a venue entry saying
a third service exists and has never been started.

`WolverineFx.RuntimeCompilation` is present in all three projects — by this lab the
student has already met that error in `31-` and fixed it.

## The one thing that doesn't get cut

**Starting `notifications` for the first time**, with a prediction written down first.

Same service, same start, two subjects: a full backlog of work assignments and not one
shift note. Everything before it is configuration; that is the only moment where
*durability is a property of the message, not of the system* becomes a thing they saw
rather than a thing you said.

If the clock is gone: cut the break step and the second question, keep the prediction.

## Before: what you set up

**The belief you plant:** that this lab is *turning on durability* — a switch, applied
to the system, after which messages survive. Everything about step four encourages it;
they will read `.WithJetStream()` on the AppHost as the moment it happened.

Do not correct it. The whole payoff is that the same running service gets a backlog for
one message and nothing for the other, in the same second.

**Hand off on:** *"Last time you lost a message and the caller never knew. Today you
fix it. It is about four lines."* It is. That is what makes the shift notes land.

## After: what you land

The lab explains the mechanism. What stays yours is the reframe:

> "Is our messaging durable?" is not a question with an answer. Each message type is its
> own decision, and somebody made it — on purpose or by default.

Worth saying once, plainly: they now have a question to take to their own systems that
they could not have asked an hour ago, and the answer at work is very often *nobody
decided, we got whatever the transport does.*

## The demo path

This works better as a demo than most things in the course, because every beat is
something changing on screen. Fifteen minutes if you do not stop for questions, which
you will.

Run the polad with the lab's changes already applied — keep a finished copy in your
mirror. Then, on screen:

1. Everything up, `crew` running. Assign work. Log line appears. *"You saw this yesterday."*
2. **Stop `crew`.** Assign work. Point at the `202`.
3. **Start `crew`.** The message arrives. One sentence: *"Four lines. Same 202."*
4. Send two shift notes. Nothing is listening. Nobody minds.
5. **Start `notifications`** — never run in its life. Backlog of assignments, zero shift
   notes. **Stop talking here.** Let them read the log.
6. Send a shift note live. It appears. *"The listener was never broken."*

The whole argument is beat five, and it only works if beat four happened while they
were not thinking about it. Do not explain streams before beat five; explain them
after, when the room has a reason to want the explanation.

If you only have five minutes: beats one through three. That is the antidote to `31-`
and it stands alone.

## Going wrong on purpose

**Theirs, and it is arranged:** the break step. Removing `.UseJetStream(...)` from `crew`
while the stream, the subject and the publisher are all unchanged. They will predict
that it still works, because the message is obviously being stored.

The reveal inside that step is the strongest thing in the lab and it is easy to rush:
change one consumer name to `audit`, start it, and every message `crew` just missed is
right there. **The message was never lost. A service could not reach it.**

**Yours, thirty seconds, if the room is quick:** start a second copy of a service with
the *same* consumer name and show that they split the messages rather than each getting
a copy. That is competing consumers, it is one line, and it is a good tangent that does
not belong in the floor.

## Where it goes wrong

- **They forget `.WithJetStream()` on the AppHost.** Everything compiles; stream creation
  fails at startup with a JetStream-not-enabled error. Common and easy to fix once seen.
- **Handler classes named in the plural.** If somebody writes their own handler class and
  calls it `Handlers`, Wolverine discovers nothing. Discovery matches type names ending
  in `Handler` or `Consumer`, and the only signal is one `warn` line at startup —
  *"Wolverine found no handlers"* — scrolled past in the noise. I hit this building the
  polad. It is the same shape as `22-`'s missing `public`, and if somebody trips it, that
  is worth naming out loud rather than just fixing.
- **Hyphen versus dot.** `work-assigned` does not match `work.>`. If they change the
  listener subject but not the publisher, or vice versa, nothing arrives and nothing
  errors.
- **The stream already exists from an earlier run.** Changing `DefineStream` limits after
  the fact does not always take. If a stream is in a strange state, the cheapest fix is to
  stop the AppHost, remove the NATS container so its volume goes, and start again.
- **"Why did they come out in the wrong order?"** They will, sometimes. Nothing in the
  lab promised ordering and the venue note says so. Good question, do not improvise a
  guarantee — ordering under JetStream is a real topic and it is latitude.
- **"So we should make everything durable."** The most likely wrong lesson. The shift
  notes exist to prevent it. If it comes up anyway, ask what a six-month replay costs on
  a new consumer's first start — which is question one.

## The casting review

**After the break step, marked as opinion.** The role is *hold an announcement until
somebody takes it*, and the room has now seen two candidates and heard of a third:

| | |
|---|---|
| **Core NATS** | Cheapest, fastest, no storage, no positions to manage. Right when losing one costs nothing |
| **JetStream** | Storage, replay, retention limits, a consumer name per service to get right. Right when losing one costs something |
| **An outbox** | Not shown. Covers the failure neither of the above touches — see question two |

My ranking, said as a ranking: **start with core NATS and promote individual messages
to JetStream when you can say what losing one costs.** The failure mode of the other
order is a system where everything is durable, nobody knows why, and the first
six-month replay takes down a service on a Tuesday morning.

That is an opinion, and their team may have a standing convention that decides it for
them. The reasoning transfers; the conclusion may not.

## Connective tissue

- **`31-`, directly.** This is the antidote to its break step. Its venue note ends by
  naming JetStream and an outbox as the two answers; this lab does one of them.
- **`32-`, if they did it.** Background work does not survive a process either. Same
  question — *what is holding this while nobody is looking at it* — with a different
  answer, because in `32-` nothing is.
- **`11-`'s ServiceDefaults moment.** Same shape: behaviour that is real, consequential,
  and configured somewhere nobody reads. There it was resilience; here it is a stream
  definition four lines long that decides which announcements survive a deployment.

## The seam into `30-`

Question two is the outbox, and it is deliberately unanswered. `orders` writes a work
order and then publishes; if the publish fails, the work order exists and the
announcement does not, and no amount of JetStream helps because nothing was ever handed
to it.

That is the application lab's problem, and this lab's job is to make the room want it
rather than to solve it. **Do not answer question two.** If somebody works out that the
message has to be written in the same transaction as the data, that is exactly right and
worth saying so — and it is still `30-`'s to build.

## Topics

Extracted into the take-home by `scripts/build-takehome.sh`. **Deliberately not in
`lab.md`** — naming the mechanism up front hands the room the answer to the question
the lab exists to pose. It is useful here for saying what is coming next.

- NATS JetStream
- streams and subjects
- durable consumers
- message replay
- WithExplicitStart
- at-least-once delivery
- retention limits

## Verified

Verified 2026-09-02 against the polad with the lab's changes applied — .NET 10,
`WolverineFx.Nats` 6.30.3, Aspire 13.5.3, `nats:latest` with JetStream.

| | |
|---|---|
| Durable consumer, first ever start, messages published before it existed | **All delivered** |
| Same service, `shift-notes` (no stream) over the same window | **None delivered** |
| Same service, `shift-notes` published while it is running | Delivered immediately |
| Two consumer names (`crew`, `notifications`) on one stream | Each gets its own full copy |
| Plain listener on `work.assigned`, started after publish | **Nothing** — while the message sits in the stream |
| A consumer name never used before | Full replay from the start of the stream |
| Replay ordering | **Not preserved.** Observed `0819, 0822, 0821, 0817` |

The last row is why the lab and the venue note both say ordering was not promised. Do
not let a lucky in-order run in class turn into a claim.

### What is not verified

**Stopping and starting services from the Aspire dashboard.** The message behaviour above
was verified by running NATS directly and starting and stopping the services as
processes, because that is what a script can do. The lab tells students to use the
dashboard's stop and start buttons, and `.WithExplicitStart()`'s Start button is the
whole staging of step eight.

That path is ordinary Aspire and there is no reason to think it differs. It has not been
run. **Run the demo path once on the VM before teaching it** — that exercises exactly the
buttons the lab depends on.

### Provenance

Reworked from a demo in `courses/web-api-200/cart/Messaging` — five services, Postgres,
Marten, JetStream, replicas, request/response and wildcard subjects, with three
Excalidraw diagrams that are better than the code. Kept: the late joiner, the stream, the
two-subject contrast, `.WithExplicitStart()`. Cut: Marten and Postgres (two containers and
a second concept, and Marten 8.35.0 currently trips a critical NuGet advisory that would
print in class), request/response over messaging (a sidebar — it has a real failure mode
with two handlers), and replicas (thirty seconds, above).

The diagrams' `pre.excalidraw` — RPC versus messaging versus a shared database, ending on
*all data is cached and suspect* — is a whiteboard beat, not a lab and not a demo. If it
earns a place it belongs in `narrative.md`.
