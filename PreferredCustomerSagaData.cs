using System;
using NServiceBus.Saga;

namespace Miljoparkering.BC.AccountsBC.PreferredCustomer
{
    public class PreferredCustomerSagaData : IContainSagaData
    {
        public Guid Id { get; set; }
        public string Originator { get; set; }
        public string OriginalMessageId { get; set; }

        [Unique]
        public Guid UserId { get; set; }
        public DateTime EndsAt { get; set; }
        public Guid TimeoutGroupId { get; set; }
    }
}