# Requests — Messages that wait

Open this file in VS Code with the **REST Client** extension and click **Send Request**
above any block. Rider and Visual Studio will run these too.

Every request here is also in the lab as a `curl` command. This is the same thing without
the shell — use whichever you prefer.

Start the AppHost first.

## Make the crew a durable consumer

Confirm it still works normally before breaking anything.

```http
POST http://localhost:5191/assign
Content-Type: application/json

{ "number": "2026-0819", "crew": "Ted", "location": "N. Salyer at the culvert" }
```

## Watch it survive

**Stop `crew` in the dashboard**, send this, then start `crew` again.

**Write down what you expect first.** Last time this message was lost.

```http
POST http://localhost:5191/assign
Content-Type: application/json

{ "number": "2026-0817", "crew": "Dale", "location": "Depot St" }
```

## A message nobody minds losing

Nothing is listening for shift notes yet. That is fine.

```http
POST http://localhost:5191/shift-note
Content-Type: application/json

{ "crew": "Ted", "note": "Culvert grate is loose, flag it" }
```

## Start the service that has never run

Send both of these **while `notifications` is still switched off**.

```http
POST http://localhost:5191/assign
Content-Type: application/json

{ "number": "2026-0821", "crew": "Dale", "location": "Third and Miami" }
```

```http
POST http://localhost:5191/shift-note
Content-Type: application/json

{ "crew": "Dale", "note": "Back gate is chained, use Sixth" }
```

**Now predict**, specifically: when you press Start on `notifications`, which of the two
message types appears in its log, and how many of each?

Then start it. Afterwards, send a shift note **while it is running** — this one arrives
immediately, which shows the listener was never broken.

```http
POST http://localhost:5191/shift-note
Content-Type: application/json

{ "crew": "Ted", "note": "Wallboard test" }
```

## Break it on purpose

After taking the JetStream line back off `crew`. Stop `crew`, send this, start `crew`.

The stream still exists. The subject still matches. **The message is being held right
now** — and `crew` will not see it.

```http
POST http://localhost:5191/assign
Content-Type: application/json

{ "number": "2026-0822", "crew": "Ted", "location": "Kossuth Ave" }
```
