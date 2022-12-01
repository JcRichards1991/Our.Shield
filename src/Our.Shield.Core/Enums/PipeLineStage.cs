namespace Our.Shield.Core.Enums
{
    public enum PipeLineStage
    {
        BeginRequest = 0,

        AuthenticateRequest = 1,

        ResolveRequestCache = 2,

        UpdateRequestCache = 3,

        EndRequest = 4
    }
}
