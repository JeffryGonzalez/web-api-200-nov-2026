# Requests — Failing faster than the default

Open this file in VS Code with the **REST Client** extension and click **Send Request**
above any block. Rider and Visual Studio will run these too.

Every request here is also in the lab as a `curl` command. This is the same thing without
the shell — use whichever you prefer.

Start the AppHost first.

**This lab is about how long things take.** The lab's `curl` commands print the elapsed
time with `-w '%{time_total}'`; the REST Client response pane shows it for you in the
top-right, so you can use these and read the number off the screen.

## Feel it first

Make the directory slow, then call your endpoint.

```http
POST http://localhost:5182/delay/30
```

```http
GET http://localhost:5181/departments-we-know-about
```

Thirty seconds, then a `500`. Now something it can survive:

```http
POST http://localhost:5182/delay/3
```

```http
GET http://localhost:5181/departments-we-know-about
```

## Change it where the client is

After adding the options at the call site. **Predict the number before you send it.**

```http
POST http://localhost:5182/delay/30
```

```http
GET http://localhost:5181/departments-we-know-about
```

## The safety feature that cannot fire

Send this one **six times** and watch the timings. Does the circuit breaker open?

```http
GET http://localhost:5181/departments-we-know-about
```

## Make it fire

Same six calls, after lowering `MinimumThroughput`. Around the third the time should drop
to nothing.

```http
GET http://localhost:5181/departments-we-know-about
```

Then make the directory healthy again and send it immediately. **This one should still
fail**, fast, against a service that is fine — that is the part worth seeing.

```http
POST http://localhost:5182/delay/0
```

```http
GET http://localhost:5181/departments-we-know-about
```
