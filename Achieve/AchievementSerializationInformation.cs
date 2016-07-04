using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Achieve
{
    public class AchievementSerializationInformation
    {
        public bool Completed
        {
            get;
            set;
        }

        public bool SeenByUser
        {
            get;
            set;
        }

        public object Status
        {
            get;
            set;
        }

        public string UniqueID
        {
            get;
            set;
        }
    }
}
