# Requests — Calling another service

Open this file in VS Code with the **REST Client** extension and click **Send Request**
above any block. Rider and Visual Studio will run these too.

Every request here is also in the lab as a `curl` command. This is the same thing without
the shell — use whichever you prefer.

Start the AppHost first.

## Look at what you're calling

The other service, directly. Read the shape before you write anything against it — and
look at whether every field is present on every record.

```http
GET http://localhost:5182/departments
```

## Build it

Your endpoint, once the typed client is in place.

```http
GET http://localhost:5181/departments-we-know-about
```

## Break it on purpose

Stop `directory` in the Aspire dashboard first, then send this.

**Write down what you expect before you click Send.** It will take about thirty seconds
to answer, which you will notice more here than in a terminal — the response pane sits
there spinning.

```http
GET http://localhost:5181/departments-we-know-about
```

Bring `directory` back up before you move on.
