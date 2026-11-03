# Requests — The message nobody can handle

Open this file in VS Code with the **REST Client** extension and click **Send Request**
above any block. Rider and Visual Studio will run these too.

Every request here is also in the lab as a `curl` command. This is the same thing without
the shell — use whichever you prefer.

Start the AppHost first.

## Make one that cannot be done

Two work orders. **Write down what you expect in `crew`'s logs, and what each caller
gets**, before you send either.

### Ordinary work

```http
POST http://localhost:5191/assign
Content-Type: application/json

{ "number": "2026-0817", "crew": "Dale", "location": "Depot St" }
```

### Work no crew is certified for

**This one throws.** Both callers get the same status code, which is part of the point.

```http
POST http://localhost:5191/assign
Content-Type: application/json

{ "number": "2026-0819", "crew": "Ted", "location": "N. Salyer at the culvert" }
```

## Try harder, on purpose

The same culvert work order, after adding the retry policy. **Predict the number of
`ATTEMPT` lines** before you send it.

```http
POST http://localhost:5191/assign
Content-Type: application/json

{ "number": "2026-0823", "crew": "Ted", "location": "N. Salyer at the culvert" }
```

## Say it out loud instead

Both again, after the handler returns a `WorkRejected` rather than throwing. Watch
`orders` this time, not just `crew`.

```http
POST http://localhost:5191/assign
Content-Type: application/json

{ "number": "2026-0824", "crew": "Dale", "location": "Depot St" }
```

```http
POST http://localhost:5191/assign
Content-Type: application/json

{ "number": "2026-0825", "crew": "Ted", "location": "N. Salyer at the culvert" }
```
