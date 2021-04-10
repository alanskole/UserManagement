using System.Threading.Tasks;

namespace ManageUsers.Interfaces.BusinessLogic
{
    /// <summary>
    /// Interface with a method used to setup the tables of your database.
    /// </summary>
    public interface ISetupTables
    {
        /// <summary>
        /// This method must be used to automatically create all the necessary tables for the database 
        /// before any other method in the library can be used.
        /// </summary>
        /// <param name="connectionString">The connection string to connect to the SQLite database.</param>
        Task CreateTablesAsync(string connectionString);
    }
}