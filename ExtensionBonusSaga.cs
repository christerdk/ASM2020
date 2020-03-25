using Miljoparkering.BusinessEvents;
using Miljoparkering.BusinessCommands;
using NServiceBus;
using NServiceBus.Saga;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Miljoparkering.BusinessCommands.Accounts;
using Miljoparkering.Tools;

namespace Miljoparkering.BC.MarketingBC.ExtensionBonus
{
    public class ExtensionBonusSaga : Saga<ExtensionBonusSagaData>, 
        IAmStartedByMessages<CustomerPreferredWarningTimeoutEvent>,
        IHandleMessages<CustomerSetPreferredEvent>
    {

        public override void ConfigureHowToFindSaga()
        {
            ConfigureMapping<CustomerSetPreferredEvent>(saga => saga.UserId, obj => obj.UserId);
            ConfigureMapping<CustomerPreferredWarningTimeoutEvent>(saga => saga.UserId, obj => obj.UserId);
        }

        public void Handle(CustomerPreferredWarningTimeoutEvent message)
        {
            if (message.Type != "first") //Offer only at first timeout warning
            {
                MarkAsComplete();
                return;
            }

            if (Data.UserId != Guid.Empty)
            {
                return;
            }


            Data.UserId = message.UserId;
            Data.DaysOfBonus = 14;
            SetupExtensionBonusOfferTimeout(GetTimeoutTime(message));
        }

        private static DateTime GetTimeoutTime(CustomerPreferredWarningTimeoutEvent message)
        {
            return DateTime.Now.AddDays(1);
        }

        private void SetupExtensionBonusOfferTimeout(DateTime when)
        {
            RequestUtcTimeout(when, new ExtensionBonusOfferTimeoutMessage());
        }

        public void Timeout(ExtensionBonusOfferTimeoutMessage state)
        {
            MarkAsComplete();
        }



        public void Handle(CustomerSetPreferredEvent message)
        {
            if (message.IsExtension)
            {
                var msg = new SetCustomerPreferredCommand
                {
                    DaysAhead = Data.DaysOfBonus,
                    UserId = Data.UserId
                };
                Bus.Send(msg);
            }
            //Remember, the action above will result in ANOTHER extension. 
            //We there make sure to mark the saga as complete, so that the bonus will not create another bonus.
            //Of course, it should be marked complete anyways, but still..
            MarkAsComplete(); 
        }
    }
}
