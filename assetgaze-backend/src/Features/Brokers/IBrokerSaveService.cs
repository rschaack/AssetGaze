using Assetgaze.Backend.Features.Brokers.DTOs;

namespace Assetgaze.Backend.Features.Brokers;

public interface IBrokerSaveService
{
    Task<Broker> SaveBrokerAsync(CreateBrokerRequest request, Guid loggedInUserId);
}