using System;
using Miljoparkering.BusinessEvents;
using Miljoparkering.Tools;
using Miljoparkering.Worker.SagaData;
using NServiceBus;
using NServiceBus.Saga;

namespace Miljoparkering.Worker.Sagas
{
    public class ZeroSubscriptionsSagaHandler : Saga<ZeroSubscriptionsFollowUpSagaData>,
                                                IAmStartedByMessages<UserReachedZeroSubscriptionsEvent>,
                                                IHandleTimeouts<ZeroSubscriptionsFollowUpTimeout>,
                                                IHandleMessages<UserAddingSubscriptionEvent>,
                                                IHandleMessages<UserRemovedEvent>
    {
        private readonly Logger _logger = new Logger(typeof(ZeroSubscriptionsSagaHandler));

        public override void ConfigureHowToFindSaga()
        {
            ConfigureMapping<UserAddingSubscriptionEvent>(obj => obj.UserId, m => m.UserId);
            ConfigureMapping<UserReachedZeroSubscriptionsEvent>(obj => obj.UserId, m => m.UserId);
            ConfigureMapping<UserRemovedEvent>(obj => obj.UserId, m => m.UserId);
        }

        public void Handle(UserReachedZeroSubscriptionsEvent message)
        {

            if (Data.CorrelationId != Guid.Empty)
                return;

            Data.CorrelationId = message.CorrelationId;
            Data.UserId = message.UserId;
            Data.FirstName = message.FirstName;
            Data.LastName = message.LastName;
            Data.Email = message.Email;
            Data.ReachedZeroSubscriptionAt = message.ReachedZeroSubscriptionAt;

            var waitTime = TimeSpan.FromHours(24);  //Follow up in 24 hours

            RequestUtcTimeout<ZeroSubscriptionsFollowUpTimeout>(waitTime);
        }

        public void Timeout(ZeroSubscriptionsFollowUpTimeout state)
        {
            var userRemainedAtZeroSubscriptions = new UserRemainedAtZeroStreetSubscriptionsEvent()
            {
                CorrelationId = Data.CorrelationId,
                Email = Data.Email,
                FirstName = Data.FirstName,
                LastName = Data.LastName,
                ReachedZeroSubscriptionAt = Data.ReachedZeroSubscriptionAt,
                RecheckedAt = DateTime.Now,
                UserId = Data.UserId
            };
            Bus.Publish(userRemainedAtZeroSubscriptions);
            MarkAsComplete();
        }

        public void Handle(UserAddingSubscriptionEvent message)
        {
            _logger.InfoFormat("Handling {0} with correlationId {1}", message.GetType(), message.CorrelationId);

            MarkAsComplete();
        }

        public void Handle(UserRemovedEvent message)
        {
            MarkAsComplete();
        }
    }
}