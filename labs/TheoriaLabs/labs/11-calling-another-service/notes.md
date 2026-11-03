# Notes — calling another service

**Thirty minutes, so budget an hour.** Ten `##` steps. No polad: the practice repo is
the starting state and nothing precedes this lab.

Follows the default shape (`../../../authoring-guide.md`): requirement, venue, roles,
given code, break it, the invisible half, the student's own venue note, two questions.
**The venue note is the closing abstraction** — there is no separate one, deliberately.

Stretch 1, floor. `10-` cannot run without it.

## The one thing that doesn't get cut

**Break it on purpose, then find out where the thirty seconds came from.** Those two
steps are one beat and neither works alone. Everything before it is plumbing
they could have got from documentation. Step 6 is the only moment where *the fault is
someone else's and it is still your problem* becomes a thing they saw rather than a
thing they were told.

If the clock is gone, cut *Look at what you're calling* and *Two ways you'll see this
done instead*. Keep *Break it on purpose* and keep the venue note — the note is the
only artifact they leave with.

## Before: what you set up

Little. They need the practice repo running and both services green in the dashboard.

**The belief you plant:** that this is wiring. Say it is a client, a registration and
an address — three small things, mostly ceremony. That is true of steps 2 through 4
and it is what makes step 6 land, because step 6 is emphatically not ceremony.

**Hand off on:** *"You are going to call something you do not own. Nothing about that
is hard until the last step."*

## After: what you land — and the mechanism is yours

Step 6 gives them a 500 and an exception in the logs and deliberately does not explain
it. The consequence is theirs; the framing is yours.

What they see: *my endpoint broke and I did not break it.*

What you say, once, after: nothing in their code is wrong. They inherited a failure
mode by taking a dependency, and that is the whole cost of the call they just made.
Then stop — `13-` is where anything gets done about it, and answering it here spends
the discomfort that lab needs.

## Going wrong on purpose

**Theirs, and it is the good one:** step 3, the address. Somebody will write
`http://localhost:5xxx` because they can see the port in the dashboard and it is
right there. It works. It works all morning. It is the best failure in the stretch
because **nothing is broken** — no error, no red, and a deployment decision has been
quietly recorded in a source file.

If you see it, do not correct it in the moment. It is worth more at the end of step 6
as *"and by the way, whose machine does this run on."*

**Yours, if the room is skeptical about typed clients:** delete the type parameter,
register a named client, and typo the name. Runtime failure, no build error. Fifteen
seconds and it makes step 5's argument for you.

## The anti-pattern in step 5

The `new HttpClient()` block is labelled in the code, not just in the prose, because
prose does not survive copy-paste. Do not remove the comment to make the slide
cleaner.

**Say the "why" out loud as well as leaving it written**, and say it while the block is
on screen rather than after. The block is what gets pasted; the sentence after it is
what gets slept through.

Worth noting for the room: the address in that block is hard-coded, which is the
mistake half of them may have made in step 3. It is deliberate that both faults appear
together, and it is a good moment to admit that one of them is theirs.

## The casting review — say this after step 5, not before

The lab characterises the three candidates and deliberately does not rank them. The
ranking is yours, and it is worth giving. Mark it as opinion when you do.

> *"You have just been shown three ways to fill the same role. Here is how I cast it,
> and what I am weighing. You may weigh it differently, and your team may already
> have."*

What I weigh, roughly in order:

- **The string disappears.** A typed client's registration and its use are the same
  type, so a rename is a build error rather than a 4am runtime one. Most of the
  argument is this.
- **One place to look.** Configuration is attached to the type, so nobody can
  reconfigure the same client from somewhere else and have it take effect here.
- **The dependency is visible in the signature.** A constructor taking
  `DepartmentDirectory` says what this class talks to. A constructor taking
  `IHttpClientFactory` says only that it talks to *something*.
- **Substitution in tests is the class, not the factory.** Fake the thing with the
  method on it rather than the machinery that makes HTTP calls.
- **It costs nothing.** Same `IHttpClientFactory` underneath, same pooling, same
  handler lifetimes. The safety is free, which is why it is a default rather than a
  preference.

**Then say the part that matters more than the conclusion:** a named client is a
perfectly reasonable choice, plenty of good codebases use them, and if their team has
standardised on one they should not go home and start a fight about it. What they
should be able to do is say what the trade is.

The reasoning transfers and the conclusion does not — they are going back to shops
with their own conventions, and *"typed clients are correct"* equips them to lose an
argument where *"here is what I weigh"* equips them to have one.

## If somebody says their shop does it differently

They will, and the named client is the likely form of it — *"we use `IHttpClientFactory`
everywhere."* That is not an objection to answer with the casting review a second time.
Answer it live:

