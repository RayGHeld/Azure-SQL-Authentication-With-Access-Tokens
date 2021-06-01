using System;
using System.Collections.Generic;
using System.Linq;

namespace Console_Connect_SQL
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!\n");

            string access_token = string.Empty;
            Dictionary<Guid, string> users = new Dictionary<Guid, string>();

            //Console.WriteLine("Trying the windows integrated auth...");
            //access_token = Azure_SQL.Get_TokenByIntegratedWindows().Result;


            // get the data from the database with a user signin....
            Console.WriteLine("Signing in as a user...\n");
            access_token = Azure_SQL.GetAccessToken_UserInteractive().Result;
            users = Azure_SQL.GetUsernames(access_token);

            foreach(KeyValuePair<Guid,string> user in users)
            {
                Console.WriteLine($"ID: {user.Key.ToString()} USER: {user.Value.ToString()}");
            }

            // get the database from the database with a client credentials grant flow...
            Console.WriteLine("\nSigning in as a service principal...\n");
            access_token = Azure_SQL.GetAccessToken_ClientCredentials().Result;
            users = Azure_SQL.GetUsernames(access_token);

            foreach (KeyValuePair<Guid, string> user in users)
            {
                Console.WriteLine($"ID: {user.Key.ToString()} USER: {user.Value.ToString()}");
            }

            Console.Write("\nPress any key to close...");
            Console.ReadKey();
        }
    }
}
