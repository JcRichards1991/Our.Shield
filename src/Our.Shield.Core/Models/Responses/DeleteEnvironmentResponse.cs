namespace Our.Shield.Core.Models.Responses
{
    /// <summary>
    /// 
    /// </summary>
    public class DeleteEnvironmentResponse : BaseResponse
    {
        /// <summary>
        /// Whether or not the environment was deleted
        /// </summary>
        public bool Successful { get; set; }
    }
}
