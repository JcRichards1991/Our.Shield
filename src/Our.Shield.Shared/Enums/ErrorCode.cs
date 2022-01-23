using System.ComponentModel;

namespace Our.Shield.Shared.Enums
{
    public enum ErrorCode
    {
        [Description("None")]
        None = 0,

        [Description("Environment - Insert")]
        EnviromentInsert = 1,

        [Description("Environment - Update")]
        EnvironmentUpdate = 2,

        [Description("Environment - Read")]
        EnvironmentRead = 3,

        [Description("Environment - Delete")]
        EnvrionmentDelete = 4,

        [Description("App - Read")]
        AppRead = 100,

        [Description("App - Create")]
        AppCreate = 101,

        [Description("App - Update")]
        AppUpdate = 102,

        [Description("App - Deserialize Configuration")]
        AppDeserializeConfiguration = 103,

        [Description("Apps - Read")]
        AppsRead = 104,
    }
}
