using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace net_barrier
{
    class Program
    {
        static void Main(string[] args)
        {
            int total_client_count = 100;
            int base_id = 101;

            var manager = new ClientManager(total_client_count, base_id);

            manager.ManaMain(total_client_count);

            Console.WriteLine("!!!!!!!!!!!!!!!");
            Console.ReadKey();
        }
    }

    class ClientManager
    {
        Client[] clients;
        int base_id;

        static ConcurrentBag<bool> TotalClientCount = new ConcurrentBag<bool>();
        Barrier barrier;

        public ClientManager(int total_client_count, int input_base_id)
        {
            clients = new Client[total_client_count];
            base_id = input_base_id;
        }

        public void ManaMain(int total_client_count)
        {
            barrier = new Barrier(total_client_count);

            for (int i = 0; i < total_client_count; ++i)
            {
                Client temp = new Client();
                temp.id = base_id + i;
                temp.cli_barrier = barrier;

                clients[i] = temp;
            }

            foreach (var client in clients)
            {
                var newTheread = new Thread(OnTime);
                newTheread.Start(client);
            }

            Thread.Sleep(-1);
        }

        static void OnTime(object client)
        {
            Client actor = (Client)client;
            while (!actor.is_closed)
            {
                actor.TestMain();

                actor.CheckTotalCount();
                if (actor.is_closed)
                    TotalClientCount.Add(true);
            }
        }
    }

    class Client
    {
        public int id = 0;
        public bool is_closed = false;

        private int total_count = 0;
        public Barrier cli_barrier;

        public void TestMain()
        {
            int count = 0;

            while (count < 100000)
                ++count;

            Console.WriteLine(string.Format("[{0}] TestMain Complete", id));
            ++total_count;
        }

        public void CheckTotalCount()
        {
            if (total_count > 20)
                is_closed = true;

            Console.WriteLine(string.Format("[{0}] Barrier signalAndWait {1}", id, total_count));
            cli_barrier.SignalAndWait();
            Console.WriteLine(string.Format("[{0}] Barrier End {1}", id, total_count));

            Thread.Sleep(500);
        }
    }
}