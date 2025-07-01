using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using RewardsAndRecognitionRepository.Dapper;
using RewardsAndRecognitionRepository.Interfaces.Dapper;

namespace RewardsAndRecognitionRepository.Repos.Dapper
{
    public class DapperNotification : ISample
    {
        private readonly DapperContext _dapperContext;
        public DapperNotification(DapperContext dapperContext)  
                   {
            _dapperContext = dapperContext;
        }


        public async Task<List<Notification>> GetNotifications()
        {
           

            //var parameters = new DynamicParameters();

            //parameters.Add("@Hours", hours);



            using (var conn = _dapperContext.CreateConnection())

            {

                var result = await conn.QueryAsync<Notification>("sp_GetNominationData");

                return result.ToList();

            }

    }
    }
}
