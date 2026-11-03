# Notes — two of the same service

**Twenty-five minutes, so budget fifty.** Twelve `##` steps, several very short. The
cheapest optional in the course and the one with the highest ratio of consequence to
typing — two lines of configuration, total.

**Optional, stretch three.** Gives Wednesday a third thing above the floor alongside
`32-` and `31.1-`. Unlike those two it needs almost nothing built, so it is a good one to
hand somebody at 4pm who has energy but not an hour.

**Polad: `31-`'s finished state.** Orders publishing to `work-assigned` over core NATS,
Crew listening, the handler, the venue note. Deliberately **not** `31.1-`'s state — no
JetStream, no `Notifications`, no second contract. This lab is about core NATS and the
extra machinery would be noise.

**One step near the end is gated on `31.1-`** and says so in its own heading. A student
who has not done that lab skips it and loses nothing.

## The one thing that doesn't get cut

**Eight log lines for four work orders**, with the prediction written down first.

That is the entire lab. Everything after it is the fix and the naming. If somebody has
twelve minutes, do `WithReplicas(2)`, publish four, count, and stop — they will remember
it, and the fix is one line they can read on the train.

## Before: what you set up

**The belief you plant:** that adding a replica is a capacity decision. It is — that is
what makes it dangerous. Frame it entirely in those terms: *one copy is not keeping up,
run two.* Do not mention delivery, subscribers or fan-out until after the count.

**Hand off on:** *"This is a one-line change and it is the sort of thing you would do
without thinking about it. Predict the log count first."*

The prediction matters more here than in most of these. Almost everyone says four. The
ones who say eight have been bitten, and they are worth finding early — ask them what
happened.

## After: what you land

The mechanism is trivial. What stays yours is the frame, and it is the one Jeff already
uses:

> **This is your team's venue meeting the broker's venue.** How many copies to run is a
> decision your team makes, in a deployment file, about load. What happens when two
> subscribers share a subject is the broker's rule and it does not change with your
> replica count. Neither side is written down anywhere the other side reads.

Worth adding, because it generalises past messaging: **the bug was there when there was
one instance.** It was invisible because one subscriber getting everything is also
correct. Scaling did not cause it, it revealed it — and the person who revealed it was
editing a number for unrelated reasons.

## Going wrong on purpose

**Theirs, arranged:** the doubled log lines.

**Yours, thirty seconds, and it is the best part:** after the fix, change the queue group
string in one place so the two instances disagree — or just ask what would happen. The
instance with the odd name is a group of one and handles everything again, so you are back
to duplicates plus a partial split, which is worse than either. There is no error. This is
question two, and doing it live is more convincing than asking.

## Where it goes wrong

- **They count wrong.** Two sets of logs in the dashboard, and it is easy to look at one
  and see four. Say explicitly: open `crew-0`, count, open `crew-1`, count, add them.
- **The split is uneven and they think it is broken.** It is not round-robin. The lab
  says so; somebody will still ask. Two-and-six in one of my runs, four-and-four in
  another, same code.
- **`WithReplicas` and pinned ports.** `crew`'s `launchSettings.json` pins 5192, and two
  replicas cannot both bind it. Aspire handles this — it does not use the launch profile
  for replicas — but if somebody has been starting `Crew` by hand from an earlier lab,
  the port is already taken and the second replica fails to start. Symptom is only one
  instance in the dashboard, which quietly makes the lab produce four lines and no
  lesson. **Check the dashboard shows `crew-0` and `crew-1` before publishing.**
- **"Why not always use a queue group?"** Good question, and the answer is that fan-out
  is genuinely what you want for *notify everyone who cares* — which is exactly what
  `Notifications` in `31.1-` is doing. Both defaults are right for something. That is why
  it is a decision rather than a setting.

## Casting review

Short one, after the venue note, marked as opinion.

The role is *decide how many times this message is processed*. The candidates are not
really competing mechanisms — they are two different intentions, and the mistake is
having one when you meant the other:

| | Means |
|---|---|
| No queue group | *everyone who is listening should know* |
| Queue group / shared consumer name | *somebody should do this, once* |

My ranking, said as one: **default to a queue group on anything whose handler has a side
effect, from the first line, before anyone has scaled anything.** It costs nothing when
there is one instance and it is the difference between a capacity change being safe and
being an incident.

The counter-argument is real and worth stating: a queue group on a genuinely
notify-everyone subject is a bug in the other direction, and a quieter one, because the
copy that did not get the message does not log anything at all.

## Connective tissue

- **`31-`, directly.** Its whole point was one message reaching one handler. This is what
  happens to that arrangement the first time somebody scales it.
- **`31.1-`, if they did it.** The gated step: a durable consumer name does the same job.
  Different names are independent readers, one name shared is one reader in two
  processes. That is why `notifications` catching up took nothing away from `crew`.
- **`20-`'s numbering race**, if they got to the optional section on Wednesday morning.
  Same species: something that was correct with one of a thing, and stopped being correct
  when there were two, with no error either time.

## The seam into `30-`

Question one is delivery guarantees — a copy dies mid-handler, after delivery. Core NATS
has already forgotten the message; nothing redelivers it. That is `31.1-`'s territory for
the storage half, and `30-`'s for what a work order's lifecycle should actually promise.

Do not resolve it. *"Another copy picks it up"* requires machinery nobody has built yet.

## Topics

Extracted into the take-home by `scripts/build-takehome.sh`. **Deliberately not in
`lab.md`** — naming the mechanism up front hands the room the answer to the question
the lab exists to pose. It is useful here for saying what is coming next.

- scaling with replicas
- WithReplicas
- queue groups
- competing consumers
- fan-out
- idempotency

## Verified

Verified 2026-09-02 against this lab's polad, .NET 10, Aspire 13.5.3, `WolverineFx.Nats`
6.30.3, `nats:latest`.

Counted through an endpoint rather than logs, so the number is not a reading of two
dashboards:

| Arrangement | Published | Handled |
|---|---|---|
| `WithReplicas(2)`, no queue group | 4 | **8** |
| `WithReplicas(2)` + `UseQueueGroup("crew")` | 4 | **4** |

And separately, with two `Crew` processes started by hand so the split was visible per
instance:

| Arrangement | Result |
|---|---|
| Two instances, no queue group | 4 published, **4 handled by each** |
| Two instances, `UseQueueGroup("crew")` | 8 published, **2 and 6** — total 8 |
| Two instances, JetStream, **same** consumer name | 8 published, **4 and 4** — total 8 |

The 2/6 and 4/4 rows are the same configuration on different runs. **Do not let a
balanced-looking run in class turn into a claim that it balances.**

The JetStream row is the gated step, and it is the symmetric case to `31.1-`'s finding
that two *different* consumer names each get a full copy.

### Not guarded by a claim

Core NATS fan-out to multiple subscribers is broker semantics and is not going to change.
The `.WithReplicas(2)` behaviour is Aspire's and could. The cheap tripwire is the one I
used to verify it: a counter endpoint, publish four, assert the count — it would catch
both the fan-out and the queue group in one run.
