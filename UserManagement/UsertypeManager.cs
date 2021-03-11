using System;
using System.Collections.Generic;
using System.Text;
using UserManagement.UOW;
using UserManagement.Model;
using UserManagement.CustomExceptions;
using System.Threading.Tasks;

namespace UserManagement
{
    public class UsertypeManager
    {
        private UnitOfWork _unitOfWork = new UnitOfWork();

        public async Task<List<Usertype>> AddMoreUsertypes(string connectionString, params string[] userTypes)
        {
            List<Usertype> types = await _unitOfWork.UsertypeRepository.Create(connectionString, userTypes);

            if (types.Count == 0)
                throw new FailedToCreateException("Usertype");

            return types;
        }

        public async Task UpdateUsertype(string connectionString, string usertype, string updatedUsertype)
        {
            Usertype type = await _unitOfWork.UsertypeRepository.GetUsertype(connectionString, usertype);

            await _unitOfWork.UsertypeRepository.Update(connectionString, type.Id, updatedUsertype);
        }
    }
}
