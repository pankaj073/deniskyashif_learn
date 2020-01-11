using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Linq;

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
            var joe = CreateMessenger("Joe", 5);
            var ann = CreateMessenger("Ann", 5);

            /*
            We ensure that our consumers won't be able to attempt
            writing to it.

            await foreach(var item in joe.ReadAllAsync())
                Console.WriteLine(item);
            */

            /*
            Suppose Ann sends more messages than Joe, We're still going to try
            and read from Joe, even when his channel is completed which is going to
            throw an exception.

            while(await joe.WaitToReadAsync() || await ann.WaitToReadAsync())
            {
                Console.WriteLine(await joe.ReadAsync());
                Console.WriteLine(await ann.ReadAsync());
            }
            */

            /*
            This will force us to wait for Joe, even though we might have
            several messages waiting ready to be read from Ann. Currently, we 
            cannot read from Ann before reading from Joe.

            try 
            {
                Console.WriteLine(await joe.ReadAsync());
                Console.WriteLine(await ann.ReadAsync());
            }
            catch(ChannelClosedException) { }
            */

            var ch = Merge(joe, ann);
            await foreach(var item in ch.ReadAllAsync())
                Console.WriteLine(item);
        }

        static ChannelReader<string> CreateMessenger(string msg, int count) 
        {
            var ch = Channel.CreateUnbounded<string>();
            var rnd = new Random();

            Task.Run(async () => 
            { 
                for(int i = 0; i < count; i++)
                {
                    await ch.Writer.WriteAsync($"{msg} {i}");
                    await Task.Delay(TimeSpan.FromSeconds(rnd.Next(3)));
                }
                ch.Writer.Complete();
            });
            return ch.Reader;
        }

        static ChannelReader<T> Merge<T>(ChannelReader<T> first, ChannelReader<T> second)
        {
            var output = Channel.CreateUnbounded<T>();
            Task.Run(async () => 
            {
                await foreach(var item in first.ReadAllAsync())
                    await output.Writer.WriteAsync(item);
            });
            Task.Run(async () => {
                await foreach(var item in second.ReadAllAsync())
                    await output.Writer.WriteAsync(item);
            });
            return output;
        }

        static ChannelReader<T> Merge<T>(params ChannelReader<T>[] inputs) 
        {
            var output = Channel.CreateUnbounded<T>();
            Task.Run(async () => 
            {
                async Task Redirect(ChannelReader<T> input)
                {
                    await foreach (var item in input.ReadAllAsync())
                        await output.Writer.WriteAsync(item);
                }

                await Task.WhenAll(inputs.Select(i => Redirect(i)).ToArray());
                output.Writer.Complete();
            });
            return output;
        }
    }
}
