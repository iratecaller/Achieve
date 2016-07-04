using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Achieve
{
    public class Achievement
    {
        public Achievement(string UniqueID, string description, Action<Achievement, long, object> processor, params long[] included_types)
        {
            Info = new AchievementSerializationInformation()
            {
                UniqueID = UniqueID,
                Completed = false,
                Status = null
            };

            Description = description;
            IncludedTypes = included_types;
            OnProcessAchievment = processor;
        }

        public string Description
        {
            get;
            set;
        }

        // filter on message types.
        public long[] IncludedTypes
        {
            get;
            set;
        }

        public AchievementSerializationInformation Info
        {
            get;
            set;
        }
        public Action<Achievement, long, object> OnProcessAchievment
        {
            get;
            set;
        }
    }
}
