using MessengerServer.Application;
using MessengerServer.Persistence;
using MessengerServer.Server;

class Program
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
