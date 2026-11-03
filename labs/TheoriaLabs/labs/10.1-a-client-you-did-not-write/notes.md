# Notes — a client you did not write

**Forty minutes, so budget eighty.** Twelve `##` steps. The meatiest optional in the
course and the only one with a prerequisite outside the SDK.

**Optional, stretch one.** Third thing on Monday, after `11.1-` and `12.1-`, and the one
to save for somebody who has finished both. It is also the only optional that reaches
back to `10-`, so it wants Tuesday morning done — or Monday, if the room is unusual.

**Polad: its own.** A single-service practice solution — `Dispatch` under
`Practice.AppHost` — plus a copy of the real `catalog-openapi.yaml` at the root. It is
not `10-`'s finished state: that lab lands in `work-orders/`, and **nothing optional
touches the engagement app**. This one generates a client in `practice/` against the same
real specification and deliberately stops short of putting it in the app, which is
question one.

**Requires the internet and the live catalog.** See *Before you teach this*, below.

## Before you teach this

Two things to do the morning of, not at 9:01:

```bash
dotnet tool install --global Microsoft.OpenApi.Kiota
curl -s -o /dev/null -w '%{http_code}\n' \
  https://theoria.hypertheory-labs.com/vendors/6bbb881d-61c4-06a8-f8c7-9cdcffbe940c/standing
```

Kiota is **not on the VM image**. It installs in about twenty seconds and the lab says so,
but sixteen people installing it at once is a different experience from one person.

The catalog must be up. This is the only lab in the course with no offline path — if the
network is out, hand somebody `12.1-` instead and come back to it.

## The one thing that doesn't get cut

**Reading the generated model, with the six required field names written down first.**

They will predict that required fields come out non-nullable and optional ones nullable,
because that is what the document says and it is the only reason to read a `required`
list. Every property is nullable. `required` appears nowhere in the output.

That is the lab. The call to the live catalog afterwards is confirmation, and it is worth
having, but the finding is in the file.

## Before: what you set up

**The belief you plant:** that a generated client is *more* faithful to the specification
than a hand-written one, because a machine read the document. It is more faithful in three
of the four roles and refuses to have an opinion in the fourth, and that is not the shape
anybody expects.

**Hand off on:** *"You wrote this client by hand on Tuesday. There has been a
specification for it the whole time. Let a machine do it and see what you get."*

Do not say anything about `required` before they read the spec. The prediction only works
if the six names are theirs.

## After: what you land

The mechanism is in the lab. What stays yours is the pair, and it is worth drawing on a
board because it is genuinely symmetrical:

> A hand-written type at the edge **makes a promise the service does not keep.** A
> generated one **refuses to make any promise at all.** Neither tells you what is true.
> Only the first ever makes you ask.

And the sentence that connects it to the rest of the week: **generated code is code, and
you are responsible for it.** This is the third generator they have met — validation on
Tuesday, Wolverine on Wednesday, this one. All three write code that is not in their
files, all three have rules they did not choose, and all three fail by silence rather than
by error.

If the room has met the other two, say it here, because this is the instance where the
generator's silent policy is a *judgement about their domain* rather than a technical
limitation.

## Going wrong on purpose

**Theirs, arranged:** the prediction about the generated model.

**Yours, ninety seconds, and it is the best version of the point:** ask what the generated
code would look like if the specification had been *correct* and the service well behaved.
Identical. Then ask what a generated client is evidence for. It is question two, and it is
better asked out loud than read.

## Where it goes wrong

- **`kiota` not on `PATH` after install.** Global tools land in `~/.dotnet/tools`, and a
  shell opened before the install will not have it. Symptom is `command not found`
  immediately after a successful install. Fix is a new terminal.
- **They generate into the project root.** `-o ./Catalog` keeps it in one folder. Without
  it the generated files land beside `Program.cs` and the project is unreadable.
- **`--clean-output` on a regenerate.** Without it, stale files from a previous run stay
  and the build breaks in a confusing way. With it, anything they hand-edited in that
  folder is destroyed — which is a real and useful thing to have happen once.
- **Kiota version drift.** I verified on 1.34.1. `kiota --version` after a stale install
  can report an older number than the one just installed; if the output looks different
  from the reveals, check the version first.
- **"Why is `Status` an enum but nullable?"** Same policy. The generator will happily
  give you a strong type and then refuse to say it is always present.
- **Somebody wants to fix the spec.** Good instinct, wrong lab. They do not own it, and
  `10-`'s whole point is that the wrong contract is a finding to report rather than a bug
  to fix. Point at the Clerk's office.

## Why this one is optional, and should stay optional

