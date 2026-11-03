# Notes — the message nobody can handle

**Thirty-five minutes, so budget seventy.** Eleven `##` steps.

**Optional, stretch three.** Wednesday's fourth thing above the floor, and the one that
needs the most already done — it only makes sense after `31.1-`, because it is about what
happens once durability has successfully delivered a message to a handler that cannot use
it.

**Polad: `31.1-`'s finished state**, trimmed. JetStream on in the AppHost, `Orders`
defining the `WORK` stream and publishing to `work.assigned`, `Crew` reading it as the
durable consumer `crew`. No `Notifications` and no `ShiftNoteAdded` — they play no part
here and would be noise.

**Directory name versus title.** The folder says *the message that keeps coming back*,
which is what I expected to build. It does not come back. The title is *The message
nobody can handle*, and the folder name is left alone because renaming it would break any
staged copy. Worth fixing if the polads are ever restaged from scratch.

## A bug this lab found in `31.1-`

`Crew` becomes a JetStream consumer of a stream that `Orders` defines at startup. If
`crew` wins that race it dies with `NatsJSApiException: stream not found` and a stack
trace, and the lab looks broken.

`31.1-`'s AppHost did not order them. It does now — `.WaitFor(orders)` on `crew`, in both
that polad and this one. **If anyone reports `31.1-` failing on a restart with a NATS
exception, that is this**, and the fix is already in.

It is a race, so it will not reproduce reliably. It did not show up when I verified
`31.1-` because I ran the services by hand, in order.

## The one thing that doesn't get cut

**Four `ATTEMPT` lines, with the prediction first**, and then rereading the handler with
that number in hand.

Not the fix — the fix is ordinary Wolverine and they can read it. The moment is the one
where they see that everything above the `throw` happened four times, and that the retry
policy could not have known better because it matched on a type.

If the clock is gone: make the poison message, show one attempt and *moved to the error
queue*, and go straight to the venue note's last line. That is fifteen minutes and it is
the argument.

## Before: what you set up

**The belief you plant:** that an unhandled exception in a message handler is caught by
something. It is the reasonable assumption — this is exactly what frameworks are for, and
the log line actively confirms it.

**Hand off on:** *"You made the message survive a service being switched off. Now here is
one the service cannot do anything with."*

Do not mention durable storage before they read the log line. The venue note is there for
them to find afterwards, and finding it is better than being warned.

## After: what you land

The mechanism is in the lab. What stays yours is the distinction, and it is worth saying
slowly because it reframes error handling generally:

> **An exception is a message with one recipient, and that recipient is the
> infrastructure.** It is right when the infrastructure can genuinely help — retry, back
> off, give up. It is wrong when the thing that went wrong has a person waiting on it,
> because the infrastructure cannot tell a person anything.

And the working rule, which they can take to any codebase: **if a human needs to know, it
is a message. If only the machinery needs to know, it is an exception.** Anything with a
reason worth writing in a sentence is the first kind.

## Going wrong on purpose

**Theirs, arranged, twice.** One attempt and a reassuring log line that names a place
that is not there; then four attempts once they ask for retries.

**Yours, thirty seconds:** ask what would have happened if the handler had written to a
database before the `throw`. Four rows. Then ask how they would find that out in
production — the answer is a support ticket about duplicates, weeks later, and no
connection to the retry policy somebody added for a different reason.

## Where it goes wrong

- **The `DeadLetterTo` step is deliberately a dead end** and somebody will think they
  configured it wrong. They did not. Say plainly that the machinery it points at is not
  enabled in this solution, and that the lab is showing them the shape of the mistake
  rather than a bug.
- **`using Wolverine.ErrorHandling;`** is required for `OnException`, and it is on
  `opts.Policies`, not `opts`. Both are in the lab text because I got both wrong first.
- **Two handlers for one message type.** If somebody puts the `WorkRejected` handler in
  `Crew` instead of `Orders`, and `Crew` also handles `WorkAssigned`, that is fine — but
  a *second* handler for the same type in one service means Wolverine calls both. Worth
  knowing before somebody does it by accident.
- **"Why not just catch the exception in the handler?"** Excellent, and it is nearly what
  the fix does. The difference is what you do next: catching and logging is the same dead
  end with more code; catching and *publishing* is the lab. Push on what happens after
  the catch.
