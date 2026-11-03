# Requests — Publishing a message

Open this file in VS Code with the **REST Client** extension and click **Send Request**
above any block. Rider and Visual Studio will run these too.

Every request here is also in the lab as a `curl` command. This is the same thing without
the shell — use whichever you prefer.

Start the AppHost first.

## Watch it cross

Then open the Aspire dashboard, go to **crew**, and look at its logs.

```http
POST http://localhost:5191/assign
Content-Type: application/json

{
  "number": "2026-0819",
  "crew": "Ted",
  "location": "N. Salyer at the culvert"
}
```

## Break it on purpose

**Stop `crew` in the dashboard first.**

**Write down what you expect** — both what this returns now, and what `crew` will do when
you start it again.

```http
POST http://localhost:5191/assign
Content-Type: application/json

{
  "number": "2026-0817",
  "crew": "Dale",
  "location": "Depot St"
}
```

Then start `crew` and look at its logs. Send one more afterwards — **it will arrive
normally**, which is the part that makes this dangerous.

```http
POST http://localhost:5191/assign
Content-Type: application/json

{
  "number": "2026-0821",
  "crew": "Dale",
  "location": "Third and Miami"
}
```
