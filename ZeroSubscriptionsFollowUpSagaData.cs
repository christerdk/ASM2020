using System;
using NServiceBus.Saga;

namespace Miljoparkering.Worker.SagaData
{
    public class ZeroSubscriptionsFollowUpSagaData : ISagaEntity
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime ReachedZeroSubscriptionAt { get; set; }

        public Guid Id { get; set; }
        public string Originator { get; set; }
        public string OriginalMessageId { get; set; }

	    public Guid CorrelationId { get; set; }
    }
}