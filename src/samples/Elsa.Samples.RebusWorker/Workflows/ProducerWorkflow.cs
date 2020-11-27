﻿using System;
using Elsa.Activities.Console;
using Elsa.Activities.Rebus;
using Elsa.Activities.Timers;
using Elsa.Builders;
using Elsa.Samples.RebusWorker.Messages;
using NodaTime;

namespace Elsa.Samples.RebusWorker.Workflows
{
    public class ProducerWorkflow : IWorkflow
    {
        private readonly IClock _clock;
        private readonly Random _random;

        public ProducerWorkflow(IClock clock)
        {
            _clock = clock;
            _random = new Random();
        }

        public void Build(IWorkflowBuilder workflow)
        {
            workflow
                .StartAt(_clock.GetCurrentInstant().Plus(Duration.FromSeconds(5)))
                .WriteLine("Sending a random greeting to the \"greetings\" queue.")
                .Then<SendRebusMessage>(sendMessage => sendMessage.Set(x => x.Message, GetRandomGreeting));
        }

        private Greeting GetRandomGreeting()
        {
            var names = new[] { "John", "Jill", "Julia", "Miriam", "Jack", "Bob" };
            var messages = new[] { "Hello!", "How do you do?", "Happy Monday!" };
            var from = _random.Next(0, names.Length);
            var to = _random.Next(0, names.Length);
            var message = _random.Next(0, messages.Length);

            return new Greeting
            {
                From = names[from],
                To = names[to],
                Message = messages[message]
            };
        }
    }
}