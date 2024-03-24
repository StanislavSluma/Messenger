using MessengerServer.Application;
using MessengerServer.Domain.Entities;
using MessengerServer.Persistence;
using MessengerServer.Server;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Transactions;

class Programm
{
    static public async Task Main(string[] args)
    {
        AppDbContext appDbContext = new AppDbContext();
        appDbContext.EnsureCreateAsync().Wait();
        //appDbContext.EnsureDeleteAsync().Wait();
        UnitOfWork unit = new UnitOfWork(appDbContext);
        ApplicationService app = new ApplicationService(unit);

        Server server = new Server(app);
        await server.ListenAsync();
    }
}
