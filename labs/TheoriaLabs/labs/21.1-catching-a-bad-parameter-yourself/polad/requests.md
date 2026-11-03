# Requests — Catching a bad parameter yourself

Open this file in VS Code with the **REST Client** extension and click **Send Request**
above any block. Rider and Visual Studio will run these too.

Every request here is also in the lab as a `curl` command. This is the same thing without
the shell — use whichever you prefer.

Start the AppHost first.

## Try all four

After the custom `TryParse` type is in place.

### No page — no longer an error

```http
GET http://localhost:5181/work-orders
```

### A real page

```http
GET http://localhost:5181/work-orders?page=2
```

### Not a number

**This should now be `problem+json` with your own words in it.**

```http
GET http://localhost:5181/work-orders?page=abc
```

### Empty

The case the previous lab left open. It should be caught now.

```http
GET http://localhost:5181/work-orders?page=
```

## Candidate two: the line you would expect to work

After putting `page` back to a plain `int` and adding `AddProblemDetails()`.

```http
GET http://localhost:5181/work-orders?page=abc
```

## Now run it the way it will actually run

Same request, same code, service started as Production:

```bash
ASPNETCORE_ENVIRONMENT=Production dotnet run --project Orders --no-launch-profile --urls http://localhost:5181
```

**Predict before you send it.** You changed nothing.

```http
GET http://localhost:5181/work-orders?page=abc
```
