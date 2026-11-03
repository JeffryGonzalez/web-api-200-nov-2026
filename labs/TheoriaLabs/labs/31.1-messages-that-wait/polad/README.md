# practice — messaging

Two services, a message broker, and one service that has never been started.

```bash
dotnet run --project Practice.AppHost
```

| | |
|---|---|
| `Orders` | Yours. Assigns work |
| `Crew` | Yours too. Finds out about it |
| `Notifications` | Written months ago. Has never run. Start it when you want to |
| `Practice.Contracts` | The one thing they all agree on |
| `Practice.AppHost` | Starts them, and NATS |

`orders` is on <http://localhost:5191>, `crew` on <http://localhost:5192>,
`notifications` on <http://localhost:5194>.

Read `venues/` before the source.

`requests.md` has every request the lab makes, as runnable blocks — open it in VS Code
with the REST Client extension if you would rather click than type `curl`.
