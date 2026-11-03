# Requests — When a field is missing

Open this file in VS Code with the **REST Client** extension and click **Send Request**
above any block. Rider and Visual Studio will run these too.

Every request here is also in the lab as a `curl` command. This is the same thing without
the shell — use whichever you prefer.

Start the AppHost first.

## Confirm it before you explain it

Your endpoint. One of the departments comes back with a blank name.

```http
GET http://localhost:5181/departments-we-know-about
```

## Look at what the other service actually sent

The same data, straight from the source, before your code has touched it. Compare the two
carefully — the difference between them is the lab.

```http
GET http://localhost:5182/departments
```
