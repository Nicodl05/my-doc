using Grpc.Core; // Assurez-vous d'inclure cette directive
using Grpc.Health.V1;

namespace GrpcProject.Services
{
    public class HealthCheckService : Health.HealthBase
    {
        public override Task<HealthCheckResponse> Check(HealthCheckRequest healthCheckRequest, ServerCallContext serverCallContext)
        {
            return Task.FromResult(new HealthCheckResponse
            {
                Status = HealthCheckResponse.Types.ServingStatus.Serving
            });
        }
    }
}
