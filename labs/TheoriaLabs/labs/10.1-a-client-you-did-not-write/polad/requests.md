# Requests — A client you did not write

Open this file in VS Code with the **REST Client** extension and click **Send Request**
above any block. Rider and Visual Studio will run these too.

Every request here is also in the lab as a `curl` command. This is the same thing without
the shell — use whichever you prefer.

Start the AppHost first.

## Use it

Three vendors through your generated client. The third is the one that matters.

### Approved

```http
GET http://localhost:5181/standing/7ff65213-fe55-5938-02e9-35c599b82f4d
```

### Suspended by village ordinance

```http
GET http://localhost:5181/standing/ae50c76e-716a-c769-764a-b92507596344
```

### Debarred by the state

**Nothing here should throw**, and that is the finding. Look at `reason` against what the
specification says about it.

```http
GET http://localhost:5181/standing/6bbb881d-61c4-06a8-f8c7-9cdcffbe940c
```

## The catalog itself

Not required by the lab, but useful — this is the real service, over the internet, without
your code in the way.

```http
GET https://theoria.hypertheory-labs.com/clerk-records/purchasing/vendors/6bbb881d-61c4-06a8-f8c7-9cdcffbe940c/standing
```
