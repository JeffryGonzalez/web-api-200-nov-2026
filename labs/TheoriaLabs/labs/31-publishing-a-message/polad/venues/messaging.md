# Messaging

Everything below is where this project **differs from what you'd assume**. If you'd
have guessed it, it isn't here.

## NATS is running and nothing uses it

**The role:** something has to carry a message from one service to another.

**How we cast it:** NATS, started by the AppHost.

Worth knowing because **no code publishes to it or listens on it yet.** It has been
provisioned in advance, which is a normal thing to find in a codebase and an odd thing
to notice.

## Neither service references the other

`Orders` and `Crew` are independent. They share `Practice.Contracts`, and that is the
only thing they have in common.

Worth knowing because it is the arrangement that makes messaging worth the trouble. If
one referenced the other you would call a method and be done.

## Ports are pinned

`orders` on 5191, `crew` on 5192, fixed in `launchSettings.json` so instructions can
name a real URL. Only one lab runs at a time.

## Wolverine writes code at startup, and the compiler is a separate package

**The role:** turn a handler method into something the runtime can call, without you
registering it.

**How we cast it:** `WolverineFx.Nats` for the transport, and **`WolverineFx.RuntimeCompilation`**
alongside it. Both are already referenced by `Orders` and `Crew`.

Worth knowing because the second one is not optional and is easy to miss. Wolverine
generates handler code when the service starts, and as of version 6 the thing that
compiles it moved into its own package. Without it a service fails at startup with
*"running in TypeLoadMode.Dynamic ... but no IAssemblyGenerator (Roslyn) is registered"*.

**Every Wolverine package has to be the same version.** They are not always published in
step, so taking the newest of each can fail to restore with `NU1102` naming a version
that does exist. Ours are pinned together in the `.csproj`.
