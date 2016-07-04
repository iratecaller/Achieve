using MQLight;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Achieve
{
    public class Achiever
    {
        /// <summary>
        /// Achiever subscribes to this message queue...
        /// </summary>
        public const long MESSAGE_QUEUE_ID = 1381488; // hopefullly this message isn't in use! ;)
        
        /// <summary>
        /// Queue of unprocessed messages.
        /// </summary>
        
        private List<Message> messages;

        private static Achiever _instance;

        /// <summary>
        /// private constructor, since Achiever should be single instance.
        /// </summary>
        private Achiever()
        {
            messages = new List<Message>();
            _achievements = new List<Achievement>();
            MQ.Default.Subscribe(SubscriptionType.DESTINATION, MESSAGE_QUEUE_ID, OnMessage);
        }


        /// <summary>
        /// The single instance
        /// </summary>
        public static Achiever Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Achiever();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Available achievements
        /// </summary>
        private List<Achievement> _achievements
        {
            get;
            set;
        }

        public IEnumerable<Achievement> Achievements
        {
            get
            {
                return _achievements;
            }
        }
        /// <summary>
        /// Serialization Helper
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AchievementSerializationInformation> GetState()
        {
            foreach (var x in _achievements)
                yield return x.Info;
            yield break;
        }

        /// <summary>
        /// Insert  achievements. Must have unique ID.
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public bool Insert(Achievement A)
        {
            if (_achievements == null)
                _achievements = new List<Achievement>();
            var existing = from x in _achievements where x.Info.UniqueID == A.Info.UniqueID select x;
            if (existing == null || existing.Count() == 0)
            {
                _achievements.Add(A);
                return true;
            }
            return false;
        }

        public bool Insert(string UniqueID, string description, Action<Achievement, long, object> processor, params long[] included_types)
        {
            return Insert(new Achievement(UniqueID, description, processor, included_types));
        }

        /// <summary>
        /// Serialization Helper
        /// </summary>
        /// <param name="state"></param>
        public void LoadState(IEnumerable<AchievementSerializationInformation> state)
        {
            foreach (var S in state)
            {
                var match = (from x in _achievements where x.Info.UniqueID == S.UniqueID select x).FirstOrDefault();
                if (match != null)
                {
                    match.Info.Completed = S.Completed;
                    match.Info.Status = S.Status;
                }
            }
        }

        /// <summary>
        /// Purge the holding area when the user cancels, fails the level, or quits prematurely.
        /// </summary>
        public void PurgeUnprocessed()
        {
            messages.Clear();
        }

        /// <summary>
        /// Update pumps all messages in the holding area to appropriate achievements.
        /// </summary>
        public void Update()
        {
            while (messages.Count > 0)
            {
                var M = messages[0];
                messages.RemoveAt(0);
                if (_achievements != null)
                {
                    foreach (var A in _achievements)
                    {
                        if (!A.Info.Completed)
                        {
                            if (A.IncludedTypes == null || A.IncludedTypes.Contains(M.type))
                            {
                                A.OnProcessAchievment(A, M.type, M.message);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Subscriber to Message Queue. Simply adds messages in holding area. (FAST)
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private bool OnMessage(Message m)
        {
            messages.Add(m);

            return true; // purge from queue
        }
    }
}