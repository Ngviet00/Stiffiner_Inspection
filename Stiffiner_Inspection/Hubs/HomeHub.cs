using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Stiffiner_Inspection.Hubs
{
    public class HomeHub : Hub
    {
        public HomeHub()
        {

        }

        [HttpPost]
        public async void GetCurrentStatusPLC()
        {
            await Clients.All.SendAsync("SetCurrentStatusPLC", 1);
        }
    }
}
