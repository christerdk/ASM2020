using System;
using NServiceBus.Saga;

namespace Miljoparkering.Worker.SagaData
{
    public class UserFollowUpSagaData : ISagaEntity
    {
        public UserFollowUpSagaData()
        {
            SendFollowupMessage = true;
        }

        public bool SendFollowupMessage { get; set; }

        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public DateTime SignUpDate { get; set; }

        public Guid Id { get; set; }
        public string Originator { get; set; }
        public string OriginalMessageId { get; set; }

	    public Guid CorrelationId { get; set; }
    }
}
