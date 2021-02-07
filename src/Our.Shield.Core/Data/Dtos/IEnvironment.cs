using System;

namespace Our.Shield.Core.Data.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEnvironment
    {
        /// <summary>
        /// 
        /// </summary>
        Guid Key { get; }

        /// <summary>
        /// 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        string Icon { get; }

        /// <summary>
        /// 
        /// </summary>
        string Domains { get; }

        /// <summary>
        /// 
        /// </summary>
        int SortOrder { get; }

        /// <summary>
        /// 
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// 
        /// </summary>
        bool ContinueProcessing { get; }
    }
}
