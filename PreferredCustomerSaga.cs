using System;
using Miljoparkering.BusinessEvents;
using Miljoparkering.Tools;
using NServiceBus;
using NServiceBus.Saga;

namespace Miljoparkering.BC.AccountsBC.PreferredCustomer
{
    public class PreferredCustomerSaga : Saga<PreferredCustomerSagaData>,
        IAmStartedByMessages<CustomerSetPreferredEvent>,
        IHandleTimeouts<PreferredCustomerSagaTimeoutMessage>,
        IHandleMessages<CustomerPreferredRemovedEvent>,
        IHandleMessages<UserRemovedEvent>

    {
        private readonly Logger _logger = new Logger(typeof(PreferredCustomerSaga));
        private const string FIRST_WARNING = "FIRST_WARNING";
        private const string SECOND_WARNING = "SECOND_WARNING";
        private const string TIMEOUT = "TIMEOUT";

        public override void ConfigureHowToFindSaga()
        {
            ConfigureMapping<CustomerSetPreferredEvent>(s => s.UserId, m => m.UserId);
            ConfigureMapping<CustomerPreferredRemovedEvent>(s => s.UserId, m => m.UserId);
            ConfigureMapping<UserRemovedEvent>(s => s.UserId, m => m.UserId);
        }

        public void Handle(CustomerSetPreferredEvent message)
        {
            //Note that the saga is meant to handle one or more of these messages, updating the saga timeout and EndsAt value.

            this.Data.UserId = message.UserId;
            this.Data.EndsAt = message.EndsAt;
            this.Data.TimeoutGroupId = Guid.NewGuid();
            
            SetupTimeout(this.Data.TimeoutGroupId, this.Data.EndsAt.AddDays(-3), FIRST_WARNING);
            SetupTimeout(this.Data.TimeoutGroupId, this.Data.EndsAt.AddDays(-1), SECOND_WARNING);
            SetupTimeout(this.Data.TimeoutGroupId, this.Data.EndsAt, TIMEOUT);
        }

        private void SetupTimeout(Guid id, DateTime when, string type)
        {
            RequestUtcTimeout(when, new PreferredCustomerSagaTimeoutMessage
                {
                    TimeoutId = id,
                    TimeoutType = type
                });
        }

        public void Timeout(PreferredCustomerSagaTimeoutMessage state)
        {
            if (state.TimeoutId == this.Data.TimeoutGroupId)
            {
                switch (state.TimeoutType)
                {
                    case FIRST_WARNING:
                        var first = new CustomerPreferredWarningTimeoutEvent
                            {
                                CorrelationId = Guid.NewGuid(),
                                UserId = this.Data.UserId,
                                EndsAt = this.Data.EndsAt,
                                Type = "first"
                            };
                        Bus.Publish(first);
                        break;
                    case SECOND_WARNING:
                        var second = new CustomerPreferredWarningTimeoutEvent
                            {
                                CorrelationId = Guid.NewGuid(),
                                UserId = this.Data.UserId,
                                EndsAt = this.Data.EndsAt,
                                Type = "second"
                            };
                        Bus.Publish(second);
                        break;
                    case TIMEOUT:
                        var evnt = new CustomerPreferredTimeoutEvent
                            {
                                CorrelationId = Guid.NewGuid(),
                                UserId = this.Data.UserId,
                                TimedOutAt = DateTime.Now,
                                EndedAt = this.Data.EndsAt
                            };
                        Bus.Publish(evnt);
                        MarkAsComplete();
                        break;
                }
            }
        }

        public void Handle(CustomerPreferredRemovedEvent message)
        {
            MarkAsComplete();
        }

        public void Handle(UserRemovedEvent message)
        {
            MarkAsComplete();
        }
    }
}
