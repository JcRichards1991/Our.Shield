namespace Our.Shield.Core.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUmbracoContentService
    {
        /// <summary>
        /// 
        /// </summary>
        void ClearCache();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string Url(int id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        string Name(int id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        int? ParentId(int id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        int? XPath(string xpath);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        object Value(int id, string alias);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        T Value<T>(int id, string alias);
    }
}
