using Our.Shield.Core.Models;
using System.Threading.Tasks;

namespace Our.Shield.Core.Data.Accessors
{
    /// <summary>
    /// Accessor for reading/writing journal messages to the database
    /// </summary>
    public interface IJournalAccessor
    {
        /// <summary>
        /// Writes a journal to the database
        /// </summary>
        /// <param name="journal">The journal to add</param>
        /// <returns></returns>
        Task<bool> Write(IJournal journal);
    }
}
