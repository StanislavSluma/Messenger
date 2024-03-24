using MessengerClient.Client;
using MessengerClient.Domain.Entities;
using System.Net.Sockets;
using System.Text.Json;


public class Programm
{
    static public async Task Main(string[] args)
    {
        Client clienttt = new Client();
        await clienttt.ClientStart();
    }
}