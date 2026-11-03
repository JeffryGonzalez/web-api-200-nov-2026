# Requests — When the whole response is wrong

Open this file in VS Code with the **REST Client** extension and click **Send Request**
above any block. Rider and Visual Studio will run these too.

Every request here is also in the lab as a `curl` command. This is the same thing without
the shell — use whichever you prefer.

Start the AppHost first.

## The venue

Where the directory is currently set. It resets to `ok` whenever the service restarts.

```http
GET http://localhost:5182/mode
```

## Break it four ways

**Predict the status code and the body for all four before you send any of them.**

Each mode is a POST, then your endpoint. Four pairs.

### 1 — the directory returns `204 No Content`

```http
POST http://localhost:5182/mode/empty
```

```http
GET http://localhost:5181/departments-we-know-about
```

### 2 — a `200` carrying an HTML error page

```http
POST http://localhost:5182/mode/html
```

```http
GET http://localhost:5181/departments-we-know-about
```

### 3 — valid JSON of the wrong shape

```http
POST http://localhost:5182/mode/object
```

```http
GET http://localhost:5181/departments-we-know-about
```

### 4 — the literal JSON value `null`

**This is the one that does not fail**, which is why it is last.

```http
POST http://localhost:5182/mode/null
```

```http
GET http://localhost:5181/departments-we-know-about
```

### And the one that is not broken at all

An empty list is a legitimate answer. Compare this response to the one above it, byte for
byte, and notice that you cannot tell them apart.

```http
POST http://localhost:5182/mode/emptylist
```

```http
GET http://localhost:5181/departments-we-know-about
```

## Put it back

```http
POST http://localhost:5182/mode/ok
```