> *"Here is what we wrote. Show me the same thing with a named client, and tell me what
> is lost and what is gained — including anything that is simply unavailable on one
> side."*

Ask for gained as well as lost, or it is theatre. And this is a case where the honest
answer is *not much, at this size* — which is worth saying out loud, because a room
that only ever sees the comparison land your way stops believing the comparisons.

## Where it goes wrong

- **`GetFromJsonAsync` returns null and nobody notices** until step 4 returns an empty
  array and they assume the other service is empty. Cheap to head off in step 2, but
  better to let it happen if there is time.
- **Ports.** The lab says `PORT` rather than a number on purpose. Somebody will paste
  a curl with a placeholder in it and be confused. Point at the dashboard.
- **They fix step 6 before finishing it.** Someone adds a try/catch and reports it
  works now. Say: *"put it back, I want you to see the shape of it first."*
- **Aspire not started, or started from the wrong directory.** Costs five minutes at
  the top and is the single most likely reason someone is behind.

## The demo this lab wants — and it takes thirty seconds

Not the code. **The dashboard, with `directory` stopped.** Show the red, show the
`orders` log line, and show that `orders` is still green and still serving other
routes.

That last part is the demo. A dependency being down is not your service being down,
and the room can see both states in the same window at the same time. It is much
harder to say than to show.

Do not demo the typed client. They are about to write it.

## Connective tissue — what this makes legible

- **`00-`, retroactively.** They read that the Clerk's office runs a service the
  village depends on. This is the first time depending on somebody else's service is
  a thing their own code does rather than a fact about the village.
- **The systems inventory's `Talks to: None`.** After this lab that column means
  something specific: not *unconnected*, but *has never inherited anyone else's
  failure*. Worth one sentence.
- **Forward into `10-`.** They will do all of this again against the real catalog,
  which is running, which they cannot restart, and which is wrong about one of its
  own fields.

## The seam into `12-`

Step 1's second question — *is every field present on every record?* — is the whole
of `12-when-a-field-is-missing`, and it is deliberately left hanging here.

If somebody answers it properly during step 1, that is a good sign and not a problem.
Let them, and tell them they are one lab ahead.

## What this lab doesn't clarify

- **Anything about failure handling.** By design; that is `13-`. But the room will
  want it at step 6 and you have to hold the line for a stretch.
- **Why `https+http://` and not just `http://`.** They get a working incantation and a
  reason, not a mechanism. Nobody has needed more than that yet.
- **When a typed client is the wrong choice.** It always is here. Somebody with a
  gRPC or messaging background will ask, and the honest answer is that this is the
  HTTP lab and the other answers are `30-`.

## Topics

Extracted into the take-home by `scripts/build-takehome.sh`. **Deliberately not in
`lab.md`** — naming the mechanism up front hands the room the answer to the question
the lab exists to pose. It is useful here for saying what is coming next.

- typed HttpClient
- IHttpClientFactory
- AddHttpClient
- service discovery
- System.Text.Json deserialisation
- HttpClient anti-patterns
- resilience defaults

## Verified, and it corrected the lab

Run against the polad in this directory — .NET 10, Aspire 13.5.3, `aspire resource
directory stop`, which is what the dashboard's Stop button does.

| | |
|---|---|
| Service discovery via `https+http://directory` | Works |
| `directory` payload | `SAN` has `contact: null`; `WTR` has **no `name` property at all** |
| `WTR` through the typed client | `"name": null` on a non-nullable `string`. No exception — `12-`'s lesson, visible here |
| **`directory` stopped, endpoint called** | **Hangs ~30s, then `500`** |

**The lab said `500` and an exception. That was wrong**, and wrong in the way that
matters: a room told to expect an error gets thirty seconds of nothing, decides they
broke something, and starts debugging their own code.

The thirty seconds is `AddStandardResilienceHandler()` in
`Practice.ServiceDefaults/Extensions.cs`, applied to every `HttpClient` through
`ConfigureHttpClientDefaults`. So the original framing — *"your endpoint had no opinion
about the other service being unavailable"* — was also wrong. Something had a very
strong opinion, inherited from a file nobody reads twice.

**This is a better lab than the one I wrote.** A hang is a worse failure than an error
and is the one they will actually meet; the prediction beat now almost always catches
the room out; and the cause is a default in a generated file, which is the course's
whole premise in miniature.

It also changes the later resilience lab. That lab is not *"add retries and timeouts"* —
they are already on. It is *"you have retries and timeouts, do you know what they do,
and are those the numbers you want?"* Which is a better lab too.

## Where this could go stale

The thirty seconds is a default in the standard resilience handler. If a future Aspire
or `Microsoft.Extensions.Http.Resilience` changes it, every number in this lab is
wrong. **Re-run the break step before each delivery** — it takes two minutes and the
lab text is specific enough to be falsified by it.
