# Notes — dispatch checks standing

**Forty minutes, so budget eighty.** Twelve `##` steps, and it is the longest thing in
the week. Tuesday morning.

**No polad.** Application labs work in the engagement app, which is already on the
machine. Nothing to push but the markdown.

**This is the spine.** `narrative.md`'s load-bearing beats two and three both land here.
If one thing survives the week, it is this lab.

## The one thing that doesn't get cut

**The half of that refusal you can't see** — Kerns and Rademacher side by side, one with
an explanation and one with a hole where the explanation should be.

Everything before it is plumbing they can do. That step is the only moment where *the
document I was handed was wrong and nothing told me* becomes a thing they saw, against a
real service, behind a real specification.

If the clock is gone, cut *Try all five* down to two — Kerns and Rademacher — and skip
*Make the decision* by giving them the reveal. Keep the venue note.

## Before: what you set up

They need the app running and Docker started. Say that first; it is the only lab where
a container has to be up before anything works.

**The belief you plant:** that this is an integration task. Call a service, check a
field, refuse. It is, for the first forty minutes, and that is what makes the last
twenty land.

**Hand off on:** *"Marguerite says you cannot send work to a vendor the village is not
allowed to use. Dale runs that list and it is not his opinion. Go and stop us doing it."*

**Do not mention the specification being wrong.** Not as a hint, not as a warning, not
as *"pay attention to the reason field."* The whole lab is arranged so they find it.

## After: what you land — and this is yours

The lab explains the null. What stays yours is the size of it.

Say, once, after: **that specification was not written by careless people.** It was
written by whoever built the catalog, about a service that then met the state debarment
feed, and nobody went back. Every API document any of them has ever published is in the
same condition and none of them know which field it is.

Then stop. The temptation is to turn it into a lecture about contract testing, and
there is a real answer there — but they have just had the experience, and the
generalising sentence is worth more than the tooling.

## Going wrong on purpose

**Theirs, arranged, and the point of the lab:** `reason` comes back null and the
refusal message has a hole in it. Nothing throws.

**Theirs, unarranged, and very likely:** the `HttpClient` base address. A leading slash
on the request path throws away the base's path and produces a `404` that reads as *the
catalog is broken*. The reveal covers it, and it is worth letting them hit it for two
minutes first — it is the same class of mistake as the hardcoded port in the practice
repo, one level up.

**Yours, thirty seconds, if they are quick:** dispatch to Kerns and then ask what
happens to work that was *already* dispatched to Kerns before May. The seed has exactly
that case — 2026-0817 went to Kerns on 25 August, months after the suspension. Nobody
has to answer it. Dale explicitly refuses to, in the thread.

## Where it goes wrong

- **They put the vendor lookup after the write.** Then the refusal happens with the work
  order already dispatched. Worth catching in review rather than in the reveal.
- **They match the vendor name exactly.** `q=` is a contains-match, and *"Kerns"* finds
  *"Kerns Excavating"*. Someone will send the full name and it will also work, and
  someone will send *"kerns"* and be surprised. That is a real API design question and a
  good three-minute tangent.
- **`400` instead of `422`.** Defensible, and worth one sentence: the request was
  well-formed and we understood it. We are refusing for a reason that has nothing to do
  with its shape.
- **They "fix" the null by making `Reason` nullable and moving on.** That is correct and
  it is not the lesson. Let them do it, then ask how they would have known to.

## The demo this lab wants

**None until the end.** They are integrating; watching someone else integrate teaches
nothing.

Afterwards, thirty seconds worth having: open the **API catalog page on the portal**
next to the actual response, and put `"reason": "required"` in the spec beside
`"reason": null` on the wire. Two windows. Say nothing.

## Connective tissue

- **`11-`** was a typed client against a service they could stop. This is the same
  shape against one they cannot, that they did not write, and that has a published
  contract. Say that out loud — the practice lab was the rehearsal and this is the
  performance.
- **`12-`** was a missing field with no contract to appeal to. Here there *is* a
  contract and it is wrong, which is worse and more common.
- **`00-`'s step 5.** They wrote down what they thought had to be built. This is most
  of it. Worth asking them to look.
- **The systems inventory** says Work Order Intake talks to **None**. After this lab
  that is false. It is the first thing all week that changes a fact about the village.

## The seam into `13-` and into Wednesday

Question one — *what happens to dispatch when the catalog is slow, or down* — is
deliberately unanswered and is Wednesday afternoon. They now have a dependency on the
critical path of a write, and no opinion about its failure.

Do not answer it. Do not let them build retries here either; they will want to.

## Topics

Extracted into the take-home by `scripts/build-takehome.sh`. **Deliberately not in
`lab.md`** — naming the mechanism up front hands the room the answer to the question
the lab exists to pose. It is useful here for saying what is coming next.

- typed HttpClient
- service discovery
- calling a service you do not own
- OpenAPI specifications
- nullable reference types at the edge
- problem details

## Verified

Built and run against the live catalog at
`theoria.hypertheory-labs.com/clerk-records/purchasing/`, 2026-08-30:

| Vendor | Result |
|---|---|
| Dutcher | `200`, dispatched. Approved |
| Kerns | `422` — *"is suspended"*, effective 5/19/2026, with the full reason |
| **Rademacher** | `422` — *"is debarred"*, effective 5/4/2026, **and no reason at all** |
| Sable Ridge | `422` — no standing on record. Registered, never reviewed |
| Nonexistent | `422` — not a registered vendor |

`AddExternalService` with a trailing slash plus relative request paths is what makes
the client work; the leading-slash version returns `404` and is written up above as the
likely stumble.

**The app ships without any of this.** These changes were made to verify the lab and
then reverted — `work-orders/` on the image has no catalog client, which is the point
of the lab.

## Where this could go stale

The refusal messages quote real dates and real text from the catalog's seed. If
`services/purchasing-catalog/src/Seed.cs` changes, the reveal in this lab is wrong.
They are the same repository, so a grep will find it — but nothing enforces it.
