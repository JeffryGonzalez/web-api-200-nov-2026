# Notes — work that runs later

**Twenty-five minutes, so fifty.** Two build steps and two break steps, and the second
break step is the one that pays for the lab.

The polad ships with the notification inline, so the caller waits three seconds. That
wait is the setup and it needs to be felt before anything is written — do not let the
room skip `curl` and go straight to the code.

## The one thing that doesn't get cut

**The second break step, with a prediction before it.**

`BackgroundServiceExceptionBehavior.Ignore` is the fix the error message recommends,
the fix every search result recommends, and the fix that turns a service that fell over
loudly into a service that lies. A room that only sees the first break learns
*background services can crash your app*. A room that sees both learns the thing this
course is actually about.

If you are out of time: cut the venue note, cut question two, run both breaks.

## Before: what you set up

- **They have already met `202` in `31-`**, and it meant *the message may be gone*.
  Here it will mean *the work may never happen*. Do not connect these for them.
- **Ted's clipboard is the source of the poison record**, and it is worth one sentence
  of framing: entries arrive days late, handwritten, sometimes without a name. The
  broken input is not contrived, it is the channel working normally.
- **Do not mention `HostOptions` or exception behaviour.** The error message will
  introduce it, which is the point — they will follow the advice the runtime gave them.

## After: what you land

**Getting work off the request thread and making the work survive are two different
problems.** This folder solves the first completely and the second not at all.

That sentence is what makes the outbox make sense tomorrow morning. Do not say it
before the second break; it is the closing abstraction, not the framing.

## Going wrong on purpose

**Both breaks want a written prediction**, ninety seconds each, per `narrative.md`'s
standing practice.

The first prediction is usually *"it'll log an error"* or *"that one notification
fails."* Nobody predicts the API going down.

The second prediction is the valuable one. Having just configured the host not to stop,
most of the room predicts the notifier keeps going and skips the bad one. **It does
not — it ended when it threw and nothing restarts it.** The gap between those is the
lab.

## Where it goes wrong

- **They add a try/catch instead of the `Ignore` setting.** Fine, and better
  engineering — but it skips the reveal. Let it happen and then ask what the setting in
  the error message would have done. Or ask them to try it and find out, which is
  better.
- **They never see the host stop**, because they only ran `curl` on the POST and it
  returned `202`. Make sure the `GET /` is run. The dead service is invisible from the
  endpoint that killed it.
- **The `throw` gets softened.** Somebody will make it a `LogWarning` and a `continue`
  while typing, because that is what a careful developer does. Ask them to put it back
  for now; they have just written the correct answer two steps early and the lab has no
  way to show them why it was correct.
- **Restart confusion.** After the host stops, `dotnet run` has exited. It looks like a
  tooling failure rather than a consequence. Say once that the service really did stop
  and that this is the finding, not a broken lab.

## The demo this lab wants

**The Aspire dashboard, showing intake go from green to stopped**, on the first break.
This is the moment `coverage.md` reserves the dashboard for: something has actually
gone wrong and there is something to point at.

On the second break the dashboard is more useful still, because it stays green. A
healthy dashboard next to a service doing nothing is the whole argument in one screen.

## The casting review

`coverage.md` lists this role as unwritten. It belongs at the end of `30-`, after both
candidates have been seen — not here.

**Role: getting work off the request thread.**

| Candidate | What it costs | What it buys |
|---|---|---|
| `Channel<T>` + `BackgroundService` (`32-`) | Nothing survives the process. No retry. Failure handling is one global setting with two bad answers | No infrastructure at all. Minutes to write. Right when losing the work is cheap |
| Publish to NATS (`31-`) | A broker to run. At-most-once by default, so a restarted listener loses messages | The work leaves the process. Another service can own it, deploy separately, scale separately |

**Mark it as opinion, out loud.** Mine: reach for `32-` first and be honest in the
venue note about what it does not do. Most background work in most services is cheap to
lose, and a broker introduced before anyone can name what it is protecting is
cargo cult. The moment you can name the cost of losing one, neither candidate is
adequate and you want the outbox — which is why both of these labs end in the same
place.

## Connective tissue

- **Back to `31-`.** Same `202`, same silent loss, different mechanism. Two ways to
  disappoint Harold.
- **Back to `22-`.** Question two asks whether validating at the edge fixes this. It
  fixes *this* record and not the class of problem — the next poison message will be a
  timeout or a `500` from somebody else's service, and no amount of edge validation
  sees it coming.
- **Back to `12-`.** A field the spec promised was missing. Here a field the code
  needed was missing. Both arrive as data, neither as an error.
- **Forward into `40-`.** Harold is the person on the other end of the notification
  that never happened.

## The seam into `41-`

The closing paragraph names what is missing — durability, retry, somewhere to put what
will never succeed — without naming the mechanism.

**Do not say "outbox" here.** Let them arrive at `41-` having already described the
thing they need.

## Topics

Extracted into the take-home by `scripts/build-takehome.sh`. **Deliberately not in
`lab.md`** — naming the mechanism up front hands the room the answer to the question
the lab exists to pose. It is useful here for saying what is coming next.

- BackgroundService
- IHostedService
- Channel<T>
- BackgroundServiceExceptionBehavior
- health checks
- in-process work queues

## Verified

Built and run, 2026-09-01, .NET 10.0.400, Aspire 13.5.3, no broker and no database:

| | |
|---|---|
| Polad as it ships, inline notification | `POST` took **3.07s**, `201 Created` |
| After the change | `POST` took **0.05s**, `202 Accepted`; `Notified Harold Mink about 2026-0817` three seconds later |
| **Poison record, default behaviour** | `202` returned. Then `BackgroundService failed`, `InvalidOperationException`, and **the host stopped**. `GET /` and every subsequent request: connection refused |
| Log text | Names `HostOptions.BackgroundServiceExceptionBehavior` and `StopHost` explicitly, and says the host is stopping |
| **Poison record, `Ignore`** | `202`. Host stays up, `GET /` returns `200`, further posts return `202`. **The next good work order was never notified.** One log line, nothing else |
| Health endpoint after `Ignore` break | Still healthy. Nothing observable from outside distinguishes it from working |

## Where this could go stale

**`StopHost` is the default and has been since .NET 6**, so the first break is stable.
If a future SDK changed that default to `Ignore`, the first break stops breaking and
the lab loses its loud half — the second half would still work, and the lab would need
rewriting rather than renumbering.

The three-second delay is a `Task.Delay`. If anyone asks whether a real notifier would
behave differently: yes, it would fail in more interesting ways, and every one of them
lands in the same two settings.
