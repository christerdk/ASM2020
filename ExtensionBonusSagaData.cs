using Miljoparkering.BusinessEvents;
using NServiceBus.Saga;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miljoparkering.BC.MarketingBC.ExtensionBonus
{
    public class ExtensionBonusSagaData : IContainSagaData
    {

        public ExtensionBonusSagaData()
        {
            UserId = Guid.Empty;
            DaysOfBonus = 7;
        }

        public Guid Id { get; set; }
        public string Originator { get; set; }
        public string OriginalMessageId { get; set; }

        [Unique]
        public Guid UserId { get; set; }
        public int DaysOfBonus { get; set; }
    }
}
