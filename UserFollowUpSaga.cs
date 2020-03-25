using System;
using Miljoparkering.BusinessEvents;
using Miljoparkering.Tools;
using Miljoparkering.Worker.SagaData;
using NServiceBus;
using NServiceBus.Saga;

namespace Miljoparkering.Worker.Sagas
{
    public class UserFollowUpSaga : Saga<UserFollowUpSagaData>,
        IAmStartedByMessages<UserCreatedEvent>,
        IHandleTimeouts<UserCreatedFollowUpTimeoutData>,
        IHandleMessages<UserRemainedAtZeroStreetSubscriptionsEvent>,
        IHandleMessages<UserAddedSubscriptionEvent>,
        IHandleMessages<UserRemovedEvent>
    {
		private readonly Logger _logger = new Logger(typeof(UserFollowUpSaga));

        public override void ConfigureHowToFindSaga()
        {
            this.ConfigureMapping<UserCreatedEvent>(s => s.UserId, m => m.UserId);
            this.ConfigureMapping<UserRemainedAtZeroStreetSubscriptionsEvent>(s => s.UserId, m => m.UserId);
            this.ConfigureMapping<UserAddedSubscriptionEvent>(s => s.UserId, m => m.UserId);
            this.ConfigureMapping<UserRemovedEvent>(s => s.UserId, m => m.UserId);
        }
        

        public void Handle(UserCreatedEvent message)
        {
	            Data.CorrelationId = message.CorrelationId;
                Data.UserId = message.UserId;
                Data.Email = message.Email;
                Data.FirstName = message.FirstName;
                Data.SignUpDate = message.SignUpDate;


                waitUntil = waitUntil.AddDays(45).NextWorkTime();
                RequestUtcTimeout<UserCreatedFollowUpTimeoutData>(waitUntil);
        }

        public void Timeout(UserCreatedFollowUpTimeoutData state)
        {
            if (Data.SendFollowupMessage)
            {
                var evnt = new FollowUpOnUserCreationCommand()
                    {
                        CorrelationId = Data.CorrelationId,
                        UserId = Data.UserId,
                        FirstName = Data.FirstName,
                        Email = Data.Email,
                        SignUpDate = Data.SignUpDate
                    };
                Bus.Send(evnt);
            }
            MarkAsComplete();
        }

        public void Handle(UserRemainedAtZeroStreetSubscriptionsEvent message)
        {
            this.Data.SendFollowupMessage = false;
        }

        public void Handle(UserAddedSubscriptionEvent message)
        {
            this.Data.SendFollowupMessage = true;
        }

        public void Handle(UserRemovedEvent message)
        {
            MarkAsComplete();
        }
    }
}
