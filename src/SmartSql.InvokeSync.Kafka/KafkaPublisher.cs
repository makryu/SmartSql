﻿using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SmartSql.InvokeSync.Kafka
{
    public class KafkaPublisher : IPublisher
    {
        private readonly KafkaOptions _kafkaOptions;
        private readonly ILogger<KafkaPublisher> _logger;
        private readonly IProducer<Null, String> _producer;

        public KafkaPublisher(KafkaOptions kafkaOptions
            , ILogger<KafkaPublisher> logger)
        {
            _kafkaOptions = kafkaOptions;
            _logger = logger;
            _producer = new ProducerBuilder<Null, String>(_kafkaOptions.AsKafkaConfig()).Build();
        }

        public async Task PublishAsync(SyncRequest publishRequest)
        {
            var data = JsonConvert.SerializeObject(publishRequest);
            var deliveryResult = await _producer.ProduceAsync(_kafkaOptions.Topic, new Message<Null, string>
            {
                Value = data
            });

            if (deliveryResult.Status == PersistenceStatus.Persisted ||
                deliveryResult.Status == PersistenceStatus.PossiblyPersisted)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug($"kafka topic message [{_kafkaOptions.Topic}] has been published.");
                }
            }
            else
            {
                _logger.LogError($"kafka topic message [{_kafkaOptions.Topic}] publish failed.");
            }
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}