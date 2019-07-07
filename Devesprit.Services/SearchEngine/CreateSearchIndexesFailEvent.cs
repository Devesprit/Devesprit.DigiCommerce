using System;
using Devesprit.Data.Events;

namespace Devesprit.Services.SearchEngine
{
    public partial class CreateSearchIndexesFailEvent : IEvent
    {
        public Exception Exception { get; }

        public CreateSearchIndexesFailEvent(Exception e)
        {
            Exception = e;
        }
    }
}