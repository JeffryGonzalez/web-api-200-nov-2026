# Requests — Two of the same service

Open this file in VS Code with the **REST Client** extension and click **Send Request**
above any block. Rider and Visual Studio will run these too.

Every request here is also in the lab as a `curl` command. This is the same thing without
the shell — use whichever you prefer.

Start the AppHost first.

The lab sends four of these in a shell loop. Here they are as four blocks — send each one
once, then count the log lines in **both** `crew-0` and `crew-1` and add them together.

**Predict the total before you start.**

```http
POST http://localhost:5191/assign
Content-Type: application/json

{ "number": "2026-0801", "crew": "Dale", "location": "Depot St" }
```

```http
POST http://localhost:5191/assign
Content-Type: application/json

{ "number": "2026-0802", "crew": "Dale", "location": "Depot St" }
```

```http
POST http://localhost:5191/assign
Content-Type: application/json

{ "number": "2026-0803", "crew": "Dale", "location": "Depot St" }
```

```http
POST http://localhost:5191/assign
Content-Type: application/json

{ "number": "2026-0804", "crew": "Dale", "location": "Depot St" }
```

Then add `UseQueueGroup("crew")`, restart, and send the same four again. The total should
change; the split between the two instances will not be even.
