using System;

namespace Epsilon.DbUpdate
{
    class Program
    {
        static void Main(string[] args)
        {
            string path, cs;
            if (args.Length < 2)
            {
                Console.Write("Assemblies Path: ");
                path = Console.ReadLine();
                Console.Write("Connection String: ");
                cs = Console.ReadLine();
            }
            else
            {
                path = args[0];
                cs = args[1];
            }

            var updater = new Updater(cs, path, Console.WriteLine);
            updater.Update();
        }
    }
}
