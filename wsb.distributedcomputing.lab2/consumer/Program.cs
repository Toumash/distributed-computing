using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace consumer
{
    class Program
    {
        static ConsumerConfig conf;
        static void Main(string[] args)
        {
            conf = new ConsumerConfig
            {
                GroupId = "test-consumer-group",
                BootstrapServers = Environment.GetEnvironmentVariable("KAFKA"),
                // Note: The AutoOffsetReset property determines the start offset in the event
                // there are not yet any committed offsets for the consumer group for the
                // topic/partitions of interest. By default, offsets are committed
                // automatically, so in this example, consumption will only start from the
                // earliest message in the topic 'my-topic' the first time you run the program.
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var topics = (Environment.GetEnvironmentVariable("TOPIC") ?? "test-topic").Split(',').ToList();
            System.Console.WriteLine("Listening on Topics:" + string.Join(",", topics));
            Task.WaitAll(topics.Select(t => Task.Run(() => CreateConsumer(t, (r) => Console.WriteLine($"Consumed message from topic {t} : {r.Message.Value} ")))).ToArray());
            System.Console.WriteLine("end");
        }

        static async Task CreateConsumer(string topic, Action<ConsumeResult<Ignore, string>> consumed)
        {
            using (var c = new ConsumerBuilder<Ignore, string>(conf).Build())
            {
                c.Subscribe(topic);

                CancellationTokenSource cts = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true; // prevent the process from terminating.
                    cts.Cancel();
                };

                try
                {
                    while (true)
                    {
                        try
                        {
                            var cr = c.Consume(cts.Token);
                            consumed(cr);
                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Error occured: {e.Error.Reason}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ensure the consumer leaves the group cleanly and final offsets are committed.
                    c.Close();
                    Console.WriteLine($"Operation cancalled");
                }
            }
        }
    }
}
