using System.Threading.Tasks;

namespace ManageUsers.BusinessLogic.Interface
{
    /// <summary>
    /// Interface with a method used to setup the tables of your database.
    /// </summary>
    public interface ISetupTables
    {
        /// <summary>
        /// A method to automatically create all the necessary tables for the database. 
        /// This method must be used before any other method in the library can be used.
        /// </summary>
        Task CreateTablesAsync();
    }
}