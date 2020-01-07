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

        static async Task<int> ChannelsDemo() 
        {
            Channel<string> ch = Channel.CreateUnbounded<string>();
            await ch.Writer.WriteAsync("My first message");
            await ch.Writer.WriteAsync("My second message");
            ch.Writer.Complete();

            while(await ch.Reader.WaitToReadAsync()) 
                Console.WriteLine(await ch.Reader.ReadAsync());
            return 0;
        }
    }
}
