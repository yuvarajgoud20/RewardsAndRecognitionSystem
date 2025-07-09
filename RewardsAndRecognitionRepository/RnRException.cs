using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RewardsAndRecognitionRepository
{
    public class RnRException : Exception
    {
        public RnRException(string ErrorMessage):base(ErrorMessage)
        {
            
        }
    }
}
