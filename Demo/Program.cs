
using Achieve;
using MQLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Demo
{

    internal class Program
    {
        // the message sent by the playboard when the user gains points.
        const long POINT_GAIN = 100;

        // unique achievement ids
        const string ACHIEVEMENT_ID_REACH10POINTS = "{a unique id}";
        const string ACHIEVEMENT_ID_WELCOME = "{another unique id}";

        private static void ConfigureAchievements()
        {
            // setup some achievements ...

            // trigger on POINT_GAIN message
            Achiever.Instance.Insert(ACHIEVEMENT_ID_REACH10POINTS, "Reach 10 points", AchievementProcessors.ProcessAchievmentReach10Points, POINT_GAIN);

            // trigger on ANY message
            Achiever.Instance.Insert(ACHIEVEMENT_ID_WELCOME, "Welcome Message", AchievementProcessors.ProcessWelcome, null);

            // ... load achievement state ...
            // See Main for example state loading/saving.
        }

        private static void PrintAchievementList()
        {

            foreach (var A in Achiever.Instance.Achievements)
            {
                Console.WriteLine(A.Description + ", Completed " + A.Info.Completed + ", SeenByUser " + A.Info.SeenByUser);
            }
            Console.WriteLine();

        }
        private static void Main(string[] args)
        {

            ConfigureAchievements();
            Console.WriteLine("Achievements before ...");
            PrintAchievementList();
            // user is playing the level ...
            // 20 Game Updates
            
            for (int i = 0; i < 20; i++)
            {
                //Console.WriteLine("Iter: " + i);

                // Update message queue EACH cycle to feed subscribers
                MQ.Default.Update();
                
                // .... process user io  ... 

                // user scored 2 points !, so tell the Achievement system ...
                MQ.Default.Enqueue(0, Achiever.MESSAGE_QUEUE_ID, POINT_GAIN, (long)2);
            }
            Console.WriteLine("User notifications:");
            // user completed the level
            // update Achiever so that it processes all Achievements ...
            Achiever.Instance.Update();
            // otherwise , if the user failed, we 'might' want to get rid of all the progress ...
            /// Achiever.Instance.PurgeUnprocessed();
            Console.WriteLine();
            Console.WriteLine("Achievements after ...");
            PrintAchievementList();
            
            // Example load/save Achievement state ...
            // do not save achievements while the user is actively playing.

            var state = Achiever.Instance.GetState().ToList();
            
            // using NewtonSoft JSON.NET as an example ...
            string SER = JsonConvert.SerializeObject(state);

            // load state from JSON string as an example ...
            var loaded = JsonConvert.DeserializeObject<List<AchievementSerializationInformation>>(SER);
            
            // Load achievement system.
            Achiever.Instance.LoadState(loaded);

            

            Console.ReadLine();
        }


    }

    /// <summary>
    /// Group Achievement processing in a single place.  
    /// Could be a 'partial' class for convenience.
    /// </summary>
    public partial class AchievementProcessors
    {
        public static void ProcessWelcome(Achievement a, long t, object o)
        {
            a.Info.Completed = true;
            Console.WriteLine("Thank you for installing! You earned 50 gems!");
            // true in this case, but sometimes the user would have to actually click on the achievement from a list.
            a.Info.SeenByUser = true;  
        }

        public static void ProcessAchievmentReach10Points(Achievement a, long t, object o)
        {
            // set the count to 0 if not initialized.
            if (a.Info.Status == null)
                a.Info.Status = (long)0;

            long? cur = (long?)a.Info.Status;
            long? newval = (long?)o;

            if (newval != null)
                cur += newval;

            a.Info.Status = cur;

            if (cur >= 10)
            {
                a.Info.Completed = true;
                Console.WriteLine("Congrats!!!! You have reached 10 points!");
                a.Info.SeenByUser = true;
            }
        }

    }

}