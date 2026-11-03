# Requests — Where parameters come from

Open this file in VS Code with the **REST Client** extension and click **Send Request**
above any block. Rider and Visual Studio will run these too.

Every request here is also in the lab as a `curl` command. This is the same thing without
the shell — use whichever you prefer.

Start the AppHost first.

## Build it

Once the endpoint is in place.

```http
GET http://localhost:5181/work-orders?page=1
```

```http
GET http://localhost:5181/work-orders?page=1&department=STR
```

## Break it on purpose

Three requests. **Write down the status code and body you expect for each** before you
send any of them.

### No `page` at all

```http
GET http://localhost:5181/work-orders
```

### A `page` that is not a number

```http
GET http://localhost:5181/work-orders?page=abc
```

### An empty `department`

**This one does not fail**, and it is the one that will reach production.

```http
GET http://localhost:5181/work-orders?page=1&department=
```
