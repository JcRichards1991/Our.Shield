using System.ComponentModel;

namespace Our.Shield.Shared.Enums
{
    public enum ErrorCode
    {
        [Description("None")]
        None,

        [Description("Environment - Insert Error")]
        EnviromentInsert,

        [Description("Environment - Update Error")]
        EnvironmentUpdate,

        [Description("Environment Read Error")]
        EnvironmentRead
    }
}
