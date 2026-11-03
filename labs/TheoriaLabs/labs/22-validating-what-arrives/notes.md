# Notes — validating what arrives

**Twenty-five minutes, so budget fifty.** Ten `##` steps.

**Polad: `21-`'s finished state, and it is built.** Continuation, not a sibling — it
opens with a `Builds on` line because the endpoint from `21-` is where this one starts.
It is `21-`'s polad plus the `GET /work-orders` endpoint, and plus the binding entry in
`venues/http.md` that `21-` has the student write.

**That venue note being in here is a departure from `12-`'s polad**, which does not
carry forward the note `11-` has the student write. Included here because `22-`'s own
text says *"add this beneath the binding entry you added last time"*, and a student
who takes the fresh polad would find nothing to add it beneath. Worth settling as a
general rule: either a polad is the previous lab's finished state including its venue
note, or venue notes are the student's artifact and never shipped. It cannot be both.

**`WorkOrders.Add` takes `(department, description)`, not a `NewWorkOrder`.** It has to:
`NewWorkOrder` is the type the student writes *in this lab*, so the store cannot
reference it. The endpoint in the lab text calls
`orders.Add(order.Department, order.Description)` for that reason, and it is also the
better shape — the store has no business knowing about a request DTO.

Stretch 2, floor. With `21-` that makes a floor of two, which is the cap.

## The one thing that doesn't get cut

**Break it on purpose** — move the record into `Program.cs` and watch validation
silently stop happening.

Everything before it is configuration they could get from documentation. That step is
the only moment where *a check I thought I had was gone and nothing told me* becomes a
thing they saw. It is also the single most likely thing in this lab to bite them at
work, because putting a small type at the bottom of `Program.cs` is a completely
reasonable instinct.

## Before: what you set up

**The belief you plant:** that `AddValidation()` is a runtime service registration.
Everything about the call site says so — it looks exactly like the twenty other
`builder.Services.Add*` lines above it. Do not correct that assumption. The break step
depends on it being held.

**Hand off on:** *"One line and some attributes. This is the easiest thing you'll do
today."* It is, right up until it isn't.

## After: what you land

The lab explains the generator. What stays yours is the size of it: **there is now a
category of C# where file placement changes behaviour**, and their instincts about
what is cosmetic are out of date. Say it once, plainly, and then say that this is
recent and will probably improve — it is a rough edge, not a design principle.

Worth adding, if the room is the kind that likes it: this is what a source generator
*is*, and they now have a reason to care about the category rather than a definition.

## Going wrong on purpose

**Theirs, and it is arranged:** the break step. They will assume they broke something
else and start re-reading the attributes.

**Yours, thirty seconds, if the room wants proof:** build with
`/p:EmitCompilerGeneratedFiles=true` and show that the generated validation file exists
in one arrangement and not in the other. Nothing is more convincing than the absence of
a file.

## Where it goes wrong

- **They add the package.** Somebody will find `Microsoft.Extensions.Validation` on
  NuGet and install it. It is unnecessary — the analyzer ships in the framework
  reference pack — and it does no harm. Worth mentioning so nobody thinks it was the fix.
- **They add `InterceptorsNamespaces` to the csproj.** Currently documented, currently
  unnecessary; the SDK already passes it. Same answer: harmless, not the fix. If they
  found it in the docs, that is a good moment for
  `sidebars/reading-a-platform-trajectory.md` — a feature this new has documentation
  that lags it.
- **"Why doesn't it just warn me?"** The best question in the lab. There is no good
  answer and you should say so; it is a genuine rough edge in a new feature.
- **Nested types.** A type reached only through another type may need
  `[ValidatableType]` for the generator to consider it. Latitude, not floor.

## The demo this lab wants

**None to start**, and specifically do not demonstrate the break — it only works if
they do it themselves and are surprised.

The generated-file demo above is a good *afterwards*, for the sceptical.

## Connective tissue

- **`12-`, directly.** Same shape: something that reads as a guarantee, is not enforced
  where you assumed, and fails silently. There it was `string Name`; here it is a
  validation attribute on a type the generator never saw.
- **`21-`'s venue note** said binding failures are not `problem+json`. This lab adds a
  second exception, and the two together make the point that the note was making a
  claim that was already only mostly true. That is the note working, not failing.
- **Forward into `20-`.** The intake channels all accept data from outside. Every one of
  them needs this.

## The seam into `20-`

Question two — where does a *content* rule live, as opposed to a *shape* rule — is the
application lab's problem. A phoned-in work order can be perfectly well-formed and
still wrong, and no attribute will catch it.

Do not answer it here.

## Topics

Extracted into the take-home by `scripts/build-takehome.sh`. **Deliberately not in
`lab.md`** — naming the mechanism up front hands the room the answer to the question
the lab exists to pose. It is useful here for saying what is coming next.

- AddValidation
- DataAnnotations
- source generators
- type accessibility
- validation problem responses
- record types

## Verified

Run against .NET 10.0.400, stock `dotnet new web`, model and endpoint as the lab has
them:

| Arrangement | Result |
|---|---|
| `public record`, its own file | **Validated.** `400` with an `errors` dictionary naming every failing field |
| `public record`, after the top-level statements in `Program.cs` | **Validated.** Placement does not matter |
| `record` (no modifier, therefore `internal`), same file | **Silently not validated.** `201`, invalid data accepted |
| `Microsoft.Extensions.Validation` package added | No change — the analyzer ships in `Microsoft.AspNetCore.App.Ref` |
| `InterceptorsNamespaces` set per the docs | No change — the SDK already passes `Microsoft.Extensions.Validation.Generated` |
| Validation failure response | `application/json`, **not** `application/problem+json` |

### I got this wrong first, and it is worth knowing why

The lab originally said the break was **file placement** — that the generator does not
find types declared after top-level statements. That is false, and I believed it
because my first probe changed two things at once: it moved the model to its own file
*and* made it `public`. I credited the move.

`claims/check-validation-placement.sh` caught it within the hour, which is the
argument for that directory existing.

The real rule is accessibility, and it makes a better lab: `record Dept { }` with no
modifier is exactly what people write for a type one project uses, and `public` on such
a type reads as noise somebody should tidy away.

**This is a defect-dependent claim.** If a future SDK teaches the generator to handle
internal types, the break step stops breaking and this lab needs a new lesson rather
than a new number. `claims/check-validation-placement.sh` is the tripwire.

### Re-verified against the polad, 2026-09-01

Built from this lab's polad, following the lab text exactly:

| | |
|---|---|
| `public record`, own file, valid POST | `201`, `Location: /work-orders/2026-0214` |
| same, invalid POST | `400`, `application/json`, **both** fields named |
| `record` with `public` deleted, invalid POST | **`201`.** Silently accepted |

The seeded ids run to `2026-0213`, so the first created work order is `2026-0214`. If
that number appears in a future screenshot or lab text, it is load-bearing.
