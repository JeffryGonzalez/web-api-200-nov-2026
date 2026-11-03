# Notes — failing faster than the default

**Thirty minutes, so budget an hour.** Thirteen `##` steps, several of them just timing a
request.

**Optional, stretch one.** With `12.1-` this gives Monday two things above the floor,
which it did not have at all. Of the two, this is the one to give the more experienced
half — it is less about a mechanism and more about a decision, and it needs somebody with
production scars to get full value.

**Polad: `11-`'s finished state**, plus `POST /delay/{seconds}` on `Directory` so it can
be slow on demand, and a venue entry saying so. `Department` still has the non-nullable
`Name` from `11-`; this lab never touches it.

## The one thing that doesn't get cut

**The circuit breaker refusing a healthy dependency.**

Not the timeout change — that is arithmetic and they can read it. The moment is the one
where the directory is demonstrably fine, they call, and get a `500` in two milliseconds
because their own service has decided not to look. Nobody predicts it, everybody
remembers it, and it is the honest price of the feature.

If the clock is gone: feel it, change it in `ServiceDefaults`, jump to the breaker, skip
the call-site detour.

## Before: what you set up

**The belief you plant:** that configuration goes where the thing being configured is
registered. It is a correct instinct everywhere else in this framework, which is what
makes the call-site step land — they will write good code that does nothing.

**Hand off on:** *"There is a number in this codebase that nobody in this room chose and
everybody in this room will inherit. Go find out what it costs."*

## After: what you land — and the thing you are worried about

This is the lab where *"talk to your team"* is the actual answer, and it is worth being
unembarrassed about that, because **the lab is built so it cannot be a dodge.**

Three things make it a finding rather than a hedge:

- **Two of the numbers have wrong answers, and they are stated.** A timeout longer than
  your caller's own timeout is always wrong; you are holding a connection nobody is
  waiting on. A blanket retry over a non-idempotent operation is always wrong. Those are
  not preferences.
- **The thing that is genuinely theirs is one fact — what their callers tolerate — and
  they are the only people who can know it.** Saying so is accurate. Refusing to say it
  would be the dishonest move.
- **The venue note is a form with a blank in it.** *The fact about our callers that
  justifies it.* A student who writes *it depends* has not filled it in, and the form
  makes that visible to them rather than to you.

The sentence worth saying out loud, once, after the venue note:

> A decision with a reason written next to it is not the same thing as no decision. The
> reason is what stops the next person changing your number because it looked high.

**Then give your own answer**, marked as yours — see the casting review. An instructor
who names the dimensions, states the constraints, and then declines to say what he would
do has hedged. One who does all three and then says *"I would start at five, here is
why"* has not.

## Going wrong on purpose

**Theirs, arranged, twice.** The call-site configuration that changes nothing, and the
breaker that stays open over a recovered dependency.

**Yours, thirty seconds:** after the call-site step, ask what they would have done if
they had shipped it. The honest answer is that they would have believed it — there is no
test in the solution that would have caught it, and the code review would have passed.
That is the argument for timing the request rather than reading the code, and it is worth
ninety seconds of discussion if the room is the kind that likes it.

## Where it goes wrong

- **They leave the call-site block in.** It is harmless once `ServiceDefaults` is right,
  but it is a second pipeline and it makes the numbers lie. The lab says to remove it;
  somebody will not.
- **Validation exceptions on the options.** `SamplingDuration` must be at least twice
  `AttemptTimeout`, and `AttemptTimeout` must not exceed `TotalRequestTimeout`. The
  startup error is clear and names the rule, which is a nice contrast with everything
  else in this course that fails silently. Worth pointing out for exactly that reason.
- **"Why is call 2 faster than call 1?"** Sharp-eyed, and correct — the second failure
  arrives partway through the sampling window and the breaker opens mid-flight. Not worth
  a detour; the honest answer is that the exact boundary is timing-dependent and the
  shape is what matters.
