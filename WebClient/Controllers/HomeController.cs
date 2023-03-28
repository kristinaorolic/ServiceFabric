using Common;
using Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using WebClient.Models;

namespace WebClient.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
                ViewBag.Devices = await AllDevices();
            return View();
        }

        public async Task<IActionResult> ShowRemonts()
        {
            ViewBag.Remonts = await ShowAllRemonts();

            return View("AllRemonts");
        }

        public async Task<IActionResult> ShowAllHistoryRemonts()
        {
            ViewBag.HistoryRemonts = await GetAllRemontsFromHistory();

            return View("HistoryRemonts");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<ActionResult> SaveDevice(string name)
        {
            Device device = new Device();
            device.Name = name;
            device.IsOnRemont = false;

            FabricClient fabricClient = new FabricClient();
            int partitionNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TestServiceFabric/DeviceService"))).Count;
            var binding = WcfUtility.CreateTcpClientBinding();
            int index = 0;

            ServicePartitionClient<WcfCommunicationClient<IDeviceService>> servicePartitionClient = new
                ServicePartitionClient<WcfCommunicationClient<IDeviceService>>(
                new WcfCommunicationClientFactory<IDeviceService>(binding),
                new Uri("fabric:/TestServiceFabric/DeviceService"),
                new ServicePartitionKey(index % partitionNumber));
            bool successfull = await servicePartitionClient.InvokeWithRetryAsync(client => client.Channel.SaveDevice(device));

            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<List<Device>> AllDevices()
        {
            FabricClient fabricClient = new FabricClient();
            int partitionNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TestServiceFabric/DeviceService"))).Count;
            var binding = WcfUtility.CreateTcpClientBinding();
            int index = 0;

            ServicePartitionClient<WcfCommunicationClient<IDeviceService>> servicePartitionClient = new
                ServicePartitionClient<WcfCommunicationClient<IDeviceService>>(
                new WcfCommunicationClientFactory<IDeviceService>(binding),
                new Uri("fabric:/TestServiceFabric/DeviceService"),
                new ServicePartitionKey(index % partitionNumber));
            return await servicePartitionClient.InvokeWithRetryAsync(client => client.Channel.GetAllDevices());
        }

        [HttpGet]
        public async Task<List<Remont>> ShowAllRemonts()
        {
            FabricClient fabricClient = new FabricClient();
            int partitionNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TestServiceFabric/RemontService"))).Count;
            var binding = WcfUtility.CreateTcpClientBinding();
            int index = 0;

            ServicePartitionClient<WcfCommunicationClient<IRemontService>> servicePartitionClient = new
                ServicePartitionClient<WcfCommunicationClient<IRemontService>>(
                new WcfCommunicationClientFactory<IRemontService>(binding),
                new Uri("fabric:/TestServiceFabric/RemontService"),
                new ServicePartitionKey(index % partitionNumber));
            return await servicePartitionClient.InvokeWithRetryAsync(client => client.Channel.GetAllRemonts());
        }

        private async Task<List<Remont>> GetAllRemontsFromHistory()
        {
            FabricClient fabricClient = new FabricClient();
            int partitionNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TestServiceFabric/RemontService"))).Count;
            var binding = WcfUtility.CreateTcpClientBinding();
            int index = 0;

            ServicePartitionClient<WcfCommunicationClient<IRemontService>> servicePartitionClient = new
                ServicePartitionClient<WcfCommunicationClient<IRemontService>>(
                new WcfCommunicationClientFactory<IRemontService>(binding),
                new Uri("fabric:/TestServiceFabric/RemontService"),
                new ServicePartitionKey(index % partitionNumber));
            return await servicePartitionClient.InvokeWithRetryAsync(client => client.Channel.GetAllHistoryRemonts());
        }


        [HttpPost]
        public async Task<ActionResult> AllRemonts(string id)
        {
            Remont remont = new Remont();
            Random rnd = new Random();
            int days = rnd.Next(1, 9);
            int daysOnRemont = rnd.Next(1, 12);
            remont.TimeInMagacin = days;
            remont.TimeOfExploatation = DateTime.Now;
            remont.TimeOnRemont = daysOnRemont;
            remont.IdOfDevice = id;

            FabricClient fabricClient = new FabricClient();
            int partitionNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TestServiceFabric/RemontService"))).Count;
            var binding = WcfUtility.CreateTcpClientBinding();
            int index = 0;

            ServicePartitionClient<WcfCommunicationClient<IRemontService>> servicePartitionClient = new
                ServicePartitionClient<WcfCommunicationClient<IRemontService>>(
                new WcfCommunicationClientFactory<IRemontService>(binding),
                new Uri("fabric:/TestServiceFabric/RemontService"),
                new ServicePartitionKey(index % partitionNumber));
            bool successfull = await servicePartitionClient.InvokeWithRetryAsync(client => client.Channel.SaveRemont(remont));

            return RedirectToAction("ShowRemonts");
        }
    }
}
