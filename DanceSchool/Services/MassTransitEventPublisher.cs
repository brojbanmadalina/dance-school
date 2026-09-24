using DanceSchool.Business.Interfaces.Common;
using MassTransit;

namespace DanceSchool.Services
{
    public class MassTransitEventPublisher : IEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public Task PublishAsync<T>(T @event) where T : class
            => _publishEndpoint.Publish(@event);
    }
}