- **Someone lowers `MinimumThroughput` in a real service after this.** Worth heading off.
  Three is a teaching value. Setting it low in production gives you a breaker that trips
  on a blip, which is its own outage.
- **`ServiceDefaults` is shared, so this changes `directory` too.** Somebody will notice
  and it is a good noticing — the file is referenced by every service, including the one
  they were told not to change.

## Casting review

**After the venue note**, marked as opinion, and give it — see above.

The role is *decide how long to wait*. The candidates are not the mechanisms, they are
the postures:

| | When it is right |
|---|---|
| **Long timeout, several retries** | The caller is a batch job or another service with patience, and a correct slow answer beats a fast failure |
| **Short timeout, one retry** | A person is waiting, and something useful can be shown without this data |
| **No inline call at all** | The answer is not needed to respond — accept the request, do the work elsewhere. Day three |

My ranking, as a ranking: **start short, and treat any timeout above about five seconds
on a path a human is waiting on as a smell.** Not because five is special, but because a
longer one usually means nobody asked what the caller does while it runs, and the answer
is normally *holds a connection and eventually gives up anyway*.

The third row is the one I actually reach for most, and it is worth saying that plainly.
A lot of what looks like a resilience problem is an inline call that did not need to be
inline. That is question one, and it is the setup for the whole of day three — do not
resolve it here.

## Connective tissue

- **`11-`, directly.** It found the thirty seconds and named the file. This changes it.
- **`12.1-`, the other optional on this day.** Its question one asks what a `502` would
  mean if the directory never replied — which is this lab. If somebody does both, either
  order works, and the pair is a decent picture of *the dependency answered badly* versus
  *the dependency did not answer*.
- **`31-` and `32-` on day three.** Question one is the seam. Accepting the request and
  doing the work later is the answer that makes most of this lab unnecessary, and it
  brings its own problems — which is the point of it being a different day rather than a
  different paragraph.

## Topics

Extracted into the take-home by `scripts/build-takehome.sh`. **Deliberately not in
`lab.md`** — naming the mechanism up front hands the room the answer to the question
the lab exists to pose. It is useful here for saying what is coming next.

- AddStandardResilienceHandler
- timeouts
- retries
- circuit breaker
- ConfigureHttpClientDefaults
- ServiceDefaults
- Polly

## Verified

Verified 2026-09-02 against this lab's polad, .NET 10, Aspire 13.5.3, `orders` 5181,
`directory` 5182. Times are wall clock from `curl -w '%{time_total}'`.

| Configuration | `directory` delayed 30s |
|---|---|
| Stock `ServiceDefaults` | `500` in **30.02s** |
| Options added at the **call site**, stock `ServiceDefaults` | `500` in **27.3s / 30.0s** — no improvement |
| Options in **`ServiceDefaults`** | `500` in **5.01s** |

Delay of 3s under stock configuration: `200` in 3.00s, so the pipeline is not simply
failing everything.

**Circuit breaker**, delay 30s, `MinimumThroughput = 3`, `FailureRatio = 0.5`,
`BreakDuration = 15s`:

| Call | Result |
|---|---|
| 1 | `500` in 5.08s |
| 2 | `500` in 2.25s |
| 3–6 | `500` in **0.002s** |
| after `delay/0`, immediately | `500` in **0.002s** — dependency healthy, breaker open |
| after waiting out 15s | `200` in 0.024s |

At the **default** `MinimumThroughput` of 100, six failing calls do not open the breaker
at all. That is the *safety feature that cannot fire* step and it is the finding I did
not expect when I started.

### Not guarded by a claim

Every number above is platform behaviour and the lab quotes them in reveals. The stock
thirty seconds is also quoted in `11-`'s reveal and in two venue notes, so it is the most
load-bearing untested number in the course. A tripwire that starts the polad, delays the
directory, and asserts the total elapsed time falls in a band would cover both labs.

The defaults could move in a minor release of `Microsoft.Extensions.Http.Resilience` and
nothing would tell us.
