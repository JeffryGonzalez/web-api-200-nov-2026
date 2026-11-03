# practice — work that runs later

One service. No broker, no database, nothing to provision.

```bash
dotnet run --project Practice.AppHost
```

| | |
|---|---|
| `Intake` | Yours. Takes work orders and notifies the resident |
| `Practice.AppHost` | Starts it |

`intake` is on <http://localhost:5193>.

Read `venues/` before the source.

`requests.md` has every request the lab makes, as runnable blocks — open it in VS Code
with the REST Client extension if you would rather click than type `curl`.