- **Somebody wants a real dead-letter queue.** That is a legitimate want and it needs
  durable storage — Marten on Postgres, which the engagement app has and this practice
  repo deliberately does not. Good tangent, do not build it.

## Casting review

**After the venue note**, marked as opinion. The role is *decide what happens to work that
cannot be done*:

| | Right for | Costs |
|---|---|---|
| **Throw and let it fail** | a genuine fault the infrastructure can act on | nothing is told, and here nothing is even kept |
| **Throw with a retry policy** | transient faults — timeouts, deadlocks, restarts | multiplies every side effect above the throw, for failures that will never succeed |
| **Return a rejection message** | outcomes: work that is understood and impossible | you have to model it, and somebody has to handle it |
| **Dead-letter storage** | keeping envelopes for a human to inspect and replay | needs a database; it is real infrastructure, not a config line |

My ranking, as a ranking: **model the outcomes you understand, and let the ones you do not
understand throw.** A handler that returns a rejection for the three things you know can
go wrong, and throws for everything else, is honest in both directions — and the exception
path stays rare enough that when it fires, it means something.

The failure mode of the other order is a codebase where everything throws, a retry policy
gets added because throwing lost data, and now half the failures are quietly happening
four times.

## Connective tissue

- **`31.1-`, required.** Durability got the message delivered. This is the next question
  and it does not have a durability answer.
- **`31.2-`, if they did it.** Same species of finding: how many times the work happens is
  decided by something that does not look like it decides that. There a string, here a
  retry policy.
- **`32-`.** `BackgroundServiceExceptionBehavior.Ignore` is the same trade — a loud
  failure made quiet by a setting that the error message recommended. Here it is a log
  line that names a destination instead.
- **`12.1-` on Monday.** *A failure and a legitimate empty answer must not share a
  variable.* Same family: a fault and an outcome must not share an exception type.

## Topics

Extracted into the take-home by `scripts/build-takehome.sh`. **Deliberately not in
`lab.md`** — naming the mechanism up front hands the room the answer to the question
the lab exists to pose. It is useful here for saying what is coming next.

- Wolverine error handling
- OnException policies
- RetryWithCooldown
- dead letter queues
- durable message storage
- modelling failure as a message

## Verified

Verified 2026-09-02 against this lab's polad, .NET 10, `WolverineFx.Nats` 6.30.3,
`nats:latest` with JetStream. Services run individually, `orders` started first.

| Arrangement | Result |
|---|---|
| Handler throws, no policy | **1 attempt**, caller got `202`, log: *was moved to the error queue* |
| Same, ordinary work order alongside | handled once, `DONE` — unaffected |
| `.DeadLetterTo("work.rejected")` | **nothing arrives.** Stream reports 1 message, 1 subject |
| `.ConfigureDeadLetterQueue(4, "work.rejected")` | **also nothing** |
| `OnException<InvalidOperationException>().RetryWithCooldown(1s, 2s, 3s)` | **4 attempts**, then the same error-queue line and the same nothing |
| Handler returns `WorkRejected`, routed to `work.rejected`, dispatch listening | **`REJECTED 2026-0819 at N. Salyer at the culvert - No crew is certified for culvert work.`** |

Stream state confirmed with `nats stream info WORK`: one subject, one message, after a
failed delivery that claimed to have been moved.

### What I could not establish

**Why** neither dead-letter configuration produced anything. The explanation in the lab —
that Wolverine's error queue is a feature of durable message storage, which this solution
does not have — is consistent with everything observed and with the fact that the
persistence packages are absent. **It is an inference, not something I confirmed from
Wolverine's source or documentation.**

The lab is written to survive being wrong about the cause: every claim in it is about what
was observed. If somebody makes the dead-letter path work in this configuration, the *Where
the error queue is* step needs rewriting and the lesson does not.

### I expected the opposite, again

The lab was proposed as *the message that keeps coming back* — a poison message redelivered
until `MaxDeliver`, with the handler running five times. Wolverine does not let NATS
redeliver: it catches the exception and moves the envelope on the **first** attempt. The
repeated-side-effects finding is real but you have to ask for it, which makes it a better
step, because they ask for it themselves.

### Not guarded by a claim

The four-attempts number and the single-attempt default are both quoted in reveals and
both are Wolverine's behaviour, which could change in a minor version. A tripwire that
publishes one poison message and counts handler invocations would cover both, and would
also catch the day the dead-letter path starts working.
