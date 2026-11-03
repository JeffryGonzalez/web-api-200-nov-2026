# Notes — the phone at Village Hall

**Application lab. Thirty minutes for the floor, so sixty.** The last section — the
numbering race — is another twenty and is designed to be skippable without anyone
feeling they missed the lab.

This is the lab `README.md` describes as *for the newer half of the room*, and it is
the only one in the course where the main path is deliberately close to copying. That
is correct here and it is worth being unembarrassed about it: the website form is the
reference implementation, the room was told on Monday to read it to learn the codebase,
and using it is the lesson rather than a shortcut around one.

## The one thing that doesn't get cut

**The reveal that the website form stores `null` for a missing name.**

Everything else in the floor can be cut to nothing — they can copy the endpoint in four
minutes. That reveal is the only place where doing this work changes what they think
about the code they were handed, and it converts `22-` from an exercise they did into a
thing they now want.

If you are down to ten minutes: build the endpoint, run the two `curl`s, run the
website-form omission, stop.

## A room that is split in half

Written 2026-09-01, for the first trial run: four developers, two well beyond this
material and two nearly lost. This lab is the one where that gap is widest, and it is
also the one best shaped to absorb it.

**The floor is genuinely achievable by someone who is lost.** It is *read the endpoint
next to it, and write one like it*. That is a real skill, it is most of what the job
is, and finishing it produces a working feature in the village's app. Do not apologise
for its size to the room — the two who need it will hear the apology, and the two who
don't do not need to be told it is easy.

**The last section is not extra credit and should not be introduced as such.** It is
the consequence of the change everybody just made: there were one writer and now there
are two. Frame it as *"the thing you just did had a side effect — who wants to go
find it"* rather than as a harder problem for the people who finished early. The
difference matters, because the second framing tells the other two they are in the slow
group, out loud, for the rest of the week.

**Where to put the fast two while the other two are still building:** the race is the
best answer, but if they are quicker still, the second-best is `Numbering.NextAsync`
itself — ask them what they would replace it with, and make them cost it for a village
that files forty work orders a month. It is a genuinely open question, they will
disagree with each other, and it costs you nothing to run.

**What not to do:** do not give the fast two the next lab. It desynchronises the room
permanently and you lose the ability to say anything to all four at once for the rest
of the day.

## Before: what you set up

- **Nothing.** No new packages, no infrastructure, no seed changes. It runs against the
  app exactly as it has been running since Monday.
- **Worth saying once:** `Channel.Phone` and work order 2026-0818 have been sitting
  there since day one. Somebody is already doing this job by hand.
- **Do not mention the numbering race** while framing. It is a consequence they should
  meet after they have caused it.

## After: what you land

**Adding a way in changes what the code around it was allowed to assume.**

Two of those in one lab: the website form's missing-name behaviour was invisible until
somebody wrote a second channel and had to make the same decision on purpose, and
`Numbering`'s comment was true until a second writer existed.

Say it once, at the end, after the race. Not before.

## Going wrong on purpose

Two predictions, both cheap:

- **Before the website-form omission.** *"`WebsiteFormSubmission` says `string
  ReportedBy`, no question mark. What happens if I leave it out?"* Most rooms say `400`.
  It is `201` and a stored `null`.
- **Before the eight concurrent posts.** Most people predict duplicates or an error.
  Almost nobody predicts **all eight identical**, which is what happens.

## Where it goes wrong

- **They put the endpoint in a new file** and then cannot find why routing does not see
  it. It works fine — `MapWorkOrders` is where the others live and the convention is
  proximity, not necessity. Worth one sentence rather than a correction.
- **They make `ReportedBy` non-nullable** and get a `400` from the framework instead of
  a decision. That is not wrong, it is a different decision, and it is a better
  conversation than the one the lab scripted. Let it stand and ask what Village Hall
  does when the caller hangs up.
- **They validate with what they learned in `22-`** and discover the source generator
  skipping `internal` again. That is the bonded lesson arriving on its own and it is
  worth the detour.
- **The race does not reproduce** if they run the eight posts sequentially. The `&` and
  the `wait` are load-bearing; check the shell if somebody reports it working fine.
- **The eight probe records stay in the database.** There is no delete endpoint. Either
  accept them as noise for the rest of the week or reseed. It is worth naming as the
  ordinary consequence of testing against a real store, which is what they will have at
  work.

## The demo this lab wants

**None.** This is the one where the room reads existing code and writes code like it,
and a demo of that is a re-enactment.

If the numbering race lands, the Aspire dashboard's structured logs are worth two
minutes afterwards — eight `Created` entries, one number.

## Connective tissue

- **Back to `21-` and `22-`.** This is where they get spent. `21-` was where parameters
  come from; this endpoint takes a body and decides what is optional. `22-` was
  validation; the reference channel turns out to have none.
- **Back to Monday's beat**, *the same pothole has two work orders*. They have just
  made the duplicate channel easier to use, which is question one.
- **Forward into `30-`.** After this, intake is genuinely consolidated into one store
  through two automated channels, which is what `narrative.md` gates *work orders route
  asynchronously* on.
- **Forward into the sidebars.** The `null` reveal is the concrete version of the
  nullable-reference-types sidebar, and it is the best possible moment to point at it
  without assigning it.

## The seam into `30-`

Do not resolve the duplicate. Question one asks what it would take and who decides what
"the same hole" means; that is Ted, and it is a conversation rather than a feature.

## Topics

Extracted into the take-home by `scripts/build-takehome.sh`. **Deliberately not in
`lab.md`** — naming the mechanism up front hands the room the answer to the question
the lab exists to pose. It is useful here for saying what is coming next.

- minimal API endpoints
- Marten document storage
- reading a reference implementation
- concurrency
- race conditions

## Verified

Built and run against the app, 2026-09-01, .NET 10.0.400, Aspire 13.5.3, Marten 9.30,
Postgres 18.3:

| | |
|---|---|
| `POST /intake/phone` before the change | `404` |
| Seed contains a phone work order | Yes — 2026-0818, `"caller did not give name"` |
| After the change, named caller | `201`, `2026-0823`, `channel: "phone"` |
| After the change, `reportedBy` omitted | `201`, `"caller did not give name"` |
| Numbering continues the shared sequence | Yes — 0823 then 0824, not a phone-specific series |
| **Website form with `reportedBy` omitted** | **`201 Created`, stored as `"reportedBy": null`** despite `string ReportedBy` |
| **Eight concurrent posts** | **All eight returned `201` with the same number.** Confirmed in Postgres: one number, eight rows |

The app was restored to its shipping state afterwards and the database returned to the
eight seed records — verify with `git status` and a `GET /work-orders` before class.

## Where this could go stale

**The `null` reveal depends on nothing validating the website form.** If anyone adds
validation to `WebsiteFormSubmission` — which `22-` might reasonably lead a room to do
in the app rather than in `practice/` — this reveal stops working, and the lab needs to
know that before it is taught rather than during.

**The race depends on `Numbering.NextAsync` staying as it is.** It is commented as
knowingly unsafe, so it is unlikely to be fixed by accident, but it is the kind of thing
a strong student fixes on Wednesday and does not mention.

Neither is guarded by a script in `../../claims/`. Both could be.
