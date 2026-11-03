# Requests — Validating what arrives

Open this file in VS Code with the **REST Client** extension and click **Send Request**
above any block. Rider and Visual Studio will run these too.

Every request here is also in the lab as a `curl` command. This is the same thing without
the shell — use whichever you prefer.

Start the AppHost first.

## Try it

### A work order that is fine

```http
POST http://localhost:5181/work-orders
Content-Type: application/json

{
  "department": "STR",
  "description": "Pothole on Depot St, eastbound lane"
}
```

### One that breaks both rules

Look for **both** fields in the response, not just the first.

```http
POST http://localhost:5181/work-orders
Content-Type: application/json

{
  "department": "S",
  "description": "pothole"
}
```

## Break it on purpose

Delete the word `public` from the record, rebuild, and send the invalid one again.

**Write down what you expect first.**

```http
POST http://localhost:5181/work-orders
Content-Type: application/json

{
  "department": "S",
  "description": "pothole"
}
```
