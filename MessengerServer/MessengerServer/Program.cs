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
        UnitOfWork unit = new UnitOfWork(appDbContext);
        await unit.CreateDataBaseAsync();
        //await unit.DeleteDataBaseAsync();
        ApplicationService app = new ApplicationService(unit);

        Server server = new Server(app);
        await server.ListenAsync();
    }
}
