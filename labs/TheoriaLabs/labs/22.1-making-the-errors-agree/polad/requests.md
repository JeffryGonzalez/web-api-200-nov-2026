# Requests — Making the errors agree

Open this file in VS Code with the **REST Client** extension and click **Send Request**
above any block. Rider and Visual Studio will run these too.

Every request here is also in the lab as a `curl` command. This is the same thing without
the shell — use whichever you prefer.

Start the AppHost first.

## The venue: three shapes

Send all three and compare the `Content-Type` of each response. Same status code, three
different bodies.

### The body is not JSON at all

```http
POST http://localhost:5181/work-orders
Content-Type: application/json

{"department":
```

### The body is fine, the data is not

```http
POST http://localhost:5181/work-orders
Content-Type: application/json

{
  "department": "S",
  "description": "short"
}
```

### A query parameter that will not parse

```http
GET http://localhost:5181/work-orders?page=abc
```

## The obvious tool

With the probe filter attached. **Predict what appears in the log for each** before you
send them — a valid request and an invalid one.

```http
POST http://localhost:5181/work-orders
Content-Type: application/json

{
  "department": "STR",
  "description": "Pothole on Depot St eastbound"
}
```

**This one will not log anything at all.** That is the lab.

```http
POST http://localhost:5181/work-orders
Content-Type: application/json

{
  "department": "S",
  "description": "short"
}
```

## The rule no attribute can express

A perfectly well-formed work order, rejected for what it says.

```http
POST http://localhost:5181/work-orders
Content-Type: application/json

{
  "department": "STR",
  "description": "Harold Mink called again about the hole"
}
```

## The one you cannot fix here

Still `text/plain`, no matter what you attach to the endpoint. Binding happens before any
filter exists.

```http
POST http://localhost:5181/work-orders
Content-Type: application/json

{"department":
```