Kiota is an edge-case machine. Ninety percent of a specification generates cleanly and
the last ten percent produces questions with no satisfying answers — unsupported
formats, odd names, discriminated unions, and the `required` behaviour this lab is built
on. Two warnings already come out of our own spec at generation time, for `format: email`
and `format: uri`, and neither means anything is wrong.

That is exactly the wrong shape for a floor lab: the questions are numerous, specific to
the tool, and not about anything this course teaches. **It is the right shape for
something somebody does while you are helping other people**, because the interesting
part — a generator discarding the contract — survives without you in the room, and the
tool's quirks are theirs to shrug at rather than yours to explain sixteen times.

If somebody asks why we are not using it everywhere, that is the honest answer and it is
worth giving: the client it writes is better than the one you would type, and the
experience of getting there is uneven enough that it should be a decision rather than a
default.

## Casting review

**After the venue note**, marked as opinion. The role is *know the shape of what the
catalog returns*:

| | Gets right | Gets wrong |
|---|---|---|
| **Hand-written** | says what you believe, and you had to think to write it | drifts the moment they change anything; claims guarantees you cannot enforce |
| **Generated** | never drifts, three roles free, address from the spec | encodes no guarantees at all, and hides that the contract was wrong |
| **Generated, plus your own type over it** | both | two things to maintain, and a boundary somebody has to respect |

My ranking, as a ranking: **generate it, and then write the six lines that say what you
actually believe.** The generated client is a transport concern and it is better than
anything you would type. The beliefs are domain knowledge and they are yours — including
*"the catalog says `reason` is required and we have seen it null"*, which is not something
either the spec or the generator can hold for you.

The thing I would not do is what most teams do, which is generate it and treat the output
as the contract. That is how the defect in this lab stays invisible for two years.

## Connective tissue

- **`12-`, directly and by contrast.** Its two candidates were `required` and nullable,
  and the lab argued they combine. This is a third party choosing nullable for everything
  on your behalf, and the reveal table sets them side by side.
- **`10-`, which is where the defect was found the hard way.** If somebody did `10-`, this
  is the lab that explains why they found it: they hand-wrote a type that asserted
  something. Had they generated, they would not have.
- **The generators beat in `narrative.md`.** This is the third instance and the one where
  the policy is most clearly a judgement rather than a limitation.

## Topics

Extracted into the take-home by `scripts/build-takehome.sh`. **Deliberately not in
`lab.md`** — naming the mechanism up front hands the room the answer to the question
the lab exists to pose. It is useful here for saying what is coming next.

- OpenAPI client generation
- Kiota
- generated code
- IParsable
- required vs nullable in a schema

## Verified

Verified 2026-09-02. Kiota 1.34.1, `Microsoft.Kiota.Bundle` 2.0.0, .NET 10, against the
**live** catalog at `https://theoria.hypertheory-labs.com/clerk-records/purchasing`.

Generated `VendorStanding`, in full:

```csharp
public StandingAuthority? Authority { get; set; }
public Date? EffectiveDate { get; set; }
public Date? ExpiresOn { get; set; }
public string? Reason { get; set; }
public Date? ReviewedOn { get; set; }
public StandingStatus? Status { get; set; }
public Guid? VendorId { get; set; }
```

The spec's `required` list is `[vendorId, status, effectiveDate, authority, reason,
reviewedOn]` — six fields, all generated nullable, indistinguishable from `expiresOn`
which the spec marks optional.

Through the generated client, live:

| Vendor | Result |
|---|---|
| Bilyeu Paving (`7ff65213…`) | `Approved` / `ClerkReview` / reason present |
| Kerns Excavating (`ae50c76e…`) | `Suspended` / `VillageOrdinance` / reason present |
| Rademacher Traffic Control (`6bbb881d…`) | `Debarred` / `StateDebarmentList` / **`reason: null`** |

**No exception, `200` in all three cases.** The generated client tolerates the contract
violation silently, which is the whole lab.

Vendor ids are stable — they are deterministic in the catalog's seed — but they are quoted
in the lab text and would break if the seed changed.

### I expected the opposite

I proposed this lab on the theory that a generated client would model `reason` as
non-nullable, faithfully implement the wrong contract, and **throw** on the state-debarred
vendor. That would have been a tidy story about generated code enforcing a bad
specification.

It does the reverse, and the real behaviour is a better lab: the generator's refusal to
encode `required` means the defect cannot be found from the generated code at all. The
tidy story would also have been wrong in front of a room.

### Not guarded by a claim

Three things here could move and the lab depends on all of them: Kiota's policy on
`required` (a generator version could start honouring it, which would flip the entire
lesson), the live catalog being up, and the seeded vendor ids. The first is the one worth
a tripwire — generate into a temp directory and assert `Reason` is nullable.
