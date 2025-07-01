using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RewardsAndRecognitionRepository.Interfaces.Dapper
{
    public interface ISample
    {
        public Task<List<Notification>> GetNotifications();
    }
    public class Notification
    {
        public string Description { get; set; }
        public string Achievements { get; set; }
    }
}
