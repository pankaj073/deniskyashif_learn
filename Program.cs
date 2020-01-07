using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace deniskyashif_learn
{
    class Program
    {
        static void Main(string[] args)
        {
            // Console.WriteLine("Hello World!");
            try 
            {
                ChannelsDemo().GetAwaiter().GetResult();
            }
            catch(Exception ex) 
            {
                Console.WriteLine($"There was an exception: {ex.ToString()}");
            }
        }

        static async Task ChannelsDemo() 
        {
            var ch = Channel.CreateUnbounded<string>();

            var consumer = Task.Run(async () => 
            {
                while(await ch.Reader.WaitToReadAsync())
                    Console.WriteLine(await ch.Reader.ReadAsync());
            });

            var producer = Task.Run(async () => {
               var rnd = new Random();
               for(int i = 0; i < 5; i++) {
                   await Task.Delay(TimeSpan.FromSeconds(rnd.Next(3)));
                   await ch.Writer.WriteAsync($"Message {i}");
               } 
               ch.Writer.Complete();
            });

            await Task.WhenAll(producer, consumer);
        }
    }
}
