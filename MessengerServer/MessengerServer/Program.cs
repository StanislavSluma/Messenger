using MessengerServer.Application;
using MessengerServer.Domain.Entities;
using MessengerServer.Persistence;
using MessengerServer.Server;
using System.Net;
using System.Net.Http.Headers;
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

/*        var list = typeof(Papa).GetProperties();
        Papa papa = new();
        foreach (var property in list)
        {
            Console.WriteLine(property.Name);
            if (property.Name == "Test")
            {
                property.SetValue(unit, 4);
                var sdf = property.PropertyType;
                Console.WriteLine(property.PropertyType);
                Console.WriteLine(sdf);
            }
        }
        Console.WriteLine(papa.Test);*/
    }

 /*   public class Papa
    {
        public int Test { get; private set; } = 0;
    }  */

}
