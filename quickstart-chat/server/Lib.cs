using SpacetimeDB;

public static partial class Module
{
    // Tables

    [Table(Name = "user", Public = true)]
    public partial class User
    {
        [PrimaryKey]
        public Identity Identity;
        public string? Name;
        public bool Online;
    }

    [Table(Name = "message", Public = true)]
    public partial class Message
    {
        public Identity Sender;
        public Timestamp Sent;
        public string Text = "";
    }


    // Reducers

    [Reducer]
    public static void SetName(ReducerContext ctx, string name)
    {
        Log.Info($"SetName {ctx.Sender} {name}");
        name = ValidateName(name);

        var user = ctx.Db.user.Identity.Find(ctx.Sender);
        if (user is not null)
        {
            user.Name = name;
            ctx.Db.user.Identity.Update(user);
        }
    }

    [Reducer]
    public static void SendMessage(ReducerContext ctx, string text)
    {
        Log.Info($"SendMessage {ctx.Sender} {text}");
        text = ValidateMessage(text);
        Log.Info(text);
        ctx.Db.message.Insert(
            new Message
            {
                Sender = ctx.Sender,
                Text = text,
                Sent = ctx.Timestamp,
            }
        );
    }

    [Reducer(ReducerKind.ClientConnected)]
    public static void ClientConnected(ReducerContext ctx)
    {
        Log.Info($"ClientConnected {ctx.Sender}");
        var user = ctx.Db.user.Identity.Find(ctx.Sender);
        if (user != null) {
            
        }

        if (user is not null)
        {
            Log.Info($"{user.Name} is now online.");
            user.Online = true;
            ctx.Db.user.Identity.Update(user);
        }
        else
        {
            ctx.Db.user.Insert(
                new User
                {
                    Name = null,
                    Identity = ctx.Sender,
                    Online = true,
                }
            );
        }
    }

    [Reducer(ReducerKind.ClientDisconnected)]
    public static void ClientDisconnected(ReducerContext ctx)
    {
        Log.Info($"ClientDisconnected {ctx.Sender}");
        var user = ctx.Db.user.Identity.Find(ctx.Sender);

        if (user is not null)
        {
            Log.Info($"{user.Name} is now offline.");
            user.Online = false;
            ctx.Db.user.Identity.Update(user);
        }
        else
        {
            Log.Warn("Warning: No user found for disconnected client.");
        }
    }
    // Functions

    private static string ValidateName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new Exception("Names must not be empty");
        }
        return name;
    }

    private static string ValidateMessage(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            throw new ArgumentException("Messages must not be empty");
        }
        return text;
    }
}