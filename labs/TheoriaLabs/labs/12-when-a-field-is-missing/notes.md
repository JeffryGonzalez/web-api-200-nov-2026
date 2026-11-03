# Notes — when a field is missing

**Twenty-five minutes, so budget fifty.** Nine `##` steps.

**Polad: lab 11's finished state.** This is worth understanding about the numbering —
`11-` and `12-` are siblings rather than a chain, because both are stretch-one floor
labs and both are required. The *number* says which stretch and whether it is required;
the *polad* says what state you start from. A sibling can still follow another in time.

Stretch 1, floor. `10-` cannot run without it, and this is the half people will want to
cut.

## The one thing that doesn't get cut

**The half of that type you can't see** — that `string Name` is a compile-time
annotation with no runtime effect, and `System.Text.Json` will hand you a null anyway.

Everything before it is a bug hunt. That step is the only moment where *the type system
did not protect me at the edge and I assumed it had* becomes a thing they saw.

If the clock is gone, cut *How you'd choose* and give it as a sentence instead. Keep the
venue note.

## Before: what you set up

They need the practice repo running with lab 11's endpoint working. Nothing else.

**The belief you plant, and it is a real misdirection:** that this is a bug in the other
service. Say the report the way a colleague would say it — *"the list is showing a blank
name"* — and let them go looking downstream. Most rooms check their own code last, which
is exactly right and is why the lab starts by checking upstream.

**Hand off on:** *"Somebody says one row has a blank name. That's all they said. Go
find out what's true."*

## After: what you land — and this part is yours

The lab explains the mechanism. What it does not do, and should not, is connect it to
the week.

Say this after, in your own words: **they are about to call a service that publishes a
specification, and that specification says a field is required.** Do not say which
field, do not say which service, and do not warn them. The last question in the lab
sets it up and then deliberately does not answer it.

This is the lab that makes `10-` land instead of frustrate. A room that has met a
silent null once will recognise the second one in about a minute. A room that has not
will spend forty minutes on it and learn less.

## Going wrong on purpose

**Theirs, and it is arranged:** the practice repo's `directory` service returns one
department with no `name` property at all — not `"name": null`, absent. That matters,
because someone will check for null in the JSON and not find one.

**Yours, if the room is quick:** add `"name": null` to a second record and show that it
behaves identically from the consumer's side, then note that those two payloads mean
different things to almost every producer and the same thing to `System.Text.Json`
unless you configure it otherwise.

## Where it goes wrong

- **They fix it before understanding it.** Someone will make `Name` nullable in the
  first three minutes, see the warnings go away, and call it done. Say: *"put it back,
  I want you to see why it was allowed."*
- **They blame the other service.** Predictable and useful — it is the misdirection
  working. The correction is in the lab and it is worth reading aloud: *if it never
  promised, it never broke a promise.*
- **`required` and records.** If somebody keeps the positional record and tries to add
  `required`, it does not work the way they expect. The lab switches to init properties
  for this reason. If they ask, `[property: JsonRequired]` is the positional-record
  answer and it is a fine tangent for one minute.
- **"Why doesn't the compiler catch this?"** The best question in the lab. It does not
  because deserialisation happens at runtime and the compiler is not there. Resist
  going further — analyzers and source generators are a different afternoon.

## The demo this lab wants

**None.** They are debugging, and watching somebody else debug is the least useful
thing in the room.

If you want to add something afterwards, the thirty-second version is showing the raw
JSON next to the C# record with the missing property highlighted. Two windows, no
narration.

## Connective tissue — what this makes legible

- **`11-`, retroactively.** Its first step asked *is every field present on every
  record?* and nobody could answer it usefully. Now they can, and they know why it was
  worth asking.
- **The venue note from `11-`** was about addresses coming from outside the code. This
  one is about types describing the wire. Both are the same shape: **the edge is not
  the same place as the inside**, and things that are true inside stop being true there.
- **Forward into `10-`.** The catalog's specification says `reason` is required. It is
  not always sent. They will meet that with their own hands.

## The seam into `10-`

The lab's last question is *"a service publishes a specification saying a field is always
present — how much would you like to bet?"* and it is deliberately unanswered.

Do not answer it. It is answered by the catalog, on Wednesday, by them.

## What this lab doesn't clarify

- **Anything about validation frameworks.** FluentValidation, DataAnnotations, minimal
  API filters — all real, all a different lab, and all beside the point here, which is
  what the *type* does and does not do.
- **Whether the other service should have published a contract.** Raised in question one
  and not resolved. It is a genuinely open question and the honest answer depends on
  who owns it.
- **Configuring `System.Text.Json` more strictly.** There are knobs. They are latitude,
  not floor.

## Topics

Extracted into the take-home by `scripts/build-takehome.sh`. **Deliberately not in
`lab.md`** — naming the mechanism up front hands the room the answer to the question
the lab exists to pose. It is useful here for saying what is coming next.

- System.Text.Json
- nullable reference types
- required members
- record types
- deserialising a contract you do not own

## Verified

Run against .NET 10 with `System.Text.Json`, on the exact types the lab uses:

| Payload | Target | Result |
|---|---|---|
| `name` absent | positional record, `string Name` | `Name` is `null`. **No exception** |
| `"name": null` | same | `Name` is `null`. No exception, and **indistinguishable from absent** |
| `name` absent | `required string Name` | Throws `JsonException`, and the message names the property |
| `"name": null` | `required string Name` | **Does not throw.** `Name` is `null` |

**The fourth row is the one to know.** `required` is a presence check, not a null check,
and a room that has just been told it "fixes" the problem will believe it fixes both
cases. The lab now makes them try the null payload against it rather than telling them.

The second row is the *"yours, if the room is quick"* demo above, and it is confirmed:
absent and explicitly null mean different things to almost every producer and the same
thing to the consumer here.

**Still to verify against the practice repo** once it exists: that the `directory`
service omits the property rather than sending null, since the lab's opening bug hunt
depends on which one it is.
