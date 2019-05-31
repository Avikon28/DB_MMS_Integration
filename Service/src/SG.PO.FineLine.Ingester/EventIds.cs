using Microsoft.Extensions.Logging;

namespace SG.PO.FineLine.Ingester
{
    public static class EventIds
    {
        private const int Offset = 300;
        public static readonly EventId ErrorProcessingEvent = new EventId(0 + Offset, "Error processing event");
        public static readonly EventId SuccessfullyProcessedEvent = new EventId(1 + Offset, "Successfully processed event");
    }
}
