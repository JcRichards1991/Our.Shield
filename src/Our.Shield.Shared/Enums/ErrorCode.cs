using System.ComponentModel;

namespace Our.Shield.Shared.Enums
{
    public enum ErrorCode
    {
        [Description("None")]
        None,

        [Description("Environment - Insert")]
        EnviromentInsert,

        [Description("Environment - Update")]
        EnvironmentUpdate,

        [Description("Environment - Read")]
        EnvironmentRead,

        [Description("Environment - Delete")]
        EnvrionmentDelete
    }
}
