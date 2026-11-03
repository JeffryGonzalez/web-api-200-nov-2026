# Requests — Work that runs later

Open this file in VS Code with the **REST Client** extension and click **Send Request**
above any block. Rider and Visual Studio will run these too.

Every request here is also in the lab as a `curl` command. This is the same thing without
the shell — use whichever you prefer.

Start the AppHost first.

## Feel it first

**Watch the response time on this one.** The REST Client shows it in the top-right of the
response pane. The caller is waiting for work that has nothing to do with them.

```http
POST http://localhost:5193/work-orders
Content-Type: application/json

{ "number": "2026-0817", "resident": "Harold Mink", "location": "Depot St" }
```

## Watch it

After moving the notification off the request thread. Same request, and the number in the
corner should be different.

```http
POST http://localhost:5193/work-orders
Content-Type: application/json

{ "number": "2026-0818", "resident": "Harold Mink", "location": "Depot St" }
```

## Break it on purpose

A clipboard entry with no resident on it. **This one should take the service down** —
predict what the next request does before you send it.

```http
POST http://localhost:5193/work-orders
Content-Type: application/json

{ "number": "2026-0819", "resident": "", "location": "N. Salyer at the culvert" }
```

```http
GET http://localhost:5193/
```

## Now fix it the way everybody fixes it

After setting `BackgroundServiceExceptionBehavior.Ignore`. The bad one, then a good one.

**Predict again.** You have configured it not to stop the host. What happens to Harold's
notification?

```http
POST http://localhost:5193/work-orders
Content-Type: application/json

{ "number": "2026-0820", "resident": "", "location": "N. Salyer at the culvert" }
```

```http
POST http://localhost:5193/work-orders
Content-Type: application/json

{ "number": "2026-0821", "resident": "Harold Mink", "location": "Depot St" }
```

```http
GET http://localhost:5193/
```
