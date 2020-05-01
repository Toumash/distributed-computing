using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using System.Threading;

namespace producer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ProducerConfig { BootstrapServers = Environment.GetEnvironmentVariable("KAFKA") };
            var topic = Environment.GetEnvironmentVariable("TOPIC");
            if (topic == null)
                Console.Error.WriteLine("no TOPIC environment variable specified");
            // If serializers are not specified, default serializers from
            // `Confluent.Kafka.Serializers` will be automatically used where
            // available. Note: by default strings are encoded as UTF8.
            using (var p = new ProducerBuilder<Null, string>(config).Build())
            {
                while (true)
                {
                    try
                    {
                        var dr = await p.ProduceAsync(topic, new Message<Null, string> { Value = DateTime.UtcNow.ToString() });
                        Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
                        Thread.Sleep(1000);
                    }
                    catch (ProduceException<Null, string> e)
                    {
                        Console.WriteLine($"Delivery failed: {e.Error.Reason}");
                    }
                }
            }
        }
    }
}
