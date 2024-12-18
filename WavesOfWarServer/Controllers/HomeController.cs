using Microsoft.AspNetCore.Mvc;
using WavesOfWarServer.Services;

namespace WavesOfWarServer.Controllers
{
    /// <summary>
    /// Controller to handle WebSocket requests.
    /// </summary>
    [ApiController]
    [Route("/")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly WebSocketService _webSocketService = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="logger">The logger to log information and errors.</param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles the WebSocket GET request.
        /// Listens messages and broadcasts them to all connected clients.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [HttpGet(Name = "WebSocket")]
        public async Task Get()
        {
            await _webSocketService.HandleWebSocketAsync(HttpContext, _logger);
        }
    }
}